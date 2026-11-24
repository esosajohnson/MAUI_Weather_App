using WeatherApp.Core.Services;

namespace WeatherApp.Core.ViewModels;

/// <summary>User-facing text for authentication errors.</summary>
internal static class AuthMessages
{
    public static string For(AuthError error) => error switch
    {
        AuthError.InvalidCredentials => "Incorrect email or password.",
        AuthError.EmailAlreadyInUse => "An account with this email already exists.",
        AuthError.WeakPassword => "Password must be at least 6 characters.",
        AuthError.InvalidEmail => "Please enter a valid email address.",
        AuthError.NotSignedIn => "Your session has expired. Please sign in again.",
        AuthError.Network => "Couldn't connect. Check your connection and try again.",
        _ => "Something went wrong. Please try again."
    };
}
