using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using WeatherApp.Core.Services;

namespace WeatherApp.Core.ViewModels;

public partial class AccountViewModel(
    IAuthService auth,
    IDialogService dialogs,
    INavigationService navigation,
    ILogger<AccountViewModel> logger) : ObservableObject
{
    [ObservableProperty]
    public partial string? Email { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ChangePasswordCommand))]
    public partial string? CurrentPassword { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ChangePasswordCommand))]
    public partial string? NewPassword { get; set; }

    public async Task OnAppearingAsync()
    {
        try
        {
            Email = (await auth.GetSessionAsync())?.Email;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Loading the account failed");
        }
    }

    private bool CanChangePassword() =>
        !string.IsNullOrWhiteSpace(CurrentPassword) && !string.IsNullOrWhiteSpace(NewPassword);

    [RelayCommand(CanExecute = nameof(CanChangePassword))]
    private async Task ChangePasswordAsync()
    {
        try
        {
            await auth.ChangePasswordAsync(CurrentPassword!, NewPassword!);
            CurrentPassword = NewPassword = string.Empty;

            // Ask for the new password straight away, as the original app did
            await dialogs.ShowAlertAsync("Success", "Password changed. Please log in with your new password.");
            await auth.SignOutAsync();
            await navigation.GoToAsync(Routes.SignIn);
        }
        catch (AuthException ex)
        {
            var message = ex.Error == AuthError.InvalidCredentials
                ? "Your current password is incorrect."
                : AuthMessages.For(ex.Error);
            await dialogs.ShowAlertAsync("Error", message);
        }
    }

    [RelayCommand]
    private async Task LogOutAsync()
    {
        if (!await dialogs.ConfirmAsync("Log Out", "Are you sure you want to log out?"))
            return;

        await auth.SignOutAsync();
        await navigation.GoToAsync(Routes.SignIn);
    }
}
