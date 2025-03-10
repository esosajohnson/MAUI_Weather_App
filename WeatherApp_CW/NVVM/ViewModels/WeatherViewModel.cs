namespace WeatherApp_CW.NVVM.ViewModels;

public class WeatherViewModel
{
    private async Task<Location> GetCoordinatesAsync(string address)
    {
        IEnumerable<Location> locations = await Geocoding.Default.GetLocationsAsync(address);
        
        Location location = locations?.FirstOrDefault();

        if (location != null)
            Console.WriteLine($"Latitude: {location.Latitude}, Longitude: {location.Longitude}, Altitude:{location.Altitude}");
        return location;
    }
}