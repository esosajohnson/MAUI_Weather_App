using System.Collections.ObjectModel;
using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using WeatherApp_CW.NVVM.Models;

namespace WeatherApp_CW.NVVM.ViewModels
{
    public partial class WeatherViewModel : ObservableObject
    {
        private readonly HttpClient _client;

        [ObservableProperty]
        private string locationName;

        [ObservableProperty]
        private string currentTemperature;

        [ObservableProperty]
        private string precipitationProbability;

        [ObservableProperty]
        private int weatherCode;

        [ObservableProperty]
        private string sunriseTime;

        [ObservableProperty]
        private string sunsetTime;

        [ObservableProperty]
        private ObservableCollection<DailyDisplay> fiveDayForecast = new();

        public DateTime CurrentDateTime { get; set; } = DateTime.Now;

        public WeatherData WeatherData { get; set; }

        public ICommand SearchCommand { get; }

        // Constructor (retreived from GitHub Copilot to fix updating issue)
        public WeatherViewModel()
        {
            _client = new HttpClient();
            SearchCommand = new Command(async searchText => await SearchAsync(searchText?.ToString()));

            var defaultLocation = Preferences.Get("DefaultLocation", string.Empty);
            if (!string.IsNullOrEmpty(defaultLocation))
            {
                _ = SearchAsync(defaultLocation);
            }
        }

        public class DailyDisplay
        {
            public string DayOfWeek { get; set; }
            public int WeatherCode { get; set; }
            public float TempMin { get; set; }
            public float TempMax { get; set; }
        }

        private async Task SearchAsync(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return;

            try
            {
                LocationName = searchText;
                var location = (await Geocoding.Default.GetLocationsAsync(searchText))?.FirstOrDefault();
                if (location == null)
                {
                    await Shell.Current.DisplayAlert("Not found", $"Couldn't find \"{searchText}\".", "OK");
                    return;
                }

                await GetWeather(location);
            }
            catch (Exception ex)
            {
                // Network/geocoding failures must not crash the app (this runs from a fire-and-forget command)
                System.Diagnostics.Debug.WriteLine($"Weather load failed: {ex}");
                if (Shell.Current != null)
                    await Shell.Current.DisplayAlert("Error", "Couldn't load the weather. Check your connection and try again.", "OK");
            }
        }

        private async Task GetWeather(Location location)
        {
            var useFahrenheit = Preferences.Get("TempUnit", "Celsius (°C)").Contains("Fahrenheit");
            var lat = location.Latitude.ToString(CultureInfo.InvariantCulture);
            var lon = location.Longitude.ToString(CultureInfo.InvariantCulture);

            // Fetch Data from Open Meteo API.
            // timezone=auto returns sunrise/sunset in the location's local time (default is GMT).
            // temperature_unit lets the API do the °C/°F conversion.
            var url = $"https://api.open-meteo.com/v1/forecast?latitude={lat}&longitude={lon}" +
                      "&daily=temperature_2m_min,temperature_2m_max,weather_code,sunrise,sunset" +
                      "&hourly=temperature_2m" +
                      "&current=temperature_2m,precipitation,precipitation_probability,weather_code,is_day,rain,wind_speed_10m" +
                      "&timezone=auto" +
                      (useFahrenheit ? "&temperature_unit=fahrenheit" : string.Empty);

            using var response = await _client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            await using var responseStream = await response.Content.ReadAsStreamAsync();
            WeatherData = await JsonSerializer.DeserializeAsync<WeatherData>(responseStream);

            var daily = WeatherData?.daily;
            if (daily?.time != null && daily.time.Length > 0)
            {
                SunriseTime = ParseApiTime(daily.sunrise[0]).ToString("h:mm tt");
                SunsetTime = ParseApiTime(daily.sunset[0]).ToString("h:mm tt");

                FiveDayForecast.Clear();

                var days = Math.Min(5, daily.time.Length); // first 5 days (or fewer if the API returns less)
                for (int i = 0; i < days; i++)
                {
                    FiveDayForecast.Add(new DailyDisplay
                    {
                        DayOfWeek = ParseApiTime(daily.time[i]).DayOfWeek.ToString(),
                        WeatherCode = daily.weather_code[i],
                        TempMin = daily.temperature_2m_min[i],
                        TempMax = daily.temperature_2m_max[i]
                    });
                }
            }

            var current = WeatherData?.current;
            if (current != null)
            {
                CurrentTemperature = $"{current.temperature_2m:F1}{(useFahrenheit ? "°F" : "°C")}";
                PrecipitationProbability = $"{current.precipitation_probability}%";
                WeatherCode = current.weather_code;
            }
        }

        // Open-Meteo returns ISO 8601 times ("2026-11-24T07:32"), so parse culture-independently
        private static DateTime ParseApiTime(string value) =>
            DateTime.Parse(value, CultureInfo.InvariantCulture);
    }
}
