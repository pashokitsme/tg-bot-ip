namespace Example.Configuration;

public class TelegramEnviromentConfiguration : ITelegramBotConfiguration
{
	public string Token { get; } = Environment.GetEnvironmentVariable("token");
	public string Webhook { get; } = Environment.GetEnvironmentVariable("webhook");
	public string ListeningUrl { get; } = Environment.GetEnvironmentVariable("listening-url");
	public string Route { get; } = Environment.GetEnvironmentVariable("route");
	public string OpenWeatherToken { get; } = Environment.GetEnvironmentVariable("openweather-token");
}