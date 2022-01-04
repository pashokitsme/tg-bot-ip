using Telegram.Bot;

namespace Example.Commands
{
    internal static class StartCommand
    {
        [ChatCommand("start", "start command", true)]
        public static async Task<bool> Start(ChatCommandContext context)
        {
            await context.Client.SendTextMessageAsync(context.Message.Chat.Id, "Привет!");
            return true;
        }
    }
}
