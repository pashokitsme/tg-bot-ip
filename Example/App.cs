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
    private readonly ChatCommandProvider _commandProvider;
    private readonly UpdateHandler _handler;

    public App()
    {
        _configuration = TelegramBotConfiguration.Get(_configurationPath);
        _listener = new TelegramBotApiListener(_configuration);
        _client = new TelegramBotClient(_configuration.Token);
        _commandProvider = new ChatCommandProvider(_client, '/');
        _handler = new UpdateHandler(_client, _commandProvider);

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
            UpdateType.Message => _handler.OnMessageReceivedAsync(update.Message!),
            UpdateType.InlineQuery => _handler.OnInlineQueryReceived(update.InlineQuery!),
            UpdateType.ChosenInlineResult => _handler.OnChoosedInlineResultReceived(update.ChosenInlineResult!),
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
            _configuration.Host + _configuration.Route.TrimStart('/'),
            allowedUpdates: new UpdateType[] { UpdateType.Message, UpdateType.InlineQuery, UpdateType.ChosenInlineResult },
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

