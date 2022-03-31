using Telegram.Bot;

namespace Example.Commands;

public class BasicCommands
{
    [ChatCommand("/test", "Тестовая команда"),]
    private static void Test(ChatCommandContext context) => context.Client.SendTextMessageAsync(context.Message.Chat.Id, "Тестовая команда, которая, кстати, работает!");

    [ChatCommand("/echo", "Эхо-команда, в аргументах нужно написать сообщение")]
    private static void Echo(ChatCommandContext context)
    {
        if (context.Args.Length < 1)
        {
            _ = context.Client.SendTextMessageAsync(context.Message.Chat.Id, "Нужно указать агрументы");
            return;
        }

        _ = context.Client.SendTextMessageAsync(context.Message.Chat.Id, string.Join(' ', context.Args));
    }
}
