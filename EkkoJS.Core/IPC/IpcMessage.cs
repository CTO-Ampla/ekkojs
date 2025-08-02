using System.Text.Json.Serialization;

namespace EkkoJS.Core.IPC;

/// <summary>
/// Represents an IPC message for communication between EkkoJS and external services
/// </summary>
public class IpcMessage
{
    /// <summary>
    /// Unique identifier for this message
    /// </summary>
    [JsonPropertyName("messageId")]
    public string MessageId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Type of message (request, response, event, subscribe, unsubscribe)
    /// </summary>
    [JsonPropertyName("type")]
    public IpcMessageType Type { get; set; }

    /// <summary>
    /// Channel or service name for routing
    /// </summary>
    [JsonPropertyName("channel")]
    public string Channel { get; set; } = string.Empty;

    /// <summary>
    /// Action or method name to invoke
    /// </summary>
    [JsonPropertyName("action")]
    public string? Action { get; set; }

    /// <summary>
    /// Message payload data
    /// </summary>
    [JsonPropertyName("data")]
    public object? Data { get; set; }

    /// <summary>
    /// Timestamp when message was created
    /// </summary>
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Correlation ID for linking requests with responses
    /// </summary>
    [JsonPropertyName("correlationId")]
    public string? CorrelationId { get; set; }

    /// <summary>
    /// Error information (if applicable)
    /// </summary>
    [JsonPropertyName("error")]
    public IpcError? Error { get; set; }

    /// <summary>
    /// Additional metadata
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Types of IPC messages
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum IpcMessageType
{
    /// <summary>
    /// Request to invoke a method
    /// </summary>
    Request,

    /// <summary>
    /// Response to a method invocation
    /// </summary>
    Response,

    /// <summary>
    /// Event notification
    /// </summary>
    Event,

    /// <summary>
    /// Subscribe to a channel
    /// </summary>
    Subscribe,

    /// <summary>
    /// Unsubscribe from a channel
    /// </summary>
    Unsubscribe,

    /// <summary>
    /// Heartbeat/ping message
    /// </summary>
    Ping,

    /// <summary>
    /// Pong response to ping
    /// </summary>
    Pong
}

/// <summary>
/// Error information for IPC messages
/// </summary>
public class IpcError
{
    /// <summary>
    /// Error code
    /// </summary>
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable error message
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Additional error details
    /// </summary>
    [JsonPropertyName("details")]
    public object? Details { get; set; }

    /// <summary>
    /// Stack trace (for debugging)
    /// </summary>
    [JsonPropertyName("stackTrace")]
    public string? StackTrace { get; set; }
}

/// <summary>
/// Standard error codes for IPC communication
/// </summary>
public static class IpcErrorCodes
{
    public const string UnknownError = "UNKNOWN_ERROR";
    public const string InvalidMessage = "INVALID_MESSAGE";
    public const string MethodNotFound = "METHOD_NOT_FOUND";
    public const string InvalidParameters = "INVALID_PARAMETERS";
    public const string ServiceUnavailable = "SERVICE_UNAVAILABLE";
    public const string Timeout = "TIMEOUT";
    public const string PermissionDenied = "PERMISSION_DENIED";
    public const string ChannelNotFound = "CHANNEL_NOT_FOUND";
    public const string SerializationError = "SERIALIZATION_ERROR";
    public const string ConnectionError = "CONNECTION_ERROR";
}