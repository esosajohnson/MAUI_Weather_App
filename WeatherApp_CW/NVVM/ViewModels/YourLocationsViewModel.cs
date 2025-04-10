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
        private bool isRefreshing;

        [ObservableProperty]
        private ObservableCollection<SavedLocationWeather> savedLocations = new();

        // Replace this with your own Firebase Realtime Database URL
        private const string FirebaseUrl = "https://weatherapp-1d487-default-rtdb.firebaseio.com/";

        public IRelayCommand SaveLocationCommand { get; }
        public IRelayCommand RefreshLocationsCommand { get; }
        public IRelayCommand<SavedLocationWeather> DeleteLocationCommand { get; }

        public YourLocationsViewModel()
        {
            SaveLocationCommand = new RelayCommand(async () => await SaveLocationAsync());
            RefreshLocationsCommand = new RelayCommand(async () => await RefreshLocationsAsync());
            DeleteLocationCommand = new RelayCommand<SavedLocationWeather>(async (location) => await DeleteLocationAsync(location));

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
            try
            {
                var token = Preferences.Get("MyFirebaseToken", string.Empty);
                if (string.IsNullOrEmpty(token))
                {
                    Console.WriteLine("No token found.");
                    return;
                }

                var auth = JsonConvert.DeserializeObject<FirebaseAuthLink>(token);
                var userId = auth?.User?.LocalId;
                var firebaseToken = auth?.FirebaseToken;

                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(firebaseToken))
                {
                    Console.WriteLine("Invalid Firebase token or user ID.");
                    return;
                }

                var firebaseUrl = $"{FirebaseUrl}/savedLocations/{userId}.json?auth={firebaseToken}";
                var response = await _client.GetAsync(firebaseUrl);

                if (!response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error loading Firebase: {result}");
                    return;
                }

                var json = await response.Content.ReadAsStringAsync();
                var locationDict = JsonConvert.DeserializeObject<Dictionary<string, SavedLocationWeather>>(json);

                SavedLocations.Clear();

                foreach (var kvp in locationDict ?? new Dictionary<string, SavedLocationWeather>())
                {
                    var item = kvp.Value;
                    item.FirebaseKey = kvp.Key;
                    SavedLocations.Add(item);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error in LoadSavedLocationsAsync: {ex.Message}");
                // Don't show alert here to prevent popup on tab switch
            }
        }


        private async Task RefreshLocationsAsync()
        {
            IsRefreshing = true;

            var token = Preferences.Get("MyFirebaseToken", string.Empty);
            var auth = JsonConvert.DeserializeObject<FirebaseAuthLink>(token);
            var userId = auth.User.LocalId;

            foreach (var location in SavedLocations)
            {
                var geo = (await Geocoding.Default.GetLocationsAsync(location.LocationName)).FirstOrDefault();
                if (geo == null) continue;

                var url = $"https://api.open-meteo.com/v1/forecast?latitude={geo.Latitude}&longitude={geo.Longitude}&current=temperature_2m,weather_code";
                var response = await _client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var data = await System.Text.Json.JsonSerializer.DeserializeAsync<WeatherData>(await response.Content.ReadAsStreamAsync());

                    if (data?.current != null)
                    {
                        // Update local values
                        location.Temperature = data.current.temperature_2m;
                        location.WeatherCode = data.current.weather_code;
                        location.SavedAt = DateTime.Now;

                        // 🔁 Update in Firebase
                        if (!string.IsNullOrEmpty(location.FirebaseKey))
                        {
                            var updateUrl = $"{FirebaseUrl}/savedLocations/{userId}/{location.FirebaseKey}.json?auth={auth.FirebaseToken}";
                            var json = JsonConvert.SerializeObject(location);
                            var content = new StringContent(json, Encoding.UTF8, "application/json");

                            var updateResponse = await _client.PatchAsync(updateUrl, content);

                            if (!updateResponse.IsSuccessStatusCode)
                            {
                                var error = await updateResponse.Content.ReadAsStringAsync();
                                await App.Current.MainPage.DisplayAlert("Firebase Update Error", error, "OK");
                            }
                        }
                    }
                }
            }

            IsRefreshing = false;
        }

        private async Task DeleteLocationAsync(SavedLocationWeather location)
        {
            if (location == null || string.IsNullOrEmpty(location.FirebaseKey))
                return;

            var token = Preferences.Get("MyFirebaseToken", string.Empty);
            var auth = JsonConvert.DeserializeObject<FirebaseAuthLink>(token);
            var userId = auth.User.LocalId;

            var deleteUrl = $"{FirebaseUrl}/savedLocations/{userId}/{location.FirebaseKey}.json?auth={auth.FirebaseToken}";
            var response = await _client.DeleteAsync(deleteUrl);

            if (response.IsSuccessStatusCode)
            {
                SavedLocations.Remove(location);
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                await App.Current.MainPage.DisplayAlert("Delete Error", error, "OK");
            }
        }

    }
}
