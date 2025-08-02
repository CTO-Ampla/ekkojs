using EkkoJS.Core.IPC;
using System.Collections.Concurrent;
using System.Text.Json;

namespace EkkoJS.Demo.IpcService;

/// <summary>
/// Demo IPC service for EkkoJS communication
/// This service provides user management and real-time notifications
/// </summary>
class Program
{
    private static readonly ConcurrentDictionary<int, User> Users = new();
    private static readonly List<IpcTransport> ConnectedClients = new();
    private static int _nextUserId = 1;

    static async Task Main(string[] args)
    {
        Console.WriteLine("🚀 EkkoJS Demo IPC Service Starting...");
        
        // Seed some demo data
        SeedDemoData();

        // Create transport configuration
        var config = new TransportConfig
        {
            Type = "namedpipe",
            Address = "ekko-demo-service",
            MaxConnections = 10
        };

        // Create and start the server
        var server = new IpcServer(config);
        server.ClientConnected += OnClientConnected;
        server.MessageReceived += OnMessageReceived;

        Console.WriteLine($"📡 Starting IPC server on pipe: {config.Address}");
        await server.StartAsync();

        Console.WriteLine("✅ Service is running! Press Ctrl+C to stop.");
        Console.WriteLine();
        Console.WriteLine("Available methods:");
        Console.WriteLine("  - getUsers() - Get all users");
        Console.WriteLine("  - getUserById(id) - Get user by ID");
        Console.WriteLine("  - createUser(userData) - Create new user");
        Console.WriteLine("  - updateUser(id, userData) - Update user");
        Console.WriteLine("  - deleteUser(id) - Delete user");
        Console.WriteLine();
        Console.WriteLine("Available events:");
        Console.WriteLine("  - user.created - When a user is created");
        Console.WriteLine("  - user.updated - When a user is updated");
        Console.WriteLine("  - user.deleted - When a user is deleted");
        Console.WriteLine("  - notifications - General notifications");
        Console.WriteLine();

        // Start background notification task
        _ = Task.Run(SendPeriodicNotifications);

        // Wait for shutdown
        var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };

        try
        {
            await Task.Delay(Timeout.Infinite, cts.Token);
        }
        catch (OperationCanceledException)
        {
            // Expected when cancellation is requested
        }

