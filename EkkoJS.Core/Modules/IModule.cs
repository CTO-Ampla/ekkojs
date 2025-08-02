namespace EkkoJS.Core.Modules;

public interface IModule
{
    string Name { get; }
    string Protocol { get; }
    object GetExports();
}