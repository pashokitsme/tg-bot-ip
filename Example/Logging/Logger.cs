using System.Diagnostics;
using System.Text;

namespace Example.Logging;

internal enum LogSeverity
{
    INFO,
    WARNING,
    ERROR
}

internal static class Logger
{

    public static void Log(string message, LogSeverity severity = LogSeverity.INFO)
    {

        Console.ForegroundColor = severity switch
        {
            LogSeverity.WARNING => ConsoleColor.Yellow,
            LogSeverity.ERROR => ConsoleColor.Red,
            _ => Console.ForegroundColor
        };

        Console.WriteLine($"{DateTime.Now:HH:mm:ss} : {message}");
        Console.ResetColor();
    }
}
