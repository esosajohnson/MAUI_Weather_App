using WeatherApp.Core.ViewModels;

namespace WeatherApp_CW.Views;

public partial class SignUpView : ContentPage
{
    public SignUpView(SignUpViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
