using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Firebase.Auth;
using Newtonsoft.Json;
using WeatherApp_CW.NVVM.Views;

namespace WeatherApp_CW.NVVM.ViewModels
{
    internal partial class SignInViewModel : ObservableObject
    {
        private const string APIkey = "AIzaSyCva-z-99c9dCpeVBpwkigDKWYQJnDBswE";

        private string userName;
        private string userPassword;

        public Command RegisterButton { get; }
        public IRelayCommand SignInButton { get; }

        public string UserName
        {
            get => userName;
            set
            {
                SetProperty(ref userName, value);
                OnPropertyChanged(nameof(UserName));
                SignInButton.NotifyCanExecuteChanged();
            }
        }

        public string UserPassword
        {
            get => userPassword;
            set
            {
                SetProperty(ref userPassword, value);
                OnPropertyChanged(nameof(UserPassword));
                SignInButton.NotifyCanExecuteChanged();
            }
        }

        public SignInViewModel()
        {
            RegisterButton = new Command(Register);
            SignInButton = new RelayCommand(SignIn, CanSignIn);
        }

        private bool CanSignIn()
        {
            return !string.IsNullOrWhiteSpace(UserName) && !string.IsNullOrWhiteSpace(UserPassword);
        }

        private async void SignIn()
        {
            var authProvider = new FirebaseAuthProvider(new FirebaseConfig(APIkey));
            try
            {
                var auth = await authProvider.SignInWithEmailAndPasswordAsync(UserName, UserPassword);
                var content = await auth.GetFreshAuthAsync();
                var serialisedContent = JsonConvert.SerializeObject(content);
                Preferences.Set("MyFirebaseToken", serialisedContent);
                if (content != null)
                {
                    await Shell.Current.GoToAsync("//WeatherView");
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Log In Failed", "Invalid Email or Password", "OK"); 
            }
        }

        private async void Register()
        {
            await Shell.Current.GoToAsync("//SignUpView");
        }
    }
}
