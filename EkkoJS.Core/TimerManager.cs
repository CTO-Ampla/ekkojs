using System.Collections.Concurrent;
using Microsoft.ClearScript;

namespace EkkoJS.Core;

public class TimerManager : IDisposable
{
    private readonly ConcurrentDictionary<int, TimerInfo> _timers = new();
    private readonly EventLoop _eventLoop;
    private int _nextTimerId = 1;
    private bool _disposed;

    public TimerManager(EventLoop eventLoop)
    {
        _eventLoop = eventLoop;
    }

    public bool HasPendingTimers()
    {
        return !_timers.IsEmpty;
    }

    public int PendingTimerCount()
    {
        return _timers.Count;
    }

    private class TimerInfo
    {
        public required int Id { get; set; }
        public required Timer Timer { get; set; }
        public required dynamic Callback { get; set; }
        public required object?[] Arguments { get; set; }
        public required bool IsInterval { get; set; }
        public required int Delay { get; set; }
    }

    public int SetTimeout(dynamic callback, int delay, params object?[] args)
    {
        if (delay < 0) delay = 0;
        
        var timerId = Interlocked.Increment(ref _nextTimerId);
        
        var timer = new Timer(_ =>
        {
            _eventLoop.QueueTask(() =>
            {
                try
                {
                    if (_timers.TryRemove(timerId, out var info))
                    {
                        if (Environment.GetEnvironmentVariable("EKKO_DEBUG") == "1")
                        {
                            Console.WriteLine($"[DEBUG TimerManager] Timer {timerId} firing, {_timers.Count} remaining");
                        }
                        InvokeCallback(info.Callback, info.Arguments);
                        info.Timer.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Timer callback error: {ex.Message}");
                }
            });
        }, null, delay, Timeout.Infinite);

        var timerInfo = new TimerInfo
        {
            Id = timerId,
            Timer = timer,
            Callback = callback,
            Arguments = args,
            IsInterval = false,
            Delay = delay
        };

        _timers[timerId] = timerInfo;
        
        if (Environment.GetEnvironmentVariable("EKKO_DEBUG") == "1")
        {
            Console.WriteLine($"[DEBUG TimerManager] Timer {timerId} created with {delay}ms delay, total timers: {_timers.Count}");
        }
        
        return timerId;
    }

    public int SetInterval(dynamic callback, int delay, params object?[] args)
    {
        if (delay < 0) delay = 0;
        if (delay < 10) delay = 10; // Minimum interval like browsers
        
        var timerId = Interlocked.Increment(ref _nextTimerId);
        
        var timer = new Timer(_ =>
        {
            _eventLoop.QueueTask(() =>
            {
                try
                {
                    if (_timers.TryGetValue(timerId, out var info))
                    {
                        InvokeCallback(info.Callback, info.Arguments);
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Interval callback error: {ex.Message}");
                }
            });
        }, null, delay, delay);

        var timerInfo = new TimerInfo
        {
            Id = timerId,
            Timer = timer,
            Callback = callback,
            Arguments = args,
            IsInterval = true,
            Delay = delay
        };

        _timers[timerId] = timerInfo;
        
        if (Environment.GetEnvironmentVariable("EKKO_DEBUG") == "1")
        {
            Console.WriteLine($"[DEBUG TimerManager] Timer {timerId} created with {delay}ms delay, total timers: {_timers.Count}");
        }
        
        return timerId;
    }

    public void ClearTimer(int timerId)
    {
        if (_timers.TryRemove(timerId, out var timerInfo))
        {
            timerInfo.Timer.Change(Timeout.Infinite, Timeout.Infinite);
            timerInfo.Timer.Dispose();
        }
    }

    private void InvokeCallback(dynamic callback, object?[] args)
    {
        try
        {
            if (callback is ScriptObject scriptObj)
            {
                scriptObj.InvokeAsFunction(args);
            }
            else if (callback is Delegate del)
            {
                del.DynamicInvoke(args);
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error invoking callback: {ex.Message}");
        }
    }

    public async Task WaitForPendingTimersAsync(CancellationToken cancellationToken = default)
    {
        // Wait until all timers are completed or cancelled
        while (HasPendingTimers() && !cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(100, cancellationToken);
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            foreach (var timer in _timers.Values)
            {
                timer.Timer.Dispose();
            }
            _timers.Clear();
            _disposed = true;
        }
    }
}