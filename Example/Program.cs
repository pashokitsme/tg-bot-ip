using Example.Logging;
using Telegram.Bot.Types.Enums;

namespace Example;

public static class Program
{
    private static bool _alreadyStopped;
    private static readonly UpdateType[] _allowedUpdates = {
        UpdateType.Message,
        UpdateType.CallbackQuery
    };

    public static void Main()
    {
        Logger.Log("Starting! Press CTRL+C to exit");

        var app = new App();
        AppDomain.CurrentDomain.ProcessExit += (_, _) => OnStop(app);
        Console.CancelKeyPress += (_, _) => OnStop(app);
        AppDomain.CurrentDomain.UnhandledException += OnException;

        app.StartAsync(_allowedUpdates);
        Thread.Sleep(-1);
        
    }

    private static void OnException(object sender, UnhandledExceptionEventArgs args)
    {
        Logger.Log($"Unhandled Exception!", LogSeverity.Error);
    }

    private static void OnStop(App app)
    {
        if (_alreadyStopped)
            return;

        _alreadyStopped = true;
        app.Stop();
    }
}