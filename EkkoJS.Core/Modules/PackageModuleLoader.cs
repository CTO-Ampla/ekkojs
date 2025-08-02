using System.Reflection;
using EkkoJS.Core.Packages;
using Microsoft.ClearScript;
using Microsoft.ClearScript.V8;

namespace EkkoJS.Core.Modules;

public class PackageModuleLoader : IModuleLoader
{
    private readonly V8ScriptEngine _engine;
    private readonly Dictionary<string, IEkkoPackage> _loadedPackages = new();
    private readonly List<string> _packageSearchPaths = new();

    public PackageModuleLoader(V8ScriptEngine engine)
    {
        _engine = engine;
        
        // Add default search paths
        _packageSearchPaths.Add(Path.Combine(Directory.GetCurrentDirectory(), "node_modules"));
        _packageSearchPaths.Add(Path.Combine(Directory.GetCurrentDirectory(), "packages"));
        _packageSearchPaths.Add(Path.Combine(Directory.GetCurrentDirectory(), "dist"));
        
        // Add demo package path for testing
        var demoPath = Path.Combine(Directory.GetCurrentDirectory(), "demo-package", "dist");
        if (Directory.Exists(demoPath))
        {
            _packageSearchPaths.Add(demoPath);
        }
    }

    public bool CanLoad(string specifier)
    {
        return specifier.StartsWith("package:");
    }

    public object Load(string specifier, string? parentUrl)
    {
        Console.WriteLine($"[PackageLoader] Load called with specifier: {specifier}");
        var packageName = specifier.Substring("package:".Length);
        Console.WriteLine($"[PackageLoader] Package name extracted: {packageName}");
        
        if (_loadedPackages.TryGetValue(packageName, out var cachedPackage))
        {
            Console.WriteLine($"[PackageLoader] Found cached package: {packageName}");
            return GetPackageMainContent(cachedPackage);
        }

        var package = LoadPackageAssembly(packageName);
        if (package == null)
        {
            throw new ModuleNotFoundException($"Package not found: {packageName}");
        }

        _loadedPackages[packageName] = package;
        return GetPackageMainContent(package);
    }

    private IEkkoPackage? LoadPackageAssembly(string packageName)
    {
        // Convert package name to assembly filename
        var assemblyName = packageName.Replace("/", ".").Replace("@", "") + ".dll";
        
        Console.WriteLine($"[PackageLoader] Looking for package: {packageName}");
        Console.WriteLine($"[PackageLoader] Assembly name: {assemblyName}");
        
        // Search for the assembly in all search paths
        foreach (var searchPath in _packageSearchPaths)
        {
            var assemblyPath = Path.Combine(searchPath, assemblyName);
            Console.WriteLine($"[PackageLoader] Checking path: {assemblyPath}");
            if (File.Exists(assemblyPath))
            {
                Console.WriteLine($"[PackageLoader] Found assembly at: {assemblyPath}");
                try
                {
                    var assembly = Assembly.LoadFrom(assemblyPath);
                    Console.WriteLine($"[PackageLoader] Assembly loaded: {assembly.FullName}");
                    
                    // Find the Package class that implements IEkkoPackage
                    var types = assembly.GetTypes();
                    Console.WriteLine($"[PackageLoader] Found {types.Length} types in assembly");
                    
                    var packageType = types
                        .FirstOrDefault(t => typeof(IEkkoPackage).IsAssignableFrom(t) && !t.IsInterface);
                    
                    if (packageType != null)
                    {
                        Console.WriteLine($"[PackageLoader] Found package type: {packageType.FullName}");
                        var packageInstance = (IEkkoPackage)Activator.CreateInstance(packageType)!;
                        Console.WriteLine($"[PackageLoader] Created package instance: {packageInstance.Name} v{packageInstance.Version}");
                        return packageInstance;
                    }
                    else
                    {
                        Console.WriteLine($"[PackageLoader] No type implementing IEkkoPackage found");
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"[PackageLoader] Failed to load package assembly {assemblyPath}: {ex.Message}");
                    Console.Error.WriteLine($"[PackageLoader] Stack trace: {ex.StackTrace}");
                }
            }
        }
        
        return null;
    }

    private object GetPackageMainContent(IEkkoPackage package)
    {
        Console.WriteLine($"[PackageLoader] Getting main content for package: {package.Name}");
        var manifest = package.GetManifest();
        var fileSystem = package.GetFileSystem();
        
        // If there's a main entry point, return its content
        if (!string.IsNullOrEmpty(manifest.Main))
        {
            Console.WriteLine($"[PackageLoader] Loading main file: {manifest.Main}");
            if (fileSystem.FileExists(manifest.Main))
            {
                var mainContent = fileSystem.ReadFile(manifest.Main);
                Console.WriteLine($"[PackageLoader] Main file content length: {mainContent.Length}");
                
                // Just return the JavaScript code - the ES module system will handle it
                return mainContent;
            }
        }
        
        // If no main file, throw an error
        throw new InvalidOperationException($"Package {package.Name} has no main entry point");
    }
}