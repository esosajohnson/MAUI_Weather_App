using WeatherApp_CW.NVVM.ViewModels;

namespace WeatherApp_CW.NVVM.Views;

public partial class YourLocationsView : ContentPage
{
    private YourLocationsViewModel vm;

    public YourLocationsView()
    {
        InitializeComponent();
        vm = new YourLocationsViewModel();
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await vm.LoadSavedLocationsAsync();
    }
}