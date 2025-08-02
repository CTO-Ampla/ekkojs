using Microsoft.ClearScript;
using Microsoft.ClearScript.V8;
using System.Collections.Concurrent;
using System.Text.Json;
using EkkoJS.Core.DotNet;
using EkkoJS.Core.Native;
using EkkoJS.Core.Modules.BuiltIn;
using EkkoJS.Core.IPC;

namespace EkkoJS.Core.Modules;

public class ESModuleLoader
{
    private readonly V8ScriptEngine _engine;
    private readonly ConcurrentDictionary<string, IModule> _modules;
    private readonly ConcurrentDictionary<string, object> _moduleCache;
    private readonly ConcurrentDictionary<string, TaskCompletionSource<object>> _loadingModules;
    private DotNetAssemblyLoader? _dotNetLoader;
    private NativeLibraryLoader? _nativeLoader;

    public ESModuleLoader(V8ScriptEngine engine)
    {
        _engine = engine;
        _modules = new ConcurrentDictionary<string, IModule>();
        _moduleCache = new ConcurrentDictionary<string, object>();
        _loadingModules = new ConcurrentDictionary<string, TaskCompletionSource<object>>();
    }

    public void RegisterModule(IModule module)
    {
        var key = $"{module.Protocol}:{module.Name}";
        _modules[key] = module;
    }

    public void SetDotNetLoader(DotNetAssemblyLoader loader)
    {
        _dotNetLoader = loader;
    }
    
    public void SetNativeLoader(NativeLibraryLoader loader)
    {
        _nativeLoader = loader;
    }

    public async Task<object> LoadModuleAsync(string moduleSpecifier)
    {
        // Check cache first
        if (_moduleCache.TryGetValue(moduleSpecifier, out var cachedModule))
        {
            return cachedModule;
        }

        // Check if already loading
        if (_loadingModules.TryGetValue(moduleSpecifier, out var existingTcs))
        {
            return await existingTcs.Task;
        }

        // Start loading
        var tcs = new TaskCompletionSource<object>();
        if (!_loadingModules.TryAdd(moduleSpecifier, tcs))
        {
            // Another thread started loading
            return await _loadingModules[moduleSpecifier].Task;
        }

        try
        {
            var (protocol, moduleName) = ParseModuleSpecifier(moduleSpecifier);
            
            if (protocol == null)
            {
                throw new InvalidOperationException($"Invalid module specifier: {moduleSpecifier}");
            }

            IModule? module = null;
            
            // Handle dotnet: protocol specially
            if (protocol == "dotnet" && _dotNetLoader != null)
            {
                // Parse assembly/export format: dotnet:AssemblyName or dotnet:AssemblyName/ExportName
                var parts = moduleName.Split('/', 2);
                var assemblyName = parts[0];
                var exportName = parts.Length > 1 ? parts[1] : null;
                
                module = new DotNetModule(assemblyName, exportName, _dotNetLoader);
            }
            // Handle native: protocol specially
            else if (protocol == "native" && _nativeLoader != null)
            {
                module = new NativeModule(moduleName, _nativeLoader);
            }
            // Handle ipc: protocol specially
            else if (protocol == "ipc")
            {
                module = CreateIpcModule(moduleName);
            }
            else
            {
                var key = $"{protocol}:{moduleName}";
                if (!_modules.TryGetValue(key, out module))
                {
                    throw new InvalidOperationException($"Module not found: {moduleSpecifier}");
                }
            }

            var exports = module.GetExports();
            
            // Special handling for path module to create JavaScript wrappers
            if (module.Name == "path" && module.Protocol == "ekko")
            {
                exports = WrapPathModule(exports);
            }
            
            // Create ES module namespace object
            var moduleNamespace = CreateModuleNamespace(exports);
            
            _moduleCache[moduleSpecifier] = moduleNamespace;
            tcs.SetResult(moduleNamespace);
            
            return moduleNamespace;
        }
        catch (Exception ex)
        {
            tcs.SetException(ex);
            throw;
        }
        finally
        {
            _loadingModules.TryRemove(moduleSpecifier, out _);
        }
    }

    private object CreateModuleNamespace(dynamic exports)
    {
        // Store exports temporarily for JavaScript access
        _engine.Script.__tempExports = exports;
        