        Console.WriteLine("\n🛑 Shutting down service...");
        await server.StopAsync();
        Console.WriteLine("✅ Service stopped.");
    }

    private static void SeedDemoData()
    {
        Users[1] = new User { Id = 1, Name = "John Doe", Email = "john@example.com", CreatedAt = DateTime.UtcNow.AddDays(-30) };
        Users[2] = new User { Id = 2, Name = "Jane Smith", Email = "jane@example.com", CreatedAt = DateTime.UtcNow.AddDays(-15) };
        Users[3] = new User { Id = 3, Name = "Bob Johnson", Email = "bob@example.com", CreatedAt = DateTime.UtcNow.AddDays(-7) };
        _nextUserId = 4;
    }

    private static void OnClientConnected(object? sender, IpcTransport transport)
    {
        Console.WriteLine($"🔗 Client connected: {transport.GetHashCode()}");
        lock (ConnectedClients)
        {
            ConnectedClients.Add(transport);
        }

        transport.Disconnected += (_, reason) =>
        {
            Console.WriteLine($"🔌 Client disconnected: {transport.GetHashCode()} - {reason}");
            lock (ConnectedClients)
            {
                ConnectedClients.Remove(transport);
            }
        };
    }

    private static async void OnMessageReceived(object? sender, (IpcTransport Transport, IpcMessage Message) e)
    {
        var (transport, message) = e;
        
        Console.WriteLine($"📨 Received {message.Type} from {transport.GetHashCode()}: {message.Channel}.{message.Action}");

        try
        {
            switch (message.Type)
            {
                case IpcMessageType.Request:
                    await HandleRequest(transport, message);
                    break;
                    
                case IpcMessageType.Subscribe:
                    await HandleSubscribe(transport, message);
                    break;
                    
                case IpcMessageType.Unsubscribe:
                    await HandleUnsubscribe(transport, message);
                    break;
                    
                case IpcMessageType.Event:
                    await HandleEvent(transport, message);
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error handling message: {ex.Message}");
            
            var errorResponse = new IpcMessage
            {
                Type = IpcMessageType.Response,
                CorrelationId = message.MessageId,
                Error = new IpcError
                {
                    Code = IpcErrorCodes.UnknownError,
                    Message = ex.Message
                }
            };
            
            await transport.SendMessageAsync(errorResponse);
        }
    }

    private static async Task HandleRequest(IpcTransport transport, IpcMessage message)
    {
        object? result = null;

        switch (message.Action?.ToLowerInvariant())
        {
            case "getusers":
                result = Users.Values.OrderBy(u => u.Id).ToArray();
                break;

            case "getuserbyid":
                if (message.Data is JsonElement idElement && idElement.TryGetInt32(out var id))
                {
                    Users.TryGetValue(id, out var user);
                    result = user;
                }
                break;

            case "createuser":
                if (message.Data is JsonElement userElement)
                {
                    var userData = userElement.Deserialize<CreateUserRequest>();
                    if (userData != null && !string.IsNullOrEmpty(userData.Name))
                    {
                        var newUser = new User
                        {
                            Id = _nextUserId++,
                            Name = userData.Name,
                            Email = userData.Email ?? "",
                            CreatedAt = DateTime.UtcNow
                        };
                        
                        Users[newUser.Id] = newUser;
                        result = newUser;

                        // Broadcast user created event
                        await BroadcastEvent("user.created", new { 
                            id = newUser.Id, 
                            name = newUser.Name, 
                            timestamp = newUser.CreatedAt 
                        });
                    }
                }
                break;

            case "updateuser":
                if (message.Data is JsonElement updateElement)
                {
                    var updateData = updateElement.Deserialize<UpdateUserRequest>();
                    if (updateData != null && Users.TryGetValue(updateData.Id, out var existingUser))
                    {
                        var changes = new Dictionary<string, object?>();
                        
                        if (!string.IsNullOrEmpty(updateData.Name) && updateData.Name != existingUser.Name)
                        {
                            changes["name"] = new { from = existingUser.Name, to = updateData.Name };
                            existingUser.Name = updateData.Name;
                        }
                        
                        if (!string.IsNullOrEmpty(updateData.Email) && updateData.Email != existingUser.Email)
                        {
                            changes["email"] = new { from = existingUser.Email, to = updateData.Email };
                            existingUser.Email = updateData.Email;
                        }

                        result = existingUser;

                        if (changes.Count > 0)
                        {
                            await BroadcastEvent("user.updated", new { 
                                id = existingUser.Id, 
                                changes, 
                                timestamp = DateTime.UtcNow 
                            });
                        }
                    }
                }
                break;

            case "deleteuser":
                if (message.Data is JsonElement deleteElement && deleteElement.TryGetInt32(out var deleteId))
                {
                    if (Users.TryRemove(deleteId, out var deletedUser))
                    {
                        result = new { success = true, deletedUser };
                        
                        await BroadcastEvent("user.deleted", new { 
                            id = deletedUser.Id, 
                            name = deletedUser.Name, 
                            timestamp = DateTime.UtcNow 
                        });
                    }
                    else
                    {
                        result = new { success = false, message = "User not found" };
                    }
                }
                break;

            default:
                throw new NotSupportedException($"Method '{message.Action}' is not supported");
        }

        var response = new IpcMessage
        {
            Type = IpcMessageType.Response,
            CorrelationId = message.CorrelationId,
            Data = result
        };

        await transport.SendMessageAsync(response);
    }

    private static Task HandleSubscribe(IpcTransport transport, IpcMessage message)
    {
        Console.WriteLine($"📡 Client subscribed to channel: {message.Channel}");
        // In a real implementation, you'd track subscriptions per transport
        return Task.CompletedTask;
    }

    private static Task HandleUnsubscribe(IpcTransport transport, IpcMessage message)
    {
        Console.WriteLine($"📡 Client unsubscribed from channel: {message.Channel}");
        return Task.CompletedTask;
    }

    private static Task HandleEvent(IpcTransport transport, IpcMessage message)
    {
        Console.WriteLine($"📢 Received event on channel {message.Channel}: {message.Data}");
        // Echo the event to all other clients
        return BroadcastEvent(message.Channel, message.Data, excludeTransport: transport);
    }

    private static async Task BroadcastEvent(string channel, object? data, IpcTransport? excludeTransport = null)
    {
        var eventMessage = new IpcMessage
        {
            Type = IpcMessageType.Event,
            Channel = channel,
            Data = data
        };

        var clients = ConnectedClients.ToList();
        Console.WriteLine($"📢 Broadcasting event to {clients.Count} clients on channel: {channel}");

        foreach (var client in clients)
        {
            if (client == excludeTransport) continue;
            
            try
            {
                await client.SendMessageAsync(eventMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to send event to client {client.GetHashCode()}: {ex.Message}");
            }
        }
    }

    private static async Task SendPeriodicNotifications()
    {
        var random = new Random();
        var notifications = new[]
        {
            "System health check completed successfully",
            "New feature deployment initiated",
            "Database backup completed",
            "Cache cleared and refreshed",
            "User activity spike detected",
            "Performance metrics updated"
        };

        while (true)
        {
            await Task.Delay(TimeSpan.FromSeconds(30));
            
            var notification = notifications[random.Next(notifications.Length)];
            await BroadcastEvent("notifications", new {
                message = notification,
                timestamp = DateTime.UtcNow,
                type = "info",
                source = "system"
            });
        }
    }
}

// Data models
public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}

