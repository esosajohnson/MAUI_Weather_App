using WeatherApp.Core.ViewModels;

namespace WeatherApp_CW.Views;

public partial class YourLocationsView : ContentPage
{
    private readonly YourLocationsViewModel _viewModel;

    public YourLocationsView(YourLocationsViewModel viewModel)
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
