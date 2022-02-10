using Example.Logging;
using Newtonsoft.Json;

namespace Example.Core
{
    internal abstract class Configuration<T> where T : class, new()
    {
        public static T Get(string path)
        {
            if (File.Exists(path) == false)
            {
                Logger.Log($"Configuration file is not exists ({path})", LogSeverity.ERROR);
                return CreateNew(path);
            }

            var json = File.ReadAllText(path);
            try
            {
                var result = JsonConvert.DeserializeObject<T>(json);

                if (result == null)
                {
                    Logger.Log($"Can't parse configuration file: {path}", LogSeverity.ERROR);
                    return CreateNew(path);
                }

                return result;
            }
            catch
            { 
                return CreateNew(path);
            }
        }

        public static T CreateNew(string path)
        {
            Logger.Log("Creating new configuration file");
            var config = new T();
            File.WriteAllText(path, JsonConvert.SerializeObject(config, Formatting.Indented));
            return config;
        }
    }
}