public class CreateUserRequest
{
    public string Name { get; set; } = "";
    public string? Email { get; set; }
}

public class UpdateUserRequest
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
}

// Simple IPC Server implementation
public class IpcServer : IDisposable
{
    private readonly TransportConfig _config;
    private readonly List<IpcTransport> _transports = new();
    private CancellationTokenSource? _cancellationTokenSource;

    public event EventHandler<IpcTransport>? ClientConnected;
    public event EventHandler<(IpcTransport Transport, IpcMessage Message)>? MessageReceived;

    public IpcServer(TransportConfig config)
    {
        _config = config;
    }

    public async Task StartAsync()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        
        // Start accepting connections in background
        _ = Task.Run(async () =>
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    var transport = IpcTransport.Create(_config);
                    transport.MessageReceived += (sender, message) =>
                    {
                        MessageReceived?.Invoke(this, (transport, message));
                    };

                    await transport.StartListeningAsync(_cancellationTokenSource.Token);
                    
                    lock (_transports)
                    {
                        _transports.Add(transport);
                    }
                    
                    ClientConnected?.Invoke(this, transport);
                }
                catch (Exception ex) when (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    Console.WriteLine($"Error accepting connection: {ex.Message}");
                    await Task.Delay(1000, _cancellationTokenSource.Token);
                }
            }
        });

        await Task.Delay(100); // Give server time to start
    }

    public async Task StopAsync()
    {
        _cancellationTokenSource?.Cancel();

        var transports = _transports.ToList();
        foreach (var transport in transports)
        {
            try
            {
                await transport.StopListeningAsync();
                transport.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error stopping transport: {ex.Message}");
            }
        }

        _transports.Clear();
    }

    public void Dispose()
    {
        try
        {
            StopAsync().Wait(5000);
        }
        catch
        {
            // Ignore disposal errors
        }
    }
}