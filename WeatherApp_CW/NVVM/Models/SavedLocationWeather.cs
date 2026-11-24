using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;

namespace WeatherApp_CW.NVVM.Models
{
    public partial class SavedLocationWeather : ObservableObject
    {
        [ObservableProperty]
        private string locationName;

        // Always stored in °C in Firebase; converted for display only
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(DisplayTemperature))]
        private float temperature;

        [ObservableProperty]
        private int weatherCode;

        [ObservableProperty]
        private DateTime savedAt;

        [ObservableProperty]
        private string userId;

        [ObservableProperty]
        private string firebaseKey;

        // Temperature formatted in the unit chosen in Settings
        [JsonIgnore]
        public string DisplayTemperature =>
            Preferences.Get("TempUnit", "Celsius (°C)").Contains("Fahrenheit")
                ? $"{Temperature * 9 / 5 + 32:F1}°F"
                : $"{Temperature:F1}°C";
    }
}
