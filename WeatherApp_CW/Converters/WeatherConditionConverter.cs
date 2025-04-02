using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace WeatherApp_CW.Converters
{
    public class WeatherCodeToDescriptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int weatherCode)
            {
                return weatherCode switch
                {
                    0 => "Clear Sky",
                    1 => "Mainly Clear",
                    2 => "Partly Cloudy",
                    3 => "Overcast",
                    45 or 48 => "Foggy",
                    51 or 53 or 55 => "Drizzle",
                    61 or 63 or 65 => "Rain Showers",
                    71 or 73 or 75 => "Snowfall",
                    80 or 81 or 82 => "Rain Showers",
                    95 => "Thunderstorm",
                    96 or 99 => "Thunderstorm with Hail",
                    _ => "Unknown",
                };
            }

            return "Unknown";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
