namespace EkkoJS.Package.GUI;

/// <summary>
/// Interface for terminal UI components
/// </summary>
public interface ITerminalUI
{
    void Render();
    void Clear();
    void Refresh();
    void HandleInput(ConsoleKeyInfo key);
    bool IsActive { get; }
}