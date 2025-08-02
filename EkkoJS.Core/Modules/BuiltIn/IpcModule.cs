using EkkoJS.Core.IPC;
using System.Text.Json;

namespace EkkoJS.Core.Modules.BuiltIn;

/// <summary>
/// IPC module for EkkoJS - enables communication with external services
/// </summary>
public class IpcModule : IModule
{
    private readonly string _serviceName;
    private readonly IpcClient _client;
    private readonly IpcServiceMapping? _serviceMapping;

    public IpcModule(string serviceName, TransportConfig transportConfig, IpcServiceMapping? serviceMapping = null)
    {
        _serviceName = serviceName;
        _serviceMapping = serviceMapping;
        Protocol = "ipc";
        Name = serviceName;
        
        _client = new IpcClient(transportConfig)
        {
            ServiceMapping = serviceMapping
        };
    }

    public string Name { get; }
    public string Protocol { get; }

    public object GetExports()
    {
        // Create JavaScript-friendly object
        var exports = new IpcModuleExports(_client, _serviceMapping);
        return exports;
    }

    public void Dispose()
    {
        _client?.Dispose();
    }
}

/// <summary>
/// JavaScript-friendly exports for IPC module
/// </summary>
public class IpcModuleExports : IDisposable
{
    private readonly IpcClient _client;
    private readonly IpcServiceMapping? _serviceMapping;

    public IpcModuleExports(IpcClient client, IpcServiceMapping? serviceMapping)
    {
        _client = client;
        _serviceMapping = serviceMapping;
    }

    /// <summary>
    /// Connect to the service
    /// </summary>
    public async Task<bool> connect()
    {
        try
        {
            await _client.ConnectAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Disconnect from the service
    /// </summary>
    public async Task disconnect()
    {
        await _client.DisconnectAsync();
    }

    /// <summary>
    /// Check if connected
    /// </summary>
    public bool isConnected => _client.IsConnected;

    /// <summary>
    /// Call a remote method
    /// </summary>
    public async Task<object?> call(string method, params object?[] args)
    {
        try
        {
            var data = args.Length switch
            {
                0 => null,
                1 => args[0],
                _ => args
            };

            return await _client.SendRequestAsync<object>(_serviceMapping?.Service.Name ?? "default", method, data);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"IPC call failed: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Send a fire-and-forget message
    /// </summary>
    public async Task send(string method, params object?[] args)
    {
        var data = args.Length switch
        {
            0 => null,
            1 => args[0],
            _ => args
        };

        await _client.SendAsync(_serviceMapping?.Service.Name ?? "default", method, data);
    }

    /// <summary>
    /// Subscribe to events
    /// </summary>
    public async Task subscribe(string channel, dynamic handler)
    {
        // Convert JavaScript function to C# delegate
        Func<object?, Task> csharpHandler = async (data) =>
        {
            try
            {
                // Call the JavaScript function
                var result = handler(data);
                
                // Handle if the result is a promise/task
                if (result is Task task)
                {
                    await task;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in event handler: {ex.Message}");
            }
        };
        
        await _client.SubscribeAsync(channel, csharpHandler);
    }

    /// <summary>
    /// Unsubscribe from events
    /// </summary>
    public async Task unsubscribe(string channel, dynamic? handler = null)
    {
        // For simplicity, just unsubscribe from the entire channel
        // TODO: Implement specific handler tracking if needed
        await _client.UnsubscribeAsync(channel, null);
    }

    /// <summary>
    /// Publish an event
    /// </summary>
    public async Task publish(string channel, object? data = null)
    {
        await _client.PublishAsync(channel, data);
    }

    /// <summary>
    /// Get service information
    /// </summary>
    public object? getServiceInfo()
    {
        if (_serviceMapping == null) return null;

        return new
        {
            name = _serviceMapping.Service.Name,
            version = _serviceMapping.Service.Version,
            description = _serviceMapping.Service.Description,
            methods = _serviceMapping.Methods.Keys.ToArray(),
            events = _serviceMapping.Events.Keys.ToArray(),
            channels = _serviceMapping.Channels.Keys.ToArray()
        };
    }

    /// <summary>
    /// Get method information
    /// </summary>
    public object? getMethodInfo(string methodName)
    {
        if (_serviceMapping?.Methods.TryGetValue(methodName, out var method) == true)
        {
            return new
            {
                name = methodName,
                description = method.Description,
                parameters = method.Parameters.Select(p => new
                {
                    name = p.Name,
                    type = p.Type,
                    required = p.Required,
                    description = p.Description
                }).ToArray(),
                returns = new
                {
                    type = method.Returns.Type,
                    nullable = method.Returns.Nullable
                },
                timeout = method.Timeout,
                async = method.IsAsync
            };
        }
        return null;
    }

    public void Dispose()
    {
        _client?.Dispose();
    }
}