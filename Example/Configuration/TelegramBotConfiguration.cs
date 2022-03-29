using Newtonsoft.Json;

namespace Example.Configuration;

public class TelegramJsonConfiguration : FileConfiguration<TelegramJsonConfiguration>, ITelegramBotConfiguration
{
    [JsonProperty("bot-token"), JsonRequired]
    public string Token { get; set; } = "<your_token>";

    [JsonProperty("webhook"), JsonRequired]
    public string Webhook { get; set; } = "https://example.com/<your_route>/";

    [JsonProperty("listening-url"), JsonRequired]
    public string ListeningUrl { get; set; } = "http://localhost:5000/";

    [JsonProperty("route"), JsonRequired] 
    public string Route { get; set; } = "/<your_route>/";

    [JsonProperty("openweather-token"), JsonRequired]
    public string OpenWeatherToken { get; set; } = "<your_token>";
};

public abstract class FileConfiguration<T> where T : class, new()
{
    public static T Get(string path)
    {
        if (File.Exists(path) == false)
        {
            Logger.Log($"Configuration file is not exists ({path})", LogSeverity.Error);
            return CreateNew(path);
        }

        try
        {
            var result = JsonConvert.DeserializeObject<T>(File.ReadAllText(path));

            if (result != null)
                return result;

            Logger.Log($"Can't parse configuration file: {path}", LogSeverity.Error);
            return CreateNew(path);
        }
        catch
        {
            return CreateNew(path);
        }
    }

    private static T CreateNew(string path)
    {
        Logger.Log("Creating new configuration file");
        var config = new T();
        File.WriteAllText(path, JsonConvert.SerializeObject(config, Formatting.Indented));
        return config;
    }
}