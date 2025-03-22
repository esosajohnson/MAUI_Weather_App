using WeatherApp_CW.NVVM.Views;

namespace WeatherApp_CW
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new NavigationPage(new SignInView());
        }
    }
}