namespace Example.Configuration;

public interface ITelegramBotConfiguration
{
	public string Token { get; }
	public string Webhook { get; }
	public string ListeningUrl { get; }
	public string Route { get; }
	public string OpenWeatherToken { get; }
}