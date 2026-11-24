using System.Text.Json.Serialization;

namespace WeatherApp.Core.Services.OpenMeteo;

// Wire format of https://api.open-meteo.com/v1/forecast (only the fields we request).
// Kept internal: the rest of the app works with the models in WeatherApp.Core.Models.

internal sealed class OpenMeteoResponse
{
    [JsonPropertyName("current")]
    public OpenMeteoCurrent? Current { get; set; }

    [JsonPropertyName("daily")]
    public OpenMeteoDaily? Daily { get; set; }
}

internal sealed class OpenMeteoCurrent
{
    [JsonPropertyName("temperature_2m")]
    public double Temperature { get; set; }

    [JsonPropertyName("precipitation_probability")]
    public int? PrecipitationProbability { get; set; }

    [JsonPropertyName("weather_code")]
    public int WeatherCode { get; set; }

    [JsonPropertyName("is_day")]
    public int IsDay { get; set; }

    [JsonPropertyName("wind_speed_10m")]
    public double WindSpeed { get; set; }
}

internal sealed class OpenMeteoDaily
{
    [JsonPropertyName("time")]
    public string[] Time { get; set; } = [];

    [JsonPropertyName("temperature_2m_min")]
    public double[] MinTemperature { get; set; } = [];

    [JsonPropertyName("temperature_2m_max")]
    public double[] MaxTemperature { get; set; } = [];

    [JsonPropertyName("weather_code")]
    public int[] WeatherCode { get; set; } = [];

    [JsonPropertyName("sunrise")]
    public string[] Sunrise { get; set; } = [];

    [JsonPropertyName("sunset")]
    public string[] Sunset { get; set; } = [];
}
