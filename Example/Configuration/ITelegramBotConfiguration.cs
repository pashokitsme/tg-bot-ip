namespace Example.Configuration;

public interface ITelegramBotConfiguration
{
	public string Token { get; }
	public string Webhook { get; }
	public int ListeningPort { get; }
	public string Route { get; }
	public string OpenWeatherToken { get; }
}