using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using SkiaSharp.Extended.UI.Controls;

namespace WeatherApp_CW.Converters
{
    public class WeatherCodeToLottieConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var code = (int)value;
            var lottieAnimation = new SKFileLottieImageSource();

            switch (code)
            {
                case 0:
                    lottieAnimation.File = "sunny.json";
                    return lottieAnimation;
                case 1:
                    lottieAnimation.File = "partlyCloudy.json";
                    return lottieAnimation;
                case 2:
                    lottieAnimation.File = "partlyCloudy.json";
                    return lottieAnimation;
                case 3:
                    lottieAnimation.File = "cloudy.json";
                    return lottieAnimation;
                case 45:
                case 48:
                    lottieAnimation.File = "fog.json";
                    return lottieAnimation;
                case 51:
                case 53:
                case 55:
                case 61:
                case 63:
                case 65:
                    lottieAnimation.File = "partlyShower.json";
                    return lottieAnimation;
                case 71:
                case 73:
                case 75:
                    lottieAnimation.File = "snow.json";
                    return lottieAnimation;
                case 80:
                case 81:
                case 82:
                    lottieAnimation.File = "shower.json";
                    return lottieAnimation;
                case 95:
                case 96:
                case 99:
                    lottieAnimation.File = "storm.json";
                    return lottieAnimation;
                default:
                    return "cloudy.json";
            }
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
