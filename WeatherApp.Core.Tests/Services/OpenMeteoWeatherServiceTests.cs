using System.Globalization;
using System.Net;
using WeatherApp.Core.Models;
using WeatherApp.Core.Services.OpenMeteo;
using WeatherApp.Core.Tests.TestDoubles;

namespace WeatherApp.Core.Tests.Services;

public class OpenMeteoWeatherServiceTests
{
    private readonly FakeHttpMessageHandler _http = new();

    private OpenMeteoWeatherService CreateService() => new(_http.CreateClient());

    [Fact]
    public void Url_requests_local_time_and_the_forecast_fields()
    {
        var url = OpenMeteoWeatherService.BuildUrl(TestData.London, TemperatureUnit.Celsius);

        Assert.StartsWith(OpenMeteoWeatherService.BaseUrl, url);
        Assert.Contains("&timezone=auto", url);
        Assert.Contains("precipitation_probability", url);
        Assert.Contains("&forecast_days=5", url);
        Assert.DoesNotContain("temperature_unit", url);
    }

    [Fact]
    public void Url_asks_the_api_for_fahrenheit()
    {
        var url = OpenMeteoWeatherService.BuildUrl(TestData.London, TemperatureUnit.Fahrenheit);

        Assert.Contains("&temperature_unit=fahrenheit", url);
    }

    [Fact]
    public void Url_uses_dots_for_coordinates_even_on_a_comma_decimal_device()
    {
        var original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("es-ES");

            var url = OpenMeteoWeatherService.BuildUrl(new GeoLocation(40.4168, -3.7038), TemperatureUnit.Celsius);

            Assert.Contains("latitude=40.4168&longitude=-3.7038", url);
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }

    [Fact]
    public async Task Maps_the_response_into_a_forecast()
    {
        _http.RespondWith(TestData.OpenMeteoJson);

        var forecast = await CreateService().GetForecastAsync(TestData.London, TemperatureUnit.Celsius);

        Assert.Equal(9.4, forecast.Current.Temperature);
        Assert.Equal(35, forecast.Current.PrecipitationProbability);
        Assert.Equal(61, forecast.Current.WeatherCode);
        Assert.True(forecast.Current.IsDay);

        var today = forecast.Daily[0];
        Assert.Equal(new DateOnly(2026, 11, 24), today.Date);
        Assert.Equal(4.1, today.MinTemperature);
        Assert.Equal(10.2, today.MaxTemperature);
        Assert.Equal(new DateTime(2026, 11, 24, 7, 32, 0), today.Sunrise);
        Assert.Equal(new DateTime(2026, 11, 24, 16, 1, 0), today.Sunset);
    }

    [Fact]
    public async Task Keeps_at_most_five_days()
    {
        _http.RespondWith(TestData.OpenMeteoJson); // contains 7 days

        var forecast = await CreateService().GetForecastAsync(TestData.London, TemperatureUnit.Celsius);

        Assert.Equal(5, forecast.Daily.Count);
    }

    [Fact]
    public async Task Copes_with_fewer_days_and_missing_precipitation()
    {
        _http.RespondWith("""
            { "current": { "temperature_2m": 1.0, "weather_code": 0, "is_day": 0, "wind_speed_10m": 0 },
              "daily": { "time": ["2026-11-24"], "temperature_2m_min": [0], "temperature_2m_max": [2],
                         "weather_code": [0], "sunrise": ["2026-11-24T07:00"], "sunset": ["2026-11-24T16:00"] } }
            """);

        var forecast = await CreateService().GetForecastAsync(TestData.London, TemperatureUnit.Celsius);

        Assert.Single(forecast.Daily);
        Assert.Equal(0, forecast.Current.PrecipitationProbability);
        Assert.False(forecast.Current.IsDay);
    }

    [Fact]
    public async Task Throws_HttpRequestException_on_server_error()
    {
        _http.RespondWith("""{"error": true, "reason": "Latitude must be in range"}""", HttpStatusCode.BadRequest);

        await Assert.ThrowsAsync<HttpRequestException>(
            () => CreateService().GetForecastAsync(TestData.London, TemperatureUnit.Celsius));
    }
}
