using System.Text.Json;
using System.Windows.Input;
using WeatherApp_CW.NVVM.Models;

namespace WeatherApp_CW.NVVM.ViewModels;

public class WeatherViewModel
{

    public WeatherData WeatherData { get; set; }
    public string LocationName { get; set; }
    public DateTime CurrentDateTime { get; set; } = DateTime.Now;
    private HttpClient _client;

    public WeatherViewModel()
    {
        _client = new HttpClient();
    }

    public ICommand SearchCommand => new Command(async (searchText) =>
    {
        LocationName = searchText.ToString();
        var location = await GetCoordinatesAsync(searchText.ToString());
        await GetWeather(location);
    });


    private async Task GetWeather(Location location)
    {
        var url =
            $"https://api.open-meteo.com/v1/forecast?latitude={location.Latitude}&longitude={location.Longitude}&daily=weather_code,temperature_2m_max,temperature_2m_min,sunrise,sunset,precipitation_probability_max&hourly=temperature_2m";
        var response = await _client.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            using (var responseStream = await response.Content.ReadAsStreamAsync())
            {
                var data = await JsonSerializer.DeserializeAsync<WeatherData>(responseStream);
                WeatherData = data;
            }
        }
    }
    private async Task<Location> GetCoordinatesAsync(string address)
    {
        IEnumerable<Location> locations = await Geocoding.Default.GetLocationsAsync(address);
        
        Location location = locations?.FirstOrDefault();

        if (location != null)
            Console.WriteLine($"Latitude: {location.Latitude}, Longitude: {location.Longitude}, Altitude:{location.Altitude}");
        return location;
    }
}

