using YamlDotNet.Serialization;

namespace Example.Core;

internal class TelegramBotConfiguration
{
    [YamlMember(Alias = "bot-token")]
    public string Token { get; set; } = "<your token>";

    [YamlMember(Alias = "host")]
    public string Host { get; set; } = "https://example.com/";

    [YamlMember(Alias = "route")]
    public string Route { get; set; } = "/<your route>/";

    public static TelegramBotConfiguration Deserialize(string path)
    {
        var deserializer = new DeserializerBuilder().Build();

        if (File.Exists(path) == false)
        {
            var serializer = new SerializerBuilder().Build();
            var config = new TelegramBotConfiguration();
            File.WriteAllText(path, serializer.Serialize(config));

            return config;
        }

        var yaml = File.ReadAllText(path);

        return deserializer.Deserialize<TelegramBotConfiguration>(yaml);
    }
}

