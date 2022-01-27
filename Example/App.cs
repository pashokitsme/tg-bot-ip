using Example.Commands;
using Example.Commands.CallbackButtons;
using Example.Core;
using Example.Logging;
using Example.Pages;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Example;

public class App
{
    public static string ConfigurationFile { get; } = "config.json";

    private readonly TelegramBotConfiguration _configuration = TelegramBotConfiguration.Get(ConfigurationFile);
    private readonly TelegramApiListener _listener;
    private readonly TelegramBotClient _client;
    private readonly ChatCommandManager _commandManager;
    private readonly CallbackCommandManager _callbackManager;
    private readonly UpdateHandler _updateHandler;
    private readonly PageManager _pageManager;

    public App()
    {
        _listener = new TelegramApiListener(_configuration);
        _client = new TelegramBotClient(_configuration.Token);
        _commandManager = new ChatCommandManager(_client);
        _callbackManager = new CallbackCommandManager(_client);
        _updateHandler = new UpdateHandler(_client, _commandManager, _callbackManager);
        _pageManager = new PageManager(_client);


        _listener.UpdateReceived += (update) => Task.Run(() => OnUpdateReceived(update));
    }

    public async void StartAsync(UpdateType[] allowedUpdates)
    {
        ConfigureCommands();
        await SetupWebhookAsync(allowedUpdates);
        await SetupBotCommandsAsync();
        _listener.Start();
    }

    [ChatCommand("start", "start command", true)]
    private async Task<bool> OnStartCommand(ChatCommandContext context)
    {
        await context.Client.SendTextMessageAsync(context.Message.Chat.Id, "Привет!");
        return true;
    }

    public void Stop()
    {
        RemoveWebhookAsync().Wait();
        _listener.Stop();
    }

    private void OnUpdateReceived(Update update)
    {
        switch(update.Type)
        {
            case UpdateType.Message:
                _updateHandler.OnMessageReceived(update.Message);
                return;

            case UpdateType.CallbackQuery:
                _updateHandler.OnCallbackReceived(update.CallbackQuery);
                return;
        }
    }

    private void ConfigureCommands()
    {
        var basic = new BasicCommands();
        _commandManager.Register(this);
        _commandManager.Register(basic);
        _commandManager.Register(_pageManager);
        _callbackManager.Register(basic);
        _callbackManager.Register(_pageManager);
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