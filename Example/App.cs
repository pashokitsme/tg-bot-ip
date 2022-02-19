using Example.Commands;
using Example.Commands.Buttons;
using Example.Core;
using Example.Logging;
using Example.Weather;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Example;

public class App
{
    private const string CONFIGURATION_FILE = "config.json";

    private readonly TelegramBotConfiguration _configuration = TelegramBotConfiguration.Get(CONFIGURATION_FILE);
    private readonly TelegramApiListener _listener;
    private readonly TelegramBotClient _client;
    private readonly ChatCommandManager _commands;
    private readonly CallbackCommandManager _callbacks;
    private readonly UpdateHandler _updateHandler;

    private readonly object[] _commandContainers;
    public App()
    {
        _listener = new TelegramApiListener(_configuration);
        _client = new TelegramBotClient(_configuration.Token);
        _commands = new ChatCommandManager(_client);
        _callbacks = new CallbackCommandManager(_client);
        _updateHandler = new UpdateHandler(_client, _commands, _callbacks);

        _commandContainers = new object[]
        {
            new WeatherForecaster(_configuration.OpenWeatherToken),
            new BasicCommands(),
            this
        };

        _listener.UpdateReceived += update => Task.Run(() => OnUpdateReceived(update));
    }
    
    [ChatCommand("start", "start command", true)]
    private async Task<bool> OnStartCommand(ChatCommandContext context)
    {
        await context.Client.SendTextMessageAsync(context.Message.Chat.Id, "Привет!");
        return true;
    }

    public async void StartAsync(UpdateType[] allowedUpdates)
    {
        ConfigureCommands(_commandContainers);
        await SetupWebhookAsync(allowedUpdates);
        await SetupBotCommandsAsync();
        _listener.Start();
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
            
            default:
                throw new ArgumentOutOfRangeException(nameof(update));
        }
    }

    private void ConfigureCommands(IEnumerable<object> targets)
    {
        targets
            .AsParallel()
            .ForAll(target =>
        {
            _callbacks.Register(target);
            _commands.Register(target);
        });
    }

    private async Task SetupWebhookAsync(UpdateType[] allowedUpdates)
    {
        await _client.SetWebhookAsync(_configuration.Webhook, allowedUpdates: allowedUpdates, dropPendingUpdates: true);
        var webhook = await _client.GetWebhookInfoAsync();
        Logger.Log($"Webhook binded to {webhook.Url}");
    }

    private async Task SetupBotCommandsAsync()
    {
        var commands = _commands.GetBotCommands();
        await _client.SetMyCommandsAsync(commands);
        Logger.Log($"Setted up {commands.Length} commands");
    }

    private async Task RemoveWebhookAsync()
    {
        await _client.DeleteWebhookAsync(true);
        Logger.Log($"Webhook removed");
    }
}