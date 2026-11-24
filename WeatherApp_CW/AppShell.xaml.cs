using WeatherApp.Core.Services;
using WeatherApp_CW.Views;

namespace WeatherApp_CW
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Sign up is pushed on top of sign in, so it gets a back button
            Routing.RegisterRoute(Routes.SignUp, typeof(SignUpView));
        }
    }
}
