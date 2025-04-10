using System.Windows.Input;
using WeatherApp_CW.NVVM.ViewModels;

namespace WeatherApp_CW.NVVM.Views;

public partial class AccountView : ContentPage
{
	private AccountViewModel viewModel;
	public AccountView()
	{
		InitializeComponent();
		viewModel = new AccountViewModel();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        viewModel.LoadUserEmail();
    }
    public ICommand GoBackCommand => new Command(async () =>
    {
        await Shell.Current.GoToAsync("//WeatherView");
    });
}