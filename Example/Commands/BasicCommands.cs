using Telegram.Bot;

namespace Example.Commands;

public class BasicCommands
{
    [ChatCommand("/test", "Тестовая команда"),]
    private static Task<bool> Test(ChatCommandContext context)
    {
        _ = context.Client.SendTextMessageAsync(context.Message.Chat.Id, "Тестовая команда, которая, кстати, работает!");
        return Task.FromResult(true);
    }

    [ChatCommand("/echo", "Эхо-команда, в аргументах нужно написать сообщение")]
    private static Task<bool> Echo(ChatCommandContext context)
    {
        if (context.Args.Length < 1)
            return Task.FromResult(false);

        _ = context.Client.SendTextMessageAsync(context.Message.Chat.Id, string.Join(' ', context.Args));
        return Task.FromResult(true);
    }
}
