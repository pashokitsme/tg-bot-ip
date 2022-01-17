using Example.Commands;
using Example.Core;
using Example.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Example;

public class App
{
    private readonly TelegramBotConfiguration _configuration = TelegramBotConfiguration.Get("config.json");
    private readonly TelegramApiListener _listener;
    private readonly TelegramBotClient _client;
    private readonly ChatCommandManager _commandManager;

    public App()
    {
        _listener = new TelegramApiListener(_configuration);
        _client = new TelegramBotClient(_configuration.Token);
        _commandManager = new ChatCommandManager(_client);

        _listener.UpdateReceived += (update) => Task.Run(() => OnUpdateReceived(update));
    }

    public async void StartAsync(UpdateType[] allowedUpdates)
    {
        await SetupWebhookAsync(allowedUpdates);
        await SetupBotCommandsAsync();
        _listener.Start();
    }

    public void Stop()
    {
        RemoveWebhookAsync().Wait();
        _listener.Stop();
    }

    private async void OnUpdateReceived(Update update)
    {
        var message = update.Message;

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
            var result = _commandManager.TryExecuteCommand(message.Text.Split(' ')[0], message);

            if (result == false)
                await _client.SendTextMessageAsync(message.Chat.Id, $@"Не удалось выполнить команду {message.Text.Split(' ')[0]} 😢");

            return;
        }

        Logger.Log($"Reply to message {message.Text} by {message.From.Username}");
        await _client.SendTextMessageAsync(message.Chat.Id, $"{message.Text}", replyToMessageId: message.MessageId);
    }

    private async Task SetupWebhookAsync(UpdateType[] allowedUpdates)
    {
        await _client.SetWebhookAsync(_configuration.Webhook, allowedUpdates: allowedUpdates, dropPendingUpdates: true);
        var webhook = await _client.GetWebhookInfoAsync();
        Logger.Log($"Webhook binded to {webhook.Url}");
    }

    private async Task SetupBotCommandsAsync()
    {
        var commands = _commandManager.GetBotCommands();
        await _client.SetMyCommandsAsync(commands);
        Logger.Log($"Setted up {commands.Length} commands");
    }

    private async Task RemoveWebhookAsync()
    {
        await _client.DeleteWebhookAsync(true);
        Logger.Log($"Webhook removed");
    }
}