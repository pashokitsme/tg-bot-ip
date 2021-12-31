namespace Example.Core;

internal enum LogSeverity
{
    INFO,
    WARNING,
    ERROR
}

internal static class Logger
{
    public static void Log(string message)
    {
        Log(message, LogSeverity.INFO);
    }

    public static void Log(string message, LogSeverity severity)
    {
        Console.ForegroundColor = severity switch
        {
            LogSeverity.WARNING => ConsoleColor.Yellow,
            LogSeverity.ERROR => ConsoleColor.Red,
            _ => Console.ForegroundColor
        };

        Console.WriteLine($"{DateTime.Now} : {message}");
        Console.ResetColor();
    }
}
