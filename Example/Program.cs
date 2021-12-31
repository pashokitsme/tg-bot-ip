using Example.Logging;

namespace Example;
public static class Program
{
    public static async Task Main()
    {
        Logger.Log("Starting! Press CTRL+C to exit");
        var app = new App();

        AppDomain.CurrentDomain.ProcessExit += (sender, args) => app.Stop();
        Console.CancelKeyPress += (sender, args) => app.Stop();

        await app.StartAsync();
    }
}