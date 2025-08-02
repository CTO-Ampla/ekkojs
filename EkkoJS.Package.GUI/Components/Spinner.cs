namespace EkkoJS.Package.GUI.Components;

public class Spinner : IDisposable
{
    private readonly string[] _frames;
    private readonly int _delay;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private Task? _spinTask;
    private int _currentFrame;
    private readonly int _x;
    private readonly int _y;
    private string _message;

    public static class Styles
    {
        public static readonly string[] Dots = { "⠋", "⠙", "⠹", "⠸", "⠼", "⠴", "⠦", "⠧", "⠇", "⠏" };
        public static readonly string[] Line = { "-", "\\", "|", "/" };
        public static readonly string[] Star = { "✶", "✸", "✹", "✺", "✹", "✸" };
        public static readonly string[] Square = { "◰", "◳", "◲", "◱" };
        public static readonly string[] Circle = { "◐", "◓", "◑", "◒" };
        public static readonly string[] Arrow = { "←", "↖", "↑", "↗", "→", "↘", "↓", "↙" };
        public static readonly string[] Bounce = { "⠁", "⠂", "⠄", "⠂" };
        public static readonly string[] Box = { "▖", "▘", "▝", "▗" };
    }

    public Spinner(string message = "", string[]? frames = null, int delay = 100)
    {
        _frames = frames ?? Styles.Dots;
        _delay = delay;
        _message = message;
        _cancellationTokenSource = new CancellationTokenSource();
        _currentFrame = 0;
        _x = Console.CursorLeft;
        _y = Console.CursorTop;
    }

    public string Message
    {
        get => _message;
        set => _message = value ?? string.Empty;
    }

    public void Start()
    {
        Console.CursorVisible = false;
        
        _spinTask = Task.Run(async () =>
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                Render();
                await Task.Delay(_delay, _cancellationTokenSource.Token);
                _currentFrame = (_currentFrame + 1) % _frames.Length;
            }
        }, _cancellationTokenSource.Token);
    }

    public void Stop(string? finalMessage = null)
    {
        _cancellationTokenSource.Cancel();
        
        try
        {
            _spinTask?.Wait(100);
        }
        catch (AggregateException) { }
        
        Console.SetCursorPosition(_x, _y);
        
        if (finalMessage != null)
        {
            Console.Write(finalMessage);
        }
        
        // Clear to end of line
        Console.Write(new string(' ', Console.WindowWidth - Console.CursorLeft));
        Console.WriteLine();
        Console.CursorVisible = true;
    }

    private void Render()
    {
        lock (this)
        {
            Console.SetCursorPosition(_x, _y);
            Console.Write($"{_frames[_currentFrame]} {_message}");
            
            // Clear to end of line
            var remaining = Console.WindowWidth - Console.CursorLeft - 1;
            if (remaining > 0)
            {
                Console.Write(new string(' ', remaining));
            }
        }
    }

    public void Dispose()
    {
        if (!_cancellationTokenSource.IsCancellationRequested)
        {
            Stop();
        }
        _cancellationTokenSource.Dispose();
    }

    public static async Task RunWithSpinner(string message, Func<Task> action, string[]? frames = null)
    {
        using var spinner = new Spinner(message, frames);
        spinner.Start();
        
        try
        {
            await action();
            spinner.Stop($"✓ {message}");
        }
        catch
        {
            spinner.Stop($"✗ {message}");
            throw;
        }
    }
}