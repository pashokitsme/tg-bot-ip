using Newtonsoft.Json;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace Example.Commands.Weather;

public class WeatherForecaster
{
    private const string API_CURRENT_WEATHER = "https://api.openweathermap.org/data/2.5/weather?lat={0}&lon={1}&appid={2}&units=metric&lang=ru";
    private const string API_GEOCODE_BY_NAME = "https://api.openweathermap.org/geo/1.0/direct?q={0}&limit={1}&appid={2}";

    private readonly string _token;

    public WeatherForecaster(string token) => _token = token;

    [ChatCommand("/weather", "Текущая погода, нужно указать город")]
    private async Task<bool> WeatherForecastTodayCommand(ChatCommandContext context)
    {
        if (context.Args.Length < 1)
            return false;

        var city = string.Join(' ', context.Args);

        _ = context.Client.SendChatActionAsync(context.Message.Chat.Id, ChatAction.Typing);
        var message = await context.Client.SendTextMessageAsync(context.Message.Chat.Id, "`Ожидайте`", ParseMode.Markdown);

        var info = (await CityNameToCoordinates(city))
            .OrderByDescending(x => x.Country == "RU")
            .FirstOrDefault();

        if (info == null)
        {
            _ = context.Client.DeleteMessageAsync(message.Chat.Id, message.MessageId);
            return false;
        }

        var response = FormatWeatherInfo(await GetWeatherInfo(info.Lat, info.Lon));
        _ = context.Client.EditMessageTextAsync(message.Chat.Id, message.MessageId, response, ParseMode.Markdown);
        return true;
    }

    private static string FormatWeatherInfo(WeatherForecastResponse weather)
    {
        var builder = new StringBuilder($"`{weather.Name}`\n");
        builder.AppendLine($"Температура: `{Math.Round(weather.Main.Temp)}℃`. Ощущается, как `{Math.Round(weather.Main.FeelsLike)}℃`");
        builder.AppendLine($"Ветер: `{weather.Wind.Speed} м/с, {weather.Wind.Deg}°`");
        builder.AppendLine($"Облачность `{weather.Clouds.All}%`. Влажность `{weather.Main.Humidity}%`. Давление: `{weather.Main.Pressure} мм.рт.ст.`");

        return builder.ToString();
    }

    private async Task<List<WeatherCityInfo>> CityNameToCoordinates(string query, int limit = 10)
    {
        var response = await DoRequest(string.Format(API_GEOCODE_BY_NAME, query, limit, _token));
        return response.IsSuccessStatusCode == false ? null : JsonConvert.DeserializeObject<List<WeatherCityInfo>>(await response.Content.ReadAsStringAsync());
    }

    private async Task<WeatherForecastResponse> GetWeatherInfo(double lat, double lon)
    {
        var response = await DoRequest(string.Format(API_CURRENT_WEATHER, lat, lon, _token));
        return response.IsSuccessStatusCode == false ? null : JsonConvert.DeserializeObject<WeatherForecastResponse>(await response.Content.ReadAsStringAsync());
    }

    private static async Task<HttpResponseMessage> DoRequest(string url)
    {
        using var client = new HttpClient();
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri(url)
        };

        return await client.SendAsync(request);
    }
}