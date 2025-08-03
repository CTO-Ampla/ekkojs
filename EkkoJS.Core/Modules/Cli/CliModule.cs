using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace EkkoJS.Core.Modules.Cli
{
    // Supporting data structures for output formatting
    public class TableOptions
    {
        public string BorderStyle { get; set; } = "single";
        public string? HeaderColor { get; set; }
        public string? BorderColor { get; set; }
        public int[]? ColWidths { get; set; }
        public string[]? ColAligns { get; set; }
        public bool WordWrap { get; set; } = false;
    }

    public class BoxOptions
    {
        public string BorderStyle { get; set; } = "single";
        public string? BorderColor { get; set; }
        public string? BackgroundColor { get; set; }
        public string Align { get; set; } = "left";
        public int Padding { get; set; } = 1;
        public int MinWidth { get; set; } = 0;
        public string? Title { get; set; }
    }

    public class TreeOptions
    {
        public string TreeChars { get; set; } = "│├└";
        public string ExpandedIcon { get; set; } = "▼";
        public string CollapsedIcon { get; set; } = "▶";
        public string LeafIcon { get; set; } = "•";
    }

    public class TreeNode
    {
        public string Label { get; set; } = "";
        public string? Icon { get; set; }
        public List<TreeNode> Children { get; set; } = new List<TreeNode>();
        public bool Expanded { get; set; } = true;
    }

    public class BorderChars
    {
        public char TopLeft { get; set; }
        public char TopRight { get; set; }
        public char BottomLeft { get; set; }
        public char BottomRight { get; set; }
        public char Horizontal { get; set; }
        public char Vertical { get; set; }
        public char TopJunction { get; set; }
        public char BottomJunction { get; set; }
        public char LeftJunction { get; set; }
        public char RightJunction { get; set; }
        public char MiddleJunction { get; set; }
    }

    public class CliModule
    {
        private readonly AnsiBuilder _ansiBuilder = new();
        private readonly Terminal _terminal = new();

        // Basic colors
        public string Red(string text) => _ansiBuilder.SetForeground(AnsiColor.Red).Build(text);
        public string Green(string text) => _ansiBuilder.SetForeground(AnsiColor.Green).Build(text);
        public string Yellow(string text) => _ansiBuilder.SetForeground(AnsiColor.Yellow).Build(text);
        public string Blue(string text) => _ansiBuilder.SetForeground(AnsiColor.Blue).Build(text);
        public string Magenta(string text) => _ansiBuilder.SetForeground(AnsiColor.Magenta).Build(text);
        public string Cyan(string text) => _ansiBuilder.SetForeground(AnsiColor.Cyan).Build(text);
        public string White(string text) => _ansiBuilder.SetForeground(AnsiColor.White).Build(text);
        public string Gray(string text) => _ansiBuilder.SetForeground(AnsiColor.Gray).Build(text);
        public string Black(string text) => _ansiBuilder.SetForeground(AnsiColor.Black).Build(text);

        // Bright colors
        public string BrightRed(string text) => _ansiBuilder.SetForeground(AnsiColor.BrightRed).Build(text);
        public string BrightGreen(string text) => _ansiBuilder.SetForeground(AnsiColor.BrightGreen).Build(text);
        public string BrightYellow(string text) => _ansiBuilder.SetForeground(AnsiColor.BrightYellow).Build(text);
        public string BrightBlue(string text) => _ansiBuilder.SetForeground(AnsiColor.BrightBlue).Build(text);
        public string BrightMagenta(string text) => _ansiBuilder.SetForeground(AnsiColor.BrightMagenta).Build(text);
        public string BrightCyan(string text) => _ansiBuilder.SetForeground(AnsiColor.BrightCyan).Build(text);
        public string BrightWhite(string text) => _ansiBuilder.SetForeground(AnsiColor.BrightWhite).Build(text);

        // Background colors
        public string BgRed(string text) => _ansiBuilder.SetBackground(AnsiColor.Red).Build(text);
        public string BgGreen(string text) => _ansiBuilder.SetBackground(AnsiColor.Green).Build(text);
        public string BgYellow(string text) => _ansiBuilder.SetBackground(AnsiColor.Yellow).Build(text);
        public string BgBlue(string text) => _ansiBuilder.SetBackground(AnsiColor.Blue).Build(text);
        public string BgMagenta(string text) => _ansiBuilder.SetBackground(AnsiColor.Magenta).Build(text);
        public string BgCyan(string text) => _ansiBuilder.SetBackground(AnsiColor.Cyan).Build(text);
        public string BgWhite(string text) => _ansiBuilder.SetBackground(AnsiColor.White).Build(text);
        public string BgGray(string text) => _ansiBuilder.SetBackground(AnsiColor.Gray).Build(text);
        public string BgBlack(string text) => _ansiBuilder.SetBackground(AnsiColor.Black).Build(text);

        // Styles
        public string Bold(string text) => _ansiBuilder.SetStyle(AnsiStyle.Bold).Build(text);
        public string Italic(string text) => _ansiBuilder.SetStyle(AnsiStyle.Italic).Build(text);
        public string Underline(string text) => _ansiBuilder.SetStyle(AnsiStyle.Underline).Build(text);
        public string Strikethrough(string text) => _ansiBuilder.SetStyle(AnsiStyle.Strikethrough).Build(text);
        public string Inverse(string text) => _ansiBuilder.SetStyle(AnsiStyle.Inverse).Build(text);
        public string Dim(string text) => _ansiBuilder.SetStyle(AnsiStyle.Dim).Build(text);
        public string Hidden(string text) => _ansiBuilder.SetStyle(AnsiStyle.Hidden).Build(text);

        // 256 color support
        public Func<string, string> Color(int code)
        {
            if (code < 0 || code > 255)
                throw new ArgumentOutOfRangeException(nameof(code), "Color code must be between 0 and 255");
                
            return (string text) => $"\x1b[38;5;{code}m{text}\x1b[0m";
        }

        public Func<string, string> BgColor(int code)
        {
            if (code < 0 || code > 255)
                throw new ArgumentOutOfRangeException(nameof(code), "Color code must be between 0 and 255");
                
            return (string text) => $"\x1b[48;5;{code}m{text}\x1b[0m";
        }

        // RGB support (true color)
        public Func<string, string> Rgb(int r, int g, int b)
        {
            ValidateRgb(r, g, b);
            return (string text) => $"\x1b[38;2;{r};{g};{b}m{text}\x1b[0m";
        }

        public Func<string, string> BgRgb(int r, int g, int b)
        {
            ValidateRgb(r, g, b);
            return (string text) => $"\x1b[48;2;{r};{g};{b}m{text}\x1b[0m";
        }

        // Hex color support
        public Func<string, string> Hex(string hex)
        {
            var (r, g, b) = ParseHex(hex);
            return Rgb(r, g, b);
        }

        public Func<string, string> BgHex(string hex)
        {
            var (r, g, b) = ParseHex(hex);
            return BgRgb(r, g, b);
        }

        // Reset
        public string Reset(string text) => $"\x1b[0m{text}\x1b[0m";

        // Strip ANSI codes
        public string StripAnsi(string text)
        {
            return System.Text.RegularExpressions.Regex.Replace(text, @"\x1b\[[0-9;]*m", "");
        }

        // Screen operations
        public void Clear()
        {
            Console.Clear();
            Console.SetCursorPosition(0, 0);
        }

        public void ClearScreen()
        {
            Console.Write("\x1b[2J");
        }

        public void ClearLine()
        {
            Console.Write("\x1b[2K");
        }

        public void ClearLineLeft()
        {
            Console.Write("\x1b[1K");
        }

        public void ClearLineRight()
        {
            Console.Write("\x1b[0K");
        }

        public void ClearDown()
        {
            Console.Write("\x1b[0J");
        }

        public void ClearUp()
        {
            Console.Write("\x1b[1J");
        }

        // Screen size
        public object GetScreenSize()
        {
            return new
            {
                width = Console.WindowWidth,
                height = Console.WindowHeight
            };
        }

        // Cursor operations
        public void CursorTo(int x, int y)
        {
            Console.SetCursorPosition(x, y);
        }

        public void CursorToColumn(int x)
        {
            Console.CursorLeft = x;
        }

        public void CursorUp(int n = 1)
        {
            Console.Write($"\x1b[{n}A");
        }

        public void CursorDown(int n = 1)
        {
            Console.Write($"\x1b[{n}B");
        }

        public void CursorForward(int n = 1)
        {
            Console.Write($"\x1b[{n}C");
        }

        public void CursorBackward(int n = 1)
        {
            Console.Write($"\x1b[{n}D");
        }

        public void CursorNextLine(int n = 1)
        {
            Console.Write($"\x1b[{n}E");
        }

        public void CursorPrevLine(int n = 1)
        {
            Console.Write($"\x1b[{n}F");
        }

        public void SaveCursor()
        {
            Console.Write("\x1b[s");
        }

        public void RestoreCursor()
        {
            Console.Write("\x1b[u");
        }

        public void HideCursor()
        {
            Console.CursorVisible = false;
        }

        public void ShowCursor()
        {
            Console.CursorVisible = true;
        }

        // Terminal capabilities
        public object GetCapabilities()
        {
            return new
            {
                colors = _terminal.GetColorSupport(),
                unicode = _terminal.SupportsUnicode(),
                interactive = !Console.IsOutputRedirected,
                width = Console.WindowWidth,
                height = Console.WindowHeight,
                isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows),
                isMac = RuntimeInformation.IsOSPlatform(OSPlatform.OSX),
                isLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux),
                program = Environment.GetEnvironmentVariable("TERM") ?? "unknown"
            };
        }

        public bool SupportsColor()
        {
            return _terminal.GetColorSupport() > 0;
        }

        public bool SupportsUnicode()
        {
            return _terminal.SupportsUnicode();
        }

        // Basic output
        public void Write(string text)
        {
            Console.Write(text);
        }

        public void WriteLine(string text = "")
        {
            Console.WriteLine(text);
        }

        // String utilities
        public int StringWidth(string text)
        {
            if (string.IsNullOrEmpty(text))
                return 0;

            // Strip ANSI codes first
            var strippedText = StripAnsi(text);
            var width = 0;

            foreach (char c in strippedText)
            {
                // Handle wide characters (CJK, etc.)
                if (IsWideCharacter(c))
                {
                    width += 2;
                }
                else if (char.IsControl(c))
                {
                    // Control characters don't add width
                    continue;
                }
                else
                {
                    width += 1;
                }
            }

            return width;
        }

        public string Truncate(string text, int maxWidth, string position = "end")
        {
            if (string.IsNullOrEmpty(text) || maxWidth <= 0)
                return "";

            var currentWidth = StringWidth(text);
            if (currentWidth <= maxWidth)
                return text;

            const string ellipsis = "...";
            var ellipsisWidth = StringWidth(ellipsis);

            if (maxWidth <= ellipsisWidth)
                return ellipsis.Substring(0, maxWidth);

            var availableWidth = maxWidth - ellipsisWidth;

            switch (position.ToLower())
            {
                case "start":
                    return ellipsis + TruncateFromEnd(text, availableWidth);
                case "middle":
                    var halfWidth = availableWidth / 2;
                    var startPart = TruncateFromStart(text, halfWidth);
                    var endPart = TruncateFromEnd(text, availableWidth - StringWidth(startPart));
                    return startPart + ellipsis + endPart;
                default: // "end"
                    return TruncateFromStart(text, availableWidth) + ellipsis;
            }
        }

        public string Wrap(string text, int width, int indent = 0, bool trim = false)
        {
            if (string.IsNullOrEmpty(text) || width <= 0)
                return text;

            var lines = new List<string>();
            var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var currentLine = new StringBuilder();
            var indentStr = new string(' ', indent);

            foreach (var word in words)
            {
                var wordWidth = StringWidth(word);
                var currentLineWidth = StringWidth(currentLine.ToString());
                var spaceWidth = currentLine.Length > 0 ? 1 : 0;

                if (currentLineWidth + spaceWidth + wordWidth <= width - indent)
                {
                    if (currentLine.Length > 0)
                        currentLine.Append(' ');
                    currentLine.Append(word);
                }
                else
                {
                    if (currentLine.Length > 0)
                    {
                        var line = indentStr + currentLine.ToString();
                        lines.Add(trim ? line.Trim() : line);
                        currentLine.Clear();
                    }
                    currentLine.Append(word);
                }
            }

            if (currentLine.Length > 0)
            {
                var line = indentStr + currentLine.ToString();
                lines.Add(trim ? line.Trim() : line);
            }

            return string.Join("\n", lines);
        }

        public string Pad(string text, int width)
        {
            if (string.IsNullOrEmpty(text))
                text = "";

            var textWidth = StringWidth(text);
            if (textWidth >= width)
                return text;

            var totalPadding = width - textWidth;
            var leftPadding = totalPadding / 2;
            var rightPadding = totalPadding - leftPadding;

            return new string(' ', leftPadding) + text + new string(' ', rightPadding);
        }

        public string PadLeft(string text, int width)
        {
            if (string.IsNullOrEmpty(text))
                text = "";

            var textWidth = StringWidth(text);
            if (textWidth >= width)
                return text;

            var padding = width - textWidth;
            return new string(' ', padding) + text;
        }

        public string PadRight(string text, int width)
        {
            if (string.IsNullOrEmpty(text))
                text = "";

            var textWidth = StringWidth(text);
            if (textWidth >= width)
                return text;

            var padding = width - textWidth;
            return text + new string(' ', padding);
        }

        public string Center(string text, int width)
        {
            return Pad(text, width);
        }

        public string Right(string text, int width)
        {
            return PadLeft(text, width);
        }

        // Helper methods for string utilities
        private bool IsWideCharacter(char c)
        {
            // Simplified wide character detection
            // This covers most CJK characters
            var code = (int)c;
            return (code >= 0x1100 && code <= 0x115F) ||  // Hangul Jamo
                   (code >= 0x2E80 && code <= 0x2EFF) ||  // CJK Radicals Supplement
                   (code >= 0x2F00 && code <= 0x2FDF) ||  // Kangxi Radicals
                   (code >= 0x3000 && code <= 0x303F) ||  // CJK Symbols and Punctuation
                   (code >= 0x3040 && code <= 0x309F) ||  // Hiragana
                   (code >= 0x30A0 && code <= 0x30FF) ||  // Katakana
                   (code >= 0x3100 && code <= 0x312F) ||  // Bopomofo
                   (code >= 0x3130 && code <= 0x318F) ||  // Hangul Compatibility Jamo
                   (code >= 0x3190 && code <= 0x319F) ||  // Kanbun
                   (code >= 0x31A0 && code <= 0x31BF) ||  // Bopomofo Extended
                   (code >= 0x31C0 && code <= 0x31EF) ||  // CJK Strokes
                   (code >= 0x31F0 && code <= 0x31FF) ||  // Katakana Phonetic Extensions
                   (code >= 0x3200 && code <= 0x32FF) ||  // Enclosed CJK Letters and Months
                   (code >= 0x3300 && code <= 0x33FF) ||  // CJK Compatibility
                   (code >= 0x3400 && code <= 0x4DBF) ||  // CJK Unified Ideographs Extension A
                   (code >= 0x4E00 && code <= 0x9FFF) ||  // CJK Unified Ideographs
                   (code >= 0xA000 && code <= 0xA48F) ||  // Yi Syllables
                   (code >= 0xA490 && code <= 0xA4CF) ||  // Yi Radicals
                   (code >= 0xAC00 && code <= 0xD7AF) ||  // Hangul Syllables
                   (code >= 0xF900 && code <= 0xFAFF) ||  // CJK Compatibility Ideographs
                   (code >= 0xFE10 && code <= 0xFE1F) ||  // Vertical Forms
                   (code >= 0xFE30 && code <= 0xFE4F) ||  // CJK Compatibility Forms
                   (code >= 0xFE50 && code <= 0xFE6F) ||  // Small Form Variants
                   (code >= 0xFF00 && code <= 0xFFEF);    // Halfwidth and Fullwidth Forms
        }

        private string TruncateFromStart(string text, int maxWidth)
        {
            var result = new StringBuilder();
            var currentWidth = 0;

            foreach (char c in text)
            {
                var charWidth = IsWideCharacter(c) ? 2 : 1;
                if (currentWidth + charWidth > maxWidth)
                    break;

                result.Append(c);
                currentWidth += charWidth;
            }

            return result.ToString();
        }

        private string TruncateFromEnd(string text, int maxWidth)
        {
            var chars = text.ToCharArray();
            var result = new StringBuilder();
            var currentWidth = 0;

            for (int i = chars.Length - 1; i >= 0; i--)
            {
                var charWidth = IsWideCharacter(chars[i]) ? 2 : 1;
                if (currentWidth + charWidth > maxWidth)
                    break;

                result.Insert(0, chars[i]);
                currentWidth += charWidth;
            }

            return result.ToString();
        }

        // Basic input
        public async System.Threading.Tasks.Task<string> Input(string prompt)
        {
            Console.Write(prompt);
            return await System.Threading.Tasks.Task.FromResult(Console.ReadLine() ?? "");
        }

        // Password input
        public async System.Threading.Tasks.Task<string> Password(string prompt, char mask = '\0')
        {
            Console.Write(prompt);
            var password = new StringBuilder();
            
            while (true)
            {
                var key = Console.ReadKey(true);
                
                if (key.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    break;
                }
                else if (key.Key == ConsoleKey.Backspace && password.Length > 0)
                {
                    password.Length--;
                    if (mask != '\0')
                    {
                        Console.Write("\b \b");
                    }
                }
                else if (!char.IsControl(key.KeyChar))
                {
                    password.Append(key.KeyChar);
                    if (mask != '\0')
                    {
                        Console.Write(mask);
                    }
                }
            }
            
            return await System.Threading.Tasks.Task.FromResult(password.ToString());
        }

        // Confirmation
        public async System.Threading.Tasks.Task<bool> Confirm(string prompt, bool defaultValue = true)
        {
            var suffix = defaultValue ? " [Y/n] " : " [y/N] ";
            Console.Write(prompt + suffix);
            
            var response = Console.ReadLine()?.Trim().ToLowerInvariant();
            
            if (string.IsNullOrEmpty(response))
            {
                return await System.Threading.Tasks.Task.FromResult(defaultValue);
            }
            
            return await System.Threading.Tasks.Task.FromResult(
                response == "y" || response == "yes"
            );
        }

        // Simple select menu
        public async System.Threading.Tasks.Task<string> Select(string prompt, string[] choices)
        {
            if (choices == null || choices.Length == 0)
                throw new ArgumentException("Choices cannot be null or empty", nameof(choices));

            var currentIndex = 0;
            const int maxVisibleItems = 8;
            int scrollOffset = 0;
            
            // Always use screen clearing to avoid cursor position issues
            bool useClearScreen = true;
            
            // Save cursor position BEFORE writing anything
            int menuStartTop = Console.CursorTop;
            
            // Now write the prompt and instructions
            Console.WriteLine(prompt);
            Console.WriteLine(Dim("Use arrow keys to navigate, Enter to select"));
            Console.WriteLine();
            
            // Helper function to render the menu
            Action renderMenu = () =>
            {
                if (useClearScreen)
                {
                    Console.Clear();
                    Console.WriteLine(prompt);
                    Console.WriteLine(Dim("Use arrow keys to navigate, Enter to select"));
                    Console.WriteLine();
                }
                else
                {
                    // Move cursor to start of menu and clear each line
                    Console.SetCursorPosition(0, menuStartTop);
                    
                    // Clear the entire menu area first
                    int linesToClear = 3 + choices.Length; // prompt + instructions + blank line + all choices
                    for (int i = 0; i < linesToClear; i++)
                    {
                        Console.SetCursorPosition(0, menuStartTop + i);
                        Console.Write(new string(' ', Console.WindowWidth - 1));
                    }
                    
                    // Now reposition to write the menu
                    Console.SetCursorPosition(0, menuStartTop);
                    Console.WriteLine(prompt);
                    Console.WriteLine(Dim("Use arrow keys to navigate, Enter to select"));
                    Console.WriteLine();
                }
                
                // Calculate visible window
                var visibleCount = Math.Min(maxVisibleItems, choices.Length);
                
                // Adjust scroll offset based on current selection
                if (currentIndex < scrollOffset)
                {
                    scrollOffset = currentIndex;
                }
                else if (currentIndex >= scrollOffset + visibleCount)
                {
                    scrollOffset = currentIndex - visibleCount + 1;
                }
                
                // Show scroll indicators
                if (scrollOffset > 0)
                {
                    Console.WriteLine(Dim("  ↑ More above"));
                }
                
                // Render visible items
                for (int i = scrollOffset; i < Math.Min(scrollOffset + visibleCount, choices.Length); i++)
                {
                    if (i == currentIndex)
                    {
                        Console.WriteLine(Cyan($"❯ {choices[i]}"));
                    }
                    else
                    {
                        Console.WriteLine($"  {choices[i]}");
                    }
                }
                
                // Show scroll indicator at bottom
                if (scrollOffset + visibleCount < choices.Length)
                {
                    Console.WriteLine(Dim("  ↓ More below"));
                }
            };
            
            // Initial render
            Console.CursorVisible = false;
            renderMenu();
            
            // Handle keyboard input
            while (true)
            {
                var key = Console.ReadKey(true);
                
                switch (key.Key)
                {
                    case ConsoleKey.UpArrow:
                        currentIndex = currentIndex > 0 ? currentIndex - 1 : choices.Length - 1;
                        renderMenu();
                        break;
                        
                    case ConsoleKey.DownArrow:
                        currentIndex = (currentIndex + 1) % choices.Length;
                        renderMenu();
                        break;
                        
                    case ConsoleKey.Enter:
                        if (!useClearScreen)
                        {
                            Console.CursorVisible = true;
                        }
                        Console.WriteLine(); // Move to next line after selection
                        return await System.Threading.Tasks.Task.FromResult(choices[currentIndex]);
                        
                    case ConsoleKey.Escape:
                        if (!useClearScreen)
                        {
                            Console.CursorVisible = true;
                        }
                        throw new OperationCanceledException("Selection cancelled");
                }
            }
        }

        // Advanced input methods
        public async System.Threading.Tasks.Task<string[]> MultiSelect(string prompt, string[] choices, string[]? defaultSelected = null)
        {
            if (choices == null || choices.Length == 0)
                throw new ArgumentException("Choices cannot be null or empty", nameof(choices));

            Console.WriteLine(prompt);
            Console.WriteLine(Dim("Use spacebar to toggle, Enter to confirm, arrow keys to navigate"));
            
            var selected = new bool[choices.Length];
            var currentIndex = 0;

            // Set default selections
            if (defaultSelected != null)
            {
                for (int i = 0; i < choices.Length; i++)
                {
                    if (defaultSelected.Contains(choices[i]))
                        selected[i] = true;
                }
            }

            // Save cursor position for menu rendering
            int menuStartTop = Console.CursorTop;
            const int maxVisibleItems = 8;
            int scrollOffset = 0;
            int lastRenderedLines = 0;
            
            // Helper function to render the menu with scrolling
            Action renderMenu = () =>
            {
                // For large lists, use screen clearing for clean display
                if (choices.Length > maxVisibleItems)
                {
                    Console.Clear();
                    Console.WriteLine(prompt);
                    Console.WriteLine(Dim("Use spacebar to toggle, Enter to confirm, arrow keys to navigate"));
                    Console.WriteLine();
                    
                    // Calculate visible window
                    var visibleCount = Math.Min(maxVisibleItems, choices.Length);
                    
                    // Adjust scroll offset based on current selection
                    if (currentIndex < scrollOffset)
                    {
                        scrollOffset = currentIndex;
                    }
                    else if (currentIndex >= scrollOffset + visibleCount)
                    {
                        scrollOffset = currentIndex - visibleCount + 1;
                    }
                    
                    // Ensure scroll offset is within bounds
                    scrollOffset = Math.Max(0, Math.Min(scrollOffset, choices.Length - visibleCount));
                    
                    // Show scroll indicator at top if there are items above
                    if (scrollOffset > 0)
                    {
                        Console.WriteLine(Dim("  ↑ More items above"));
                    }
                    
                    // Render visible items
                    for (int i = 0; i < visibleCount; i++)
                    {
                        var choiceIndex = scrollOffset + i;
                        var prefix = choiceIndex == currentIndex ? ">" : " ";
                        var checkbox = selected[choiceIndex] ? "✓" : " ";
                        string coloredChoice;
                        if (choiceIndex == currentIndex)
                            coloredChoice = Cyan(choices[choiceIndex]);
                        else if (selected[choiceIndex])
                            coloredChoice = Green(choices[choiceIndex]);
                        else
                            coloredChoice = Reset(choices[choiceIndex]);
                        
                        Console.WriteLine($"{prefix} [{checkbox}] {coloredChoice}");
                    }
                    
                    // Show scroll indicator at bottom if there are items below
                    if (scrollOffset + visibleCount < choices.Length)
                    {
                        Console.WriteLine(Dim("  ↓ More items below"));
                    }
                    
                    // Show current position indicator
                    Console.WriteLine(Dim($"  ({currentIndex + 1}/{choices.Length})"));
                }
                else
                {
                    // For small lists, use the original in-place approach
                    try
                    {
                        Console.SetCursorPosition(0, menuStartTop);
                        for (int i = 0; i < choices.Length; i++)
                        {
                            var prefix = i == currentIndex ? ">" : " ";
                            var checkbox = selected[i] ? "✓" : " ";
                            string coloredChoice;
                            if (i == currentIndex)
                                coloredChoice = Cyan(choices[i]);
                            else if (selected[i])
                                coloredChoice = Green(choices[i]);
                            else
                                coloredChoice = Reset(choices[i]);
                            
                            var line = $"{prefix} [{checkbox}] {coloredChoice}";
                            Console.Write(line);
                            Console.Write("\u001b[K");
                            Console.WriteLine();
                        }
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        // Fallback to screen clearing
                        Console.Clear();
                        Console.WriteLine(prompt);
                        Console.WriteLine(Dim("Use spacebar to toggle, Enter to confirm, arrow keys to navigate"));
                        for (int i = 0; i < choices.Length; i++)
                        {
                            var prefix = i == currentIndex ? ">" : " ";
                            var checkbox = selected[i] ? "✓" : " ";
                            string coloredChoice;
                            if (i == currentIndex)
                                coloredChoice = Cyan(choices[i]);
                            else if (selected[i])
                                coloredChoice = Green(choices[i]);
                            else
                                coloredChoice = Reset(choices[i]);
                            
                            Console.WriteLine($"{prefix} [{checkbox}] {coloredChoice}");
                        }
                    }
                }
            };

            // Initial render
            renderMenu();

            while (true)
            {
                var key = Console.ReadKey(true);
                bool needsRedraw = false;

                switch (key.Key)
                {
                    case ConsoleKey.UpArrow:
                        currentIndex = Math.Max(0, currentIndex - 1);
                        needsRedraw = true;
                        break;
                    case ConsoleKey.DownArrow:
                        currentIndex = Math.Min(choices.Length - 1, currentIndex + 1);
                        needsRedraw = true;
                        break;
                    case ConsoleKey.Spacebar:
                        selected[currentIndex] = !selected[currentIndex];
                        needsRedraw = true;
                        break;
                    case ConsoleKey.Enter:
                        var result = new List<string>();
                        for (int i = 0; i < choices.Length; i++)
                        {
                            if (selected[i])
                                result.Add(choices[i]);
                        }
                        // Clear screen and show final result
                        Console.Clear();
                        Console.WriteLine(prompt);
                        Console.WriteLine();
                        return await System.Threading.Tasks.Task.FromResult(result.ToArray());
                    case ConsoleKey.Escape:
                        // Clear screen and exit
                        Console.Clear();
                        Console.WriteLine(prompt);
                        Console.WriteLine();
                        return await System.Threading.Tasks.Task.FromResult(new string[0]);
                }

                if (needsRedraw)
                {
                    renderMenu();
                }
            }
        }

        public async System.Threading.Tasks.Task<double> Number(string prompt, double defaultValue = 0, double? min = null, double? max = null)
        {
            while (true)
            {
                Console.Write($"{prompt} [{defaultValue}]: ");
                var input = Console.ReadLine()?.Trim();
                
                if (string.IsNullOrEmpty(input))
                {
                    return await System.Threading.Tasks.Task.FromResult(defaultValue);
                }

                if (double.TryParse(input, out var value))
                {
                    if (min.HasValue && value < min.Value)
                    {
                        Console.WriteLine(Red($"Value must be at least {min.Value}"));
                        continue;
                    }
                    
                    if (max.HasValue && value > max.Value)
                    {
                        Console.WriteLine(Red($"Value must be at most {max.Value}"));
                        continue;
                    }
                    
                    return await System.Threading.Tasks.Task.FromResult(value);
                }
                
                Console.WriteLine(Red("Please enter a valid number"));
            }
        }

        public async System.Threading.Tasks.Task<DateTime> Date(string prompt, DateTime? defaultValue = null, DateTime? min = null, DateTime? max = null)
        {
            var def = defaultValue ?? DateTime.Today;
            
            while (true)
            {
                Console.Write($"{prompt} [{def:yyyy-MM-dd}]: ");
                var input = Console.ReadLine()?.Trim();
                
                if (string.IsNullOrEmpty(input))
                {
                    return await System.Threading.Tasks.Task.FromResult(def);
                }

                if (DateTime.TryParse(input, out var date))
                {
                    if (min.HasValue && date < min.Value)
                    {
                        Console.WriteLine(Red($"Date must be on or after {min.Value:yyyy-MM-dd}"));
                        continue;
                    }
                    
                    if (max.HasValue && date > max.Value)
                    {
                        Console.WriteLine(Red($"Date must be on or before {max.Value:yyyy-MM-dd}"));
                        continue;
                    }
                    
                    return await System.Threading.Tasks.Task.FromResult(date);
                }
                
                Console.WriteLine(Red("Please enter a valid date (yyyy-mm-dd)"));
            }
        }

        public async System.Threading.Tasks.Task<string> Autocomplete(string prompt, Func<string, string[]> source, bool allowCustom = true)
        {
            Console.Write(prompt);
            var input = new StringBuilder();
            var suggestions = new List<string>();
            var selectedSuggestion = -1;

            while (true)
            {
                var key = Console.ReadKey(true);
                
                switch (key.Key)
                {
                    case ConsoleKey.Enter:
                        if (selectedSuggestion >= 0 && selectedSuggestion < suggestions.Count)
                        {
                            var result = suggestions[selectedSuggestion];
                            Console.WriteLine(result);
                            return await System.Threading.Tasks.Task.FromResult(result);
                        }
                        else if (allowCustom || suggestions.Contains(input.ToString()))
                        {
                            var result = input.ToString();
                            Console.WriteLine(result);
                            return await System.Threading.Tasks.Task.FromResult(result);
                        }
                        break;
                        
                    case ConsoleKey.Tab:
                        if (suggestions.Count > 0)
                        {
                            selectedSuggestion = (selectedSuggestion + 1) % suggestions.Count;
                            UpdateAutocompleteDisplay(input.ToString(), suggestions, selectedSuggestion);
                        }
                        break;
                        
                    case ConsoleKey.Escape:
                        Console.WriteLine();
                        return await System.Threading.Tasks.Task.FromResult("");
                        
                    case ConsoleKey.Backspace:
                        if (input.Length > 0)
                        {
                            input.Remove(input.Length - 1, 1);
                            suggestions = source(input.ToString())?.ToList() ?? new List<string>();
                            selectedSuggestion = -1;
                            UpdateAutocompleteDisplay(input.ToString(), suggestions, selectedSuggestion);
                        }
                        break;
                        
                    default:
                        if (!char.IsControl(key.KeyChar))
                        {
                            input.Append(key.KeyChar);
                            suggestions = source(input.ToString())?.ToList() ?? new List<string>();
                            selectedSuggestion = -1;
                            UpdateAutocompleteDisplay(input.ToString(), suggestions, selectedSuggestion);
                        }
                        break;
                }
            }
        }

        private void UpdateAutocompleteDisplay(string input, List<string> suggestions, int selectedIndex)
        {
            // Clear current line and suggestions
            Console.Write("\r" + new string(' ', Console.WindowWidth - 1) + "\r");
            
            // Show input
            Console.Write($"{input}");
            
            // Show suggestions
            if (suggestions.Count > 0)
            {
                Console.Write(" (");
                for (int i = 0; i < Math.Min(3, suggestions.Count); i++)
                {
                    if (i == selectedIndex)
                        Console.Write(Cyan(suggestions[i]));
                    else
                        Console.Write(Dim(suggestions[i]));
                        
                    if (i < Math.Min(2, suggestions.Count - 1))
                        Console.Write(", ");
                }
                if (suggestions.Count > 3)
                    Console.Write("...");
                Console.Write(")");
            }
        }

        // Progress bar
        public ProgressBar CreateProgressBar(int total = 100, int width = 40)
        {
            return new ProgressBar(total, width);
        }

        // Spinner
        public Spinner CreateSpinner(string text)
        {
            return new Spinner(text);
        }

        // Beep
        public void Beep()
        {
            Console.Beep();
        }

        // Output formatting methods
        
        // Table rendering
        public void Table(string[][] data, TableOptions? options = null)
        {
            if (data == null || data.Length == 0) return;
            
            var opts = options ?? new TableOptions();
            var columnCount = data[0].Length;
            var columnWidths = new int[columnCount];
            
            // Calculate column widths
            for (int col = 0; col < columnCount; col++)
            {
                int maxWidth = 0;
                for (int row = 0; row < data.Length; row++)
                {
                    if (col < data[row].Length)
                    {
                        maxWidth = Math.Max(maxWidth, StringWidth(data[row][col] ?? ""));
                    }
                }
                columnWidths[col] = opts.ColWidths != null && col < opts.ColWidths.Length 
                    ? opts.ColWidths[col] 
                    : maxWidth + 2;
            }
            
            RenderTableRow(data[0], columnWidths, opts, true, true, data.Length == 1); // Header
            if (data.Length > 1)
            {
                RenderTableSeparator(columnWidths, opts);
                for (int i = 1; i < data.Length; i++)
                {
                    RenderTableRow(data[i], columnWidths, opts, false, false, i == data.Length - 1);
                }
            }
        }

        public void Table(object tableConfig)
        {
            // Handle dynamic table configuration from JavaScript
            var dict = tableConfig as IDictionary<string, object>;
            if (dict == null) return;

            string[][]? data = null;
            TableOptions options = new TableOptions();

            if (dict.ContainsKey("head") && dict.ContainsKey("rows"))
            {
                var head = dict["head"] as object[];
                var rows = dict["rows"] as object[];
                
                if (head != null && rows != null)
                {
                    var dataList = new List<string[]>();
                    dataList.Add(head.Select(h => h?.ToString() ?? "").ToArray());
                    
                    foreach (var row in rows)
                    {
                        if (row is object[] rowArray)
                        {
                            dataList.Add(rowArray.Select(c => c?.ToString() ?? "").ToArray());
                        }
                    }
                    data = dataList.ToArray();
                }
            }

            if (data != null)
            {
                Table(data, options);
            }
        }

        // Box drawing
        public void Box(string content, BoxOptions? options = null)
        {
            var opts = options ?? new BoxOptions();
            var lines = content.Split('\n');
            var contentWidth = lines.Max(line => StringWidth(line));
            var boxWidth = Math.Max(contentWidth + (opts.Padding * 2), opts.MinWidth);
            
            var borderChars = GetBorderChars(opts.BorderStyle);
            
            // Top border
            Console.WriteLine(opts.BorderColor != null 
                ? ApplyColor(borderChars.TopLeft + new string(borderChars.Horizontal, boxWidth - 2) + borderChars.TopRight, opts.BorderColor)
                : borderChars.TopLeft + new string(borderChars.Horizontal, boxWidth - 2) + borderChars.TopRight);
            
            // Content lines
            foreach (var line in lines)
            {
                var paddedLine = opts.Align switch
                {
                    "center" => Center(line, boxWidth - 2),
                    "right" => PadLeft(line, boxWidth - 2),
                    _ => PadRight(line, boxWidth - 2)
                };
                
                var contentLine = opts.BorderColor != null 
                    ? ApplyColor(borderChars.Vertical.ToString(), opts.BorderColor) + paddedLine + ApplyColor(borderChars.Vertical.ToString(), opts.BorderColor)
                    : borderChars.Vertical + paddedLine + borderChars.Vertical;
                    
                Console.WriteLine(contentLine);
            }
            
            // Bottom border
            Console.WriteLine(opts.BorderColor != null 
                ? ApplyColor(borderChars.BottomLeft + new string(borderChars.Horizontal, boxWidth - 2) + borderChars.BottomRight, opts.BorderColor)
                : borderChars.BottomLeft + new string(borderChars.Horizontal, boxWidth - 2) + borderChars.BottomRight);
        }

        // Tree display
        public void Tree(TreeNode node, TreeOptions? options = null)
        {
            var opts = options ?? new TreeOptions();
            RenderTreeNode(node, "", true, opts);
        }

        public void Tree(object treeConfig)
        {
            // Handle dynamic tree configuration from JavaScript
            var dict = treeConfig as IDictionary<string, object>;
            if (dict == null) return;

            var node = ParseTreeNode(dict);
            if (node != null)
            {
                Tree(node, new TreeOptions());
            }
        }

        // Helper methods for output formatting
        
        private void RenderTableRow(string[] row, int[] columnWidths, TableOptions opts, bool isHeader, bool isFirst, bool isLast)
        {
            var borderChars = GetBorderChars(opts.BorderStyle);
            
            if (isFirst)
            {
                // Top border
                Console.Write(borderChars.TopLeft);
                for (int i = 0; i < columnWidths.Length; i++)
                {
                    Console.Write(new string(borderChars.Horizontal, columnWidths[i]));
                    if (i < columnWidths.Length - 1)
                        Console.Write(borderChars.TopJunction);
                }
                Console.WriteLine(borderChars.TopRight);
            }
            
            // Content row
            Console.Write(borderChars.Vertical);
            for (int i = 0; i < columnWidths.Length; i++)
            {
                var content = i < row.Length ? (row[i] ?? "") : "";
                var align = opts.ColAligns != null && i < opts.ColAligns.Length ? opts.ColAligns[i] : "left";
                var cellContent = align switch
                {
                    "center" => Center(content, columnWidths[i]),
                    "right" => PadLeft(content, columnWidths[i]),
                    _ => PadRight(content, columnWidths[i])
                };
                
                if (isHeader && opts.HeaderColor != null)
                    cellContent = ApplyColor(cellContent, opts.HeaderColor);
                    
                Console.Write(cellContent);
                if (i < columnWidths.Length - 1)
                    Console.Write(borderChars.Vertical);
            }
            Console.WriteLine(borderChars.Vertical);
            
            if (isLast)
            {
                // Bottom border
                Console.Write(borderChars.BottomLeft);
                for (int i = 0; i < columnWidths.Length; i++)
                {
                    Console.Write(new string(borderChars.Horizontal, columnWidths[i]));
                    if (i < columnWidths.Length - 1)
                        Console.Write(borderChars.BottomJunction);
                }
                Console.WriteLine(borderChars.BottomRight);
            }
        }
        
        private void RenderTableSeparator(int[] columnWidths, TableOptions opts)
        {
            var borderChars = GetBorderChars(opts.BorderStyle);
            Console.Write(borderChars.LeftJunction);
            for (int i = 0; i < columnWidths.Length; i++)
            {
                Console.Write(new string(borderChars.Horizontal, columnWidths[i]));
                if (i < columnWidths.Length - 1)
                    Console.Write(borderChars.MiddleJunction);
            }
            Console.WriteLine(borderChars.RightJunction);
        }
        
        private BorderChars GetBorderChars(string style)
        {
            return style switch
            {
                "double" => new BorderChars
                {
                    TopLeft = '╔', TopRight = '╗', BottomLeft = '╚', BottomRight = '╝',
                    Horizontal = '═', Vertical = '║',
                    TopJunction = '╦', BottomJunction = '╩', LeftJunction = '╠', RightJunction = '╣',
                    MiddleJunction = '╬'
                },
                "round" => new BorderChars
                {
                    TopLeft = '╭', TopRight = '╮', BottomLeft = '╰', BottomRight = '╯',
                    Horizontal = '─', Vertical = '│',
                    TopJunction = '┬', BottomJunction = '┴', LeftJunction = '├', RightJunction = '┤',
                    MiddleJunction = '┼'
                },
                "bold" => new BorderChars
                {
                    TopLeft = '┏', TopRight = '┓', BottomLeft = '┗', BottomRight = '┛',
                    Horizontal = '━', Vertical = '┃',
                    TopJunction = '┳', BottomJunction = '┻', LeftJunction = '┣', RightJunction = '┫',
                    MiddleJunction = '╋'
                },
                "ascii" => new BorderChars
                {
                    TopLeft = '+', TopRight = '+', BottomLeft = '+', BottomRight = '+',
                    Horizontal = '-', Vertical = '|',
                    TopJunction = '+', BottomJunction = '+', LeftJunction = '+', RightJunction = '+',
                    MiddleJunction = '+'
                },
                _ => new BorderChars // single (default)
                {
                    TopLeft = '┌', TopRight = '┐', BottomLeft = '└', BottomRight = '┘',
                    Horizontal = '─', Vertical = '│',
                    TopJunction = '┬', BottomJunction = '┴', LeftJunction = '├', RightJunction = '┤',
                    MiddleJunction = '┼'
                }
            };
        }
        
        private void RenderTreeNode(TreeNode node, string prefix, bool isLast, TreeOptions opts)
        {
            var connector = isLast ? "└── " : "├── ";
            Console.WriteLine(prefix + connector + (node.Icon ?? "") + node.Label);
            
            if (node.Children != null && node.Children.Count > 0)
            {
                var childPrefix = prefix + (isLast ? "    " : "│   ");
                for (int i = 0; i < node.Children.Count; i++)
                {
                    RenderTreeNode(node.Children[i], childPrefix, i == node.Children.Count - 1, opts);
                }
            }
        }
        
        private TreeNode? ParseTreeNode(IDictionary<string, object> dict)
        {
            if (!dict.ContainsKey("label")) return null;
            
            var node = new TreeNode
            {
                Label = dict["label"]?.ToString() ?? "",
                Icon = dict.ContainsKey("icon") ? dict["icon"]?.ToString() : null,
                Children = new List<TreeNode>()
            };
            
            if (dict.ContainsKey("children") && dict["children"] is object[] children)
            {
                foreach (var child in children)
                {
                    if (child is IDictionary<string, object> childDict)
                    {
                        var childNode = ParseTreeNode(childDict);
                        if (childNode != null)
                            node.Children.Add(childNode);
                    }
                }
            }
            
            return node;
        }
        
        private string ApplyColor(string text, string color)
        {
            return color.ToLower() switch
            {
                "red" => Red(text),
                "green" => Green(text),
                "yellow" => Yellow(text),
                "blue" => Blue(text),
                "magenta" => Magenta(text),
                "cyan" => Cyan(text),
                "white" => White(text),
                "gray" => Gray(text),
                "black" => Black(text),
                _ => text
            };
        }

        // Helper methods for validation
        private void ValidateRgb(int r, int g, int b)
        {
            if (r < 0 || r > 255)
                throw new ArgumentOutOfRangeException(nameof(r), "Red value must be between 0 and 255");
            if (g < 0 || g > 255)
                throw new ArgumentOutOfRangeException(nameof(g), "Green value must be between 0 and 255");
            if (b < 0 || b > 255)
                throw new ArgumentOutOfRangeException(nameof(b), "Blue value must be between 0 and 255");
        }

        private (int r, int g, int b) ParseHex(string hex)
        {
            hex = hex.TrimStart('#');
            
            if (hex.Length != 6)
                throw new ArgumentException("Hex color must be in format #RRGGBB or RRGGBB");
                
            try
            {
                var r = Convert.ToInt32(hex.Substring(0, 2), 16);
                var g = Convert.ToInt32(hex.Substring(2, 2), 16);
                var b = Convert.ToInt32(hex.Substring(4, 2), 16);
                return (r, g, b);
            }
            catch
            {
                throw new ArgumentException("Invalid hex color format");
            }
        }
    }

    // ANSI color enum
    public enum AnsiColor
    {
        Black = 30,
        Red = 31,
        Green = 32,
        Yellow = 33,
        Blue = 34,
        Magenta = 35,
        Cyan = 36,
        White = 37,
        Gray = 90,
        BrightRed = 91,
        BrightGreen = 92,
        BrightYellow = 93,
        BrightBlue = 94,
        BrightMagenta = 95,
        BrightCyan = 96,
        BrightWhite = 97
    }

    // ANSI style enum
    public enum AnsiStyle
    {
        Bold = 1,
        Dim = 2,
        Italic = 3,
        Underline = 4,
        Blink = 5,
        Inverse = 7,
        Hidden = 8,
        Strikethrough = 9
    }

    // ANSI builder
    public class AnsiBuilder
    {
        private readonly List<int> _codes = new();

        public AnsiBuilder SetForeground(AnsiColor color)
        {
            _codes.Clear();
            _codes.Add((int)color);
            return this;
        }

        public AnsiBuilder SetBackground(AnsiColor color)
        {
            _codes.Clear();
            _codes.Add((int)color + 10);
            return this;
        }

        public AnsiBuilder SetStyle(AnsiStyle style)
        {
            _codes.Clear();
            _codes.Add((int)style);
            return this;
        }

        public string Build(string text)
        {
            if (_codes.Count == 0)
                return text;
                
            var codes = string.Join(";", _codes);
            return $"\x1b[{codes}m{text}\x1b[0m";
        }
    }

    // Terminal capabilities
    public class Terminal
    {
        public int GetColorSupport()
        {
            // Check for NO_COLOR environment variable
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("NO_COLOR")))
                return 0;

            // Windows Terminal supports true color
            if (Environment.GetEnvironmentVariable("WT_SESSION") != null)
                return 16777216;

            // Check COLORTERM for true color support
            var colorTerm = Environment.GetEnvironmentVariable("COLORTERM");
            if (colorTerm == "truecolor" || colorTerm == "24bit")
                return 16777216;

            // Check TERM for color support
            var term = Environment.GetEnvironmentVariable("TERM") ?? "";
            if (term.Contains("256color"))
                return 256;
            if (term.Contains("color"))
                return 16;

            // Default to basic support on non-redirected terminals
            return Console.IsOutputRedirected ? 0 : 16;
        }

        public bool SupportsUnicode()
        {
            // Check console encoding
            try
            {
                return Console.OutputEncoding.CodePage == 65001 || // UTF-8
                       Console.OutputEncoding == Encoding.UTF8 ||
                       Console.OutputEncoding == Encoding.Unicode;
            }
            catch
            {
                return false;
            }
        }
    }

    // Progress bar
    public class ProgressBar
    {
        private readonly int _total;
        private readonly int _width;
        private int _current;
        private readonly DateTime _startTime;

        public ProgressBar(int total, int width)
        {
            _total = total;
            _width = width;
            _current = 0;
            _startTime = DateTime.Now;
        }

        public void Update(int value)
        {
            _current = Math.Min(value, _total);
            Render();
        }

        public void Increment(int delta = 1)
        {
            Update(_current + delta);
        }

        public void Complete()
        {
            Update(_total);
            Console.WriteLine();
        }

        private void Render()
        {
            var percent = (double)_current / _total;
            var filled = (int)(_width * percent);
            var empty = _width - filled;
            
            var bar = new string('=', filled) + new string('-', empty);
            var percentText = $"{(int)(percent * 100)}%";
            
            Console.Write($"\r[{bar}] {percentText} {_current}/{_total}");
            Console.Out.Flush(); // Force flush to ensure output is displayed
        }
    }

    // Spinner
    public class Spinner
    {
        private static readonly string[] Frames = { "⠋", "⠙", "⠹", "⠸", "⠼", "⠴", "⠦", "⠧", "⠇", "⠏" };
        private int _currentFrame;
        private string _text;
        private System.Threading.Timer? _timer;

        public string Text
        {
            get => _text;
            set => _text = value;
        }

        public Spinner(string text)
        {
            _text = text;
            _currentFrame = 0;
        }

        public void Start()
        {
            Console.CursorVisible = false;
            _timer = new System.Threading.Timer(_ => Render(), null, 0, 80);
        }

        public void Stop()
        {
            _timer?.Dispose();
            Console.Write("\r" + new string(' ', Console.WindowWidth - 1) + "\r");
            Console.CursorVisible = true;
        }

        public void Succeed(string text)
        {
            Stop();
            Console.WriteLine($"✓ {text}");
        }

        public void Fail(string text)
        {
            Stop();
            Console.WriteLine($"✗ {text}");
        }

        public void Warn(string text)
        {
            Stop();
            Console.WriteLine($"⚠ {text}");
        }

        public void Info(string text)
        {
            Stop();
            Console.WriteLine($"ℹ {text}");
        }

        private void Render()
        {
            var frame = Frames[_currentFrame];
            Console.Write($"\r{frame} {_text}");
            Console.Out.Flush(); // Force flush to ensure output is displayed
            _currentFrame = (_currentFrame + 1) % Frames.Length;
        }
    }
}