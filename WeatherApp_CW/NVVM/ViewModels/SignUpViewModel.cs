using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Firebase.Auth;

namespace WeatherApp_CW.NVVM.ViewModels
{
    internal partial class SignUpViewModel : INotifyPropertyChanged
    {
        public string APIkey = "AIzaSyCva-z-99c9dCpeVBpwkigDKWYQJnDBswE";

        private INavigation navigation;
        private string email;
        private string password;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Email
        {
            get => email;
            set {email = value; OnPropertyChanged("Email"); }
        }

        public string Password
        {
            get => password;
            set { password = value; OnPropertyChanged("Password"); }
        }

        private INavigation _navigation;

        public Command RegisterUserButton { get; }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public SignUpViewModel(INavigation navigation)
        {
            this._navigation = navigation;

            RegisterUserButton = new Command(Register);
        }

        private async void Register(object obj)
        {
            try
            {
                var authProvider = new FirebaseAuthProvider(new FirebaseConfig(APIkey));
                var auth = await authProvider.CreateUserWithEmailAndPasswordAsync(Email, Password);
                string token = auth.FirebaseToken;
                if (token != null)
                {
                    await App.Current.MainPage.DisplayAlert("Success", "User Registered", "OK");
                    await this._navigation.PopAsync();
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("EmailExists"))
                {
                    await App.Current.MainPage.DisplayAlert("Registration Failed", "Email already exists", "OK");
                }
                else
                {
                    await App.Current.MainPage.DisplayAlert("Registration Failed", "An Error Occurred", "OK");
                }

            }
        }
    }
}
