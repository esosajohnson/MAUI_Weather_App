using WeatherApp.Core.Models;

namespace WeatherApp.Core.Services;

public interface ISettingsService
{
    TemperatureUnit TemperatureUnit { get; set; }

    /// <summary>Location loaded when the app starts; empty for none.</summary>
    string DefaultLocation { get; set; }
}
