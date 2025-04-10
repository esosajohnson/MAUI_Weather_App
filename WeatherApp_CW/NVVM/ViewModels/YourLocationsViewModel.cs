using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Firebase.Auth;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using Microsoft.Maui.Devices.Sensors;
using WeatherApp_CW.NVVM.Models;

namespace WeatherApp_CW.NVVM.ViewModels
{
    public partial class YourLocationsViewModel : ObservableObject
    {
        private readonly HttpClient _client = new();

        [ObservableProperty]
        private string searchQuery;

        [ObservableProperty]
        private ObservableCollection<SavedLocationWeather> savedLocations = new();

        // Replace this with your own Firebase Realtime Database URL
        private const string FirebaseUrl = "https://weatherapp-1d487-default-rtdb.firebaseio.com/";

        public IRelayCommand SaveLocationCommand { get; }

        public YourLocationsViewModel()
        {
            SaveLocationCommand = new RelayCommand(async () => await SaveLocationAsync());
        }

        private async Task SaveLocationAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchQuery))
                return;

            var location = (await Geocoding.Default.GetLocationsAsync(SearchQuery)).FirstOrDefault();
            if (location == null)
            {
                await App.Current.MainPage.DisplayAlert("Error", "Location not found.", "OK");
                return;
            }

            var url = $"https://api.open-meteo.com/v1/forecast?latitude={location.Latitude}&longitude={location.Longitude}&current=temperature_2m,weather_code";
            var response = await _client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                await App.Current.MainPage.DisplayAlert("Error", "Failed to fetch weather data.", "OK");
                return;
            }

            var data = await System.Text.Json.JsonSerializer.DeserializeAsync<WeatherData>(
    await response.Content.ReadAsStreamAsync());

            var current = data?.current;

            if (current == null)
            {
                await App.Current.MainPage.DisplayAlert("Error", "Weather data unavailable.", "OK");
                return;
            }

            // Get authenticated user's Firebase token
            var token = Preferences.Get("MyFirebaseToken", string.Empty);
            if (string.IsNullOrEmpty(token))
            {
                await App.Current.MainPage.DisplayAlert("Error", "User not authenticated.", "OK");
                return;
            }

            var auth = JsonConvert.DeserializeObject<FirebaseAuthLink>(token);
            var userId = auth.User.LocalId;

            var savedLocation = new SavedLocationWeather
            {
                LocationName = SearchQuery,
                Temperature = current.temperature_2m,
                WeatherCode = current.weather_code,
                SavedAt = DateTime.Now,
                UserId = userId
            };

            var firebaseUrl = $"{FirebaseUrl}/savedLocations/{userId}.json?auth={auth.FirebaseToken}";
            var json = JsonConvert.SerializeObject(savedLocation);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var firebaseResponse = await _client.PostAsync(firebaseUrl, content);
            var result = await firebaseResponse.Content.ReadAsStringAsync();

            if (!firebaseResponse.IsSuccessStatusCode)
            {
                await App.Current.MainPage.DisplayAlert("Firebase Error", result, "OK");
                return;
            }

            // Add to local collection for UI display
            SavedLocations.Add(savedLocation);
            SearchQuery = string.Empty;
        }

        public async Task LoadSavedLocationsAsync()
        {
            // Get the current user's Firebase token
            var token = Preferences.Get("MyFirebaseToken", string.Empty);
            if (string.IsNullOrEmpty(token))
            {
                await App.Current.MainPage.DisplayAlert("Error", "User not authenticated.", "OK");
                return;
            }

            var auth = JsonConvert.DeserializeObject<FirebaseAuthLink>(token);
            var userId = auth.User.LocalId;

            // Construct the URL with ?auth=
            var firebaseUrl = $"{FirebaseUrl}/savedLocations/{userId}.json?auth={auth.FirebaseToken}";

            try
            {
                var response = await _client.GetAsync(firebaseUrl);
                if (!response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    await App.Current.MainPage.DisplayAlert("Error loading locations", result, "OK");
                    return;
                }

                var json = await response.Content.ReadAsStringAsync();

                var locationDict = JsonConvert.DeserializeObject<Dictionary<string, SavedLocationWeather>>(json);

                if (locationDict != null)
                {
                    SavedLocations.Clear();

                    foreach (var item in locationDict.Values)
                    {
                        SavedLocations.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
        }

    }
}
