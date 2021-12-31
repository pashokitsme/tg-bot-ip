using Example.Core;
using Telegram.Bot;

namespace Example;
public static class Program
{
    public static void Main()
    {
        Logger.Log("Starting! Press any key to exit");
        var app = new App();
        app.StartAsync();
        Console.ReadKey(false);
        app.Stop();
    }
}