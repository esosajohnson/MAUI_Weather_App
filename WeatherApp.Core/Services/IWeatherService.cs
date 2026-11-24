using WeatherApp.Core.Models;

namespace WeatherApp.Core.Services;

public interface IWeatherService
{
    /// <summary>Current conditions and the next days' forecast for a location.</summary>
    /// <exception cref="HttpRequestException">The weather API could not be reached or returned an error.</exception>
    Task<WeatherForecast> GetForecastAsync(GeoLocation location, TemperatureUnit unit, CancellationToken cancellationToken = default);
}
