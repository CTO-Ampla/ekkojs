using EkkoJS.Core.DotNet;
using System.Dynamic;

namespace EkkoJS.Core.Modules.BuiltIn;

public class DotNetModule : IModule
{
    private readonly string _assemblyName;
    private readonly string? _exportName;
    private readonly DotNetAssemblyLoader _loader;

    public DotNetModule(string assemblyName, string? exportName, DotNetAssemblyLoader loader)
    {
        _assemblyName = assemblyName;
        _exportName = exportName;
        _loader = loader;
        
        // Extract protocol and name for IModule interface
        var parts = assemblyName.Split('/');
        Protocol = "dotnet";
        Name = exportName != null ? $"{assemblyName}/{exportName}" : assemblyName;
    }

    public string Name { get; }
    public string Protocol { get; }

    public object GetExports()
    {
        if (_exportName != null)
        {
            // Return specific export
            return _loader.GetExport(_assemblyName, _exportName) 
                ?? throw new InvalidOperationException($"Export not found: {_exportName}");
        }
        else
        {
            // Return all exports from the assembly
            return _loader.GetAllExports(_assemblyName);
        }
    }
}