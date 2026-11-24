using WeatherApp.Core.ViewModels;

namespace WeatherApp_CW.Views;

public partial class WeatherView : ContentPage
{
    private readonly WeatherViewModel _viewModel;

    public WeatherView(WeatherViewModel viewModel)
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
