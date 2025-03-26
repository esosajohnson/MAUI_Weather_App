using WeatherApp_CW.NVVM.ViewModels;

namespace WeatherApp_CW.NVVM.Views;

public partial class SignInView : ContentPage
{
    public SignInView()
    {
        InitializeComponent();
        BindingContext = new SignInViewModel();

    }
}
