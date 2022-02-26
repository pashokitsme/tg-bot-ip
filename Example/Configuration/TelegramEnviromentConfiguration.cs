namespace Example.Configuration;

public class TelegramEnviromentConfiguration : ITelegramBotConfiguration
{
	public string Token { get; } = Environment.GetEnvironmentVariable("token");
	public string Webhook { get; } = Environment.GetEnvironmentVariable("webhook");
	public string ListeningUrl { get; }
	public string Route { get; } = Environment.GetEnvironmentVariable("route");
	public string OpenWeatherToken { get; } = Environment.GetEnvironmentVariable("openweather-token");
	
	public TelegramEnviromentConfiguration()
	{
		ListeningUrl = string.Format(Environment.GetEnvironmentVariable("listening-url") ?? "http://+:{0}/", Environment.GetEnvironmentVariable("PORT"));
		
	}
}