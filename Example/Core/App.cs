using Example.Commands;
using Example.Commands.Weather;
using Example.Configuration;
using Example.UserStates;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Example.Core;

public class App
{
    private readonly ITelegramBotConfiguration _configuration;
    private readonly TelegramApiListener _listener;
    private readonly TelegramBotClient _client;
    private readonly ChatCommandManager _commands;
    private readonly UserStateManager _states;

    public App(ITelegramBotConfiguration configuration)
    {
        _configuration = configuration;
        _listener = new TelegramApiListener(_configuration);
        _client = new TelegramBotClient(_configuration.Token);
        _commands = new ChatCommandManager(_client);
        _states = new UserStateManager(_client);

        ConfigureCommands(new object[]
        {
            new WeatherForecaster(_configuration.OpenWeatherToken),
            new BasicCommands(),
            this
        });

        _listener.UpdateReceived += update => Task.Run(() => OnUpdateReceived(update));
    }

    public async void StartAsync(UpdateType[] allowedUpdates)
    {
        await SetupWebhookAsync(allowedUpdates);
        await SetupBotCommandsAsync();
        _listener.Start();
    }

    public void Stop() => _listener.Stop();

    [ChatCommand("/start", "start command", true)]
    private static void OnStartCommand(ChatCommandContext context) => context.Client.SendTextMessageAsync(context.Message.Chat.Id, "Привет!");

    private void OnUpdateReceived(Update update)
    {
        var message = update.Message;

        if (message == null || message.Type != MessageType.Text || message.Text == null)
        {
            Logger.Log($"Message should be {UpdateType.Message}/{MessageType.Text} but given {(message == null ? "null" : message.Type)}", LogSeverity.Error);
            return;
        }

        var substring = message.Text.Split(' ')[0];

        if (substring[0] == '/')
        {
            _commands.Execute(substring, message);
            return;
        }

        if (_states.Has(message.From.Id))
            _states.Update(message.From.Id, message);
    }

    private void ConfigureCommands(object[] targets)
    {
        foreach (var target in targets)
            _commands.Register(target);

        _commands.Register(
            new ChatCommandAttribute("/words", "Игра в слова"), 
            ctx => _states.EnterOrExitIfSame<WordsGame>(ctx.Message.From.Id));

        _commands.Register(
            new ChatCommandAttribute("/math", "Устный счёт"),
            ctx => _states.EnterOrExitIfSame<MathGame>(ctx.Message.From.Id));

        _commands.Register(
            new ChatCommandAttribute("/stop", "Закончить игру"),
           async ctx => await _states.Exit(ctx.Message.From.Id));
    }

    private async Task SetupWebhookAsync(UpdateType[] allowedUpdates)
    {
        await _client.SetWebhookAsync(_configuration.Webhook, allowedUpdates: allowedUpdates);
        var webhook = await _client.GetWebhookInfoAsync();
        Logger.Log($"Webhook binded to {webhook.Url}");
    }

    private async Task SetupBotCommandsAsync()
    {
        var commands = _commands.GetBotCommands();
        await _client.SetMyCommandsAsync(commands);
        Logger.Log($"Setted up {commands.Length} commands");
    }
}