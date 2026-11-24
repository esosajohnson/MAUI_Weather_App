using WeatherApp.Core.Models;

namespace WeatherApp.Core.Services;

public interface IGeocodingService
{
    /// <summary>Finds the coordinates of a place name, or null if it isn't found.</summary>
    Task<GeoLocation?> FindAsync(string query, CancellationToken cancellationToken = default);
}
