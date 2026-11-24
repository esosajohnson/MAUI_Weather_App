using System.Globalization;
using SkiaSharp.Extended.UI.Controls;
using WeatherApp.Core.Models;

namespace WeatherApp_CW.Converters
{
    /// <summary>WMO weather code to a description, e.g. 3 to "Overcast".</summary>
    public class WeatherCodeToDescriptionConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            WeatherCodes.Describe(value is int code ? code : -1);

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            throw new NotSupportedException();
    }

    /// <summary>WMO weather code to its Lottie animation.</summary>
    public class WeatherCodeToLottieConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            new SKFileLottieImageSource { File = WeatherCodes.AnimationFile(value is int code ? code : -1) };

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            throw new NotSupportedException();
    }
}
