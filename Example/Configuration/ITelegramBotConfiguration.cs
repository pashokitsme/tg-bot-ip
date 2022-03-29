namespace Example.Configuration;

public interface ITelegramBotConfiguration
{
    string Token { get; }
    string Webhook { get; }
    string ListeningUrl { get; }
    string Route { get; }
    string OpenWeatherToken { get; }
}