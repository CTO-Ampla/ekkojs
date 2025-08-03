using System.Collections.Concurrent;

namespace EkkoJS.Core;

public class EventLoop : IDisposable
{
    private readonly BlockingCollection<Action> _taskQueue = new();
    private readonly Thread _eventLoopThread;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private bool _disposed;
    private int _pendingTasks = 0;

    public EventLoop()
    {
        _eventLoopThread = new Thread(ProcessEventLoop)
        {
            Name = "EkkoJS Event Loop",
            IsBackground = false
        };
        _eventLoopThread.Start();
    }

    public bool HasPendingTasks()
    {
        return _pendingTasks > 0 || _taskQueue.Count > 0;
    }

    private void ProcessEventLoop()
    {
        while (!_cancellationTokenSource.Token.IsCancellationRequested)
        {
            try
            {
                if (_taskQueue.TryTake(out var task, 100, _cancellationTokenSource.Token))
                {
                    Interlocked.Decrement(ref _pendingTasks);
                    task.Invoke();
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Event loop error: {ex.Message}");
            }
        }
    }

    public void QueueTask(Action task)
    {
        if (_disposed || _cancellationTokenSource.Token.IsCancellationRequested) return;
        
        try
        {
            Interlocked.Increment(ref _pendingTasks);
            _taskQueue.Add(task, _cancellationTokenSource.Token);
        }
        catch (InvalidOperationException)
        {
            // Queue is completing
            Interlocked.Decrement(ref _pendingTasks);
        }
        catch (OperationCanceledException)
        {
            // Cancellation requested
            Interlocked.Decrement(ref _pendingTasks);
        }
    }

    public Task QueueTaskAsync(Action task)
    {
        var tcs = new TaskCompletionSource();
        
        QueueTask(() =>
        {
            try
            {
                task();
                tcs.SetResult();
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        });
        
        return tcs.Task;
    }

    public async Task WaitForPendingTasksAsync(CancellationToken cancellationToken = default)
    {
        while (HasPendingTasks() && !cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(100, cancellationToken);
        }
    }

    public void Stop()
    {
        _cancellationTokenSource.Cancel();
        _taskQueue.CompleteAdding();
        _eventLoopThread.Join(5000); // Wait max 5 seconds
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            Stop();
            _taskQueue.Dispose();
            _cancellationTokenSource.Dispose();
            _disposed = true;
        }
    }
}