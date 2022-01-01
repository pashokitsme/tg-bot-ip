using Example.Logging;
using Newtonsoft.Json;

namespace Example.Core;

internal class TelegramBotConfiguration
{
    [JsonProperty("bot-token"), JsonRequired]
    public string Token { get; set; } = "<your token>";

    [JsonProperty("host"), JsonRequired]
    public string Host { get; set; } = "https://example.com/";
   
    [JsonProperty("listening-address"), JsonRequired]
    public string ListeningAddress { get; set; } = "http://localhost:5000/";

    [JsonProperty("route"), JsonRequired]
    public string Route { get; set; } = "/<your route>/";

    public static TelegramBotConfiguration Get(string path)
    {
        if (File.Exists(path) == false)
        {
            Logger.Log($"Configuration file is not exists ({path})", LogSeverity.ERROR);
            return CreateNew(path);
        }

        var json = File.ReadAllText(path);
        var result = JsonConvert.DeserializeObject<TelegramBotConfiguration>(json);

        if (result == null)
        {
            Logger.Log($"Can't parse configuration file: {path}", LogSeverity.ERROR);
            return CreateNew(path);
        }

        return result;
    }

    public static TelegramBotConfiguration CreateNew(string path)
    {
        Logger.Log("Creating new configuration file");
        var config = new TelegramBotConfiguration();
        File.WriteAllText(path, JsonConvert.SerializeObject(config, Formatting.Indented));
        return config;
    }
}

