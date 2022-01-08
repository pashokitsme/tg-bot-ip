using Telegram.Bot;

namespace Example.Commands;
internal static class BasicCommands
{
    [ChatCommand("test", "тестовая команда")]
    public static async Task<bool> Test(ChatCommandContext context)
    {
        await context.Client.SendTextMessageAsync(context.Message.Chat.Id, $"Тестовая команда, которая, кстати, работает!");
        return true;
    }

    [ChatCommand("start", "start command", true)]
    public static async Task<bool> Start(ChatCommandContext context)
    {
        await context.Client.SendTextMessageAsync(context.Message.Chat.Id, "Привет!");
        return true;
    }
}
