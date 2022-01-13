using Example.Commands;
using Example.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;

namespace Example.Core;

internal class UpdateHandler
{
    private readonly TelegramBotClient _client;
    private readonly ChatCommandManager _commandManager;

    public UpdateHandler(TelegramBotClient client, ChatCommandManager provider)
    {
        _client = client;
        _commandManager = provider;
    }

    public async void OnMessageReceivedAsync(Message message)
    {
        if(message.Type != MessageType.Text || message.Text == null)
        {
            Logger.Log($"Message should be Text Message, not {message.Type}", LogSeverity.ERROR);
            return;
        }

        if (message.Text[0] == '/')
        {
            var result = _commandManager.TryExecuteCommand(message.Text.Split(' ')[0], message);

            if (result == false)
                await _client.SendTextMessageAsync(message.Chat.Id, $@"Не удалось выполнить команду {message.Text.Split(' ')[0]} 😢");
            return;
        }

        Logger.Log($"Reply to message {message.Text} by {message.From?.Username}");
        await _client.SendTextMessageAsync(message.Chat.Id, $"{message.Text}", replyToMessageId: message.MessageId);
    }

    public async void OnInlineQueryReceived(InlineQuery query)
    {
        Logger.Log($"Received inline query");
        var results = new List<InlineQueryResult>()
        {
            new InlineQueryResultArticle("text", "Текст", new InputTextMessageContent("Какой то текст"))
            {
                Description = "описание"
            }
        };

        await _client.AnswerInlineQueryAsync(query.Id, results);
    }
}
