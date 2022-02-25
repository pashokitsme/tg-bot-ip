namespace Example.Configuration;

public class TelegramEnviromentConfiguration : ITelegramBotConfiguration
{
	public string Token { get; }
	public string Webhook { get; }
	public string ListeningUrl { get; }
	public string Route { get; }
	public string OpenWeatherToken { get; }

	public TelegramEnviromentConfiguration()
	{
		Token = Environment.GetEnvironmentVariable("token");
		Webhook = Environment.GetEnvironmentVariable("webhook");
		Route = Environment.GetEnvironmentVariable("route");
		OpenWeatherToken = Environment.GetEnvironmentVariable("open-weather-token");
		
		var url = ListeningUrl = Environment.GetEnvironmentVariable("listening-url");
		ListeningUrl = string.Format(url, Environment.GetEnvironmentVariable("PORT"));
	}
}