using System.Globalization;
using System.Net.Http.Json;
using WeatherApp.Core.Models;

namespace WeatherApp.Core.Services.OpenMeteo;

/// <summary>Weather from the free Open-Meteo API (https://open-meteo.com).</summary>
public sealed class OpenMeteoWeatherService(HttpClient http) : IWeatherService
{
    public const string BaseUrl = "https://api.open-meteo.com/v1/forecast";
    public const int ForecastDays = 5;

    public async Task<WeatherForecast> GetForecastAsync(GeoLocation location, TemperatureUnit unit, CancellationToken cancellationToken = default)
    {
        var response = await http.GetFromJsonAsync<OpenMeteoResponse>(BuildUrl(location, unit), cancellationToken)
            ?? throw new HttpRequestException("The weather service returned an empty response.");

        return Map(response, unit);
    }

    internal static string BuildUrl(GeoLocation location, TemperatureUnit unit)
    {
        // Coordinates must use '.' as the decimal separator whatever the device language is
        var lat = location.Latitude.ToString(CultureInfo.InvariantCulture);
        var lon = location.Longitude.ToString(CultureInfo.InvariantCulture);

        return $"{BaseUrl}?latitude={lat}&longitude={lon}" +
               "&current=temperature_2m,precipitation_probability,weather_code,is_day,wind_speed_10m" +
               "&daily=temperature_2m_min,temperature_2m_max,weather_code,sunrise,sunset" +
               $"&forecast_days={ForecastDays}" +
               "&timezone=auto" + // times in the location's local time instead of GMT
               (unit == TemperatureUnit.Fahrenheit ? "&temperature_unit=fahrenheit" : string.Empty);
    }

    internal static WeatherForecast Map(OpenMeteoResponse response, TemperatureUnit unit)
    {
        var current = response.Current
            ?? throw new HttpRequestException("The weather service response had no current conditions.");

        var conditions = new CurrentConditions(
            current.Temperature,
            current.PrecipitationProbability ?? 0,
            current.WeatherCode,
            current.IsDay == 1,
            current.WindSpeed);

        var daily = new List<DailyForecast>();
        if (response.Daily is { } d)
        {
            // The arrays should all be the same length, but don't rely on it
            var count = new[] { d.Time.Length, d.MinTemperature.Length, d.MaxTemperature.Length, d.WeatherCode.Length, d.Sunrise.Length, d.Sunset.Length }.Min();

            for (var i = 0; i < Math.Min(count, ForecastDays); i++)
            {
                daily.Add(new DailyForecast(
                    DateOnly.Parse(d.Time[i], CultureInfo.InvariantCulture),
                    d.MinTemperature[i],
                    d.MaxTemperature[i],
                    d.WeatherCode[i],
                    ParseLocalTime(d.Sunrise[i]),
                    ParseLocalTime(d.Sunset[i])));
            }
        }

        return new WeatherForecast(conditions, daily, unit);
    }

    // Open-Meteo returns ISO 8601 local times without an offset, e.g. "2026-11-24T07:32"
    private static DateTime ParseLocalTime(string value) =>
        DateTime.Parse(value, CultureInfo.InvariantCulture);
}
