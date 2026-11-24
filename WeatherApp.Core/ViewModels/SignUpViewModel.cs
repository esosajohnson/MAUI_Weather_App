using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WeatherApp.Core.Services;

namespace WeatherApp.Core.ViewModels;

public partial class SignUpViewModel(
    IAuthService auth,
    IDialogService dialogs,
    INavigationService navigation) : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanRegister))]
    [NotifyCanExecuteChangedFor(nameof(RegisterCommand))]
    public partial string? Email { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanRegister))]
    [NotifyCanExecuteChangedFor(nameof(RegisterCommand))]
    public partial string? Password { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanRegister))]
    [NotifyCanExecuteChangedFor(nameof(RegisterCommand))]
    public partial bool IsBusy { get; set; }

    public bool CanRegister => !IsBusy && !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Password);

    [RelayCommand(CanExecute = nameof(CanRegister))]
    private async Task RegisterAsync()
    {
        IsBusy = true;
        try
        {
            await auth.RegisterAsync(Email!.Trim(), Password!);
            await dialogs.ShowAlertAsync("Success", "Account created. You can now log in.");
            Email = Password = string.Empty;
            await navigation.GoToAsync(Routes.SignIn);
        }
        catch (AuthException ex)
        {
            await dialogs.ShowAlertAsync("Registration Failed", AuthMessages.For(ex.Error));
        }
        finally
        {
            IsBusy = false;
        }
    }
}
