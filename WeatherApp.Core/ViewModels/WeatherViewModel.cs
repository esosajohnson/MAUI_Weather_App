using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using WeatherApp.Core.Models;
using WeatherApp.Core.Services;

namespace WeatherApp.Core.ViewModels;

/// <summary>One row of the 5-day forecast list.</summary>
public sealed record DailyForecastItem(string DayOfWeek, int WeatherCode, double MaxTemperature, double MinTemperature);

public partial class WeatherViewModel(
    IWeatherService weatherService,
    IGeocodingService geocoding,
    ISettingsService settings,
    IDialogService dialogs,
    ILogger<WeatherViewModel> logger) : ObservableObject
{
    // What is currently displayed, so it can be reloaded when the unit setting changes
    private (string Name, GeoLocation Location)? _shown;
    private TemperatureUnit? _shownUnit;

    [ObservableProperty]
    public partial string? LocationName { get; set; }

    [ObservableProperty]
    public partial string? CurrentTemperature { get; set; }

    [ObservableProperty]
    public partial string? PrecipitationProbability { get; set; }

    [ObservableProperty]
    public partial int WeatherCode { get; set; }

    [ObservableProperty]
    public partial string? SunriseTime { get; set; }

    [ObservableProperty]
    public partial string? SunsetTime { get; set; }

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    public DateTime CurrentDateTime => DateTime.Now;

    public ObservableCollection<DailyForecastItem> FiveDayForecast { get; } = [];

    /// <summary>Called when the page appears: loads the default location the first time, and reloads if the unit changed.</summary>
    public async Task OnAppearingAsync()
    {
        if (_shown is { } shown)
        {
            if (_shownUnit != settings.TemperatureUnit)
                await LoadForecastAsync(shown.Name, shown.Location);
            return;
        }

        if (!string.IsNullOrWhiteSpace(settings.DefaultLocation))
            await SearchAsync(settings.DefaultLocation);
    }

    [RelayCommand]
    private async Task SearchAsync(string? query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return;

        query = query.Trim();
        IsBusy = true;
        try
        {
            var location = await geocoding.FindAsync(query);
            if (location is null)
            {
                await dialogs.ShowAlertAsync("Not found", $"Couldn't find \"{query}\".");
                return;
            }

            await LoadForecastAsync(query, location);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Searching for {Query} failed", query);
            await dialogs.ShowAlertAsync("Error", "Couldn't load the weather. Check your connection and try again.");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadForecastAsync(string name, GeoLocation location)
    {
        var unit = settings.TemperatureUnit;
        var forecast = await weatherService.GetForecastAsync(location, unit);

        LocationName = name;
        CurrentTemperature = Temperature.Format(forecast.Current.Temperature, unit);
        PrecipitationProbability = $"{forecast.Current.PrecipitationProbability}%";
        WeatherCode = forecast.Current.WeatherCode;

        var today = forecast.Daily.FirstOrDefault();
        SunriseTime = today?.Sunrise.ToString("h:mm tt");
        SunsetTime = today?.Sunset.ToString("h:mm tt");

        FiveDayForecast.Clear();
        foreach (var day in forecast.Daily)
        {
            FiveDayForecast.Add(new DailyForecastItem(
                day.Date.DayOfWeek.ToString(), day.WeatherCode, day.MaxTemperature, day.MinTemperature));
        }

        _shown = (name, location);
        _shownUnit = unit;
    }
}
