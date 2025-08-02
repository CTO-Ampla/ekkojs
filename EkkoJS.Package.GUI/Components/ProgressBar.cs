namespace EkkoJS.Package.GUI.Components;

public class ProgressBar
{
    private readonly int _width;
    private readonly char _fillChar;
    private readonly char _emptyChar;
    private double _value;
    private string _label;

    public ProgressBar(int width = 50, char fillChar = '█', char emptyChar = '░')
    {
        _width = width;
        _fillChar = fillChar;
        _emptyChar = emptyChar;
        _value = 0;
        _label = string.Empty;
    }

    public double Value
    {
        get => _value;
        set => _value = Math.Clamp(value, 0, 1);
    }

    public string Label
    {
        get => _label;
        set => _label = value ?? string.Empty;
    }

    public void Render(int x, int y)
    {
        Console.SetCursorPosition(x, y);
        
        var filled = (int)(_width * _value);
        var empty = _width - filled;
        
        Console.Write("[");
        Console.Write(new string(_fillChar, filled));
        Console.Write(new string(_emptyChar, empty));
        Console.Write($"] {_value:P0}");
        
        if (!string.IsNullOrEmpty(_label))
        {
            Console.Write($" - {_label}");
        }
        
        // Clear to end of line
        Console.Write(new string(' ', Console.WindowWidth - Console.CursorLeft));
    }

    public void Update(double value, string? label = null)
    {
        Value = value;
        if (label != null) Label = label;
    }
}