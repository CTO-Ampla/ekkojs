using System.Text.Json.Serialization;

namespace EkkoJS.Core.IPC;

/// <summary>
/// Defines the mapping and configuration for an IPC service
/// </summary>
public class IpcServiceMapping
{
    /// <summary>
    /// Service information
    /// </summary>
    [JsonPropertyName("service")]
    public ServiceInfo Service { get; set; } = new();

    /// <summary>
    /// Available methods/endpoints
    /// </summary>
    [JsonPropertyName("methods")]
    public Dictionary<string, MethodDefinition> Methods { get; set; } = new();

    /// <summary>
    /// Available events that can be subscribed to
    /// </summary>
    [JsonPropertyName("events")]
    public Dictionary<string, EventDefinition> Events { get; set; } = new();

    /// <summary>
    /// Available channels for pub/sub
    /// </summary>
    [JsonPropertyName("channels")]
    public Dictionary<string, ChannelDefinition> Channels { get; set; } = new();
}

/// <summary>
/// Basic service information
/// </summary>
public class ServiceInfo
{
    /// <summary>
    /// Service name/identifier
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Service version
    /// </summary>
    [JsonPropertyName("version")]
    public string Version { get; set; } = "1.0.0";

    /// <summary>
    /// Service description
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Transport configuration
    /// </summary>
    [JsonPropertyName("transport")]
    public TransportConfig Transport { get; set; } = new();

    /// <summary>
    /// Service tags for discovery
    /// </summary>
    [JsonPropertyName("tags")]
    public List<string> Tags { get; set; } = new();
}

/// <summary>
/// Transport configuration for the service
/// </summary>
public class TransportConfig
{
    /// <summary>
    /// Transport type (namedpipe, socket, tcp)
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = "namedpipe";

    /// <summary>
    /// Transport address (pipe name, socket path, etc.)
    /// </summary>
    [JsonPropertyName("address")]
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// Port number (for TCP)
    /// </summary>
    [JsonPropertyName("port")]
    public int? Port { get; set; }

    /// <summary>
    /// Connection timeout in milliseconds
    /// </summary>
    [JsonPropertyName("timeout")]
    public int Timeout { get; set; } = 30000;

    /// <summary>
    /// Maximum number of concurrent connections
    /// </summary>
    [JsonPropertyName("maxConnections")]
    public int MaxConnections { get; set; } = 100;
}

/// <summary>
/// Definition of a service method
/// </summary>
public class MethodDefinition
{
    /// <summary>
    /// Method description
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Method parameters
    /// </summary>
    [JsonPropertyName("parameters")]
    public List<ParameterDefinition> Parameters { get; set; } = new();

    /// <summary>
    /// Return type information
    /// </summary>
    [JsonPropertyName("returns")]
    public TypeDefinition Returns { get; set; } = new();

    /// <summary>
    /// Whether method is asynchronous
    /// </summary>
    [JsonPropertyName("async")]
    public bool IsAsync { get; set; } = true;

    /// <summary>
    /// Method timeout in milliseconds
    /// </summary>
    [JsonPropertyName("timeout")]
    public int Timeout { get; set; } = 30000;

    /// <summary>
    /// Required permissions to call this method
    /// </summary>
    [JsonPropertyName("permissions")]
    public List<string> Permissions { get; set; } = new();
}

/// <summary>
/// Parameter definition for method calls
/// </summary>
public class ParameterDefinition
{
    /// <summary>
    /// Parameter name
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Parameter type
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = "any";

    /// <summary>
    /// Whether parameter is required
    /// </summary>
    [JsonPropertyName("required")]
    public bool Required { get; set; } = false;

    /// <summary>
    /// Default value (if not required)
    /// </summary>
    [JsonPropertyName("default")]
    public object? Default { get; set; }

    /// <summary>
    /// Parameter description
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Validation rules
    /// </summary>
    [JsonPropertyName("validation")]
    public Dictionary<string, object>? Validation { get; set; }
}

/// <summary>
/// Type definition for parameters and return values
/// </summary>
public class TypeDefinition
{
    /// <summary>
    /// Type name (string, number, object, array, etc.)
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = "any";

    /// <summary>
    /// Schema definition for complex types
    /// </summary>
    [JsonPropertyName("schema")]
    public Dictionary<string, object>? Schema { get; set; }

    /// <summary>
    /// Array element type (if type is array)
    /// </summary>
    [JsonPropertyName("elementType")]
    public string? ElementType { get; set; }

    /// <summary>
    /// Whether the value can be null
    /// </summary>
    [JsonPropertyName("nullable")]
    public bool Nullable { get; set; } = false;
}

/// <summary>
/// Definition of an event that can be subscribed to
/// </summary>
public class EventDefinition
{
    /// <summary>
    /// Event description
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Event data schema
    /// </summary>
    [JsonPropertyName("schema")]
    public Dictionary<string, object>? Schema { get; set; }

    /// <summary>
    /// Event category/topic
    /// </summary>
    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Event priority level
    /// </summary>
    [JsonPropertyName("priority")]
    public EventPriority Priority { get; set; } = EventPriority.Normal;
}

/// <summary>
/// Definition of a pub/sub channel
/// </summary>
public class ChannelDefinition
{
    /// <summary>
    /// Channel description
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Channel type (broadcast, topic, queue)
    /// </summary>
    [JsonPropertyName("type")]
    public ChannelType Type { get; set; } = ChannelType.Topic;

    /// <summary>
    /// Whether channel is persistent
    /// </summary>
    [JsonPropertyName("persistent")]
    public bool Persistent { get; set; } = false;

    /// <summary>
    /// Maximum number of subscribers
    /// </summary>
    [JsonPropertyName("maxSubscribers")]
    public int? MaxSubscribers { get; set; }

    /// <summary>
    /// Message retention policy
    /// </summary>
    [JsonPropertyName("retention")]
    public RetentionPolicy? Retention { get; set; }
}

/// <summary>
/// Event priority levels
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EventPriority
{
    Low,
    Normal,
    High,
    Critical
}

/// <summary>
/// Channel types for pub/sub
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ChannelType
{
    /// <summary>
    /// Broadcast to all subscribers
    /// </summary>
    Broadcast,

    /// <summary>
    /// Topic-based subscription
    /// </summary>
    Topic,

    /// <summary>
    /// Queue-based (single consumer)
    /// </summary>
    Queue
}

/// <summary>
/// Message retention policy for channels
/// </summary>
public class RetentionPolicy
{
    /// <summary>
    /// Maximum number of messages to retain
    /// </summary>
    [JsonPropertyName("maxMessages")]
    public int? MaxMessages { get; set; }

    /// <summary>
    /// Maximum age of messages in seconds
    /// </summary>
    [JsonPropertyName("maxAge")]
    public int? MaxAge { get; set; }

    /// <summary>
    /// Maximum size in bytes
    /// </summary>
    [JsonPropertyName("maxSize")]
    public long? MaxSize { get; set; }
}