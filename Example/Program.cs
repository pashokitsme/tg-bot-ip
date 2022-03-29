using Example.Configuration;
using Example.Core;
using Telegram.Bot.Types.Enums;

namespace Example;

public static class Program
{
    private const string CONFIG_PATH = "config.json";

    private static readonly UpdateType[] _allowedUpdates =
    {
        UpdateType.Message
    };

    public static void Main()
    {
        Logger.Log("Starting! Press CTRL+C to exit");

        var config = GetRelevantConfiguration();
        var app = new App(config);

        AppDomain.CurrentDomain.ProcessExit += (_, _) => app.Stop();
        Console.CancelKeyPress += (_, _) => app.Stop();

        app.StartAsync(_allowedUpdates);
        Thread.Sleep(-1);
    }

    private static ITelegramBotConfiguration GetRelevantConfiguration()
    {
        if (Environment.GetEnvironmentVariable("IS_HEROKU") is null or "false")
        {
            Logger.Log($"Using {CONFIG_PATH}");
            return TelegramJsonConfiguration.Get(CONFIG_PATH);
        }

        Logger.Log("Using enviroment");
        return new TelegramEnviromentConfiguration();
    }
}