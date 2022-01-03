using Example.Commands;
using Example.Core;
using Example.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Example;
internal class App
{
    private readonly TelegramBotConfiguration _configuration = TelegramBotConfiguration.Get("config.json");
    private readonly TelegramApiListener _listener;
    private readonly TelegramBotClient _client;
    private readonly ChatCommandProvider _commandProvider;
    private readonly UpdateHandler _updateHandler;

    public App()
    {
        _listener = new TelegramApiListener(_configuration);
        _client = new TelegramBotClient(_configuration.Token);
        _commandProvider = new ChatCommandProvider(_client);
        _updateHandler = new UpdateHandler(_client, _commandProvider);

        _listener.UpdateReceived += update => OnUpdateReceived(update);

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

    private Task OnUpdateReceived(Update update)
    {
        return update.Type switch
        {
            UpdateType.Message => _updateHandler.OnMessageReceivedAsync(update.Message!),
            UpdateType.InlineQuery => _updateHandler.OnInlineQueryReceived(update.InlineQuery!),
            _ => throw new NotImplementedException()
        };
    }

    private async Task SetupWebhookAsync()
    {
        if (await _client.TestApiAsync() == false)
        {
            Logger.Log("API Token invalid", LogSeverity.ERROR);
            return;
        }

        await _client.SetWebhookAsync(
            _configuration.Webhook,
            allowedUpdates: new UpdateType[] { UpdateType.Message, UpdateType.InlineQuery },
            dropPendingUpdates: true);

        var webhook = await _client.GetWebhookInfoAsync();

        Logger.Log($"Webhook binded to {webhook.Url}");
    }

    private async Task SetupBotCommands()
    {
        var commands = _commandProvider.GetBotCommands();
        await _client.SetMyCommandsAsync(commands);
        Logger.Log($"Setted up {commands.Length} commands");
    }
}

