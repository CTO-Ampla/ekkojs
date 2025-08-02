using System.Reflection;
using System.Text.Json;
using System.Dynamic;
using System.Collections.Concurrent;
using Microsoft.ClearScript.V8;

namespace EkkoJS.Core.DotNet;

public class DotNetAssemblyLoader
{
    private readonly ConcurrentDictionary<string, LoadedAssembly> _loadedAssemblies = new();
    private readonly ConcurrentDictionary<string, AssemblyMapping> _mappings = new();
    private V8ScriptEngine? _engine;
    
    public void SetEngine(V8ScriptEngine engine)
    {
        _engine = engine;
    }

    public void LoadMapping(string mappingFilePath)
    {
        var json = File.ReadAllText(mappingFilePath);
        var mapping = JsonSerializer.Deserialize<AssemblyMapping>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? throw new InvalidOperationException($"Failed to parse mapping file: {mappingFilePath}");

        var mappingDir = Path.GetDirectoryName(mappingFilePath) ?? "";
        var assemblyPath = Path.Combine(mappingDir, mapping.Assembly);
        
        if (!File.Exists(assemblyPath))
        {
            throw new FileNotFoundException($"Assembly not found: {assemblyPath}");
        }

        // Load the assembly
        var assembly = Assembly.LoadFrom(assemblyPath);
        var loadedAssembly = new LoadedAssembly
        {
            Assembly = assembly,
            Mapping = mapping,
            MappingPath = mappingFilePath
        };

        var assemblyName = Path.GetFileNameWithoutExtension(mapping.Assembly);
        _loadedAssemblies[assemblyName] = loadedAssembly;
        _mappings[assemblyName] = mapping;
    }

    public object? GetExport(string assemblyName, string exportName)
    {
        if (!_loadedAssemblies.TryGetValue(assemblyName, out var loadedAssembly))
        {
            // Try to load from standard locations
            var mappingPath = FindMappingFile(assemblyName);
            if (mappingPath != null)
            {
                LoadMapping(mappingPath);
                loadedAssembly = _loadedAssemblies[assemblyName];
            }
            else
            {
                throw new InvalidOperationException($"Assembly not loaded: {assemblyName}");
            }
        }

        if (!loadedAssembly.Mapping.Exports.TryGetValue(exportName, out var exportDef))
        {
            throw new InvalidOperationException($"Export not found: {exportName} in {assemblyName}");
        }

        var type = loadedAssembly.Assembly.GetType(exportDef.Type) 
            ?? throw new InvalidOperationException($"Type not found: {exportDef.Type}");

        // Create a wrapper object for the type
        var wrapper = new DotNetTypeWrapper(type, exportDef, this);
        
        // If we have an engine, create a proper JavaScript object
        if (_engine != null)
        {
            return wrapper.CreateJavaScriptObject(_engine);
        }
        
        return wrapper;
    }

    public object GetAllExports(string assemblyName)
    {
        if (!_loadedAssemblies.TryGetValue(assemblyName, out var loadedAssembly))
        {
            var mappingPath = FindMappingFile(assemblyName);
            if (mappingPath != null)
            {
                LoadMapping(mappingPath);
                loadedAssembly = _loadedAssemblies[assemblyName];
            }
            else
            {
                throw new InvalidOperationException($"Assembly not loaded: {assemblyName}");
            }
        }

        dynamic exports = new ExpandoObject();
        var exportDict = (IDictionary<string, object>)exports;

        foreach (var (name, exportDef) in loadedAssembly.Mapping.Exports)
        {
            var type = loadedAssembly.Assembly.GetType(exportDef.Type);
            if (type != null)
            {
                var wrapper = new DotNetTypeWrapper(type, exportDef, this);
                if (_engine != null)
                {
                    exportDict[name] = wrapper.CreateJavaScriptObject(_engine);
                }
                else
                {
                    exportDict[name] = wrapper;
                }
            }
        }

        return exports;
    }

    private string? FindMappingFile(string assemblyName)
    {
        // Search in common locations
        var searchPaths = new[]
        {
            Path.Combine(Directory.GetCurrentDirectory(), $"{assemblyName}.ekko.json"),
            Path.Combine(Directory.GetCurrentDirectory(), "_jsTest", "test-assemblies", $"{assemblyName}.ekko.json"),
            Path.Combine(Directory.GetCurrentDirectory(), "assemblies", $"{assemblyName}.ekko.json"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), 
                ".ekkojs", "assemblies", $"{assemblyName}.ekko.json")
        };

        return searchPaths.FirstOrDefault(File.Exists);
    }

    private class LoadedAssembly
    {
        public required Assembly Assembly { get; set; }
        public required AssemblyMapping Mapping { get; set; }
        public required string MappingPath { get; set; }
    }
}

public class AssemblyMapping
{
    public string Assembly { get; set; } = "";
    public string Version { get; set; } = "1.0.0";
    public Dictionary<string, ExportDefinition> Exports { get; set; } = new();
    public Dictionary<string, string> TypeAliases { get; set; } = new();
}

public class ExportDefinition
{
    public string Type { get; set; } = "";
    public bool CreateInstance { get; set; }
    public ConstructorDefinition? Constructor { get; set; }
    public Dictionary<string, MethodDefinition> Methods { get; set; } = new();
    public Dictionary<string, PropertyDefinition> Properties { get; set; } = new();
    public Dictionary<string, string> Fields { get; set; } = new();
    public Dictionary<string, EventDefinition> Events { get; set; } = new();
}

public class ConstructorDefinition
{
    public string[] Parameters { get; set; } = Array.Empty<string>();
}

public class MethodDefinition
{
    public string Name { get; set; } = "";
    public bool Static { get; set; }
    public bool Async { get; set; }
    public string[]? Parameters { get; set; }
    public string[]? Generic { get; set; }
}

public class PropertyDefinition
{
    public string Name { get; set; } = "";
    public bool ReadOnly { get; set; }
    public bool Static { get; set; }
}

public class EventDefinition
{
    public string Name { get; set; } = "";
    public string? EventArgs { get; set; }
}