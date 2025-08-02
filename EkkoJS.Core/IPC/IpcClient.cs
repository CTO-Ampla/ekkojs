using System.Collections.Concurrent;

namespace EkkoJS.Core.IPC;

/// <summary>
/// IPC client for making requests and subscribing to events
/// </summary>
public class IpcClient : IDisposable
{
    private readonly IpcTransport _transport;
    private readonly ConcurrentDictionary<string, TaskCompletionSource<IpcMessage>> _pendingRequests = new();
    private readonly ConcurrentDictionary<string, List<Func<object?, Task>>> _subscriptions = new();
    private readonly SemaphoreSlim _connectionSemaphore = new(1, 1);
    private bool _disposed = false;

    /// <summary>
    /// Event fired when an event message is received
    /// </summary>
    public event EventHandler<IpcMessage>? EventReceived;

    /// <summary>
    /// Whether the client is connected
    /// </summary>
    public bool IsConnected => _transport.IsConnected;

    /// <summary>
    /// Service mapping configuration
    /// </summary>
    public IpcServiceMapping? ServiceMapping { get; set; }

    public IpcClient(TransportConfig config)
    {
        _transport = IpcTransport.Create(config);
        _transport.MessageReceived += OnMessageReceived;
        _transport.Disconnected += OnDisconnected;
    }

