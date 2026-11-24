using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using WeatherApp.Core.Models;

namespace WeatherApp.Core.Services.Firebase;

/// <summary>Saved locations stored per user in the Firebase Realtime Database (REST API).</summary>
public sealed class FirebaseSavedLocationsService(HttpClient http, IAuthService auth, FirebaseOptions options)
    : ISavedLocationsService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public async Task<IReadOnlyList<SavedLocation>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var session = await auth.RequireSessionAsync();

        // Firebase returns {"<key>": {...}, ...}, or null when the user has nothing saved
        var records = await http.GetFromJsonAsync<Dictionary<string, SavedLocationRecord>?>(
            Url(session), JsonOptions, cancellationToken);

        return (records ?? [])
            .Select(kvp => kvp.Value.ToModel(kvp.Key))
            .OrderBy(l => l.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }

    public async Task AddAsync(SavedLocation location, CancellationToken cancellationToken = default)
    {
        var session = await auth.RequireSessionAsync();

        using var response = await http.PostAsJsonAsync(Url(session), SavedLocationRecord.From(location, session.UserId), JsonOptions, cancellationToken);
        response.EnsureSuccessStatusCode();

        // A POST returns the generated key: {"name": "-Nx..."}
        var created = await response.Content.ReadFromJsonAsync<PostResult>(cancellationToken);
        location.Id = created?.Name ?? throw new HttpRequestException("Firebase did not return a key for the new location.");
    }

    public async Task UpdateAsync(SavedLocation location, CancellationToken cancellationToken = default)
    {
        var session = await auth.RequireSessionAsync();

        using var response = await http.PatchAsJsonAsync(Url(session, location.Id), SavedLocationRecord.From(location, session.UserId), JsonOptions, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteAsync(SavedLocation location, CancellationToken cancellationToken = default)
    {
        var session = await auth.RequireSessionAsync();

        using var response = await http.DeleteAsync(Url(session, location.Id), cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    internal string Url(UserSession session, string? key = null)
    {
        var path = $"savedLocations/{Uri.EscapeDataString(session.UserId)}";
        if (!string.IsNullOrEmpty(key))
            path += "/" + Uri.EscapeDataString(key);

        return $"{options.DatabaseUrl.TrimEnd('/')}/{path}.json?auth={Uri.EscapeDataString(session.IdToken)}";
    }

    private sealed record PostResult([property: JsonPropertyName("name")] string? Name);
}

/// <summary>
/// Shape of a record in the database. Property names match what the original version of the
/// app wrote, so locations saved before this refactor still load.
/// </summary>
internal sealed class SavedLocationRecord
{
    public string? LocationName { get; set; }
    public double Temperature { get; set; }
    public int WeatherCode { get; set; }
    public DateTimeOffset SavedAt { get; set; }
    public string? UserId { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    public SavedLocation ToModel(string key) => new(LocationName ?? "Unknown")
    {
        Id = key,
        Coordinates = Latitude is { } lat && Longitude is { } lon ? new GeoLocation(lat, lon) : null,
        TemperatureCelsius = Temperature,
        WeatherCode = WeatherCode,
        UpdatedAt = SavedAt
    };

    public static SavedLocationRecord From(SavedLocation location, string userId) => new()
    {
        LocationName = location.Name,
        Temperature = location.TemperatureCelsius,
        WeatherCode = location.WeatherCode,
        SavedAt = location.UpdatedAt,
        UserId = userId,
        Latitude = location.Coordinates?.Latitude,
        Longitude = location.Coordinates?.Longitude
    };
}
