using Microsoft.ClearScript;
using Microsoft.ClearScript.JavaScript;
using Microsoft.ClearScript.V8;
using System.IO;
using System.Text;
using System.Reflection;
using EkkoJS.Core.Packages;
using EkkoJS.Core.Modules.BuiltIn;
using EkkoJS.Core.DotNet;
using EkkoJS.Core.Native;
using EkkoJS.Core.IPC;

namespace EkkoJS.Core.Modules;

public class ModuleDocumentLoader : DocumentLoader
{
    private readonly Dictionary<string, IEkkoPackage> _loadedPackages = new();
    private readonly List<string> _packageSearchPaths = new();
    private readonly Dictionary<string, IModule> _builtInModules = new();
    private readonly V8ScriptEngine _engine;
    private readonly DotNetAssemblyLoader _dotNetLoader;
    private readonly NativeLibraryLoader _nativeLoader;

    public ModuleDocumentLoader(V8ScriptEngine engine, DotNetAssemblyLoader dotNetLoader, NativeLibraryLoader nativeLoader)
    {
        _engine = engine;
        _dotNetLoader = dotNetLoader;
        _nativeLoader = nativeLoader;
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
        
        // Handle dotnet: protocol
        if (specifier.StartsWith("dotnet:"))
        {
            var moduleSpec = specifier.Substring("dotnet:".Length);
            var jsCode = GetDotNetModule(moduleSpec);
            
            Console.WriteLine($"[DocumentLoader] Loaded .NET module: {specifier} ({jsCode.Length} chars)");
            
            // Return as ES module with ModuleCategory.Standard
            return new StringDocument(
                new DocumentInfo(new Uri($"ekko://dotnet/{moduleSpec}")) { Category = ModuleCategory.Standard },
                jsCode
            );
        }
        
        // Handle native: protocol
        if (specifier.StartsWith("native:"))
        {
            var libraryName = specifier.Substring("native:".Length);
            var jsCode = GetNativeModule(libraryName);
            
            Console.WriteLine($"[DocumentLoader] Loaded native module: {specifier} ({jsCode.Length} chars)");
            
            // Return as ES module with ModuleCategory.Standard
            return new StringDocument(
                new DocumentInfo(new Uri($"ekko://native/{libraryName}")) { Category = ModuleCategory.Standard },
                jsCode
            );
        }
        
        // Handle ipc: protocol
        if (specifier.StartsWith("ipc:"))
        {
            var serviceName = specifier.Substring("ipc:".Length);
            var jsCode = GetIpcModule(serviceName);
            
            Console.WriteLine($"[DocumentLoader] Loaded IPC module: {specifier} ({jsCode.Length} chars)");
            
            // Return as ES module with ModuleCategory.Standard
            return new StringDocument(
                new DocumentInfo(new Uri($"ekko://ipc/{serviceName}")) { Category = ModuleCategory.Standard },
                jsCode
            );
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
    
    private string GetDotNetModule(string moduleSpec)
    {
        // Parse module spec: "dotnet:AssemblyName" or "dotnet:AssemblyName/ExportName"
        var parts = moduleSpec.Split('/');
        var assemblyName = parts[0];
        var exportName = parts.Length > 1 ? parts[1] : null;
        
        Console.WriteLine($"[DocumentLoader] Loading .NET module: {moduleSpec}");
        
        var sb = new StringBuilder();
        
        try
        {
            if (exportName != null)
            {
                // Get specific export
                var export = _dotNetLoader.GetExport(assemblyName, exportName);
                
                // Store in global for JavaScript access
                _engine.Script[$"__dotnet_{assemblyName}_{exportName}"] = export;
                
                sb.AppendLine($@"
// .NET type: {assemblyName}/{exportName}
const hostType = globalThis.__dotnet_{assemblyName}_{exportName};
if (!hostType) {{
    throw new Error('Export not found: {assemblyName}/{exportName}');
}}

// Export the type as default
export default hostType;
");
            }
            else
            {
                // Get all exports from assembly
                var exports = _dotNetLoader.GetAllExports(assemblyName);
                
                // Store in global for JavaScript access
                _engine.Script[$"__dotnet_{assemblyName}"] = exports;
                
                sb.AppendLine($@"
// .NET assembly: {assemblyName}
const assembly = globalThis.__dotnet_{assemblyName};
if (!assembly) {{
    throw new Error('Assembly not found: {assemblyName}');
}}

// Export all types as named exports
const exportNames = Object.keys(assembly);
for (const name of exportNames) {{
    if (name !== 'default') {{
        exports[name] = assembly[name];
    }}
}}

// Also export the entire assembly namespace as default
export default assembly;
");
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to load .NET module: {moduleSpec}", ex);
        }
        
        return sb.ToString();
    }
    
    private string GetNativeModule(string libraryName)
    {
        Console.WriteLine($"[DocumentLoader] Loading native module: {libraryName}");
        
        var sb = new StringBuilder();
        
        try
        {
            // Load the native library
            var exports = _nativeLoader.LoadLibrary(libraryName);
            
            // Store in global for JavaScript access
            _engine.Script[$"__native_{libraryName}"] = exports;
            
            sb.AppendLine($@"
// Native library: {libraryName}
const lib = globalThis.__native_{libraryName};
if (!lib) {{
    throw new Error('Native library not found: {libraryName}');
}}

// Re-export all functions from the library
");
            
            // We need to know the function names at compile time for ES modules
            // For now, we'll export the whole library object and let users destructure
            sb.AppendLine("// Export the library object - destructure as needed");
            sb.AppendLine("export default lib;");
            sb.AppendLine("");
            sb.AppendLine("// For convenience, also expose common patterns");
            sb.AppendLine("export const call = (funcName, ...args) => lib[funcName](...args);");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to load native module: {libraryName}", ex);
        }
        
        return sb.ToString();
    }
    
    private string GetIpcModule(string serviceName)
    {
        Console.WriteLine($"[DocumentLoader] Loading IPC module: {serviceName}");
        
        var sb = new StringBuilder();
        
        try
        {
            // Find IPC service mapping file
            var mappingPath = FindIpcMappingFile(serviceName);
            IpcServiceMapping? serviceMapping = null;
            TransportConfig transportConfig;

            if (mappingPath != null && File.Exists(mappingPath))
            {
                var json = File.ReadAllText(mappingPath);
                serviceMapping = System.Text.Json.JsonSerializer.Deserialize<IpcServiceMapping>(json, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                transportConfig = serviceMapping?.Service.Transport ?? new TransportConfig();
            }
            else
            {
                // Default configuration if no mapping file found
                transportConfig = new TransportConfig
                {
                    Type = "namedpipe",
                    Address = serviceName
                };
            }

            // Create IPC module instance
            var ipcModule = new IpcModule(serviceName, transportConfig, serviceMapping);
            var exports = ipcModule.GetExports();
            
            // Store in global for JavaScript access
            _engine.Script[$"__ipc_{serviceName}"] = exports;
            
            sb.AppendLine($@"
// IPC service: {serviceName}
const client = globalThis.__ipc_{serviceName};
if (!client) {{
    throw new Error('IPC service not found: {serviceName}');
}}

// Export IPC client methods
export const {{ connect, disconnect, call, subscribe, publish, isConnected }} = client;

// Also export the entire client as default
export default client;
");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to load IPC module: {serviceName}", ex);
        }
        
        return sb.ToString();
    }
    
    private string? FindIpcMappingFile(string serviceName)
    {
        var searchPaths = new[]
        {
            Path.Combine(Directory.GetCurrentDirectory(), $"{serviceName}.ekko.ipc.json"),
            Path.Combine(Directory.GetCurrentDirectory(), "ipc", $"{serviceName}.ekko.ipc.json"),
            Path.Combine(Directory.GetCurrentDirectory(), "_jsTest", "ipc", $"{serviceName}.ekko.ipc.json"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), 
                ".ekkojs", "ipc", $"{serviceName}.ekko.ipc.json")
        };

        return searchPaths.FirstOrDefault(File.Exists);
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