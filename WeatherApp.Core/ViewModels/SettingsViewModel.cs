using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WeatherApp.Core.Models;
using WeatherApp.Core.Services;

namespace WeatherApp.Core.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly ISettingsService _settings;
    private readonly IDialogService _dialogs;

    public SettingsViewModel(ISettingsService settings, IDialogService dialogs)
    {
        _settings = settings;
        _dialogs = dialogs;

        SelectedTemperatureUnit = settings.TemperatureUnit;
        DefaultLocation = settings.DefaultLocation;
    }

    public IReadOnlyList<TemperatureUnit> TemperatureUnits { get; } = Enum.GetValues<TemperatureUnit>();

    [ObservableProperty]
    public partial TemperatureUnit SelectedTemperatureUnit { get; set; }

    [ObservableProperty]
    public partial string? DefaultLocation { get; set; }

    [RelayCommand]
    private async Task SaveAsync()
    {
        _settings.TemperatureUnit = SelectedTemperatureUnit;
        _settings.DefaultLocation = DefaultLocation?.Trim() ?? string.Empty;

        await _dialogs.ShowAlertAsync("Saved", "Your preferences have been updated.");
    }
}
