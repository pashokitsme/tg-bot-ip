using Example.Commands;
using Example.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InlineQueryResults;

namespace Example.Core
{
    internal class UpdateHandler
    {
        private readonly TelegramBotClient _client;
        private readonly ChatCommandProvider _commandProvider;

        public UpdateHandler(TelegramBotClient client, ChatCommandProvider provider)
        {
            _client = client;
            _commandProvider = provider;
        }

        public async Task OnMessageReceivedAsync(Message message)
        {
            if (message == null || message.Text == null || message.Text.Length < 1)
            {
                Logger.Log("Message is invalid", LogSeverity.ERROR);
                return;
            }

            if (message.Text[0] == '/')
            {
                var result = _commandProvider.TryExecuteCommand(message.Text.Split(' ')[0], message);

                if (result == false)
                    await _client.SendTextMessageAsync(message.Chat.Id, $@"Не удалось выполнить команду {message.Text.Split(' ')[0]} 😢");
                return;
            }

            Logger.Log($"Reply to message {message.Text} by {message.From?.Username}");
            await _client.SendTextMessageAsync(message.Chat.Id, $"{message.Text}", replyToMessageId: message.MessageId);
        }

        public async Task OnInlineQueryReceived(InlineQuery query)
        {
            Logger.Log($"Received inline query");
            var results = new List<InlineQueryResult>()
            {
                new InlineQueryResultArticle("address", "ТестТест", new InputVenueMessageContent("Title", "Адрес", 56.06906198865623, 47.24778056589566)),
                new InlineQueryResultArticle("another_address", "ТестТыест", new InputVenueMessageContent("Titleвыаф", "Другой адрес", 58.06906198865623, 42.24778056589566)),
                new InlineQueryResultArticle("text", "ТестТыест", new InputTextMessageContent("Какой то текст"))
                {
                    Description = "описание"
                }
            };

            await _client.AnswerInlineQueryAsync(query.Id, results);
        }
    }
}
