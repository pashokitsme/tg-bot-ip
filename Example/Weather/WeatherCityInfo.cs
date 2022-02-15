using Newtonsoft.Json;

namespace Example.Weather;

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