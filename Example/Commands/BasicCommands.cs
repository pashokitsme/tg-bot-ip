using Telegram.Bot;

namespace Example.Commands;

public class BasicCommands
{
    [ChatCommand("test", "Тестовая команда")]
    private async Task<bool> Test(ChatCommandContext context)
    {
        await context.Client.SendTextMessageAsync(context.Message.Chat.Id, "Тестовая команда, которая, кстати, работает!");
        return true;
    }
    
    [ChatCommand("statictest", "Тестовая статическая команда")]
    private static async Task<bool> TestStatic(ChatCommandContext context)
    {
        await context.Client.SendTextMessageAsync(context.Message.Chat.Id, "Тестовая команда, метод которой статический!");
        return true;
    }

    [ChatCommand("echo", "Эхо-команда, в аргументах нужно написать сообщение")]
    private async Task<bool> Echo(ChatCommandContext context)
    {
        if (context.Args.Length < 1)
            return false;

        var reply = string.Join(' ', context.Args);
        await context.Client.SendTextMessageAsync(context.Message.Chat.Id, reply);
        return true;
    }
}
