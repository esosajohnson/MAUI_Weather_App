using System.Globalization;

namespace WeatherApp.Core.Models;

public static class Temperature
{
    public static double FromCelsius(double celsius, TemperatureUnit unit) =>
        unit == TemperatureUnit.Fahrenheit ? celsius * 9 / 5 + 32 : celsius;

    public static string Symbol(TemperatureUnit unit) =>
        unit == TemperatureUnit.Fahrenheit ? "°F" : "°C";

    /// <summary>Formats a value that is already in <paramref name="unit"/>, e.g. "12.3°C".</summary>
    public static string Format(double value, TemperatureUnit unit, IFormatProvider? provider = null) =>
        value.ToString("F1", provider ?? CultureInfo.CurrentCulture) + Symbol(unit);
}
