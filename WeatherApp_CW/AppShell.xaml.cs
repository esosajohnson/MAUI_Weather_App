using WeatherApp_CW.NVVM.Views;

namespace WeatherApp_CW
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("SignUpView", typeof(SignUpView));
        }
    }
}
