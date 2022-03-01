using Newtonsoft.Json;

namespace Example.Commands.Weather;

public class Main
{
    [JsonProperty("temp")] public double Temp;

    [JsonProperty("feels_like")] public double FeelsLike;

    [JsonProperty("temp_min")] public double TempMin;

    [JsonProperty("temp_max")] public double TempMax;

    [JsonProperty("pressure")] public int Pressure;

    [JsonProperty("humidity")] public int Humidity;
}

public class Wind
{
    [JsonProperty("speed")] public double Speed;

    [JsonProperty("deg")] public int Deg;
    [JsonProperty("pressure")] public int Pressure;
}

public class Clouds
{
    [JsonProperty("all")] public int All;
}

public class WeatherForecastResponse
{
    [JsonProperty("main")] public Main Main;

    [JsonProperty("wind")] public Wind Wind;

    [JsonProperty("name")] public string Name;

    [JsonProperty("clouds")] public Clouds Clouds;
}

public class WeatherCityInfo
{
    [JsonProperty("name")]
    public string Name;

    [JsonProperty("lat")]
    public double Lat;

    [JsonProperty("lon")]
    public double Lon;

    [JsonProperty("country")]
    public string Country;

    [JsonProperty("state")]
    public string State;
}