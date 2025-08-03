using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using EkkoJS.Core.Modules.Cli;

namespace EkkoJS.Core.Modules.BuiltIn
{
    public class CliBuiltInModule : IModule
    {
        private readonly CliModule _cliModule;

        public string Name => "cli";
        public string Protocol => "ekko:cli";

        public CliBuiltInModule()
        {
            _cliModule = new CliModule();
        }

        public object GetExports()
        {
            dynamic exports = new ExpandoObject();
            
            // Basic colors
            exports.red = new Func<string, string>(_cliModule.Red);
            exports.green = new Func<string, string>(_cliModule.Green);
            exports.yellow = new Func<string, string>(_cliModule.Yellow);
            exports.blue = new Func<string, string>(_cliModule.Blue);
            exports.magenta = new Func<string, string>(_cliModule.Magenta);
            exports.cyan = new Func<string, string>(_cliModule.Cyan);
            exports.white = new Func<string, string>(_cliModule.White);
            exports.gray = new Func<string, string>(_cliModule.Gray);
            exports.black = new Func<string, string>(_cliModule.Black);
            
            // Bright colors
            exports.brightRed = new Func<string, string>(_cliModule.BrightRed);
            exports.brightGreen = new Func<string, string>(_cliModule.BrightGreen);
            exports.brightYellow = new Func<string, string>(_cliModule.BrightYellow);
            exports.brightBlue = new Func<string, string>(_cliModule.BrightBlue);
            exports.brightMagenta = new Func<string, string>(_cliModule.BrightMagenta);
            exports.brightCyan = new Func<string, string>(_cliModule.BrightCyan);
            exports.brightWhite = new Func<string, string>(_cliModule.BrightWhite);
            
            // Background colors
            exports.bgRed = new Func<string, string>(_cliModule.BgRed);
            exports.bgGreen = new Func<string, string>(_cliModule.BgGreen);
            exports.bgYellow = new Func<string, string>(_cliModule.BgYellow);
            exports.bgBlue = new Func<string, string>(_cliModule.BgBlue);
            exports.bgMagenta = new Func<string, string>(_cliModule.BgMagenta);
            exports.bgCyan = new Func<string, string>(_cliModule.BgCyan);
            exports.bgWhite = new Func<string, string>(_cliModule.BgWhite);
            exports.bgGray = new Func<string, string>(_cliModule.BgGray);
            exports.bgBlack = new Func<string, string>(_cliModule.BgBlack);
            
            // Styles
            exports.bold = new Func<string, string>(_cliModule.Bold);
            exports.italic = new Func<string, string>(_cliModule.Italic);
            exports.underline = new Func<string, string>(_cliModule.Underline);
            exports.strikethrough = new Func<string, string>(_cliModule.Strikethrough);
            exports.inverse = new Func<string, string>(_cliModule.Inverse);
            exports.dim = new Func<string, string>(_cliModule.Dim);
            exports.hidden = new Func<string, string>(_cliModule.Hidden);
            
            // 256 color support
            exports.color = new Func<int, Func<string, string>>(_cliModule.Color);
            exports.bgColor = new Func<int, Func<string, string>>(_cliModule.BgColor);
            
            // RGB support
            exports.rgb = new Func<int, int, int, Func<string, string>>(_cliModule.Rgb);
            exports.bgRgb = new Func<int, int, int, Func<string, string>>(_cliModule.BgRgb);
            
            // Hex support
            exports.hex = new Func<string, Func<string, string>>(_cliModule.Hex);
            exports.bgHex = new Func<string, Func<string, string>>(_cliModule.BgHex);
            
            // Reset
            exports.reset = new Func<string, string>(_cliModule.Reset);
            
            // Strip ANSI
            exports.stripAnsi = new Func<string, string>(_cliModule.StripAnsi);
            
            // Screen operations
            exports.clear = new Action(_cliModule.Clear);
            exports.clearScreen = new Action(_cliModule.ClearScreen);
            exports.clearLine = new Action(_cliModule.ClearLine);
            exports.clearLineLeft = new Action(_cliModule.ClearLineLeft);
            exports.clearLineRight = new Action(_cliModule.ClearLineRight);
            exports.clearDown = new Action(_cliModule.ClearDown);
            exports.clearUp = new Action(_cliModule.ClearUp);
            
            // Screen size
            exports.getScreenSize = new Func<object>(_cliModule.GetScreenSize);
            
