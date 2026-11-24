using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using WeatherApp.Core.Models;
using WeatherApp.Core.Services;

namespace WeatherApp.Core.ViewModels;

public partial class YourLocationsViewModel(
    IWeatherService weatherService,
    IGeocodingService geocoding,
    ISavedLocationsService savedLocations,
    ISettingsService settings,
    IDialogService dialogs,
    INavigationService navigation,
    TimeProvider time,
    ILogger<YourLocationsViewModel> logger) : ObservableObject
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveLocationCommand))]
    public partial string? SearchQuery { get; set; }

    [ObservableProperty]
    public partial bool IsRefreshing { get; set; }

    public ObservableCollection<SavedLocation> SavedLocations { get; } = [];

    /// <summary>Loads the list when the page appears. Fails quietly, so switching tabs never shows a popup.</summary>
    public async Task OnAppearingAsync()
    {
        try
        {
            var locations = await savedLocations.GetAllAsync();
            var unit = settings.TemperatureUnit;

            SavedLocations.Clear();
            foreach (var location in locations)
            {
                location.DisplayUnit = unit;
                SavedLocations.Add(location);
            }
        }
        catch (AuthException ex) when (ex.Error == AuthError.NotSignedIn)
        {
            SavedLocations.Clear();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Loading saved locations failed");
        }
    }

    private bool CanSaveLocation() => !string.IsNullOrWhiteSpace(SearchQuery);

    [RelayCommand(CanExecute = nameof(CanSaveLocation))]
    private async Task SaveLocationAsync()
    {
        var name = SearchQuery!.Trim();

        if (SavedLocations.Any(l => string.Equals(l.Name, name, StringComparison.CurrentCultureIgnoreCase)))
        {
            await dialogs.ShowAlertAsync("Already saved", $"\"{name}\" is already in your locations.");
            return;
        }

        await RunAsync("save this location", async () =>
        {
            var coordinates = await geocoding.FindAsync(name);
            if (coordinates is null)
            {
                await dialogs.ShowAlertAsync("Not found", $"Couldn't find \"{name}\".");
                return;
            }

            var forecast = await weatherService.GetForecastAsync(coordinates, TemperatureUnit.Celsius);
            var location = new SavedLocation(name)
            {
                Coordinates = coordinates,
                TemperatureCelsius = forecast.Current.Temperature,
                WeatherCode = forecast.Current.WeatherCode,
                UpdatedAt = time.GetLocalNow(),
                DisplayUnit = settings.TemperatureUnit
            };

            await savedLocations.AddAsync(location);
            SavedLocations.Add(location);
            SearchQuery = string.Empty;
        });
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        IsRefreshing = true;
        try
        {
            await RunAsync("refresh your locations", async () =>
            {
                var failed = 0;
                foreach (var location in SavedLocations.ToList())
                {
                    try
                    {
                        // Older records have no coordinates; look them up once and store them
                        location.Coordinates ??= await geocoding.FindAsync(location.Name);
                        if (location.Coordinates is null)
                        {
                            failed++;
                            continue;
                        }

                        var forecast = await weatherService.GetForecastAsync(location.Coordinates, TemperatureUnit.Celsius);
                        location.TemperatureCelsius = forecast.Current.Temperature;
                        location.WeatherCode = forecast.Current.WeatherCode;
                        location.UpdatedAt = time.GetLocalNow();

                        await savedLocations.UpdateAsync(location);
                    }
                    catch (Exception ex) when (ex is not AuthException)
                    {
                        logger.LogWarning(ex, "Refreshing {Location} failed", location.Name);
                        failed++;
                    }
                }

                if (failed > 0)
                    await dialogs.ShowAlertAsync("Error", $"{failed} location(s) couldn't be updated.");
            });
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private Task DeleteAsync(SavedLocation? location)
    {
        if (location is null)
            return Task.CompletedTask;

        return RunAsync("delete this location", async () =>
        {
            await savedLocations.DeleteAsync(location);
            SavedLocations.Remove(location);
        });
    }

    // Shared error handling: an expired session sends the user to sign in, anything else shows a message
    private async Task RunAsync(string action, Func<Task> work)
    {
        try
        {
            await work();
        }
        catch (AuthException ex) when (ex.Error == AuthError.NotSignedIn)
        {
            await dialogs.ShowAlertAsync("Signed out", "Your session has expired. Please sign in again.");
            await navigation.GoToAsync(Routes.SignIn);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to {Action}", action);
            await dialogs.ShowAlertAsync("Error", $"Couldn't {action}. Check your connection and try again.");
        }
    }
}
