namespace EkkoJS.Package.GUI.Components;

public class Menu
{
    private readonly List<MenuItem> _items;
    private int _selectedIndex;
    private readonly string _title;
    private readonly ConsoleColor _selectedForeground;
    private readonly ConsoleColor _selectedBackground;

    public class MenuItem
    {
        public string Text { get; set; }
        public Action? Action { get; set; }
        public object? Tag { get; set; }
        public bool IsEnabled { get; set; } = true;

        public MenuItem(string text, Action? action = null)
        {
            Text = text;
            Action = action;
        }
    }

    public Menu(string title = "")
    {
        _title = title;
        _items = new List<MenuItem>();
        _selectedIndex = 0;
        _selectedForeground = ConsoleColor.Black;
        _selectedBackground = ConsoleColor.White;
    }

    public void AddItem(string text, Action? action = null)
    {
        _items.Add(new MenuItem(text, action));
    }

    public void AddItem(MenuItem item)
    {
        _items.Add(item);
    }

    public void AddSeparator()
    {
        _items.Add(new MenuItem("─────────────────") { IsEnabled = false });
    }

    public MenuItem? Show()
    {
        Console.CursorVisible = false;
        var startY = Console.CursorTop;
        
        try
        {
            while (true)
            {
                Render(0, startY);
                
                var key = Console.ReadKey(true);
                
                switch (key.Key)
                {
                    case ConsoleKey.UpArrow:
                        MovePrevious();
                        break;
                    
                    case ConsoleKey.DownArrow:
                        MoveNext();
                        break;
                    
                    case ConsoleKey.Enter:
                        var selected = _items[_selectedIndex];
                        if (selected.IsEnabled)
                        {
                            Console.SetCursorPosition(0, startY + _items.Count + 2);
                            selected.Action?.Invoke();
                            return selected;
                        }
                        break;
                    
                    case ConsoleKey.Escape:
                        Console.SetCursorPosition(0, startY + _items.Count + 2);
                        return null;
                }
            }
        }
        finally
        {
            Console.CursorVisible = true;
            Console.ResetColor();
        }
    }

    private void MovePrevious()
    {
        do
        {
            _selectedIndex--;
            if (_selectedIndex < 0)
                _selectedIndex = _items.Count - 1;
        } while (!_items[_selectedIndex].IsEnabled);
    }

    private void MoveNext()
    {
        do
        {
            _selectedIndex++;
            if (_selectedIndex >= _items.Count)
                _selectedIndex = 0;
        } while (!_items[_selectedIndex].IsEnabled);
    }

    private void Render(int x, int y)
    {
        Console.SetCursorPosition(x, y);
        
        if (!string.IsNullOrEmpty(_title))
        {
            Console.WriteLine(_title);
            Console.WriteLine(new string('═', _title.Length));
        }
        
        for (int i = 0; i < _items.Count; i++)
        {
            var item = _items[i];
            Console.SetCursorPosition(x, Console.CursorTop);
            
            if (!item.IsEnabled)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine($"  {item.Text}");
                Console.ResetColor();
            }
            else if (i == _selectedIndex)
            {
                Console.ForegroundColor = _selectedForeground;
                Console.BackgroundColor = _selectedBackground;
                Console.WriteLine($"► {item.Text} ");
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine($"  {item.Text} ");
            }
        }
    }
}