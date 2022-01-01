using Example.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Example.Commands;
internal class TestCommand
{
    [ChatCommand("test", "тестовая команда")]
    public static void Test(TelegramBotClient client, Message message)
    {
        Logger.Log("Test command executed!");
        client.SendTextMessageAsync(message.Chat.Id, $"{message.From!.Username}, ты выполнил тестовую команду. Она, кстати, работает!");
    }
}
