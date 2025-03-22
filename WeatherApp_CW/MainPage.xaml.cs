using WeatherApp_CW.NVVM.ViewModels;

namespace WeatherApp_CW
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            BindingContext = new SignInViewModel(Navigation);
        }
    }
}
