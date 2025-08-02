using System.Text;
using System.Text.Json;

namespace EkkoJS.Core.IPC;

/// <summary>
/// Abstract base class for IPC transport implementations
/// </summary>
public abstract class IpcTransport : IDisposable
{
    /// <summary>
    /// Event fired when a message is received
    /// </summary>
    public event EventHandler<IpcMessage>? MessageReceived;

    /// <summary>
    /// Event fired when connection is established
    /// </summary>
    public event EventHandler? Connected;

    /// <summary>
    /// Event fired when connection is lost
    /// </summary>
    public event EventHandler<string>? Disconnected;

    /// <summary>
    /// Whether the transport is currently connected
    /// </summary>
    public abstract bool IsConnected { get; }

    /// <summary>
    /// Transport configuration
    /// </summary>
    public TransportConfig Config { get; protected set; } = new();

    /// <summary>
    /// JSON serializer options
    /// </summary>
    protected static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Connect to the transport endpoint
    /// </summary>
    public abstract Task ConnectAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Disconnect from the transport endpoint
    /// </summary>
    public abstract Task DisconnectAsync();

    /// <summary>
    /// Send a message through the transport
    /// </summary>
    public abstract Task SendMessageAsync(IpcMessage message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Start listening for incoming messages (server mode)
    /// </summary>
    public abstract Task StartListeningAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stop listening for incoming messages
    /// </summary>
    public abstract Task StopListeningAsync();

    /// <summary>
    /// Fire the MessageReceived event
    /// </summary>
    protected virtual void OnMessageReceived(IpcMessage message)
    {
        MessageReceived?.Invoke(this, message);
    }

    /// <summary>
    /// Fire the Connected event
    /// </summary>
    protected virtual void OnConnected()
    {
        Connected?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Fire the Disconnected event
    /// </summary>
    protected virtual void OnDisconnected(string reason)
    {
        Disconnected?.Invoke(this, reason);
    }

    /// <summary>
    /// Serialize message to JSON bytes
    /// </summary>
    protected byte[] SerializeMessage(IpcMessage message)
    {
        try
        {
            var json = JsonSerializer.Serialize(message, JsonOptions);
            return Encoding.UTF8.GetBytes(json);
        }
        catch (Exception ex)
        {
            throw new IpcException($"Failed to serialize message: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Deserialize JSON bytes to message
    /// </summary>
    protected IpcMessage DeserializeMessage(byte[] data)
    {
        try
        {
            var json = Encoding.UTF8.GetString(data);
            var message = JsonSerializer.Deserialize<IpcMessage>(json, JsonOptions);
            return message ?? throw new InvalidOperationException("Deserialized message is null");
        }
        catch (Exception ex)
        {
            throw new IpcException($"Failed to deserialize message: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Create transport instance based on configuration
    /// </summary>
    public static IpcTransport Create(TransportConfig config)
    {
        return config.Type.ToLowerInvariant() switch
        {
            "namedpipe" => new NamedPipeTransport(config),
            "tcp" => new TcpTransport(config),
            "socket" => new UnixSocketTransport(config),
            _ => throw new NotSupportedException($"Transport type '{config.Type}' is not supported")
        };
    }

    /// <summary>
    /// Dispose resources
    /// </summary>
    public virtual void Dispose()
    {
        try
        {
            DisconnectAsync().Wait(5000);
        }
        catch
        {
            // Ignore disposal errors
        }
    }
}

/// <summary>
/// Exception thrown by IPC operations
/// </summary>
public class IpcException : Exception
{
    public IpcException(string message) : base(message) { }
    public IpcException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Named pipe transport implementation (Windows/Linux/macOS)
/// </summary>
public class NamedPipeTransport : IpcTransport
{
    private System.IO.Pipes.NamedPipeClientStream? _clientPipe;
    private System.IO.Pipes.NamedPipeServerStream? _serverPipe;
    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _readingTask;

    public NamedPipeTransport(TransportConfig config)
    {
        Config = config;
    }

    public override bool IsConnected => 
        _clientPipe?.IsConnected == true || _serverPipe?.IsConnected == true;

    public override async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _clientPipe = new System.IO.Pipes.NamedPipeClientStream(
                ".", Config.Address, System.IO.Pipes.PipeDirection.InOut);
            
            await _clientPipe.ConnectAsync(Config.Timeout, cancellationToken);
            
            _cancellationTokenSource = new CancellationTokenSource();
            _readingTask = Task.Run(() => ReadMessagesAsync(_cancellationTokenSource.Token));
            
            OnConnected();
        }
        catch (Exception ex)
        {
            throw new IpcException($"Failed to connect to named pipe '{Config.Address}': {ex.Message}", ex);
        }
    }

    public override async Task DisconnectAsync()
    {
        _cancellationTokenSource?.Cancel();
        
        if (_readingTask != null)
        {
            await _readingTask;
        }

        if (_clientPipe?.IsConnected == true)
        {
            _clientPipe.Close();
        }
        _clientPipe?.Dispose();
        _clientPipe = null;

        if (_serverPipe?.IsConnected == true)
        {
            _serverPipe.Disconnect();
        }
        _serverPipe?.Dispose();
        _serverPipe = null;
    }

    public override async Task SendMessageAsync(IpcMessage message, CancellationToken cancellationToken = default)
    {
        if (!IsConnected)
            throw new IpcException("Not connected");

        var stream = (Stream?)_clientPipe ?? _serverPipe;
        if (stream == null)
            throw new IpcException("No active stream");

        var data = SerializeMessage(message);
        var lengthBytes = BitConverter.GetBytes(data.Length);
        
        await stream.WriteAsync(lengthBytes, 0, 4, cancellationToken);
        await stream.WriteAsync(data, 0, data.Length, cancellationToken);
        await stream.FlushAsync(cancellationToken);
    }

    public override async Task StartListeningAsync(CancellationToken cancellationToken = default)
    {
        _serverPipe = new System.IO.Pipes.NamedPipeServerStream(
            Config.Address, System.IO.Pipes.PipeDirection.InOut, Config.MaxConnections);

        await _serverPipe.WaitForConnectionAsync(cancellationToken);
        
        _cancellationTokenSource = new CancellationTokenSource();
        _readingTask = Task.Run(() => ReadMessagesAsync(_cancellationTokenSource.Token));
        
        OnConnected();
    }

    public override async Task StopListeningAsync()
    {
        await DisconnectAsync();
    }

    private async Task ReadMessagesAsync(CancellationToken cancellationToken)
    {
        var stream = (Stream?)_clientPipe ?? _serverPipe;
        if (stream == null) return;

        var lengthBuffer = new byte[4];
        
        try
        {
            while (!cancellationToken.IsCancellationRequested && IsConnected)
            {
                // Read message length
                var bytesRead = await stream.ReadAsync(lengthBuffer, 0, 4, cancellationToken);
                if (bytesRead != 4) break;

                var messageLength = BitConverter.ToInt32(lengthBuffer, 0);
                if (messageLength <= 0 || messageLength > 1024 * 1024) // 1MB limit
                    break;

                // Read message data
                var messageBuffer = new byte[messageLength];
                var totalRead = 0;
                
                while (totalRead < messageLength && !cancellationToken.IsCancellationRequested)
                {
                    bytesRead = await stream.ReadAsync(
                        messageBuffer, totalRead, messageLength - totalRead, cancellationToken);
                    if (bytesRead == 0) break;
                    totalRead += bytesRead;
                }

                if (totalRead == messageLength)
                {
                    var message = DeserializeMessage(messageBuffer);
                    OnMessageReceived(message);
                }
            }
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            OnDisconnected($"Read error: {ex.Message}");
        }
    }
}

/// <summary>
/// TCP socket transport implementation
/// </summary>
public class TcpTransport : IpcTransport
{
    public TcpTransport(TransportConfig config)
    {
        Config = config;
    }

    // TODO: Implement TCP transport
    public override bool IsConnected => false;
    public override Task ConnectAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public override Task DisconnectAsync() => throw new NotImplementedException();
    public override Task SendMessageAsync(IpcMessage message, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public override Task StartListeningAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public override Task StopListeningAsync() => throw new NotImplementedException();
}

/// <summary>
/// Unix domain socket transport implementation
/// </summary>
public class UnixSocketTransport : IpcTransport
{
    public UnixSocketTransport(TransportConfig config)
    {
        Config = config;
    }

    // TODO: Implement Unix socket transport
    public override bool IsConnected => false;
    public override Task ConnectAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public override Task DisconnectAsync() => throw new NotImplementedException();
    public override Task SendMessageAsync(IpcMessage message, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public override Task StartListeningAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public override Task StopListeningAsync() => throw new NotImplementedException();
}