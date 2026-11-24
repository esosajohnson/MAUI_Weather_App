namespace WeatherApp.Core.Models;

/// <summary>
/// Meaning of the WMO weather interpretation codes returned by Open-Meteo.
/// See https://open-meteo.com/en/docs ("WMO Weather interpretation codes").
/// </summary>
public static class WeatherCodes
{
    public static string Describe(int code) => code switch
    {
        0 => "Clear Sky",
        1 => "Mainly Clear",
        2 => "Partly Cloudy",
        3 => "Overcast",
        45 or 48 => "Foggy",
        51 or 53 or 55 => "Drizzle",
        56 or 57 => "Freezing Drizzle",
        61 or 63 or 65 => "Rain",
        66 or 67 => "Freezing Rain",
        71 or 73 or 75 => "Snowfall",
        77 => "Snow Grains",
        80 or 81 or 82 => "Rain Showers",
        85 or 86 => "Snow Showers",
        95 => "Thunderstorm",
        96 or 99 => "Thunderstorm with Hail",
        _ => "Unknown"
    };

    /// <summary>Name of the Lottie animation file (in Resources/Raw) for a code.</summary>
    public static string AnimationFile(int code) => code switch
    {
        0 => "sunny.json",
        1 or 2 => "partlyCloudy.json",
        3 => "cloudy.json",
        45 or 48 => "fog.json",
        51 or 53 or 55 or 56 or 57 => "partlyShower.json",
        61 or 63 or 65 or 66 or 67 => "partlyShower.json",
        71 or 73 or 75 or 77 or 85 or 86 => "snow.json",
        80 or 81 or 82 => "shower.json",
        95 or 96 or 99 => "storm.json",
        _ => "cloudy.json"
    };
}
