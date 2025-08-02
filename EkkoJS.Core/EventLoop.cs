using System.Collections.Concurrent;

namespace EkkoJS.Core;

public class EventLoop : IDisposable
{
    private readonly BlockingCollection<Action> _taskQueue = new();
    private readonly Thread _eventLoopThread;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private bool _disposed;

    public EventLoop()
    {
        _eventLoopThread = new Thread(ProcessEventLoop)
        {
            Name = "EkkoJS Event Loop",
            IsBackground = false
        };
        _eventLoopThread.Start();
    }

    private void ProcessEventLoop()
    {
        while (!_cancellationTokenSource.Token.IsCancellationRequested)
        {
            try
            {
                if (_taskQueue.TryTake(out var task, 100, _cancellationTokenSource.Token))
                {
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
            _taskQueue.Add(task, _cancellationTokenSource.Token);
        }
        catch (InvalidOperationException)
        {
            // Queue is completing
        }
        catch (OperationCanceledException)
        {
            // Cancellation requested
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