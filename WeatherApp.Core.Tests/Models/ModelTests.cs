using System.Globalization;
using WeatherApp.Core.Models;

namespace WeatherApp.Core.Tests.Models;

public class WeatherCodesTests
{
    [Theory]
    [InlineData(0, "Clear Sky", "sunny.json")]
    [InlineData(2, "Partly Cloudy", "partlyCloudy.json")]
    [InlineData(45, "Foggy", "fog.json")]
    [InlineData(56, "Freezing Drizzle", "partlyShower.json")]
    [InlineData(67, "Freezing Rain", "partlyShower.json")]
    [InlineData(77, "Snow Grains", "snow.json")]
    [InlineData(86, "Snow Showers", "snow.json")]
    [InlineData(81, "Rain Showers", "shower.json")]
    [InlineData(99, "Thunderstorm with Hail", "storm.json")]
    public void Maps_known_codes(int code, string description, string animation)
    {
        Assert.Equal(description, WeatherCodes.Describe(code));
        Assert.Equal(animation, WeatherCodes.AnimationFile(code));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(4)]
    [InlineData(1000)]
    public void Unknown_codes_fall_back_safely(int code)
    {
        Assert.Equal("Unknown", WeatherCodes.Describe(code));
        Assert.Equal("cloudy.json", WeatherCodes.AnimationFile(code));
    }

    [Fact]
    public void Every_code_with_a_description_has_an_animation_file()
    {
        // Guards against adding a code to one mapping but not the other
        for (var code = 0; code <= 99; code++)
        {
            if (WeatherCodes.Describe(code) != "Unknown")
                Assert.EndsWith(".json", WeatherCodes.AnimationFile(code));
        }
    }
}

public class TemperatureTests
{
    [Theory]
    [InlineData(0, 32)]
    [InlineData(100, 212)]
    [InlineData(-40, -40)]
    public void Converts_celsius_to_fahrenheit(double celsius, double fahrenheit) =>
        Assert.Equal(fahrenheit, Temperature.FromCelsius(celsius, TemperatureUnit.Fahrenheit), precision: 6);

    [Fact]
    public void Celsius_is_unchanged() =>
        Assert.Equal(21.5, Temperature.FromCelsius(21.5, TemperatureUnit.Celsius));

    [Theory]
    [InlineData(TemperatureUnit.Celsius, "12.3°C")]
    [InlineData(TemperatureUnit.Fahrenheit, "12.3°F")]
    public void Formats_with_one_decimal_and_symbol(TemperatureUnit unit, string expected) =>
        Assert.Equal(expected, Temperature.Format(12.34, unit, CultureInfo.InvariantCulture));

    [Fact]
    public void Saved_location_displays_in_the_chosen_unit_and_notifies_on_change()
    {
        var location = new SavedLocation("Paris") { TemperatureCelsius = 20 };
        var changed = new List<string?>();
        location.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        location.DisplayUnit = TemperatureUnit.Fahrenheit;

        Assert.EndsWith("°F", location.DisplayTemperature);
        Assert.StartsWith("68", location.DisplayTemperature);
        Assert.Contains(nameof(SavedLocation.DisplayTemperature), changed);
    }
}
