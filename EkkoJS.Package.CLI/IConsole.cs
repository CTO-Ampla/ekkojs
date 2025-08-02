namespace EkkoJS.Package.CLI;

/// <summary>
/// Abstraction for console operations to enable testing and different console implementations
/// </summary>
public interface IConsole
{
    // Output
    void Write(string? text);
    void WriteLine(string? text = null);
    void WriteError(string? text);
    void WriteErrorLine(string? text = null);
    
    // Input
    string? ReadLine();
    ConsoleKeyInfo ReadKey(bool intercept = false);
    
    // Console properties
    int CursorLeft { get; set; }
    int CursorTop { get; set; }
    ConsoleColor ForegroundColor { get; set; }
    ConsoleColor BackgroundColor { get; set; }
    int WindowWidth { get; }
    int WindowHeight { get; }
    int BufferWidth { get; }
    int BufferHeight { get; }
    
    // Console operations
    void Clear();
    void SetCursorPosition(int left, int top);
    void ResetColor();
    
    // Advanced features
    bool IsInputRedirected { get; }
    bool IsOutputRedirected { get; }
    bool IsErrorRedirected { get; }
    bool KeyAvailable { get; }
}