using Newtonsoft.Json;

namespace Example.Configuration;

public class FileConfiguration : FileConfiguration<FileConfiguration>, ITelegramBotConfiguration
{
	[JsonProperty("bot-token"), JsonRequired]
	public string Token { get; set; } = "<your_token>";

	[JsonProperty("webhook"), JsonRequired]
	public string Webhook { get; set; } = "https://example.com/<your_route>/";

	[JsonProperty("listening-url"), JsonRequired]
	public string ListeningUrl { get; set; } = "http://localhost:5000/";

	[JsonProperty("route"), JsonRequired] public string Route { get; set; } = "/<your_route>/";

	[JsonProperty("openweather-token"), JsonRequired]
	public string OpenWeatherToken { get; set; } = "<your_token>";
};