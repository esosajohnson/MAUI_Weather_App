using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute.ExceptionExtensions;
using WeatherApp.Core.Models;
using WeatherApp.Core.Services;
using WeatherApp.Core.Tests.TestDoubles;
using WeatherApp.Core.ViewModels;

namespace WeatherApp.Core.Tests.ViewModels;

public class SignInViewModelTests
{
    private readonly IAuthService _auth = Substitute.For<IAuthService>();
    private readonly IDialogService _dialogs = Substitute.For<IDialogService>();
    private readonly INavigationService _navigation = Substitute.For<INavigationService>();

    private SignInViewModel CreateViewModel() =>
        new(_auth, _dialogs, _navigation, NullLogger<SignInViewModel>.Instance);

    [Fact]
    public void Can_sign_in_only_with_email_and_password()
    {
        var vm = CreateViewModel();
        Assert.False(vm.CanSignIn);

        vm.Email = "me@example.com";
        Assert.False(vm.SignInCommand.CanExecute(null));

        vm.Password = "secret";
        Assert.True(vm.CanSignIn);
        Assert.True(vm.SignInCommand.CanExecute(null));
    }

    [Fact]
    public async Task Successful_sign_in_goes_to_the_weather_page()
    {
        var vm = CreateViewModel();
        vm.Email = " me@example.com ";
        vm.Password = "secret";

        await vm.SignInCommand.ExecuteAsync(null);

        await _auth.Received().SignInAsync("me@example.com", "secret");
        await _navigation.Received().GoToAsync(Routes.Weather);
        Assert.Equal(string.Empty, vm.Password);
    }

    [Fact]
    public async Task Wrong_password_shows_a_friendly_message()
    {
        _auth.SignInAsync(default!, default!).ThrowsAsyncForAnyArgs(new AuthException(AuthError.InvalidCredentials));
        var vm = CreateViewModel();
        vm.Email = "me@example.com";
        vm.Password = "wrong";

        await vm.SignInCommand.ExecuteAsync(null);

        await _dialogs.Received().ShowAlertAsync("Log In Failed", "Incorrect email or password.", Arg.Any<string>());
        await _navigation.DidNotReceiveWithAnyArgs().GoToAsync(default!);
        Assert.False(vm.IsBusy);
    }

    [Fact]
    public async Task Saved_session_skips_the_sign_in_page()
    {
        _auth.GetSessionAsync().Returns(TestData.Session());

        await CreateViewModel().OnAppearingAsync();

        await _navigation.Received().GoToAsync(Routes.Weather);
    }

    [Fact]
    public async Task No_session_stays_on_the_sign_in_page()
    {
        _auth.GetSessionAsync().Returns((UserSession?)null);

        await CreateViewModel().OnAppearingAsync();

        await _navigation.DidNotReceiveWithAnyArgs().GoToAsync(default!);
    }
}

public class SignUpViewModelTests
{
    private readonly IAuthService _auth = Substitute.For<IAuthService>();
    private readonly IDialogService _dialogs = Substitute.For<IDialogService>();
    private readonly INavigationService _navigation = Substitute.For<INavigationService>();

    [Fact]
    public async Task Existing_email_is_explained()
    {
        _auth.RegisterAsync(default!, default!).ThrowsAsyncForAnyArgs(new AuthException(AuthError.EmailAlreadyInUse));
        var vm = new SignUpViewModel(_auth, _dialogs, _navigation) { Email = "me@example.com", Password = "secret1" };

        await vm.RegisterCommand.ExecuteAsync(null);

        await _dialogs.Received().ShowAlertAsync("Registration Failed", "An account with this email already exists.", Arg.Any<string>());
    }

    [Fact]
    public async Task Successful_registration_returns_to_sign_in()
    {
        var vm = new SignUpViewModel(_auth, _dialogs, _navigation) { Email = "me@example.com", Password = "secret1" };

        await vm.RegisterCommand.ExecuteAsync(null);

        await _navigation.Received().GoToAsync(Routes.SignIn);
    }
}

public class AccountViewModelTests
{
    private readonly IAuthService _auth = Substitute.For<IAuthService>();
    private readonly IDialogService _dialogs = Substitute.For<IDialogService>();
    private readonly INavigationService _navigation = Substitute.For<INavigationService>();

    private AccountViewModel CreateViewModel() =>
        new(_auth, _dialogs, _navigation, NullLogger<AccountViewModel>.Instance);

    [Fact]
    public async Task Shows_the_signed_in_email()
    {
        _auth.GetSessionAsync().Returns(TestData.Session());
        var vm = CreateViewModel();

        await vm.OnAppearingAsync();

        Assert.Equal("me@example.com", vm.Email);
    }

    [Fact]
    public async Task Wrong_current_password_is_explained()
    {
        _auth.ChangePasswordAsync(default!, default!).ThrowsAsyncForAnyArgs(new AuthException(AuthError.InvalidCredentials));
        var vm = CreateViewModel();
        vm.CurrentPassword = "wrong";
        vm.NewPassword = "new-secret";

        await vm.ChangePasswordCommand.ExecuteAsync(null);

        await _dialogs.Received().ShowAlertAsync("Error", "Your current password is incorrect.", Arg.Any<string>());
        await _auth.DidNotReceive().SignOutAsync();
    }

    [Fact]
    public async Task Log_out_asks_first()
    {
        _dialogs.ConfirmAsync(default!, default!).ReturnsForAnyArgs(false);
        var vm = CreateViewModel();

        await vm.LogOutCommand.ExecuteAsync(null);

        await _auth.DidNotReceive().SignOutAsync();

        _dialogs.ConfirmAsync(default!, default!).ReturnsForAnyArgs(true);
        await vm.LogOutCommand.ExecuteAsync(null);

        await _auth.Received(1).SignOutAsync();
        await _navigation.Received().GoToAsync(Routes.SignIn);
    }
}

public class SettingsViewModelTests
{
    [Fact]
    public async Task Loads_and_saves_the_settings()
    {
        var settings = Substitute.For<ISettingsService>();
        settings.TemperatureUnit.Returns(TemperatureUnit.Celsius);
        settings.DefaultLocation.Returns("Coventry");
        var vm = new SettingsViewModel(settings, Substitute.For<IDialogService>());
        Assert.Equal("Coventry", vm.DefaultLocation);

        vm.SelectedTemperatureUnit = TemperatureUnit.Fahrenheit;
        vm.DefaultLocation = "  Lagos ";
        await vm.SaveCommand.ExecuteAsync(null);

        settings.Received().TemperatureUnit = TemperatureUnit.Fahrenheit;
        settings.Received().DefaultLocation = "Lagos";
    }
}
