using Newtonsoft.Json;

namespace Example.Core;

public class TelegramBotConfiguration : Configuration<TelegramBotConfiguration>
{
    [JsonProperty("bot-token"), JsonRequired]
    public string Token { get; set; } = "<your_token>";

    [JsonProperty("webhook"), JsonRequired]
    public string Webhook { get; set; } = "https://example.com/<your_route>/";

    [JsonProperty("listening-port"), JsonRequired]
    public int ListeningPort { get; set; } = 5000;

    [JsonProperty("route"), JsonRequired]
    public string Route { get; set; } = "/<your_route>/";

    [JsonProperty("openweather-token"), JsonRequired]
    public string OpenWeatherToken = "";
}

