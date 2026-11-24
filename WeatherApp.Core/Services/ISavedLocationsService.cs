using WeatherApp.Core.Models;

namespace WeatherApp.Core.Services;

/// <summary>The signed-in user's saved locations. All methods throw <see cref="AuthException"/> (NotSignedIn) when signed out.</summary>
public interface ISavedLocationsService
{
    Task<IReadOnlyList<SavedLocation>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>Stores a new location and sets its <see cref="SavedLocation.Id"/>.</summary>
    Task AddAsync(SavedLocation location, CancellationToken cancellationToken = default);

    Task UpdateAsync(SavedLocation location, CancellationToken cancellationToken = default);

    Task DeleteAsync(SavedLocation location, CancellationToken cancellationToken = default);
}
