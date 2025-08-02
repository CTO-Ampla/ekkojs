# EkkoJS Timers Implementation

## Overview
Implementing `setTimeout`, `setInterval`, `clearTimeout`, and `clearInterval` requires creating an event loop that bridges V8's JavaScript execution with .NET's timer infrastructure.

## Architecture

### 1. Timer Manager Class
```csharp
public class TimerManager : IDisposable
{
    private readonly ConcurrentDictionary<int, TimerInfo> _timers = new();
    private readonly V8ScriptEngine _engine;
    private int _nextTimerId = 1;
    private readonly object _timerLock = new();

    public class TimerInfo
    {
        public int Id { get; set; }
        public Timer Timer { get; set; }
        public ScriptObject Callback { get; set; }
        public object[] Arguments { get; set; }
        public bool IsInterval { get; set; }
        public int Delay { get; set; }
        public CancellationTokenSource CancellationToken { get; set; }
    }
}
```

### 2. Event Loop Integration

```csharp
public class EkkoEventLoop
{
    private readonly BlockingCollection<Action> _taskQueue = new();
    private readonly Thread _eventLoopThread;
    private readonly V8ScriptEngine _engine;
    private volatile bool _running = true;

    public EkkoEventLoop(V8ScriptEngine engine)
    {
        _engine = engine;
        _eventLoopThread = new Thread(ProcessEventLoop)
        {
            Name = "EkkoJS Event Loop",
            IsBackground = false
        };
        _eventLoopThread.Start();
    }

    private void ProcessEventLoop()
    {
        while (_running)
        {
            try
            {
                var task = _taskQueue.Take();
                task.Invoke();
            }
            catch (InvalidOperationException)
            {
                // Queue was completed
                break;
            }
        }
    }

    public void QueueTask(Action task)
    {
        if (!_running) return;
        _taskQueue.Add(task);
    }

    public void Stop()
    {
        _running = false;
        _taskQueue.CompleteAdding();
        _eventLoopThread.Join();
    }
}
```

### 3. Timer Implementation

```csharp
public class TimerImplementation
{
    private readonly TimerManager _timerManager;
    private readonly EkkoEventLoop _eventLoop;
    private readonly V8ScriptEngine _engine;

    public int SetTimeout(ScriptObject callback, int delay, params object[] args)
    {
        if (callback == null || !IsFunction(callback))
            throw new ArgumentException("Callback must be a function");

        var timerId = _timerManager.GetNextId();
        var cts = new CancellationTokenSource();

        var timer = new Timer(_ =>
        {
            if (cts.Token.IsCancellationRequested) return;

            _eventLoop.QueueTask(() =>
            {
                try
                {
                    // Execute callback in V8 context
                    callback.InvokeAsFunction(args);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Timer callback error: {ex.Message}");
                }
                finally
                {
                    _timerManager.RemoveTimer(timerId);
                }
            });
        }, null, delay, Timeout.Infinite);

        _timerManager.AddTimer(timerId, timer, callback, args, false, delay, cts);
        return timerId;
    }

    public int SetInterval(ScriptObject callback, int delay, params object[] args)
    {
        if (callback == null || !IsFunction(callback))
            throw new ArgumentException("Callback must be a function");

        var timerId = _timerManager.GetNextId();
        var cts = new CancellationTokenSource();

        var timer = new Timer(_ =>
        {
            if (cts.Token.IsCancellationRequested) return;

            _eventLoop.QueueTask(() =>
            {
                try
                {
                    callback.InvokeAsFunction(args);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Interval callback error: {ex.Message}");
                    // Don't remove interval on error, it continues
                }
            });
        }, null, delay, delay);

        _timerManager.AddTimer(timerId, timer, callback, args, true, delay, cts);
        return timerId;
    }

    public void ClearTimeout(int timerId) => ClearTimer(timerId);
    public void ClearInterval(int timerId) => ClearTimer(timerId);

    private void ClearTimer(int timerId)
    {
        if (_timerManager.TryGetTimer(timerId, out var timerInfo))
        {
            timerInfo.CancellationToken.Cancel();
            timerInfo.Timer.Dispose();
            _timerManager.RemoveTimer(timerId);
        }
    }
}
```

### 4. JavaScript Binding

```csharp
public void InitializeTimers()
{
    var timerImpl = new TimerImplementation(_timerManager, _eventLoop, _engine);
    
    _engine.AddHostObject("__timerImpl", timerImpl);
    
    _engine.Execute(@"
        // setTimeout implementation
        globalThis.setTimeout = function(callback, delay, ...args) {
            if (typeof callback === 'string') {
                callback = new Function(callback);
            }
            if (typeof callback !== 'function') {
                throw new TypeError('Callback must be a function');
            }
            delay = Math.max(0, Number(delay) || 0);
            return __timerImpl.SetTimeout(callback, delay, ...args);
        };

        // setInterval implementation
        globalThis.setInterval = function(callback, delay, ...args) {
            if (typeof callback === 'string') {
                callback = new Function(callback);
            }
            if (typeof callback !== 'function') {
                throw new TypeError('Callback must be a function');
            }
            delay = Math.max(0, Number(delay) || 0);
            return __timerImpl.SetInterval(callback, delay, ...args);
        };

        // clearTimeout implementation
        globalThis.clearTimeout = function(timerId) {
            if (timerId == null) return;
            __timerImpl.ClearTimeout(Number(timerId));
        };

        // clearInterval implementation
        globalThis.clearInterval = function(timerId) {
            if (timerId == null) return;
            __timerImpl.ClearInterval(Number(timerId));
        };

        // Node.js compatibility
        globalThis.setImmediate = function(callback, ...args) {
            return setTimeout(callback, 0, ...args);
        };

        globalThis.clearImmediate = clearTimeout;
    ");
}
```

