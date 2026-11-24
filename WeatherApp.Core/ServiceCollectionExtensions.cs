using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using WeatherApp.Core.Services;
using WeatherApp.Core.Services.Firebase;
using WeatherApp.Core.Services.OpenMeteo;
using WeatherApp.Core.ViewModels;

namespace WeatherApp.Core;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the platform-independent services and view models.
    /// The host app must also register the platform services: <see cref="IGeocodingService"/>,
    /// <see cref="ISettingsService"/>, <see cref="ISessionStore"/>, <see cref="IDialogService"/>
    /// and <see cref="INavigationService"/>.
    /// </summary>
    public static IServiceCollection AddWeatherAppCore(this IServiceCollection services, FirebaseOptions firebase)
    {
        services.AddSingleton(firebase);
        services.TryAddSingleton(TimeProvider.System);

        services.AddHttpClient<IWeatherService, OpenMeteoWeatherService>(client => client.Timeout = TimeSpan.FromSeconds(15));
        services.AddHttpClient<ISavedLocationsService, FirebaseSavedLocationsService>(client => client.Timeout = TimeSpan.FromSeconds(15));
        services.AddSingleton<IAuthService, FirebaseAuthService>();

        services.AddTransient<SignInViewModel>();
        services.AddTransient<SignUpViewModel>();
        services.AddTransient<WeatherViewModel>();
        services.AddTransient<YourLocationsViewModel>();
        services.AddTransient<AccountViewModel>();
        services.AddTransient<SettingsViewModel>();

        return services;
    }
}
