using EkkoJS.Core.Native;

namespace EkkoJS.Core.Modules.BuiltIn;

public class NativeModule : IModule
{
    private readonly string _libraryName;
    private readonly NativeLibraryLoader _loader;

    public NativeModule(string libraryName, NativeLibraryLoader loader)
    {
        _libraryName = libraryName;
        _loader = loader;
        Protocol = "native";
        Name = libraryName;
    }

    public string Name { get; }
    public string Protocol { get; }

    public object GetExports()
    {
        Console.WriteLine($"[NativeModule] Getting exports for: {_libraryName}");
        try
        {
            // Load native library synchronously
            var exports = _loader.LoadLibrary(_libraryName);
            Console.WriteLine($"[NativeModule] Exports loaded successfully");
            return exports;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[NativeModule] Error loading exports: {ex.Message}");
            throw;
        }
    }
}