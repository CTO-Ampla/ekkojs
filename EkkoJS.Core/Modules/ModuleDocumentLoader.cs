using Microsoft.ClearScript;
using Microsoft.ClearScript.JavaScript;
using Microsoft.ClearScript.V8;
using System.IO;
using System.Text;
using System.Reflection;
using EkkoJS.Core.Packages;
using EkkoJS.Core.Modules.BuiltIn;

namespace EkkoJS.Core.Modules;

public class ModuleDocumentLoader : DocumentLoader
{
    private readonly Dictionary<string, IEkkoPackage> _loadedPackages = new();
    private readonly List<string> _packageSearchPaths = new();
    private readonly Dictionary<string, IModule> _builtInModules = new();
    private readonly V8ScriptEngine _engine;

    public ModuleDocumentLoader(V8ScriptEngine engine)
    {
        _engine = engine;
        // Add default search paths for packages
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

    public override async Task<Document> LoadDocumentAsync(DocumentSettings settings, DocumentInfo? sourceInfo, string specifier, DocumentCategory category, DocumentContextCallback contextCallback)
    {
        Console.WriteLine($"[DocumentLoader] Loading: {specifier}");
        
        // Handle package: protocol
        if (specifier.StartsWith("package:"))
        {
            var packageName = specifier.Substring("package:".Length);
            var jsCode = LoadPackageModule(packageName);
            
            Console.WriteLine($"[DocumentLoader] Loaded package module: {specifier} ({jsCode.Length} chars)");
            
            // Return as ES module with ModuleCategory.Standard
            return new StringDocument(
                new DocumentInfo(new Uri($"ekko://packages/{specifier}")) { Category = ModuleCategory.Standard },
                jsCode
            );
        }
        
        // Handle ekko: protocol - built-in modules
        if (specifier.StartsWith("ekko:"))
        {
            var moduleName = specifier.Substring("ekko:".Length);
            var jsCode = GetBuiltInModule(moduleName);
            
            // Return as ES module with ModuleCategory.Standard
            return new StringDocument(
                new DocumentInfo(new Uri($"ekko://modules/{moduleName}")) { Category = ModuleCategory.Standard },
                jsCode
            );
        }
        
        // Handle dotnet:, native:, ipc: protocols - not implemented yet
        if (specifier.StartsWith("dotnet:") || specifier.StartsWith("native:") || specifier.StartsWith("ipc:"))
        {
            throw new NotImplementedException($"Protocol not yet implemented in document loader: {specifier}");
        }
        
        // Handle file paths - load from disk
        if (File.Exists(specifier))
        {
            var content = await File.ReadAllTextAsync(specifier);
            
            // ALL files are ES modules - ModuleCategory.Standard
            return new StringDocument(
                new DocumentInfo(new Uri($"file:///{specifier.Replace('\\', '/')}")) { Category = ModuleCategory.Standard },
                content
            );
        }
        
        throw new FileNotFoundException($"Module not found: {specifier}");
    }
    
    private string LoadPackageModule(string packageName)
    {
        // Check if already loaded
        if (_loadedPackages.TryGetValue(packageName, out var cachedPackage))
        {
            return GetPackageMainContent(cachedPackage);
        }

        // Load package assembly
        var package = LoadPackageAssembly(packageName);
        if (package == null)
        {
            throw new FileNotFoundException($"Package not found: {packageName}");
        }

        _loadedPackages[packageName] = package;
        return GetPackageMainContent(package);
    }
    
    private IEkkoPackage? LoadPackageAssembly(string packageName)
    {
        // Convert package name to assembly filename
        var assemblyName = packageName.Replace("/", ".").Replace("@", "") + ".dll";
        
        Console.WriteLine($"[DocumentLoader] Looking for package: {packageName}");
        Console.WriteLine($"[DocumentLoader] Assembly name: {assemblyName}");
        
        // Search for the assembly in all search paths
        foreach (var searchPath in _packageSearchPaths)
        {
            var assemblyPath = Path.Combine(searchPath, assemblyName);
            Console.WriteLine($"[DocumentLoader] Checking path: {assemblyPath}");
            if (File.Exists(assemblyPath))
            {
                Console.WriteLine($"[DocumentLoader] Found assembly at: {assemblyPath}");
                try
                {
                    var assembly = Assembly.LoadFrom(assemblyPath);
                    Console.WriteLine($"[DocumentLoader] Assembly loaded: {assembly.FullName}");
                    
                    // Find the Package class that implements IEkkoPackage
                    var packageType = assembly.GetTypes()
                        .FirstOrDefault(t => typeof(IEkkoPackage).IsAssignableFrom(t) && !t.IsInterface);
                    
                    if (packageType != null)
                    {
                        Console.WriteLine($"[DocumentLoader] Found package type: {packageType.FullName}");
                        var packageInstance = (IEkkoPackage)Activator.CreateInstance(packageType)!;
                        Console.WriteLine($"[DocumentLoader] Created package instance: {packageInstance.Name} v{packageInstance.Version}");
                        return packageInstance;
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"[DocumentLoader] Failed to load package assembly {assemblyPath}: {ex.Message}");
                }
            }
        }
        
        return null;
    }
    
    private string GetPackageMainContent(IEkkoPackage package)
    {
        var manifest = package.GetManifest();
        var fileSystem = package.GetFileSystem();
        
        // Get the main entry point
        if (!string.IsNullOrEmpty(manifest.Main))
        {
            if (fileSystem.FileExists(manifest.Main))
            {
                // Return the JavaScript code as-is
                return fileSystem.ReadFile(manifest.Main);
            }
        }
        
        throw new InvalidOperationException($"Package {package.Name} has no main entry point");
    }
    
    public void RegisterBuiltInModule(string name, IModule module)
    {
        _builtInModules[name] = module;
    }
    
    private string GetBuiltInModule(string moduleName)
    {
        if (_builtInModules.TryGetValue(moduleName, out var module))
        {
            var exports = module.GetExports();
            
            // Create ES module code that exports the module
            var sb = new StringBuilder();
            
            // Special handling for path module
            if (moduleName == "path")
            {
                sb.AppendLine(@"
const pathExports = globalThis.__tempPathExports;
export const sep = pathExports.sep;
export const delimiter = pathExports.delimiter;
export const join = (...parts) => pathExports._internals.joinPaths(parts);
export const resolve = (...parts) => pathExports._internals.resolvePaths(parts);
export const dirname = (path) => pathExports._internals.dirname(path);
export const basename = (path, ext) => pathExports._internals.basename(path, ext || null);
export const extname = (path) => pathExports._internals.extname(path);
export const parse = (path) => pathExports._internals.parse(path);
export const format = (pathObject) => pathExports._internals.format(pathObject);
export const isAbsolute = (path) => pathExports._internals.isAbsolute(path);
export const relative = (from, to) => pathExports._internals.relative(from, to);

export default {
    sep, delimiter, join, resolve, dirname, basename, extname, parse, format, isAbsolute, relative
};
delete globalThis.__tempPathExports;
");
                // Store the exports temporarily
                _engine.Script.__tempPathExports = exports;
            }
            else
            {
                // For other modules, create proper ES module exports
                _engine.Script.__tempExports = exports;
                
                // Get all the keys from the exports object
                var exportKeys = _engine.Evaluate(@"
                    const exp = globalThis.__tempExports;
                    Object.keys(exp);
                ");
                
                sb.AppendLine("const exports = globalThis.__tempExports;");
                
                // Generate individual named exports
                if (exportKeys is object[] keys)
                {
                    foreach (var key in keys)
                    {
                        if (key is string keyStr)
                        {
                            sb.AppendLine($"export const {keyStr} = exports.{keyStr};");
                        }
                    }
                }
                
                // Add default export
                sb.AppendLine(@"
export default exports;
delete globalThis.__tempExports;
");
            }
            
            return sb.ToString();
        }
        
        throw new InvalidOperationException($"Built-in module not found: ekko:{moduleName}");
    }
}

// Simple string document implementation
public class StringDocument : Document
{
    private readonly MemoryStream _stream;
    private readonly DocumentInfo _info;
    
    public StringDocument(DocumentInfo info, string contents)
    {
        _info = info;
        _stream = new MemoryStream(Encoding.UTF8.GetBytes(contents));
    }
    
    public override DocumentInfo Info => _info;
    
    public override Stream Contents => _stream;
}