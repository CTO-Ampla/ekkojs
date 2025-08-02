using System.Text.Json.Serialization;

namespace EkkoJS.Core.Native;

public class NativeLibraryMapping
{
    [JsonPropertyName("library")]
    public LibraryPaths Library { get; set; } = new();
    
    [JsonPropertyName("version")]
    public string Version { get; set; } = "";
    
    [JsonPropertyName("exports")]
    public Dictionary<string, FunctionDefinition> Exports { get; set; } = new();
    
    [JsonPropertyName("structs")]
    public Dictionary<string, StructDefinition> Structs { get; set; } = new();
    
    [JsonPropertyName("callbacks")]
    public Dictionary<string, CallbackDefinition> Callbacks { get; set; } = new();
}

public class LibraryPaths
{
    [JsonPropertyName("windows")]
    public Dictionary<string, string> Windows { get; set; } = new();
    
    [JsonPropertyName("linux")]
    public Dictionary<string, string> Linux { get; set; } = new();
    
    [JsonPropertyName("darwin")]
    public Dictionary<string, string> Darwin { get; set; } = new();
}

public class FunctionDefinition
{
    [JsonPropertyName("entryPoint")]
    public string EntryPoint { get; set; } = "";
    
    [JsonPropertyName("returns")]
    public string Returns { get; set; } = "void";
    
    [JsonPropertyName("parameters")]
    public List<ParameterDefinition> Parameters { get; set; } = new();
    
    [JsonPropertyName("callingConvention")]
    public string? CallingConvention { get; set; }
}

public class ParameterDefinition
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";
    
    [JsonPropertyName("type")]
    public string Type { get; set; } = "";
    
    [JsonPropertyName("ref")]
    public bool Ref { get; set; }
    
    [JsonPropertyName("out")]
    public bool Out { get; set; }
}

public class StructDefinition
{
    [JsonPropertyName("fields")]
    public List<FieldDefinition> Fields { get; set; } = new();
    
    [JsonPropertyName("layout")]
    public string? Layout { get; set; } // Sequential, Explicit, Auto
}

public class FieldDefinition
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";
    
    [JsonPropertyName("type")]
    public string Type { get; set; } = "";
    
    [JsonPropertyName("offset")]
    public int? Offset { get; set; }
}

public class CallbackDefinition
{
    [JsonPropertyName("returns")]
    public string Returns { get; set; } = "void";
    
    [JsonPropertyName("parameters")]
    public List<ParameterDefinition> Parameters { get; set; } = new();
}