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
    private readonly UpdateHandler _updateHandler;

    public App()
    {
        _listener = new TelegramApiListener(_configuration);
        _client = new TelegramBotClient(_configuration.Token);
        _commandManager = new ChatCommandManager(_client);
        _updateHandler = new UpdateHandler(_client, _commandManager);

        _listener.UpdateReceived += OnUpdateReceived;
    }

    public async void StartAsync(UpdateType[] allowedUpdates)
    {
        await SetupWebhookAsync(allowedUpdates);
        await SetupBotCommandsAsync();
        await _listener.StartAsync();
    }

    public void Stop()
    {
        RemoveWebhookAsync().Wait();
        _listener.Stop();
    }

    private void OnUpdateReceived(Update update)
    {
        switch (update.Type)
        {
            case UpdateType.Message:
                _updateHandler.OnMessageReceivedAsync(update.Message!);
                return;

            case UpdateType.InlineQuery:
                _updateHandler.OnInlineQueryReceived(update.InlineQuery!);
                return;

            default:
                throw new NotImplementedException();
        }
    }

    private async Task SetupWebhookAsync(UpdateType[] allowedUpdates)
    {
        await _client.SetWebhookAsync(
            _configuration.Webhook,
            allowedUpdates: allowedUpdates,
            dropPendingUpdates: true);

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