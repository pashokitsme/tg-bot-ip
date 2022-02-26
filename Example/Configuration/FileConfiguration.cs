using Newtonsoft.Json;

namespace Example.Configuration
{
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
}
