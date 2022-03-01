using Example.Commands;
using Example.Configuration;
using Example.Weather;
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

	private readonly object[] _commandContainers;

	private bool _run = false;

	public App(ITelegramBotConfiguration configuration)
	{
		_configuration = configuration;
		_listener = new TelegramApiListener(_configuration);
		_client = new TelegramBotClient(_configuration.Token);
		_commands = new ChatCommandManager(_client);

		_commandContainers = new object[]
		{
			new WeatherForecaster(_configuration.OpenWeatherToken),
			new BasicCommands(),
			this
		};

		_listener.UpdateReceived += update => Task.Run(() => OnUpdateReceived(update));
	}

	public async void StartAsync(UpdateType[] allowedUpdates)
	{
		ConfigureCommands(_commandContainers);
		await SetupWebhookAsync(allowedUpdates);
		await SetupBotCommandsAsync();
		_listener.Start();
		_run = true;
	}

    public void Stop()
    {
        if (_run == true)
            _listener.Stop();

        _run = false;
    }

    private void OnUpdateReceived(Update update)
	{
		var message = update.Message;

		if (message == null || message.Type != MessageType.Text || message.Text == null)
		{
			Logger.Log($"Message should be {UpdateType.Message}/{MessageType.Text} but given {(message == null ? "null" : message.Type)}", LogSeverity.Error);
			return;
		}

		if (message.Text[0] != '/')
			return;

		var substring = message.Text.Split(' ')[0];
		if (_commands.TryExecute(substring, message) == false)
			_ = _client.SendTextMessageAsync(message.Chat.Id, $@"Не удалось выполнить команду {message.Text.Split(' ')[0]} 😢");
	}
	
	[ChatCommand("start", "start command", true)]
	private static Task<bool> OnStartCommand(ChatCommandContext context)
	{
		_ = context.Client.SendTextMessageAsync(context.Message.Chat.Id, "Привет!");
		return Task.FromResult(true);
	}

    private void ConfigureCommands(object[] targets)
	{
		foreach (var target in targets)
			_commands.Register(target);
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

	private async Task RemoveWebhookAsync()
	{
		await _client.DeleteWebhookAsync(true);
		Logger.Log($"Webhook removed");
	}
}