            // Cursor operations
            exports.cursorTo = new Action<int, int>(_cliModule.CursorTo);
            exports.cursorToColumn = new Action<int>(_cliModule.CursorToColumn);
            exports.cursorUp = new Action(() => _cliModule.CursorUp(1));
            exports.cursorDown = new Action(() => _cliModule.CursorDown(1));
            exports.cursorForward = new Action(() => _cliModule.CursorForward(1));
            exports.cursorBackward = new Action(() => _cliModule.CursorBackward(1));
            exports.cursorNextLine = new Action(() => _cliModule.CursorNextLine(1));
            exports.cursorPrevLine = new Action(() => _cliModule.CursorPrevLine(1));
            exports.saveCursor = new Action(_cliModule.SaveCursor);
            exports.restoreCursor = new Action(_cliModule.RestoreCursor);
            exports.hideCursor = new Action(_cliModule.HideCursor);
            exports.showCursor = new Action(_cliModule.ShowCursor);
            
            // Terminal capabilities
            exports.getCapabilities = new Func<object>(_cliModule.GetCapabilities);
            exports.supportsColor = new Func<bool>(_cliModule.SupportsColor);
            exports.supportsUnicode = new Func<bool>(_cliModule.SupportsUnicode);
            
            // Basic output
            exports.write = new Action<string>(_cliModule.Write);
            exports.writeLine = new Action(() => _cliModule.WriteLine());
            
            // Input methods
            exports.input = new Func<string, System.Threading.Tasks.Task<string>>(_cliModule.Input);
            exports.password = new Func<string, System.Threading.Tasks.Task<string>>(async (prompt) => await _cliModule.Password(prompt, '\0'));
            exports.confirm = new Func<string, System.Threading.Tasks.Task<bool>>(async (prompt) => await _cliModule.Confirm(prompt, true));
            exports.select = new Func<string, object, System.Threading.Tasks.Task<string>>(async (prompt, choicesObj) => 
            {
                // Convert JavaScript array to C# string array
                string[] choices;
                
                if (choicesObj is System.Collections.IEnumerable enumerable && !(choicesObj is string))
                {
                    // Convert any enumerable to string array
                    var list = new System.Collections.Generic.List<string>();
                    foreach (var item in enumerable)
                    {
                        list.Add(item?.ToString() ?? "");
                    }
                    choices = list.ToArray();
                }
                else if (choicesObj is object[] objArray)
                {
                    choices = objArray.Select(o => o?.ToString() ?? "").ToArray();
                }
                else if (choicesObj is string[] strArray)
                {
                    choices = strArray;
                }
                else
                {
                    throw new ArgumentException($"choices must be an array of strings");
                }
                
                return await _cliModule.Select(prompt, choices);
            });
            
            // Advanced input methods
            exports.multiSelect = new Func<string, object, System.Threading.Tasks.Task<object>>(async (prompt, choicesObj) => 
            {
                // Convert JavaScript array to C# string array
                string[] choices;
                
                // Debug: Check what type we actually get
                var objType = choicesObj?.GetType().FullName ?? "null";
                
                if (choicesObj is System.Collections.IEnumerable enumerable && !(choicesObj is string))
                {
                    // Convert any enumerable to string array
                    var list = new System.Collections.Generic.List<string>();
                    foreach (var item in enumerable)
                    {
                        list.Add(item?.ToString() ?? "");
                    }
                    choices = list.ToArray();
                }
                else if (choicesObj is object[] objArray)
                {
                    choices = objArray.Select(o => o?.ToString() ?? "").ToArray();
                }
                else if (choicesObj is string[] strArray)
                {
                    choices = strArray;
                }
                else
                {
                    throw new ArgumentException($"choices must be an array of strings, got: {objType}");
                }
                
                var result = await _cliModule.MultiSelect(prompt, choices, null);
                
                // Create a JavaScript-compatible array
                dynamic jsArray = new System.Dynamic.ExpandoObject();
                var dict = (IDictionary<string, object>)jsArray;
                
                // Add array elements
                for (int i = 0; i < result.Length; i++)
                {
                    dict[i.ToString()] = result[i];
                }
                
                // Add array properties
                dict["length"] = result.Length;
                dict["join"] = new Func<string, string>((separator) => string.Join(separator ?? ",", result));
                dict["toString"] = new Func<string>(() => string.Join(",", result));
                
                return jsArray;
            });
            exports.number = new Func<string, double, System.Threading.Tasks.Task<double>>(async (prompt, defaultValue) => await _cliModule.Number(prompt, defaultValue));
            exports.date = new Func<string, System.Threading.Tasks.Task<object>>(async (prompt) => 
            {
                var date = await _cliModule.Date(prompt);
                // Convert DateTime to JavaScript-compatible object
                return new { 
                    year = date.Year,
                    month = date.Month - 1, // JavaScript months are 0-based
                    day = date.Day,
                    toISOString = new Func<string>(() => date.ToString("yyyy-MM-dd")),
                    toString = new Func<string>(() => date.ToString("yyyy-MM-dd"))
                };
            });
            
