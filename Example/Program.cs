using Example.Configuration;
using Example.Core;
using Telegram.Bot.Types.Enums;

namespace Example;

public static class Program
{
    private const string CONFIG_PATH = "config.json";
    
    private static bool _alreadyStopped;
    private static readonly UpdateType[] _allowedUpdates = 
    {
        UpdateType.Message
    };

    public static void Main()
    {
        Logger.Log("Starting! Press CTRL+C to exit");

        var config = GetRelevantConfiguration();
        var app = new App(config);
        
        AppDomain.CurrentDomain.ProcessExit += (_, _) => OnStop(app);
        Console.CancelKeyPress += (_, _) => OnStop(app);

        app.StartAsync(_allowedUpdates);
        Thread.Sleep(-1);
    }

    private static ITelegramBotConfiguration GetRelevantConfiguration()
    {
        if (Environment.GetEnvironmentVariable("IS_HEROKU") is null or "false")
        {
            Logger.Log($"Using {CONFIG_PATH}");
            return FileConfiguration.Get(CONFIG_PATH);
        }

        Logger.Log("Using enviroment");
        return new TelegramEnviromentConfiguration();
    }

    private static void OnStop(App app)
    {
        if (_alreadyStopped)
            return;

        _alreadyStopped = true;
        app.Stop();
    }
}