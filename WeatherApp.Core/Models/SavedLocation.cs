using CommunityToolkit.Mvvm.ComponentModel;

namespace WeatherApp.Core.Models;

/// <summary>A location the user saved, with its last known weather.</summary>
public partial class SavedLocation : ObservableObject
{
    public SavedLocation(string name)
    {
        Name = name;
    }

    /// <summary>Key of the record in the Firebase Realtime Database.</summary>
    public string Id { get; set; } = string.Empty;

    public string Name { get; }

    /// <summary>Coordinates, stored so refreshing doesn't need to geocode again (null for older records).</summary>
    public GeoLocation? Coordinates { get; set; }

    /// <summary>Always stored in °C; use <see cref="DisplayTemperature"/> for the UI.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayTemperature))]
    public partial double TemperatureCelsius { get; set; }

    [ObservableProperty]
    public partial int WeatherCode { get; set; }

    [ObservableProperty]
    public partial DateTimeOffset UpdatedAt { get; set; }

    /// <summary>Unit chosen in Settings, set by the view model.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayTemperature))]
    public partial TemperatureUnit DisplayUnit { get; set; }

    public string DisplayTemperature =>
        Temperature.Format(Temperature.FromCelsius(TemperatureCelsius, DisplayUnit), DisplayUnit);
}
