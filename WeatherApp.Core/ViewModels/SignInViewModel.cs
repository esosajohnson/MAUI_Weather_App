using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using WeatherApp.Core.Services;

namespace WeatherApp.Core.ViewModels;

public partial class SignInViewModel(
    IAuthService auth,
    IDialogService dialogs,
    INavigationService navigation,
    ILogger<SignInViewModel> logger) : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSignIn))]
    [NotifyCanExecuteChangedFor(nameof(SignInCommand))]
    public partial string? Email { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSignIn))]
    [NotifyCanExecuteChangedFor(nameof(SignInCommand))]
    public partial string? Password { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSignIn))]
    [NotifyCanExecuteChangedFor(nameof(SignInCommand))]
    public partial bool IsBusy { get; set; }

    public bool CanSignIn => !IsBusy && !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Password);

    /// <summary>Skips the sign-in screen when a saved session is still valid (or can be refreshed).</summary>
    public async Task OnAppearingAsync()
    {
        try
        {
            if (await auth.GetSessionAsync() is not null)
                await navigation.GoToAsync(Routes.Weather);
        }
        catch (Exception ex)
        {
            // e.g. offline while refreshing: stay on the sign-in screen
            logger.LogWarning(ex, "Resuming the saved session failed");
        }
    }

    [RelayCommand(CanExecute = nameof(CanSignIn))]
    private async Task SignInAsync()
    {
        IsBusy = true;
        try
        {
            await auth.SignInAsync(Email!.Trim(), Password!);
            Password = string.Empty;
            await navigation.GoToAsync(Routes.Weather);
        }
        catch (AuthException ex)
        {
            await dialogs.ShowAlertAsync("Log In Failed", AuthMessages.For(ex.Error));
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private Task RegisterAsync() => navigation.GoToAsync(Routes.SignUp);
}
