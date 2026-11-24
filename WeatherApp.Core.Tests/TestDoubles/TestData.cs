using WeatherApp.Core.Models;
using WeatherApp.Core.Services;

namespace WeatherApp.Core.Tests.TestDoubles;

public static class TestData
{
    public static readonly GeoLocation London = new(51.5072, -0.1276);

    public static UserSession Session(DateTimeOffset? expiresAt = null) =>
        new("user-1", "me@example.com", "id-token", "refresh-token", expiresAt ?? DateTimeOffset.UtcNow.AddHours(1));

    public static WeatherForecast Forecast(double temperature = 12.3, int weatherCode = 3, TemperatureUnit unit = TemperatureUnit.Celsius, int days = 5) =>
        new(
            new CurrentConditions(temperature, 40, weatherCode, IsDay: true, WindSpeed: 10),
            Enumerable.Range(0, days)
                .Select(i => new DailyForecast(
                    new DateOnly(2026, 11, 24).AddDays(i),
                    MinTemperature: 5 + i,
                    MaxTemperature: 10 + i,
                    WeatherCode: 1,
                    Sunrise: new DateTime(2026, 11, 24, 7, 32, 0),
                    Sunset: new DateTime(2026, 11, 24, 16, 5, 0)))
                .ToList(),
            unit);

    /// <summary>A trimmed real response from api.open-meteo.com with timezone=auto.</summary>
    public const string OpenMeteoJson = """
        {
          "latitude": 51.5, "longitude": -0.12, "timezone": "Europe/London",
          "current": { "time": "2026-11-24T14:00", "temperature_2m": 9.4, "precipitation_probability": 35,
                       "weather_code": 61, "is_day": 1, "wind_speed_10m": 14.2 },
          "daily": {
            "time": ["2026-11-24","2026-11-25","2026-11-26","2026-11-27","2026-11-28","2026-11-29","2026-11-30"],
            "temperature_2m_min": [4.1, 3.0, 2.2, 5.5, 6.0, 1.0, 0.5],
            "temperature_2m_max": [10.2, 9.8, 8.1, 11.0, 12.4, 7.0, 6.5],
            "weather_code": [61, 3, 0, 80, 95, 71, 2],
            "sunrise": ["2026-11-24T07:32","2026-11-25T07:34","2026-11-26T07:35","2026-11-27T07:37","2026-11-28T07:38","2026-11-29T07:40","2026-11-30T07:41"],
            "sunset": ["2026-11-24T16:01","2026-11-25T16:00","2026-11-26T15:59","2026-11-27T15:58","2026-11-28T15:57","2026-11-29T15:57","2026-11-30T15:56"]
          }
        }
        """;
}
