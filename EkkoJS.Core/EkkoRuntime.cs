using Microsoft.ClearScript.V8;
using EkkoJS.Core.Modules;
using EkkoJS.Core.Modules.BuiltIn;
using EkkoJS.Core.TypeScript;
using EkkoJS.Core.DotNet;
using EkkoJS.Core.Native;
using EkkoJS.Core.IPC;

namespace EkkoJS.Core;

public class EkkoRuntime : IDisposable
{
    private V8ScriptEngine? _engine;
    private EventLoop? _eventLoop;
    private TimerManager? _timerManager;
    private ESModuleLoader? _moduleLoader;
    private TypeScriptCompiler? _typeScriptCompiler;
    private DotNetAssemblyLoader? _dotNetLoader;
    private NativeLibraryLoader? _nativeLoader;
    private bool _disposed;

    public async Task InitializeAsync()
    {
        _engine = new V8ScriptEngine(V8ScriptEngineFlags.EnableTaskPromiseConversion);
        
        // Initialize event loop
        _eventLoop = new EventLoop();
        
        // Initialize timer manager
        _timerManager = new TimerManager(_eventLoop);
        
        // Initialize module loader
        _moduleLoader = new ESModuleLoader(_engine);
        RegisterBuiltInModules();
        _moduleLoader.InstallImportHandler();
        
        // Initialize TypeScript compiler
        _typeScriptCompiler = new TypeScriptCompiler();
        await _typeScriptCompiler.InitializeAsync();
        
        // Initialize .NET assembly loader
        _dotNetLoader = new DotNetAssemblyLoader();
        _dotNetLoader.SetEngine(_engine);
        _moduleLoader.SetDotNetLoader(_dotNetLoader);
        
        // Initialize native library loader
        _nativeLoader = new NativeLibraryLoader(_engine);
        _moduleLoader.SetNativeLoader(_nativeLoader);
        
        // Add host bridge first
        _engine.AddHostObject("host", new HostBridge());
        _engine.AddHostObject("__timerManager", _timerManager);
        
        // Set up basic console
        _engine.Execute(@"
            const console = {
                log: (...args) => host.ConsoleLog(args.map(arg => {
                    if (typeof arg === 'object') {
                        try {
                            return JSON.stringify(arg, null, 2);
                        } catch {
                            return String(arg);
                        }
                    }
                    return String(arg);
                }).join(' ')),
                error: (...args) => host.ConsoleError(args.map(String).join(' ')),
                warn: (...args) => host.ConsoleWarn(args.map(String).join(' ')),
                info: (...args) => host.ConsoleInfo(args.map(String).join(' '))
            };
            globalThis.console = console;
        ");
        
        // Set up timers
        InitializeTimers();
        
        await Task.CompletedTask;
    }

    private void RegisterBuiltInModules()
    {
        _moduleLoader!.RegisterModule(new FileSystemModule());
        _moduleLoader!.RegisterModule(new PathModule());
    }

    public async Task<object?> ExecuteAsync(string code, string fileName = "<anonymous>")
    {
        if (_engine == null)
            throw new InvalidOperationException("Runtime not initialized");
            
        try
        {
            // Check if this is a TypeScript file
            bool isTypeScript = fileName.EndsWith(".ts") || fileName.EndsWith(".tsx");
            
            if (isTypeScript)
            {
                // Compile TypeScript to JavaScript
                var compilationResult = await _typeScriptCompiler!.CompileAsync(code, fileName);
                
                if (!compilationResult.Success)
                {
                    var errors = string.Join("\n", compilationResult.Diagnostics);
                    throw new RuntimeException($"TypeScript compilation failed:\n{errors}");
                }
                
                code = compilationResult.JavaScriptCode;
                
                // Debug: log compiled code
                if (Environment.GetEnvironmentVariable("EKKO_DEBUG") == "1")
                {
                    Console.WriteLine("=== TypeScript Compiled ===");
                    Console.WriteLine(code);
                    Console.WriteLine("=== End Compiled ===");
                }
            }
            
            // Check if the code contains ES module syntax
            bool isESModule = code.Contains("import ") || code.Contains("export ");
            
            if (isESModule)
            {
                // Transform ES module code
                var transformedCode = ESModuleCompiler.TransformESModule(code, fileName);
                code = transformedCode;
                
                // Debug: log transformed code
                if (Environment.GetEnvironmentVariable("EKKO_DEBUG") == "1")
                {
                    Console.WriteLine("=== Transformed ES Module ===");
                    Console.WriteLine(code);
                    Console.WriteLine("=== End Transformed ===");
                }
            }
            
            // Execute in event loop context
            if (isESModule)
            {
                // For ES modules, execute and wait for the module promise
                await _eventLoop!.QueueTaskAsync(() =>
                {
                    // Execute the transformed module code
                    _engine.Execute(documentName: fileName, code: code);
                    
                    // Wait for the module promise to complete
                    _engine.Execute(@"
                        if (globalThis.__currentModulePromise) {
                            globalThis.__currentModulePromise.then(() => {
                                // Module execution completed
                                delete globalThis.__currentModulePromise;
                            }).catch(error => {
                                console.error('Module execution error:', error);
                                delete globalThis.__currentModulePromise;
                            });
                        }
                    ");
                });
                
                // Give time for async operations to complete
                await Task.Delay(100);
            }
            else
            {
                await _eventLoop!.QueueTaskAsync(() =>
                {
                    _engine.Execute(documentName: fileName, code: code);
                });
            }
            return null;
        }
        catch (Microsoft.ClearScript.ScriptEngineException ex)
        {
            var errorMessage = ex.ErrorDetails ?? ex.Message;
            throw new RuntimeException($"JavaScript Error in {fileName}: {errorMessage}", ex);
        }
        catch (Exception ex)
        {
            throw new RuntimeException($"Error executing {fileName}", ex);
        }
    }

    private void InitializeTimers()
    {
        _engine!.Execute(@"
            // setTimeout implementation
            globalThis.setTimeout = function(callback, delay, ...args) {
                if (typeof callback === 'string') {
                    callback = new Function(callback);
                }
                if (typeof callback !== 'function') {
                    throw new TypeError('Callback must be a function');
                }
                delay = Math.max(0, Number(delay) || 0);
                return __timerManager.SetTimeout(callback, delay, ...args);
            };

            // setInterval implementation
            globalThis.setInterval = function(callback, delay, ...args) {
                if (typeof callback === 'string') {
                    callback = new Function(callback);
                }
                if (typeof callback !== 'function') {
                    throw new TypeError('Callback must be a function');
                }
                delay = Math.max(0, Number(delay) || 0);
                return __timerManager.SetInterval(callback, delay, ...args);
            };

            // clearTimeout implementation
            globalThis.clearTimeout = function(timerId) {
                if (timerId == null) return;
                __timerManager.ClearTimer(Number(timerId));
            };

            // clearInterval implementation
            globalThis.clearInterval = function(timerId) {
                if (timerId == null) return;
                __timerManager.ClearTimer(Number(timerId));
            };
        ");
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _eventLoop?.Stop();
            _timerManager?.Dispose();
            _eventLoop?.Dispose();
            _typeScriptCompiler?.Dispose();
            _engine?.Dispose();
            _disposed = true;
        }
    }
}

public class HostBridge
{
    public void ConsoleLog(string message) => Console.WriteLine(message);
    public void ConsoleError(string message) => Console.Error.WriteLine($"ERROR: {message}");
    public void ConsoleWarn(string message) => Console.WriteLine($"WARN: {message}");
    public void ConsoleInfo(string message) => Console.WriteLine($"INFO: {message}");
}

public class RuntimeException : Exception
{
    public RuntimeException(string message) : base(message) { }
    public RuntimeException(string message, Exception innerException) : base(message, innerException) { }
}