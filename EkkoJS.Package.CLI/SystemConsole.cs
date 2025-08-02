namespace EkkoJS.Package.CLI;

/// <summary>
/// Default implementation using System.Console
/// </summary>
public class SystemConsole : IConsole
{
    public void Write(string? text) => Console.Write(text ?? string.Empty);
    public void WriteLine(string? text = null) => Console.WriteLine(text ?? string.Empty);
    public void WriteError(string? text) => Console.Error.Write(text ?? string.Empty);
    public void WriteErrorLine(string? text = null) => Console.Error.WriteLine(text ?? string.Empty);
    
    public string? ReadLine() => Console.ReadLine();
    public ConsoleKeyInfo ReadKey(bool intercept = false) => Console.ReadKey(intercept);
    
    public int CursorLeft 
    { 
        get => Console.CursorLeft; 
        set => Console.CursorLeft = value; 
    }
    
    public int CursorTop 
    { 
        get => Console.CursorTop; 
        set => Console.CursorTop = value; 
    }
    
    public ConsoleColor ForegroundColor 
    { 
        get => Console.ForegroundColor; 
        set => Console.ForegroundColor = value; 
    }
    
    public ConsoleColor BackgroundColor 
    { 
        get => Console.BackgroundColor; 
        set => Console.BackgroundColor = value; 
    }
    
    public int WindowWidth => Console.WindowWidth;
    public int WindowHeight => Console.WindowHeight;
    public int BufferWidth => Console.BufferWidth;
    public int BufferHeight => Console.BufferHeight;
    
    public void Clear() => Console.Clear();
    public void SetCursorPosition(int left, int top) => Console.SetCursorPosition(left, top);
    public void ResetColor() => Console.ResetColor();
    
    public bool IsInputRedirected => Console.IsInputRedirected;
    public bool IsOutputRedirected => Console.IsOutputRedirected;
    public bool IsErrorRedirected => Console.IsErrorRedirected;
    public bool KeyAvailable => Console.KeyAvailable;
}