### 5. Advanced Features

#### Immediate Timers (setImmediate)
```csharp
public int SetImmediate(ScriptObject callback, params object[] args)
{
    // Queue immediately to event loop without delay
    _eventLoop.QueueTask(() =>
    {
        try
        {
            callback.InvokeAsFunction(args);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Immediate callback error: {ex.Message}");
        }
    });
    
    return _timerManager.GetNextId(); // Return ID for compatibility
}
```

#### Process.nextTick Equivalent
```csharp
public void NextTick(ScriptObject callback, params object[] args)
{
    // Execute at the end of current event loop iteration
    _eventLoop.QueueMicrotask(() =>
    {
        callback.InvokeAsFunction(args);
    });
}
```

### 6. Integration with EkkoRuntime

```csharp
public class EkkoRuntime : IDisposable
{
    private V8ScriptEngine? _engine;
    private EkkoEventLoop? _eventLoop;
    private TimerManager? _timerManager;
    private bool _disposed;

    public async Task InitializeAsync()
    {
        _engine = new V8ScriptEngine(
            V8ScriptEngineFlags.EnableTaskPromiseConversion |
            V8ScriptEngineFlags.EnableDateTimeConversion
        );
        
        // Initialize event loop
        _eventLoop = new EkkoEventLoop(_engine);
        
        // Initialize timer manager
        _timerManager = new TimerManager(_engine);
        
        // Add host bridge first
        _engine.AddHostObject("host", new HostBridge());
        
        // Initialize console
        InitializeConsole();
        
        // Initialize timers
        InitializeTimers();
        
        await Task.CompletedTask;
    }

    public async Task<object?> ExecuteAsync(string code, string fileName = "<anonymous>")
    {
        if (_engine == null)
            throw new InvalidOperationException("Runtime not initialized");
            
        try
        {
            // Execute in event loop context
            var tcs = new TaskCompletionSource<object?>();
            
            _eventLoop.QueueTask(() =>
            {
                try
                {
                    _engine.Execute(documentName: fileName, code: code);
                    tcs.SetResult(null);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });
            
            return await tcs.Task;
        }
        catch (Microsoft.ClearScript.ScriptEngineException ex)
        {
            var errorMessage = ex.ErrorDetails ?? ex.Message;
            throw new RuntimeException($"JavaScript Error in {fileName}: {errorMessage}", ex);
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _eventLoop?.Stop();
            _timerManager?.Dispose();
            _engine?.Dispose();
            _disposed = true;
        }
    }
}
```

### 7. Test Examples

```javascript
// Basic setTimeout
setTimeout(() => {
    console.log("Hello after 1 second");
}, 1000);

// setTimeout with arguments
setTimeout((name, age) => {
    console.log(`Hello ${name}, you are ${age} years old`);
}, 500, "John", 30);

// setInterval
let count = 0;
const intervalId = setInterval(() => {
    console.log(`Count: ${count++}`);
    if (count >= 5) {
        clearInterval(intervalId);
        console.log("Interval cleared");
    }
}, 200);

// Nested timers
setTimeout(() => {
    console.log("Outer timeout");
    setTimeout(() => {
        console.log("Inner timeout");
    }, 500);
}, 1000);

// String callback (legacy support)
setTimeout("console.log('String callback executed')", 100);

// Clear before execution
const timeoutId = setTimeout(() => {
    console.log("This won't run");
}, 5000);
clearTimeout(timeoutId);
```

### 8. Performance Considerations

1. **Timer Coalescing**: Group timers that fire close together
2. **Minimum Delay**: Enforce minimum delay (4ms like browsers)
3. **Maximum Timers**: Limit number of active timers
4. **Memory Management**: Properly dispose of completed timers

### 9. Edge Cases to Handle

1. **Recursive setTimeout**: Prevent stack overflow
2. **Zero delay**: Should still yield to event loop
3. **Negative delays**: Treat as zero
4. **Invalid callbacks**: Proper error handling
5. **Timer drift**: Account for execution time in intervals

### 10. Future Enhancements

1. **High-resolution timers**: `process.hrtime()` equivalent
2. **Performance timing**: `performance.now()`
3. **Animation frames**: `requestAnimationFrame` for UI scenarios
4. **Worker timers**: Timers in worker threads
5. **Timer priorities**: Different queues for different priorities