            // Autocomplete
            exports.autocomplete = new Func<string, object, bool, System.Threading.Tasks.Task<string>>(async (prompt, sourceFunc, allowCustom) =>
            {
                // For now, just return a simple implementation until we can properly handle the callback
                // The full autocomplete with dynamic source will need more complex interop
                Console.WriteLine(prompt);
                Console.Write("> ");
                var input = Console.ReadLine() ?? "";
                return await System.Threading.Tasks.Task.FromResult(input);
            });
            
            // Progress bar and spinner
            exports.createProgressBar = new Func<int, int, object>((total, width) => 
            {
                var progressBar = _cliModule.CreateProgressBar(total, width);
                dynamic pbWrapper = new ExpandoObject();
                pbWrapper.update = new Action<int>(progressBar.Update);
                pbWrapper.increment = new Action<int>(progressBar.Increment);
                pbWrapper.complete = new Action(progressBar.Complete);
                return pbWrapper;
            });
            
            exports.createSpinner = new Func<string, object>((text) => 
            {
                var spinner = _cliModule.CreateSpinner(text);
                dynamic spinnerWrapper = new ExpandoObject();
                spinnerWrapper.start = new Action(spinner.Start);
                spinnerWrapper.stop = new Action(spinner.Stop);
                spinnerWrapper.succeed = new Action<string>(spinner.Succeed);
                spinnerWrapper.fail = new Action<string>(spinner.Fail);
                spinnerWrapper.warn = new Action<string>(spinner.Warn);
                spinnerWrapper.info = new Action<string>(spinner.Info);
                spinnerWrapper.text = text;
                return spinnerWrapper;
            });
            
            // Beep
            exports.beep = new Action(_cliModule.Beep);
            
            // String utilities
            exports.stringWidth = new Func<string, int>(_cliModule.StringWidth);
            exports.truncate = new Func<string, int, string>((text, maxWidth) => _cliModule.Truncate(text, maxWidth, "end"));
            exports.truncateStart = new Func<string, int, string>((text, maxWidth) => _cliModule.Truncate(text, maxWidth, "start"));
            exports.truncateMiddle = new Func<string, int, string>((text, maxWidth) => _cliModule.Truncate(text, maxWidth, "middle"));
            exports.wrap = new Func<string, int, string>((text, width) => _cliModule.Wrap(text, width, 0, false));
            exports.wrapIndent = new Func<string, int, int, string>((text, width, indent) => _cliModule.Wrap(text, width, indent, false));
            exports.pad = new Func<string, int, string>(_cliModule.Pad);
            exports.padLeft = new Func<string, int, string>(_cliModule.PadLeft);
            exports.padRight = new Func<string, int, string>(_cliModule.PadRight);
            exports.center = new Func<string, int, string>(_cliModule.Center);
            exports.right = new Func<string, int, string>(_cliModule.Right);
            
            // Output formatting methods
            exports.table = new Action<object>((data) => 
            {
                if (data is System.Collections.IEnumerable enumerable && !(data is string))
                {
                    // Handle array of arrays format
                    var rows = new System.Collections.Generic.List<string[]>();
                    foreach (var item in enumerable)
                    {
                        if (item is System.Collections.IEnumerable rowEnum && !(item is string))
                        {
                            var row = new System.Collections.Generic.List<string>();
                            foreach (var cell in rowEnum)
                            {
                                row.Add(cell?.ToString() ?? "");
                            }
                            rows.Add(row.ToArray());
                        }
                    }
                    if (rows.Count > 0)
                    {
                        _cliModule.Table(rows.ToArray());
                    }
                }
                else
                {
                    // Handle object format
                    _cliModule.Table(data);
                }
            });
            exports.box = new Action<string>((content) => _cliModule.Box(content, null));
            exports.tree = new Action<object>(_cliModule.Tree);
            
            return exports;
        }
    }
}