namespace Example.Configuration;

public class TelegramEnviromentConfiguration : ITelegramBotConfiguration
{
    public string Token { get; } = Environment.GetEnvironmentVariable("token");
    public string Webhook { get; } = Environment.GetEnvironmentVariable("webhook");
    public string ListeningUrl { get; } = string.Format(Environment.GetEnvironmentVariable("listening-url") ?? "http://+:{0}/", Environment.GetEnvironmentVariable("PORT"));
    public string Route { get; } = Environment.GetEnvironmentVariable("route");
    public string OpenWeatherToken { get; } = Environment.GetEnvironmentVariable("openweather-token");
}