    /// <summary>
    /// Connect to the IPC service
    /// </summary>
    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        await _connectionSemaphore.WaitAsync(cancellationToken);
        try
        {
            if (!_transport.IsConnected)
            {
                await _transport.ConnectAsync(cancellationToken);
            }
        }
        finally
        {
            _connectionSemaphore.Release();
        }
    }

    /// <summary>
    /// Disconnect from the IPC service
    /// </summary>
    public async Task DisconnectAsync()
    {
        await _connectionSemaphore.WaitAsync();
        try
        {
            // Cancel all pending requests
            foreach (var tcs in _pendingRequests.Values)
            {
                tcs.TrySetCanceled();
            }
            _pendingRequests.Clear();

            if (_transport.IsConnected)
            {
                await _transport.DisconnectAsync();
            }
        }
        finally
        {
            _connectionSemaphore.Release();
        }
    }

    /// <summary>
    /// Send a request and wait for response
    /// </summary>
    public async Task<T?> SendRequestAsync<T>(string channel, string action, object? data = null, 
        CancellationToken cancellationToken = default)
    {
        if (!_transport.IsConnected)
        {
            await ConnectAsync(cancellationToken);
        }

        var message = new IpcMessage
        {
            Type = IpcMessageType.Request,
            Channel = channel,
            Action = action,
            Data = data,
            CorrelationId = Guid.NewGuid().ToString()
        };

        var tcs = new TaskCompletionSource<IpcMessage>();
        _pendingRequests[message.CorrelationId] = tcs;

        try
        {
            await _transport.SendMessageAsync(message, cancellationToken);

            using var timeoutCts = new CancellationTokenSource(30000); // 30 second timeout
            using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            combinedCts.Token.Register(() => tcs.TrySetCanceled());

            var response = await tcs.Task;

            if (response.Error != null)
            {
                throw new IpcException($"IPC Error ({response.Error.Code}): {response.Error.Message}");
            }

            if (response.Data == null)
                return default(T);

            if (response.Data is T directResult)
                return directResult;

            if (response.Data is System.Text.Json.JsonElement jsonElement)
            {
                return System.Text.Json.JsonSerializer.Deserialize<T>(jsonElement.GetRawText());
            }

            // Try to convert using System.Convert
            return (T?)Convert.ChangeType(response.Data, typeof(T));
        }
        finally
        {
            _pendingRequests.TryRemove(message.CorrelationId, out _);
        }
    }

    /// <summary>
    /// Send a fire-and-forget message
    /// </summary>
    public async Task SendAsync(string channel, string action, object? data = null, 
        CancellationToken cancellationToken = default)
    {
        if (!_transport.IsConnected)
        {
            await ConnectAsync(cancellationToken);
        }

        var message = new IpcMessage
        {
            Type = IpcMessageType.Request,
            Channel = channel,
            Action = action,
            Data = data
        };

        await _transport.SendMessageAsync(message, cancellationToken);
    }

    /// <summary>
    /// Subscribe to events on a channel
    /// </summary>
    public async Task SubscribeAsync(string channel, Func<object?, Task> handler, 
        CancellationToken cancellationToken = default)
    {
        if (!_transport.IsConnected)
        {
            await ConnectAsync(cancellationToken);
        }

        // Add handler to local subscriptions
        _subscriptions.AddOrUpdate(channel, 
            [handler], 
            (_, existing) => 
            {
                existing.Add(handler);
                return existing;
            });

        // Send subscription message to server
        var message = new IpcMessage
        {
            Type = IpcMessageType.Subscribe,
            Channel = channel
        };

        await _transport.SendMessageAsync(message, cancellationToken);
    }

    /// <summary>
    /// Unsubscribe from events on a channel
    /// </summary>
    public async Task UnsubscribeAsync(string channel, Func<object?, Task>? handler = null, 
        CancellationToken cancellationToken = default)
    {
        if (_subscriptions.TryGetValue(channel, out var handlers))
        {
            if (handler != null)
            {
                handlers.Remove(handler);
                if (handlers.Count == 0)
                {
                    _subscriptions.TryRemove(channel, out _);
                }
            }
            else
            {
                _subscriptions.TryRemove(channel, out _);
            }
        }

        if (_transport.IsConnected)
        {
            var message = new IpcMessage
            {
                Type = IpcMessageType.Unsubscribe,
                Channel = channel
            };

            await _transport.SendMessageAsync(message, cancellationToken);
        }
    }

    /// <summary>
    /// Publish an event to a channel
    /// </summary>
    public async Task PublishAsync(string channel, object? data = null, 
        CancellationToken cancellationToken = default)
    {
        if (!_transport.IsConnected)
        {
            await ConnectAsync(cancellationToken);
        }

        var message = new IpcMessage
        {
            Type = IpcMessageType.Event,
            Channel = channel,
            Data = data
        };

        await _transport.SendMessageAsync(message, cancellationToken);
    }

    /// <summary>
    /// Handle incoming messages
    /// </summary>
    private void OnMessageReceived(object? sender, IpcMessage message)
    {
        switch (message.Type)
        {
            case IpcMessageType.Response when !string.IsNullOrEmpty(message.CorrelationId):
                if (_pendingRequests.TryRemove(message.CorrelationId, out var tcs))
                {
                    tcs.SetResult(message);
                }
                break;

            case IpcMessageType.Event:
                HandleEventMessage(message);
                break;

            case IpcMessageType.Ping:
                // Respond to ping with pong
                _ = Task.Run(async () =>
                {
                    var pong = new IpcMessage
                    {
                        Type = IpcMessageType.Pong,
                        CorrelationId = message.MessageId
                    };
                    await _transport.SendMessageAsync(pong);
                });
                break;
        }
    }

    /// <summary>
    /// Handle event messages
    /// </summary>
    private void HandleEventMessage(IpcMessage message)
    {
        // Fire global event
        EventReceived?.Invoke(this, message);

        // Fire channel-specific handlers
        if (_subscriptions.TryGetValue(message.Channel, out var handlers))
        {
            foreach (var handler in handlers.ToList()) // ToList to avoid collection modification
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await handler(message.Data);
                    }
                    catch (Exception ex)
                    {
                        // Log error but don't propagate
                        Console.WriteLine($"Error in event handler for channel '{message.Channel}': {ex.Message}");
                    }
                });
            }
        }
    }

    /// <summary>
    /// Handle disconnection
    /// </summary>
    private void OnDisconnected(object? sender, string reason)
    {
        // Cancel all pending requests
        foreach (var tcs in _pendingRequests.Values)
        {
            tcs.TrySetException(new IpcException($"Connection lost: {reason}"));
        }
        _pendingRequests.Clear();
    }

    /// <summary>
    /// Dispose resources
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        try
        {
            DisconnectAsync().Wait(5000);
        }
        catch
        {
            // Ignore disposal errors
        }

        _transport?.Dispose();
        _connectionSemaphore.Dispose();
        
        GC.SuppressFinalize(this);
    }
}