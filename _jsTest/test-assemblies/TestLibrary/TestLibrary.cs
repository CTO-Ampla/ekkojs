using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TestLibrary
{
    // Static calculator class
    public static class Calculator
    {
        public static int Add(int a, int b) => a + b;
        public static int Subtract(int a, int b) => a - b;
        public static double Multiply(double a, double b) => a * b;
        public static double Divide(double a, double b) => b != 0 ? a / b : throw new DivideByZeroException();
    }

    // Instance-based string utilities
    public class StringHelper
    {
        private string _prefix;

        public StringHelper(string prefix = "")
        {
            _prefix = prefix;
        }

        public string Prefix
        {
            get => _prefix;
            set => _prefix = value ?? "";
        }

        public string Format(string text)
        {
            return $"{_prefix}{text}";
        }

        public string[] Split(string text, char separator)
        {
            return text.Split(separator);
        }

        public async Task<string> ProcessAsync(string text)
        {
            await Task.Delay(100); // Simulate async work
            return Format(text.ToUpper());
        }
    }

    // Constants and configuration
    public static class Constants
    {
        public const string VERSION = "1.0.0";
        public const int MAX_LENGTH = 1000;
        public static readonly DateTime BUILD_DATE = new DateTime(2024, 1, 1);
    }

    // Event example
    public class EventEmitter
    {
        public event EventHandler<ProgressEventArgs>? ProgressChanged;

        public void DoWork(int steps)
        {
            for (int i = 0; i < steps; i++)
            {
                var progress = (i + 1) * 100 / steps;
                ProgressChanged?.Invoke(this, new ProgressEventArgs { Progress = progress });
                System.Threading.Thread.Sleep(100);
            }
        }
    }

    public class ProgressEventArgs : EventArgs
    {
        public int Progress { get; set; }
    }
}