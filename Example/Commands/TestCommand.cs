using Telegram.Bot;

namespace Example.Commands;
internal class TestCommand
{
    [ChatCommand("test", "тестовая команда")]
    public static async Task<bool> Test(ChatCommandContext context)
    {
        await context.Client.SendTextMessageAsync(context.Message.Chat.Id, $"{context.Message.From!.Username}, ты выполнил тестовую команду. Она, кстати, работает!");
        return true;
    }

    [ChatCommand("ne_test", "тестовая команда, всегда неправильная")]
    public static async Task<bool> Test2(ChatCommandContext context)
    {
        return false;
    }

    [ChatCommand("super_test", "тессссст")]
    public static async Task<bool> Test3(ChatCommandContext context)
    {
        await context.Client.SendTextMessageAsync(context.Message.Chat.Id, $"А теперь и третья");
        return true;
    }
}
