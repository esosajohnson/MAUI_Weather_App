using WeatherApp.Core.ViewModels;

namespace WeatherApp_CW.Views;

public partial class AccountView : ContentPage
{
    private readonly AccountViewModel _viewModel;

    public AccountView(AccountViewModel viewModel)
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
