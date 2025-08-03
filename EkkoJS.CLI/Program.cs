using EkkoJS.Core;
using EkkoJS.Package.CLI.Compiler;

namespace EkkoJS.CLI;

class Program
{
    static async Task<int> Main(string[] args)
    {
        if (args.Length == 0)
        {
            ShowHelp();
            return 0;
        }

        var command = args[0].ToLowerInvariant();

        switch (command)
        {
            case "run":
                if (args.Length < 2)
                {
                    Console.Error.WriteLine("Error: Please specify a file to run");
                    Console.Error.WriteLine("Usage: ekko run <file>");
                    return 1;
                }
                await RunFile(args[1]);
                return 0;

            case "repl":
                await StartRepl();
                return 0;

            case "version":
            case "-v":
            case "--version":
                ShowVersion();
                return 0;

            case "help":
            case "-h":
            case "--help":
                ShowHelp();
                return 0;

            case "build":
                if (args.Length < 2)
                {
                    Console.Error.WriteLine("Error: Please specify what to build");
                    Console.Error.WriteLine("Usage: ekko build package [path]");
                    return 1;
                }
                if (args[1].ToLowerInvariant() == "package")
                {
                    var packagePath = args.Length > 2 ? args[2] : Directory.GetCurrentDirectory();
                    return await BuildPackage(packagePath);
                }
                else
                {
                    Console.Error.WriteLine($"Unknown build target: {args[1]}");
                    return 1;
                }

            default:
                Console.Error.WriteLine($"Unknown command: {command}");
                ShowHelp();
                return 1;
        }
    }

    private static void ShowHelp()
    {
        Console.WriteLine("EkkoJS - A modern JavaScript runtime built on .NET and V8");
        Console.WriteLine();
        Console.WriteLine("Usage: ekko <command> [arguments]");
        Console.WriteLine();
        Console.WriteLine("Commands:");
        Console.WriteLine("  run <file>          Execute a JavaScript or TypeScript file");
        Console.WriteLine("  repl                Start an interactive REPL session");
        Console.WriteLine("  build package       Build current directory as an EkkoJS package");
        Console.WriteLine("  build package <dir> Build specified directory as an EkkoJS package");
        Console.WriteLine("  version             Display version information");
        Console.WriteLine("  help                Show this help message");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  -v, --version Show version information");
        Console.WriteLine("  -h, --help    Show this help message");
    }

    private static void ShowVersion()
    {
        Console.WriteLine($"EkkoJS v0.1.0");
        Console.WriteLine($".NET Runtime: {Environment.Version}");
        Console.WriteLine($"OS: {Environment.OSVersion}");
    }

    private static async Task RunFile(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                Console.Error.WriteLine($"Error: File not found: {filePath}");
                Environment.Exit(1);
                return;
            }
            
            var code = await File.ReadAllTextAsync(filePath);
            
            using var runtime = new EkkoRuntime();
            await runtime.InitializeAsync();
            await runtime.ExecuteAsync(code, filePath);
            
