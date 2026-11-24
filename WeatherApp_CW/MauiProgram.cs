using Microsoft.Extensions.Logging;
using SkiaSharp.Views.Maui.Controls.Hosting;
using WeatherApp.Core;
using WeatherApp.Core.Services;
using WeatherApp.Core.Services.Firebase;
using WeatherApp_CW.Services;
using WeatherApp_CW.Views;

namespace WeatherApp_CW
{
    public static class MauiProgram
    {
        // Firebase Web API key: it identifies the project, access is enforced by the database rules
        private static readonly FirebaseOptions Firebase = new(
            ApiKey: "AIzaSyCva-z-99c9dCpeVBpwkigDKWYQJnDBswE",
            DatabaseUrl: "https://weatherapp-1d487-default-rtdb.firebaseio.com");

        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseSkiaSharp()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("Aptos.ttf", "Aptos");
                    fonts.AddFont("Aptos-Light.ttf", "AptosLight");
                });

            // Services and view models from WeatherApp.Core
            builder.Services.AddWeatherAppCore(Firebase);

            // Platform implementations of the Core interfaces
            builder.Services.AddSingleton(Geocoding.Default);
            builder.Services.AddSingleton(Preferences.Default);
            builder.Services.AddSingleton(SecureStorage.Default);
            builder.Services.AddSingleton<IGeocodingService, MauiGeocodingService>();
            builder.Services.AddSingleton<ISettingsService, PreferencesSettingsService>();
            builder.Services.AddSingleton<ISessionStore, SecureSessionStore>();
            builder.Services.AddSingleton<IDialogService, ShellDialogService>();
            builder.Services.AddSingleton<INavigationService, ShellNavigationService>();

            // Pages are created by Shell through the container, so they get their view models injected
            builder.Services.AddTransient<SignInView>();
            builder.Services.AddTransient<SignUpView>();
            builder.Services.AddTransient<WeatherView>();
            builder.Services.AddTransient<YourLocationsView>();
            builder.Services.AddTransient<AccountView>();
            builder.Services.AddTransient<SettingsView>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
