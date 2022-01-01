using Example.Commands;
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
    private readonly ChatCommandProvider _provider;

    public App()
    {
        _configuration = TelegramBotConfiguration.Get(_configurationPath);
        _listener = new TelegramBotApiListener(_configuration);
        _client = new TelegramBotClient(_configuration.Token);
        _provider = new ChatCommandProvider(_client, '/');

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
        await SetupBotCommands();
        await _listener.StartAsync();
    }

    public void Stop()
    {
        _listener.Stop();
    }

    private async void OnUpdateReceived(TelegramBotClient client, Update update)
    {
        var message = update.Message;

        if (message == null || message.Text == null || message.Text.Length < 1)
        {
            Logger.Log("Message is invalid", LogSeverity.ERROR);
            return;
        }

        if (message.Text[0] == _provider.Prefix)
        {
            
            await _provider.TryExecuteCommandAsync(message.Text.Split(' ')[0], message);
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
            _configuration.Host + _configuration.Route.TrimStart('/'),
            allowedUpdates: new UpdateType[] { UpdateType.Message },
            dropPendingUpdates: true);

        var webhook = await _client.GetWebhookInfoAsync();

        Logger.Log($"Webhook binded to {webhook.Url}");
    }

    private async Task SetupBotCommands()
    {
        var commands = _provider.GetBotCommands();
        await _client.SetMyCommandsAsync(commands);
        Logger.Log($"Setted up {commands.Length} commands");
    }
}