            // Wait for any pending timers and tasks to complete
            // Default timeout of 5 minutes, but can be overridden with EKKO_TIMEOUT environment variable
            var timeoutSeconds = 300; // 5 minutes default
            if (int.TryParse(Environment.GetEnvironmentVariable("EKKO_TIMEOUT"), out var customTimeout) && customTimeout > 0)
            {
                timeoutSeconds = customTimeout;
            }
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
            try
            {
                // Debug logging
                if (Environment.GetEnvironmentVariable("EKKO_DEBUG") == "1")
                {
                    Console.WriteLine($"[DEBUG] Initial timer count: {runtime.TimerManager?.PendingTimerCount() ?? 0}");
                    Console.WriteLine($"[DEBUG] Event loop has tasks: {runtime.EventLoop?.HasPendingTasks() ?? false}");
                }
                
                // Wait for both timers and event loop tasks
                var waitCount = 0;
                var noActivityCount = 0;
                const int noActivityThreshold = 10; // 1 second of no activity
                var lastTimerCount = -1;
                
                while (!cts.Token.IsCancellationRequested)
                {
                    var currentTimerCount = runtime.TimerManager?.PendingTimerCount() ?? 0;
                    var hasTasks = runtime.EventLoop?.HasPendingTasks() ?? false;
                    
                    // Check if there's been any change in timer count
                    var hasActivity = (currentTimerCount != lastTimerCount) || hasTasks || currentTimerCount > 0;
                    lastTimerCount = currentTimerCount;
                    
                    if (hasActivity)
                    {
                        // Reset no-activity counter if we have work
                        noActivityCount = 0;
                    }
                    else
                    {
                        // No activity detected
                        noActivityCount++;
                        
                        if (Environment.GetEnvironmentVariable("EKKO_DEBUG") == "1" && noActivityCount > 5)
                        {
                            Console.WriteLine($"[DEBUG] No activity for {noActivityCount * 100}ms");
                        }
                        
                        if (noActivityCount >= noActivityThreshold)
                        {
                            // No activity for 1 second, safe to exit
                            if (Environment.GetEnvironmentVariable("EKKO_DEBUG") == "1")
                            {
                                Console.WriteLine("[DEBUG] No activity threshold reached, exiting...");
                            }
                            break;
                        }
                    }
                    
                    await Task.Delay(100, cts.Token);
                    waitCount++;
                    
                    if (Environment.GetEnvironmentVariable("EKKO_DEBUG") == "1" && waitCount % 10 == 0)
                    {
                        Console.WriteLine($"[DEBUG] Waiting... Timers: {currentTimerCount}, Tasks: {hasTasks}, NoActivity: {noActivityCount}/{noActivityThreshold}");
                    }
                }
                
                if (Environment.GetEnvironmentVariable("EKKO_DEBUG") == "1")
                {
                    Console.WriteLine($"[DEBUG] Wait complete. Total wait cycles: {waitCount}");
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine($"Warning: Script execution timed out after {timeoutSeconds} seconds with pending operations.");
            }
        }
        catch (RuntimeException ex)
        {
            Console.Error.WriteLine($"Runtime error: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.Error.WriteLine($"Details: {ex.InnerException.Message}");
            }
            Environment.Exit(1);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            Environment.Exit(1);
        }
    }

    private static async Task StartRepl()
    {
        Console.WriteLine("EkkoJS REPL v0.1.0");
        Console.WriteLine("Type '.help' for more information, '.exit' to quit");
        
        using var runtime = new EkkoRuntime();
        await runtime.InitializeAsync();
        
        while (true)
        {
            Console.Write("> ");
            var input = Console.ReadLine();
            
            if (string.IsNullOrEmpty(input))
                continue;
                
            if (input == ".exit")
                break;
                
            if (input == ".help")
            {
                Console.WriteLine(".exit  - Exit the REPL");
                Console.WriteLine(".help  - Show this help");
                continue;
            }
            
            try
            {
                var result = await runtime.ExecuteAsync(input, "<repl>");
                if (result != null)
                {
                    Console.WriteLine(result);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
            }
        }
    }

    private static async Task<int> BuildPackage(string packagePath)
    {
        try
        {
            Console.WriteLine($"Building package from: {packagePath}");
            
            var compiler = new PackageCompiler();
            var options = new CompilerOptions
            {
                Debug = false,
                OutputPath = Path.Combine(packagePath, "dist")
            };

            var result = await compiler.CompilePackageAsync(packagePath, options);
            
            if (!result.Success)
            {
                Console.Error.WriteLine("Compilation failed:");
                foreach (var error in result.Errors)
                {
                    Console.Error.WriteLine($"  {error}");
                }
                return 1;
            }

            // Ensure output directory exists
            if (!string.IsNullOrEmpty(options.OutputPath))
            {
                Directory.CreateDirectory(options.OutputPath);
            }

            // Write the assembly
            var manifestPath = Path.Combine(packagePath, ".ekko", "ekko.json");
            var manifestJson = await File.ReadAllTextAsync(manifestPath);
            dynamic manifest = Newtonsoft.Json.JsonConvert.DeserializeObject(manifestJson)!;
            var packageName = ((string)manifest.name).Replace("/", ".").Replace("@", "");
            var outputFile = Path.Combine(options.OutputPath ?? packagePath, $"{packageName}.dll");
            
            await File.WriteAllBytesAsync(outputFile, result.Assembly!);
            
            Console.WriteLine($"✓ Package built successfully: {outputFile}");
            Console.WriteLine($"  Size: {result.Assembly!.Length / 1024}KB");
            
            return 0;
        }
        catch (FileNotFoundException ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            Console.Error.WriteLine("Make sure you have a valid .ekko/ekko.json manifest file");
            return 1;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Build failed: {ex.Message}");
            return 1;
        }
    }
}