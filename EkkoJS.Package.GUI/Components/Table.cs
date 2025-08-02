namespace EkkoJS.Package.GUI.Components;

public class Table
{
    private readonly List<string> _headers;
    private readonly List<List<string>> _rows;
    private readonly List<int> _columnWidths;
    private TableStyle _style;

    public enum TableStyle
    {
        Simple,
        Bordered,
        Compact,
        Markdown
    }

    public Table(params string[] headers)
    {
        _headers = headers.ToList();
        _rows = new List<List<string>>();
        _columnWidths = new List<int>();
        _style = TableStyle.Simple;
        
        UpdateColumnWidths();
    }

    public Table WithStyle(TableStyle style)
    {
        var newTable = new Table(_headers.ToArray());
        foreach (var row in _rows)
        {
            newTable.AddRow(row.ToArray());
        }
        newTable._style = style;
        return newTable;
    }

    public void AddRow(params string[] values)
    {
        var row = values.Select(v => v ?? string.Empty).ToList();
        
        // Pad with empty strings if needed
        while (row.Count < _headers.Count)
        {
            row.Add(string.Empty);
        }
        
        _rows.Add(row);
        UpdateColumnWidths();
    }

    private void UpdateColumnWidths()
    {
        _columnWidths.Clear();
        
        for (int i = 0; i < _headers.Count; i++)
        {
            var maxWidth = _headers[i].Length;
            
            foreach (var row in _rows)
            {
                if (i < row.Count)
                {
                    maxWidth = Math.Max(maxWidth, row[i].Length);
                }
            }
            
            _columnWidths.Add(maxWidth);
        }
    }

    public void Render()
    {
        switch (_style)
        {
            case TableStyle.Simple:
                RenderSimple();
                break;
            case TableStyle.Bordered:
                RenderBordered();
                break;
            case TableStyle.Compact:
                RenderCompact();
                break;
            case TableStyle.Markdown:
                RenderMarkdown();
                break;
        }
    }

    private void RenderSimple()
    {
        // Headers
        for (int i = 0; i < _headers.Count; i++)
        {
            Console.Write(_headers[i].PadRight(_columnWidths[i] + 2));
        }
        Console.WriteLine();
        
        // Separator
        for (int i = 0; i < _headers.Count; i++)
        {
            Console.Write(new string('-', _columnWidths[i]) + "  ");
        }
        Console.WriteLine();
        
        // Rows
        foreach (var row in _rows)
        {
            for (int i = 0; i < row.Count && i < _columnWidths.Count; i++)
            {
                Console.Write(row[i].PadRight(_columnWidths[i] + 2));
            }
            Console.WriteLine();
        }
    }

    private void RenderBordered()
    {
        var totalWidth = _columnWidths.Sum() + (_columnWidths.Count * 3) + 1;
        
        // Top border
        Console.WriteLine("┌" + new string('─', totalWidth - 2) + "┐");
        
        // Headers
        Console.Write("│");
        for (int i = 0; i < _headers.Count; i++)
        {
            Console.Write($" {_headers[i].PadRight(_columnWidths[i])} │");
        }
        Console.WriteLine();
        
        // Header separator
        Console.Write("├");
        for (int i = 0; i < _headers.Count; i++)
        {
            Console.Write(new string('─', _columnWidths[i] + 2));
            Console.Write(i < _headers.Count - 1 ? "┼" : "┤");
        }
        Console.WriteLine();
        
        // Rows
        foreach (var row in _rows)
        {
            Console.Write("│");
            for (int i = 0; i < row.Count && i < _columnWidths.Count; i++)
            {
                Console.Write($" {row[i].PadRight(_columnWidths[i])} │");
            }
            Console.WriteLine();
        }
        
        // Bottom border
        Console.WriteLine("└" + new string('─', totalWidth - 2) + "┘");
    }

    private void RenderCompact()
    {
        // Headers with minimal spacing
        for (int i = 0; i < _headers.Count; i++)
        {
            Console.Write(_headers[i].PadRight(_columnWidths[i] + 1));
        }
        Console.WriteLine();
        
        // Rows
        foreach (var row in _rows)
        {
            for (int i = 0; i < row.Count && i < _columnWidths.Count; i++)
            {
                Console.Write(row[i].PadRight(_columnWidths[i] + 1));
            }
            Console.WriteLine();
        }
    }

    private void RenderMarkdown()
    {
        // Headers
        Console.Write("| ");
        for (int i = 0; i < _headers.Count; i++)
        {
            Console.Write(_headers[i].PadRight(_columnWidths[i]) + " | ");
        }
        Console.WriteLine();
        
        // Separator
        Console.Write("| ");
        for (int i = 0; i < _headers.Count; i++)
        {
            Console.Write(new string('-', _columnWidths[i]) + " | ");
        }
        Console.WriteLine();
        
        // Rows
        foreach (var row in _rows)
        {
            Console.Write("| ");
            for (int i = 0; i < row.Count && i < _columnWidths.Count; i++)
            {
                Console.Write(row[i].PadRight(_columnWidths[i]) + " | ");
            }
            Console.WriteLine();
        }
    }
}