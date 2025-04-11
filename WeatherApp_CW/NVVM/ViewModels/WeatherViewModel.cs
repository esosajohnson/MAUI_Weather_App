using System.Text.Json;
using System.Windows.Input;
using WeatherApp_CW.NVVM.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Devices.Sensors;
using System.Collections.ObjectModel;


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

        public WeatherViewModel()
        {
            _client = new HttpClient();
            var defaultLocation = Preferences.Get("DefaultLocation", string.Empty);
            if (!string.IsNullOrEmpty(defaultLocation))
            {
                _ = LoadWeatherForDefaultLocation(defaultLocation);
            }
        }

        public class DailyDisplay
        {
            public string DayOfWeek { get; set; }
            public int WeatherCode { get; set; }
            public float TempMin { get; set; }
            public float TempMax { get; set; }
        }


        public ICommand SearchCommand => new Command(async (searchText) =>
        {
            if (string.IsNullOrWhiteSpace(searchText?.ToString()))
                return;

            LocationName = searchText.ToString();
            var location = await GetCoordinatesAsync(LocationName);
            await GetWeather(location);
        });

        private async Task LoadWeatherForDefaultLocation(string locationName)
        {
            LocationName = locationName;

            var location = (await Geocoding.Default.GetLocationsAsync(locationName)).FirstOrDefault();
            if (location != null)
            {
                await GetWeather(location);
            }
        }


        private async Task GetWeather(Location location)
        {
            if (location == null)
                return;

            var url = $"https://api.open-meteo.com/v1/forecast?latitude={location.Latitude}&longitude={location.Longitude}" +
                      $"&daily=temperature_2m_min,temperature_2m_max,weather_code,sunrise,sunset" +
                      $"&hourly=temperature_2m" +
                      $"&current=temperature_2m,precipitation,weather_code,is_day,rain,wind_speed_10m";

            var response = await _client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                using (var responseStream = await response.Content.ReadAsStreamAsync())
                {
                    var data = await JsonSerializer.DeserializeAsync<WeatherData>(responseStream);
                    WeatherData = data;

                    if (WeatherData?.daily != null && WeatherData.daily.time.Length > 0)
                    {
                        SunriseTime = DateTime.Parse(WeatherData.daily.sunrise[0]).ToString("h:mm tt");
                        SunsetTime = DateTime.Parse(WeatherData.daily.sunset[0]).ToString("h:mm tt");

                        FiveDayForecast.Clear();

                        for (int i = 0; i < 5; i++) // first 5 days
                        {
                            var date = DateTime.Parse(WeatherData.daily.time[i]);
                            var dayOfWeek = date.DayOfWeek.ToString();

                            var unit = Preferences.Get("TempUnit", "Celsius (°C)");
                            var tempMin = WeatherData.daily.temperature_2m_min[i];
                            var tempMax = WeatherData.daily.temperature_2m_max[i];

                            if (unit.Contains("Fahrenheit"))
                            {
                                tempMin = (tempMin * 9 / 5) + 32;
                                tempMax = (tempMax * 9 / 5) + 32;
                            }

                            FiveDayForecast.Add(new DailyDisplay
                            {
                                DayOfWeek = dayOfWeek,
                                WeatherCode = WeatherData.daily.weather_code[i],
                                TempMin = tempMin,
                                TempMax = tempMax
                            });

                        }
                    }

                    if (WeatherData?.current != null)
                    {
                        var unit = Preferences.Get("TempUnit", "Celsius (°C)");
                        var temp = WeatherData.current.temperature_2m;

                        if (unit.Contains("Fahrenheit"))
                        {
                            temp = (temp * 9 / 5) + 32;
                            CurrentTemperature = $"{temp:F1}°F";
                        }
                        else
                        {
                            CurrentTemperature = $"{temp:F1}°C";
                        }
                        PrecipitationProbability = $"{WeatherData.current.precipitation}%";
                        WeatherCode = WeatherData.current.weather_code;
                    }
                }
            }
        }

        private async Task<Location> GetCoordinatesAsync(string address)
        {
            IEnumerable<Location> locations = await Geocoding.Default.GetLocationsAsync(address);
            return locations?.FirstOrDefault();
        }
    }
}