        // Debug logging
        if (Environment.GetEnvironmentVariable("EKKO_DEBUG") == "1")
        {
            _engine.Execute(@"
                console.log('CreateModuleNamespace - exports type:', typeof globalThis.__tempExports);
                console.log('CreateModuleNamespace - exports:', globalThis.__tempExports);
                if (typeof globalThis.__tempExports === 'object' && globalThis.__tempExports !== null) {
                    console.log('CreateModuleNamespace - keys:', Object.keys(globalThis.__tempExports));
                }
            ");
        }
        
        // Create a proper ES module namespace object
        var namespaceCode = @"
            (function(exports) {
                // Simple approach - just add default export
                return {
                    default: exports,
                    ...exports
                };
            })
        ";

        var finalResult = _engine.Evaluate(documentName: "[module-namespace]", code: $"{namespaceCode}(globalThis.__tempExports)");
        _engine.Script.__tempExports = null;
        
        return finalResult;
    }

    private object WrapPathModule(dynamic exports)
    {
        // Store the exports object in engine for access
        _engine.Script.__pathExports = exports;
        
        // Create JavaScript wrappers for variadic functions
        var wrapperCode = @"
            (function() {
                const exports = globalThis.__pathExports;
                const wrapped = {
                    sep: exports.sep,
                    delimiter: exports.delimiter,
                    join: (...parts) => exports._internals.joinPaths(parts),
                    resolve: (...parts) => exports._internals.resolvePaths(parts),
                    dirname: (path) => exports._internals.dirname(path),
                    basename: (path, ext) => exports._internals.basename(path, ext || null),
                    extname: (path) => exports._internals.extname(path),
                    parse: (path) => exports._internals.parse(path),
                    format: (pathObject) => exports._internals.format(pathObject),
                    isAbsolute: (path) => exports._internals.isAbsolute(path),
                    relative: (from, to) => exports._internals.relative(from, to)
                };
                delete globalThis.__pathExports;
                return wrapped;
            })()
        ";
        
        return _engine.Evaluate(documentName: "[path-wrapper]", code: wrapperCode);
    }

    private (string? protocol, string moduleName) ParseModuleSpecifier(string specifier)
    {
        var colonIndex = specifier.IndexOf(':');
        if (colonIndex < 0)
        {
            return (null, specifier);
        }

        var protocol = specifier.Substring(0, colonIndex);
        var moduleName = specifier.Substring(colonIndex + 1);
        return (protocol, moduleName);
    }

    public void InstallImportHandler()
    {
        // Create the import meta object
        var importMetaCode = @"
            // Create import.meta object
            globalThis.__createImportMeta = function(url) {
                return {
                    url: url,
                    resolve: (specifier) => new URL(specifier, url).href
                };
            };
        ";
        
        _engine.Execute(documentName: "[import-meta]", code: importMetaCode);
        
        // Install dynamic import() function
        _engine.AddHostObject("__ekkoLoadModule", new Func<string, object>(moduleSpecifier =>
        {
            var task = LoadModuleAsync(moduleSpecifier);
            task.Wait();
            return task.Result;
        }));
        
        var dynamicImportCode = @"
            // Override the global import function
            globalThis.import = function(specifier) {
                return new Promise((resolve, reject) => {
                    try {
                        const module = __ekkoLoadModule(specifier);
                        resolve(module);
                    } catch (error) {
                        reject(new Error(`Failed to import module '${specifier}': ${error.message || error}`));
                    }
                });
            };
        ";
        
        _engine.Execute(documentName: "[dynamic-import]", code: dynamicImportCode);
    }

    private IModule CreateIpcModule(string serviceName)
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
                Address = serviceName,
                Timeout = 30000
            };
        }

        return new IpcModule(serviceName, transportConfig, serviceMapping);
    }

    private string? FindIpcMappingFile(string serviceName)
    {
        var searchPaths = new[]
        {
            Path.Combine(Directory.GetCurrentDirectory(), $"{serviceName}.ekko.ipc.json"),
            Path.Combine(Directory.GetCurrentDirectory(), "_jsTest", "ipc-configs", $"{serviceName}.ekko.ipc.json"),
            Path.Combine(Directory.GetCurrentDirectory(), "services", $"{serviceName}.ekko.ipc.json"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), 
                ".ekkojs", "services", $"{serviceName}.ekko.ipc.json")
        };

        return searchPaths.FirstOrDefault(File.Exists);
    }
}