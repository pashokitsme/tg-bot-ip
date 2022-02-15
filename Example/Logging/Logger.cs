namespace Example.Logging;

public enum LogSeverity
{
    Info,
    Warning,
    Error
}

public static class Logger
{
    public static void Log(string message, LogSeverity severity = LogSeverity.Info)
    {
        Console.ForegroundColor = severity switch
        {
            LogSeverity.Warning => ConsoleColor.Yellow,
            LogSeverity.Error => ConsoleColor.Red,
            _ => Console.ForegroundColor
        };

        Console.WriteLine($"{DateTime.Now:HH:mm:ss} : {message}");
        Console.ResetColor();
    }
}
