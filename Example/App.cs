using Example.Core;
using Example.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Example;
internal class App
{
    private readonly string _configurationPath = "config.json";

    private readonly TelegramBotConfiguration _configuration;
    private readonly TelegramBotApiListener _listener;
    private readonly TelegramBotClient _client;

    public App()
    {
        _configuration = TelegramBotConfiguration.Get(_configurationPath);
        _listener = new TelegramBotApiListener(_configuration);
        _client = new TelegramBotClient(_configuration.Token);

        _listener.UpdateReceived += update => OnUpdateReceived(_client, update);

        _listener.Stopped += () =>
        {
            _client.DeleteWebhookAsync();
            Logger.Log("Webhook deleted");
        };
    }

    public async Task StartAsync()
    {
        await SetupWebhookAsync();
        await _listener.StartAsync();
    }

    public void Stop()
    {
        _listener.Stop();
    }

    private async void OnUpdateReceived(TelegramBotClient client, Update update)
    {
        var message = update.Message;

        if (message == null)
        {
            Logger.Log("Message is null", LogSeverity.ERROR);
            return;
        }

        Logger.Log($"Reply to message {message.Text} by {message.From?.Username}");
        await client.SendTextMessageAsync(message.Chat.Id, $"{message.Text}", replyToMessageId: message.MessageId);
    }

    private async Task SetupWebhookAsync()
    {
        if (await _client.TestApiAsync() == false)
        {
            Logger.Log("API Token invalid", LogSeverity.ERROR);
            return;
        }

        await _client.SetWebhookAsync(
            _configuration.ExternalHost + _configuration.Route.TrimStart('/'),
            allowedUpdates: new UpdateType[] { UpdateType.Message },
            dropPendingUpdates: true);

        var webhook = await _client.GetWebhookInfoAsync();

        Logger.Log($"Webhook binded to {webhook.Url}");
    }
}

