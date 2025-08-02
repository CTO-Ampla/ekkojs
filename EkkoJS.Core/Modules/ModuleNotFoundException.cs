namespace EkkoJS.Core.Modules;

public class ModuleNotFoundException : Exception
{
    public ModuleNotFoundException(string message) : base(message) { }
    public ModuleNotFoundException(string message, Exception innerException) : base(message, innerException) { }
}