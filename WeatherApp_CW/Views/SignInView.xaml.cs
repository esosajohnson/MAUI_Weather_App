using WeatherApp.Core.ViewModels;

namespace WeatherApp_CW.Views;

public partial class SignInView : ContentPage
{
    private readonly SignInViewModel _viewModel;

    public SignInView(SignInViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.OnAppearingAsync(); // view models handle their own errors
    }
}
