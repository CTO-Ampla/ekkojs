namespace EkkoJS.Package.CLI;

/// <summary>
/// ANSI escape sequence support for rich console output
/// </summary>
public static class AnsiConsole
{
    // Text formatting
    public const string Reset = "\x1b[0m";
    public const string Bold = "\x1b[1m";
    public const string Dim = "\x1b[2m";
    public const string Italic = "\x1b[3m";
    public const string Underline = "\x1b[4m";
    public const string Blink = "\x1b[5m";
    public const string Reverse = "\x1b[7m";
    public const string Hidden = "\x1b[8m";
    public const string Strikethrough = "\x1b[9m";
    
    // Foreground colors
    public static class Foreground
    {
        public const string Black = "\x1b[30m";
        public const string Red = "\x1b[31m";
        public const string Green = "\x1b[32m";
        public const string Yellow = "\x1b[33m";
        public const string Blue = "\x1b[34m";
        public const string Magenta = "\x1b[35m";
        public const string Cyan = "\x1b[36m";
        public const string White = "\x1b[37m";
        public const string Default = "\x1b[39m";
        
        // Bright colors
        public const string BrightBlack = "\x1b[90m";
        public const string BrightRed = "\x1b[91m";
        public const string BrightGreen = "\x1b[92m";
        public const string BrightYellow = "\x1b[93m";
        public const string BrightBlue = "\x1b[94m";
        public const string BrightMagenta = "\x1b[95m";
        public const string BrightCyan = "\x1b[96m";
        public const string BrightWhite = "\x1b[97m";
        
        public static string Rgb(byte r, byte g, byte b) => $"\x1b[38;2;{r};{g};{b}m";
    }
    
    // Background colors
    public static class Background
    {
        public const string Black = "\x1b[40m";
        public const string Red = "\x1b[41m";
        public const string Green = "\x1b[42m";
        public const string Yellow = "\x1b[43m";
        public const string Blue = "\x1b[44m";
        public const string Magenta = "\x1b[45m";
        public const string Cyan = "\x1b[46m";
        public const string White = "\x1b[47m";
        public const string Default = "\x1b[49m";
        
        // Bright colors
        public const string BrightBlack = "\x1b[100m";
        public const string BrightRed = "\x1b[101m";
        public const string BrightGreen = "\x1b[102m";
        public const string BrightYellow = "\x1b[103m";
        public const string BrightBlue = "\x1b[104m";
        public const string BrightMagenta = "\x1b[105m";
        public const string BrightCyan = "\x1b[106m";
        public const string BrightWhite = "\x1b[107m";
        
        public static string Rgb(byte r, byte g, byte b) => $"\x1b[48;2;{r};{g};{b}m";
    }
    
    // Cursor control
    public static class Cursor
    {
        public static string Up(int n = 1) => $"\x1b[{n}A";
        public static string Down(int n = 1) => $"\x1b[{n}B";
        public static string Forward(int n = 1) => $"\x1b[{n}C";
        public static string Back(int n = 1) => $"\x1b[{n}D";
        public static string NextLine(int n = 1) => $"\x1b[{n}E";
        public static string PreviousLine(int n = 1) => $"\x1b[{n}F";
        public static string HorizontalAbsolute(int n) => $"\x1b[{n}G";
        public static string Position(int row, int col) => $"\x1b[{row};{col}H";
        
        public const string Save = "\x1b[s";
        public const string Restore = "\x1b[u";
        public const string Hide = "\x1b[?25l";
        public const string Show = "\x1b[?25h";
    }
    
    // Screen control
    public static class Screen
    {
        public const string Clear = "\x1b[2J";
        public const string ClearFromCursor = "\x1b[0J";
        public const string ClearToCursor = "\x1b[1J";
        public const string ClearLine = "\x1b[2K";
        public const string ClearLineFromCursor = "\x1b[0K";
        public const string ClearLineToCursor = "\x1b[1K";
    }
    
    // Helper methods
    public static bool IsSupported()
    {
        // Check if terminal supports ANSI
        var term = Environment.GetEnvironmentVariable("TERM");
        var colorterm = Environment.GetEnvironmentVariable("COLORTERM");
        
        if (Console.IsOutputRedirected)
            return false;
            
        if (OperatingSystem.IsWindows())
        {
            // Windows 10+ supports ANSI
            return Environment.OSVersion.Version.Major >= 10;
        }
        
        return !string.IsNullOrEmpty(term) && term != "dumb";
    }
}