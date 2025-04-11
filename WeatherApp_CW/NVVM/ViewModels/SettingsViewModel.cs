using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Storage;
using System.Collections.ObjectModel;

namespace WeatherApp_CW.NVVM.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<string> temperatureUnits = new() { "Celsius (°C)", "Fahrenheit (°F)" };

        [ObservableProperty]
        private string selectedTemperatureUnit;

        [ObservableProperty]
        private string defaultLocation;

        public IRelayCommand SaveSettingsCommand { get; }

        public SettingsViewModel()
        {
            SaveSettingsCommand = new RelayCommand(SaveSettings);
            LoadSettings();
        }

        private void LoadSettings()
        {
            SelectedTemperatureUnit = Preferences.Get("TempUnit", "Celsius (°C)");
            DefaultLocation = Preferences.Get("DefaultLocation", string.Empty);
        }
        private void SaveSettings()
        {
            Preferences.Set("TempUnit", SelectedTemperatureUnit);
            Preferences.Set("DefaultLocation", DefaultLocation);

            App.Current.MainPage.DisplayAlert("Saved", "Your preferences have been updated.", "OK");
        }
    }
}
