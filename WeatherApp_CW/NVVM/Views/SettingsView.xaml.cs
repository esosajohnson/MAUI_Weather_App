using WeatherApp_CW.NVVM.ViewModels;

namespace WeatherApp_CW.NVVM.Views;

public partial class SettingsView : ContentPage
{
    private SettingsViewModel vm;
    public SettingsView()
	{
		InitializeComponent();
        vm = new SettingsViewModel();
        BindingContext = vm;
    }
}