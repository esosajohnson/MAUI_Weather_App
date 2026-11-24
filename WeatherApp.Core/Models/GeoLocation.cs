namespace WeatherApp.Core.Models;

/// <summary>A point on the map, as returned by geocoding.</summary>
public sealed record GeoLocation(double Latitude, double Longitude);
