using Example.Logging;

namespace Example;
public static class Program
{
    private static bool _alreadyStopped = false;

    public static async Task Main()
    {
        Logger.Log("Starting! Press CTRL+C to exit");
        var app = new App();

        AppDomain.CurrentDomain.ProcessExit += (sender, args) => OnStop(app);
        Console.CancelKeyPress += (sender, args) => OnStop(app);

        await app.StartAsync();
    }

    private static void OnStop(App app)
    {
        if (_alreadyStopped)
            return;

        _alreadyStopped = true;
        app.Stop();
    }
}