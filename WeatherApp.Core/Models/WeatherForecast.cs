namespace WeatherApp.Core.Models;

/// <summary>Conditions right now. Temperatures are in <see cref="WeatherForecast.Unit"/>.</summary>
public sealed record CurrentConditions(
    double Temperature,
    int PrecipitationProbability,
    int WeatherCode,
    bool IsDay,
    double WindSpeed);

/// <summary>One day of the forecast. Times are local to the forecast location.</summary>
public sealed record DailyForecast(
    DateOnly Date,
    double MinTemperature,
    double MaxTemperature,
    int WeatherCode,
    DateTime Sunrise,
    DateTime Sunset);

public sealed record WeatherForecast(
    CurrentConditions Current,
    IReadOnlyList<DailyForecast> Daily,
    TemperatureUnit Unit);
