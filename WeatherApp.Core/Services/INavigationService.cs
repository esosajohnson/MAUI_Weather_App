namespace WeatherApp.Core.Services;

public interface INavigationService
{
    Task GoToAsync(string route);
}

/// <summary>Shell routes, as declared in AppShell.xaml.</summary>
public static class Routes
{
    public const string SignIn = "//SignInView";
    public const string SignUp = "SignUpView"; // relative: pushed on top of sign in
    public const string Weather = "//WeatherView";
}
