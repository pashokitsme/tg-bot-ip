using Example.Commands;
using Example.Commands.CallbackButtons;
using Example.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Example
{
    internal class UpdateHandler
    {
        private readonly TelegramBotClient _client;
        private readonly ChatCommandManager _chatCommandManager;
        private readonly CallbackCommandManager _callbackManager;

        public UpdateHandler(TelegramBotClient client, ChatCommandManager chatCommandManager, CallbackCommandManager callbackCommandManager)
        {
            _client = client;
            _chatCommandManager = chatCommandManager;
            _callbackManager = callbackCommandManager;
        }

        public async void OnMessageReceived(Message message)
        {
            if (message == null)
            {
                Logger.Log("Message is null", LogSeverity.ERROR);
                return;
            }

            if (message.Type != MessageType.Text || message.Text == null)
            {
                Logger.Log($"Message should be {UpdateType.Message}/{MessageType.Text}, not {message.Type}", LogSeverity.ERROR);
                return;
            }

            if (message.Text[0] == '/')
            {
                var result = _chatCommandManager.TryExecute(message.Text.Split(' ')[0], message);

                if (result == false)
                    await _client.SendTextMessageAsync(message.Chat.Id, $@"Не удалось выполнить команду {message.Text.Split(' ')[0]} 😢");

                return;
            }

            Logger.Log($"Reply to message {message.Text} by {message.From.Username}");
            await _client.SendTextMessageAsync(message.Chat.Id, $"{message.Text}", replyToMessageId: message.MessageId);
        }

        public void OnCallbackReceived(CallbackQuery query)
        {
            if(query == null)
            {
                Logger.Log("Query is null", LogSeverity.ERROR);
                return;
            }

            _callbackManager.TryExecute(query);
        }
    }
}