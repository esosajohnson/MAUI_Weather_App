using System.Text.Json;
using Microsoft.Extensions.Logging;
using WeatherApp.Core.Models;
using WeatherApp.Core.Services;

namespace WeatherApp_CW.Services;

// Thin adapters from the Core interfaces to MAUI APIs. All app logic lives in WeatherApp.Core.

public sealed class MauiGeocodingService(IGeocoding geocoding) : IGeocodingService
{
    public async Task<GeoLocation?> FindAsync(string query, CancellationToken cancellationToken = default)
    {
        var location = (await geocoding.GetLocationsAsync(query))?.FirstOrDefault();
        return location is null ? null : new GeoLocation(location.Latitude, location.Longitude);
    }
}

public sealed class PreferencesSettingsService(IPreferences preferences) : ISettingsService
{
    // Keys and values match what earlier versions of the app stored
    private const string TemperatureUnitKey = "TempUnit";
    private const string DefaultLocationKey = "DefaultLocation";

    public TemperatureUnit TemperatureUnit
    {
        get => preferences.Get(TemperatureUnitKey, string.Empty).Contains("Fahrenheit")
            ? TemperatureUnit.Fahrenheit
            : TemperatureUnit.Celsius;
        set => preferences.Set(TemperatureUnitKey, value == TemperatureUnit.Fahrenheit ? "Fahrenheit (°F)" : "Celsius (°C)");
    }

    public string DefaultLocation
    {
        get => preferences.Get(DefaultLocationKey, string.Empty);
        set => preferences.Set(DefaultLocationKey, value);
    }
}

/// <summary>Keeps the Firebase tokens in the platform's secure storage (Keystore, Keychain, DPAPI).</summary>
public sealed class SecureSessionStore(ISecureStorage secureStorage, IPreferences preferences, ILogger<SecureSessionStore> logger)
    : ISessionStore
{
    private const string Key = "FirebaseSession";

    // Earlier versions kept the token in plain preferences; remove it so it isn't left lying around
    private const string LegacyPreferencesKey = "MyFirebaseToken";

    public async Task<UserSession?> LoadAsync()
    {
        preferences.Remove(LegacyPreferencesKey);
        preferences.Remove("UserEmail");

        try
        {
            var json = await secureStorage.GetAsync(Key);
            return json is null ? null : JsonSerializer.Deserialize<UserSession>(json);
        }
        catch (Exception ex)
        {
            // e.g. keys lost after an Android backup restore: treat as signed out
            logger.LogWarning(ex, "Reading the saved session failed");
            secureStorage.Remove(Key);
            return null;
        }
    }

    public Task SaveAsync(UserSession session) =>
        secureStorage.SetAsync(Key, JsonSerializer.Serialize(session));

    public Task ClearAsync()
    {
        secureStorage.Remove(Key);
        return Task.CompletedTask;
    }
}

public sealed class ShellDialogService : IDialogService
{
    public Task ShowAlertAsync(string title, string message, string cancel = "OK") =>
        Shell.Current.DisplayAlertAsync(title, message, cancel);

    public Task<bool> ConfirmAsync(string title, string message, string accept = "Yes", string cancel = "No") =>
        Shell.Current.DisplayAlertAsync(title, message, accept, cancel);
}

public sealed class ShellNavigationService : INavigationService
{
    public Task GoToAsync(string route) => Shell.Current.GoToAsync(route);
}
