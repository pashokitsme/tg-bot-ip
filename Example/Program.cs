using Example.Core;
using Telegram.Bot;

namespace Example;
public static class Program
{
    public static void Main(string[] args)
    {
        Logger.Log("Starting! Press any key to exit");
        new App().StartAsync();
        Console.ReadKey();
    }
}