using EkkoJS.Package.CLI;
using EkkoJS.Package.GUI.Components;

namespace EkkoJS.Demo.CLI;

class Program
{
    static async Task Main(string[] args)
    {
        Console.Clear();
        
        // Show title with ANSI colors if supported
        if (AnsiConsole.IsSupported())
        {
            Console.WriteLine($"{AnsiConsole.Foreground.BrightCyan}{AnsiConsole.Bold}EkkoJS CLI/GUI Demo{AnsiConsole.Reset}");
            Console.WriteLine($"{AnsiConsole.Foreground.Yellow}═══════════════════{AnsiConsole.Reset}");
        }
        else
        {
            Console.WriteLine("EkkoJS CLI/GUI Demo");
            Console.WriteLine("===================");
        }
        Console.WriteLine();

        // Main menu
        var mainMenu = new Menu("Select a demo:");
        mainMenu.AddItem("Progress Bar Demo", () => RunProgressBarDemo().Wait());
        mainMenu.AddItem("Table Demo", () => RunTableDemo());
        mainMenu.AddItem("Spinner Demo", () => RunSpinnerDemo().Wait());
        mainMenu.AddItem("ANSI Colors Demo", () => RunAnsiDemo());
        mainMenu.AddItem("Interactive Menu Demo", () => RunInteractiveMenuDemo());
        mainMenu.AddSeparator();
        mainMenu.AddItem("Exit", () => Environment.Exit(0));

        while (true)
        {
            var selected = mainMenu.Show();
            if (selected == null || selected.Text == "Exit")
                break;
                
            Console.WriteLine("\nPress any key to return to menu...");
            Console.ReadKey(true);
            Console.Clear();
            
            // Redraw title
            if (AnsiConsole.IsSupported())
            {
                Console.WriteLine($"{AnsiConsole.Foreground.BrightCyan}{AnsiConsole.Bold}EkkoJS CLI/GUI Demo{AnsiConsole.Reset}");
                Console.WriteLine($"{AnsiConsole.Foreground.Yellow}═══════════════════{AnsiConsole.Reset}");
            }
            else
            {
                Console.WriteLine("EkkoJS CLI/GUI Demo");
                Console.WriteLine("===================");
            }
            Console.WriteLine();
        }
    }

    static async Task RunProgressBarDemo()
    {
        Console.Clear();
        Console.WriteLine("Progress Bar Demo");
        Console.WriteLine("=================\n");

        var tasks = new[]
        {
            ("Downloading files", 0.75),
            ("Processing data", 0.5),
            ("Building index", 0.3),
            ("Optimizing", 1.0)
        };

        var progressBars = tasks.Select((t, i) => new ProgressBar(40)).ToArray();
        var startY = Console.CursorTop;

        // Reserve space for progress bars
        for (int i = 0; i < tasks.Length; i++)
        {
            Console.WriteLine();
            Console.WriteLine();
        }

        // Animate progress bars
        for (int step = 0; step <= 100; step++)
        {
            for (int i = 0; i < tasks.Length; i++)
            {
                var (label, speed) = tasks[i];
                var progress = Math.Min(1.0, (step * speed) / 100.0);
                progressBars[i].Update(progress, label);
                progressBars[i].Render(0, startY + (i * 2));
            }
            await Task.Delay(50);
        }

        Console.SetCursorPosition(0, startY + (tasks.Length * 2));
        Console.WriteLine("\n✓ All tasks completed!");
    }

    static void RunTableDemo()
    {
        Console.Clear();
        Console.WriteLine("Table Demo");
        Console.WriteLine("==========\n");

        var table = new Table("Name", "Language", "Experience", "Location");
        table.AddRow("Alice Johnson", "C#", "5 years", "New York");
        table.AddRow("Bob Smith", "JavaScript", "3 years", "San Francisco");
        table.AddRow("Charlie Brown", "Python", "7 years", "London");
        table.AddRow("Diana Prince", "Java", "4 years", "Tokyo");
        table.AddRow("Eve Wilson", "Go", "2 years", "Berlin");

        Console.WriteLine("Simple Style:");
        table.WithStyle(Table.TableStyle.Simple).Render();

        Console.WriteLine("\nBordered Style:");
        table.WithStyle(Table.TableStyle.Bordered).Render();

        Console.WriteLine("\nCompact Style:");
        table.WithStyle(Table.TableStyle.Compact).Render();

        Console.WriteLine("\nMarkdown Style:");
        table.WithStyle(Table.TableStyle.Markdown).Render();
    }

