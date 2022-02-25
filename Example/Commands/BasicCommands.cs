using Telegram.Bot;

namespace Example.Commands;

public class BasicCommands
{
    [ChatCommand("test", "Тестовая команда")]
    private Task<bool> Test(ChatCommandContext context)
    {
        _ = context.Client.SendTextMessageAsync(context.Message.Chat.Id, "Тестовая команда, которая, кстати, работает!");
        return Task.FromResult(true);
    }
    
    [ChatCommand("statictest", "Тестовая статическая команда")]
    private static Task<bool> TestStatic(ChatCommandContext context)
    {
        _ = context.Client.SendTextMessageAsync(context.Message.Chat.Id, "Тестовая команда, метод которой статический!");
        return Task.FromResult(true);
    }

    [ChatCommand("echo", "Эхо-команда, в аргументах нужно написать сообщение")]
    private Task<bool> Echo(ChatCommandContext context)
    {
        if (context.Args.Length < 1)
            return Task.FromResult(false);

        var reply = string.Join(' ', context.Args);
        _ = context.Client.SendTextMessageAsync(context.Message.Chat.Id, reply);
        return Task.FromResult(true);
    }
}
