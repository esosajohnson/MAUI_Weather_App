using System.Collections.ObjectModel;
using System.Globalization;
using System.Net.Http;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Firebase.Auth;
using Newtonsoft.Json;
using WeatherApp_CW.NVVM.Models;
using WeatherApp_CW.Services;

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

        // Firebase URL (no trailing slash; paths below start with "/")
        private const string FirebaseUrl = "https://weatherapp-1d487-default-rtdb.firebaseio.com";

        public IAsyncRelayCommand SaveLocationCommand { get; }
        public IAsyncRelayCommand RefreshLocationsCommand { get; }
        public IAsyncRelayCommand<SavedLocationWeather> DeleteLocationCommand { get; }

        // Constructor with commands
        public YourLocationsViewModel()
        {
            SaveLocationCommand = new AsyncRelayCommand(SaveLocationAsync);
            RefreshLocationsCommand = new AsyncRelayCommand(RefreshLocationsAsync);
            DeleteLocationCommand = new AsyncRelayCommand<SavedLocationWeather>(DeleteLocationAsync);
        }

        // Gets a valid (refreshed if needed) session, or tells the user to sign in again
        private static async Task<FirebaseAuth?> RequireAuthAsync()
        {
            var auth = await AuthSession.GetValidAuthAsync();
            if (auth == null)
            {
                await Shell.Current.DisplayAlert("Signed out", "Your session has expired. Please sign in again.", "OK");
                await Shell.Current.GoToAsync("//SignInView");
            }
            return auth;
        }

        private static string UserUrl(FirebaseAuth auth, string? key = null) =>
            $"{FirebaseUrl}/savedLocations/{auth.User.LocalId}{(key == null ? "" : "/" + key)}.json?auth={auth.FirebaseToken}";

        // Current temperature (°C) and weather code for a place
        private async Task<Current?> FetchCurrentAsync(Location location)
        {
            var lat = location.Latitude.ToString(CultureInfo.InvariantCulture);
            var lon = location.Longitude.ToString(CultureInfo.InvariantCulture);
            var url = $"https://api.open-meteo.com/v1/forecast?latitude={lat}&longitude={lon}&current=temperature_2m,weather_code";

            using var response = await _client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return null;

            await using var stream = await response.Content.ReadAsStreamAsync();
            var data = await System.Text.Json.JsonSerializer.DeserializeAsync<WeatherData>(stream);
            return data?.current;
        }

        // Save a new location for the signed-in user
        private async Task SaveLocationAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchQuery))
                return;

            try
            {
                // Geocode the location
                var location = (await Geocoding.Default.GetLocationsAsync(SearchQuery))?.FirstOrDefault();
                if (location == null)
                {
                    await Shell.Current.DisplayAlert("Error", "Location not found.", "OK");
                    return;
                }

                var current = await FetchCurrentAsync(location);
                if (current == null)
                {
                    await Shell.Current.DisplayAlert("Error", "Weather data unavailable.", "OK");
                    return;
                }

                var auth = await RequireAuthAsync();
                if (auth == null)
                    return;

                var savedLocation = new SavedLocationWeather
                {
                    LocationName = SearchQuery,
                    Temperature = current.temperature_2m,
                    WeatherCode = current.weather_code,
                    SavedAt = DateTime.Now,
                    UserId = auth.User.LocalId
                };

                // Save to Firebase
                var json = JsonConvert.SerializeObject(savedLocation);
                using var content = new StringContent(json, Encoding.UTF8, "application/json");
                using var firebaseResponse = await _client.PostAsync(UserUrl(auth), content);

                if (!firebaseResponse.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine($"Firebase save failed: {await firebaseResponse.Content.ReadAsStringAsync()}");
                    await Shell.Current.DisplayAlert("Error", "Couldn't save this location. Please try again.", "OK");
                    return;
                }

                // Firebase POST returns {"name": "<generated key>"}; keep it so the item can be deleted/updated later
                var result = JsonConvert.DeserializeObject<Dictionary<string, string>>(await firebaseResponse.Content.ReadAsStringAsync());
                if (result != null && result.TryGetValue("name", out var key))
                    savedLocation.FirebaseKey = key;

                // Add to local collection for UI display
                SavedLocations.Add(savedLocation);
                SearchQuery = string.Empty;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SaveLocationAsync failed: {ex}");
                await Shell.Current.DisplayAlert("Error", "Couldn't save this location. Check your connection and try again.", "OK");
            }
        }

        // Load saved locations from Firebase
        public async Task LoadSavedLocationsAsync()
        {
            try
            {
                // No alert here, to prevent a popup on every tab switch
                var auth = await AuthSession.GetValidAuthAsync();
                if (auth == null)
                    return;

                using var response = await _client.GetAsync(UserUrl(auth));
                var json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine($"Error loading Firebase: {json}");
                    return;
                }

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
                System.Diagnostics.Debug.WriteLine($"Unexpected error in LoadSavedLocationsAsync: {ex.Message}");
            }
        }

        // Refresh saved locations
        private async Task RefreshLocationsAsync()
        {
            IsRefreshing = true;

            try
            {
                var auth = await RequireAuthAsync();
                if (auth == null)
                    return;

                var failed = false;

                foreach (var location in SavedLocations)
                {
                    var geo = (await Geocoding.Default.GetLocationsAsync(location.LocationName))?.FirstOrDefault();
                    if (geo == null) continue;

                    var current = await FetchCurrentAsync(geo);
                    if (current == null) continue;

                    // Update local metrics
                    location.Temperature = current.temperature_2m;
                    location.WeatherCode = current.weather_code;
                    location.SavedAt = DateTime.Now;

                    // Update in Firebase
                    if (!string.IsNullOrEmpty(location.FirebaseKey))
                    {
                        var json = JsonConvert.SerializeObject(location);
                        using var content = new StringContent(json, Encoding.UTF8, "application/json");
                        using var updateResponse = await _client.PatchAsync(UserUrl(auth, location.FirebaseKey), content);

                        if (!updateResponse.IsSuccessStatusCode)
                        {
                            System.Diagnostics.Debug.WriteLine($"Firebase update failed: {await updateResponse.Content.ReadAsStringAsync()}");
                            failed = true;
                        }
                    }
                }

                if (failed)
                    await Shell.Current.DisplayAlert("Error", "Some locations couldn't be updated.", "OK");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"RefreshLocationsAsync failed: {ex}");
                await Shell.Current.DisplayAlert("Error", "Couldn't refresh. Check your connection and try again.", "OK");
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        // Delete a saved location
        private async Task DeleteLocationAsync(SavedLocationWeather? location)
        {
            if (location == null || string.IsNullOrEmpty(location.FirebaseKey))
                return;

            try
            {
                var auth = await RequireAuthAsync();
                if (auth == null)
                    return;

                using var response = await _client.DeleteAsync(UserUrl(auth, location.FirebaseKey));

                if (response.IsSuccessStatusCode)
                {
                    SavedLocations.Remove(location);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Firebase delete failed: {await response.Content.ReadAsStringAsync()}");
                    await Shell.Current.DisplayAlert("Error", "Couldn't delete this location.", "OK");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DeleteLocationAsync failed: {ex}");
                await Shell.Current.DisplayAlert("Error", "Couldn't delete this location. Check your connection and try again.", "OK");
            }
        }
    }
}
