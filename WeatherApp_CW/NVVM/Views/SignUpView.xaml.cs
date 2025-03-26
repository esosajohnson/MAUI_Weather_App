using System.Windows.Input;
using WeatherApp_CW.NVVM.ViewModels;

namespace WeatherApp_CW.NVVM.Views;


public partial class SignUpView : ContentPage
{
	public SignUpView()
	{
		InitializeComponent();
		BindingContext = new SignUpViewModel();
	}

    public ICommand GoBackCommand => new Command(async () =>
    {
        await Shell.Current.GoToAsync("//SignInView");
    });
}

