namespace Example.Configuration;

public class TelegramEnviromentConfiguration : ITelegramBotConfiguration
{
	public string Token { get; }
	public string Webhook { get; }
	public int ListeningPort { get; }
	public string Route { get; }
	public string OpenWeatherToken { get; }

	public TelegramEnviromentConfiguration()
	{
		Token = Environment.GetEnvironmentVariable("token");
		Webhook = Environment.GetEnvironmentVariable("webhook");
		ListeningPort = int.Parse(Environment.GetEnvironmentVariable("PORT") ?? "-1");
		Route = Environment.GetEnvironmentVariable("route");
		OpenWeatherToken = Environment.GetEnvironmentVariable("open-weather-token");
	}
}