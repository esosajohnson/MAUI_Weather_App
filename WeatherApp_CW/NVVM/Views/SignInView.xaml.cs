using WeatherApp_CW.NVVM.ViewModels;
using WeatherApp_CW.Services;

namespace WeatherApp_CW.NVVM.Views;

public partial class SignInView : ContentPage
{
    public SignInView()
    {
        InitializeComponent();
        BindingContext = new SignInViewModel();

    }

    // Skip the sign-in screen if a still-valid (or refreshable) session is saved
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (await AuthSession.GetValidAuthAsync() != null)
        {
            await Shell.Current.GoToAsync("//WeatherView");
        }
    }
}