    static async Task RunSpinnerDemo()
    {
        Console.Clear();
        Console.WriteLine("Spinner Demo");
        Console.WriteLine("============\n");

        var spinnerStyles = new[]
        {
            ("Dots", Spinner.Styles.Dots),
            ("Line", Spinner.Styles.Line),
            ("Star", Spinner.Styles.Star),
            ("Square", Spinner.Styles.Square),
            ("Circle", Spinner.Styles.Circle),
            ("Arrow", Spinner.Styles.Arrow),
            ("Bounce", Spinner.Styles.Bounce),
            ("Box", Spinner.Styles.Box)
        };

        foreach (var (name, style) in spinnerStyles)
        {
            await Spinner.RunWithSpinner($"Loading with {name} style", async () =>
            {
                await Task.Delay(2000);
            }, style);
        }

        Console.WriteLine("\nCustom spinner with changing message:");
        using (var spinner = new Spinner("Initializing", Spinner.Styles.Dots))
        {
            spinner.Start();
            await Task.Delay(1000);
            
            spinner.Message = "Connecting to server";
            await Task.Delay(1000);
            
            spinner.Message = "Downloading data";
            await Task.Delay(1000);
            
            spinner.Message = "Processing";
            await Task.Delay(1000);
            
            spinner.Stop("✓ Operation completed successfully!");
        }
    }

    static void RunAnsiDemo()
    {
        Console.Clear();
        Console.WriteLine("ANSI Colors Demo");
        Console.WriteLine("================\n");

        if (!AnsiConsole.IsSupported())
        {
            Console.WriteLine("ANSI colors are not supported in this terminal.");
            return;
        }

        // Basic colors
        Console.WriteLine("Basic Foreground Colors:");
        Console.WriteLine($"{AnsiConsole.Foreground.Red}Red{AnsiConsole.Reset} " +
                         $"{AnsiConsole.Foreground.Green}Green{AnsiConsole.Reset} " +
                         $"{AnsiConsole.Foreground.Yellow}Yellow{AnsiConsole.Reset} " +
                         $"{AnsiConsole.Foreground.Blue}Blue{AnsiConsole.Reset} " +
                         $"{AnsiConsole.Foreground.Magenta}Magenta{AnsiConsole.Reset} " +
                         $"{AnsiConsole.Foreground.Cyan}Cyan{AnsiConsole.Reset}");

        Console.WriteLine("\nBright Colors:");
        Console.WriteLine($"{AnsiConsole.Foreground.BrightRed}Bright Red{AnsiConsole.Reset} " +
                         $"{AnsiConsole.Foreground.BrightGreen}Bright Green{AnsiConsole.Reset} " +
                         $"{AnsiConsole.Foreground.BrightYellow}Bright Yellow{AnsiConsole.Reset} " +
                         $"{AnsiConsole.Foreground.BrightBlue}Bright Blue{AnsiConsole.Reset}");

        Console.WriteLine("\nText Formatting:");
        Console.WriteLine($"{AnsiConsole.Bold}Bold Text{AnsiConsole.Reset} " +
                         $"{AnsiConsole.Italic}Italic Text{AnsiConsole.Reset} " +
                         $"{AnsiConsole.Underline}Underlined{AnsiConsole.Reset} " +
                         $"{AnsiConsole.Strikethrough}Strikethrough{AnsiConsole.Reset}");

        Console.WriteLine("\nBackground Colors:");
        Console.WriteLine($"{AnsiConsole.Background.Red}Red Background{AnsiConsole.Reset} " +
                         $"{AnsiConsole.Background.Green}Green Background{AnsiConsole.Reset} " +
                         $"{AnsiConsole.Background.Blue}Blue Background{AnsiConsole.Reset}");

        Console.WriteLine("\nRGB Colors (True Color):");
        for (int i = 0; i < 360; i += 10)
        {
            var r = (byte)(127 + 127 * Math.Sin(i * Math.PI / 180));
            var g = (byte)(127 + 127 * Math.Sin((i + 120) * Math.PI / 180));
            var b = (byte)(127 + 127 * Math.Sin((i + 240) * Math.PI / 180));
            Console.Write($"{AnsiConsole.Foreground.Rgb(r, g, b)}█{AnsiConsole.Reset}");
        }
        Console.WriteLine();

        Console.WriteLine("\nCombined Effects:");
        Console.WriteLine($"{AnsiConsole.Bold}{AnsiConsole.Foreground.Yellow}{AnsiConsole.Background.Blue} Bold Yellow on Blue {AnsiConsole.Reset}");
        Console.WriteLine($"{AnsiConsole.Underline}{AnsiConsole.Foreground.BrightCyan}Underlined Bright Cyan{AnsiConsole.Reset}");
    }

    static void RunInteractiveMenuDemo()
    {
        Console.Clear();
        Console.WriteLine("Interactive Menu Demo");
        Console.WriteLine("====================\n");

        var colorMenu = new Menu("Choose your favorite color:");
        var colors = new[] { "Red", "Green", "Blue", "Yellow", "Purple", "Orange", "Pink" };
        
        foreach (var color in colors)
        {
            colorMenu.AddItem(color, () => Console.WriteLine($"\nYou selected: {color}"));
        }

        colorMenu.AddSeparator();
        colorMenu.AddItem("I can't decide!", () => 
        {
            var random = new Random();
            var randomColor = colors[random.Next(colors.Length)];
            Console.WriteLine($"\nWe chose for you: {randomColor}");
        });

        var selected = colorMenu.Show();
        
        if (selected != null)
        {
            Console.WriteLine("\nMenu demo completed!");
        }
        else
        {
            Console.WriteLine("\nMenu cancelled!");
        }
    }
}