namespace EkkoJS.Core.Modules;

public interface IModuleLoader
{
    bool CanLoad(string specifier);
    object Load(string specifier, string? parentUrl);
}