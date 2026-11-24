using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using SkiaSharp.Extended.UI.Controls;

namespace WeatherApp_CW.Converters
{
    // Maps WMO weather codes (as returned by Open-Meteo) to a Lottie animation
    public class WeatherCodeToLottieConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // Bindings can pass null or a non-int while the page is still loading
            var code = value is int i ? i : -1;

            var file = code switch
            {
                0 => "sunny.json",
                1 or 2 => "partlyCloudy.json",
                3 => "cloudy.json",
                45 or 48 => "fog.json",
                51 or 53 or 55 or 56 or 57 => "partlyShower.json",           // drizzle / freezing drizzle
                61 or 63 or 65 or 66 or 67 => "partlyShower.json",           // rain / freezing rain
                71 or 73 or 75 or 77 or 85 or 86 => "snow.json",             // snow, snow grains, snow showers
                80 or 81 or 82 => "shower.json",
                95 or 96 or 99 => "storm.json",
                _ => "cloudy.json"
            };

            return new SKFileLottieImageSource { File = file };
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
