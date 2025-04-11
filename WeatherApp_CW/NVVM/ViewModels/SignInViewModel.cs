using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Firebase.Auth;
using Newtonsoft.Json;
using WeatherApp_CW.NVVM.Views;

// This code was inspired by the following Youtube Video: https://www.youtube.com/watch?v=dGrh-z1G8lI&feature=youtu.be
// This code was used as motivation to get a better understanding
// of how to implement Firebase Authentication in a .NET MAUI application.
namespace WeatherApp_CW.NVVM.ViewModels
{
    internal partial class SignInViewModel : ObservableObject
    {
        private const string APIkey = "AIzaSyCva-z-99c9dCpeVBpwkigDKWYQJnDBswE";

        private string userEmail;
        private string userPassword;

        public Command RegisterButton { get; }
        public IRelayCommand SignInButton { get; }

        public string UserEmail
        {
            get => userEmail;
            set
            {
                SetProperty(ref userEmail, value);
                OnPropertyChanged(nameof(UserEmail));
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
            return !string.IsNullOrWhiteSpace(UserEmail) && !string.IsNullOrWhiteSpace(UserPassword);
        }

        // Sign in with email and password
        private async void SignIn()
        {
            var authProvider = new FirebaseAuthProvider(new FirebaseConfig(APIkey));
            try
            {
                var auth = await authProvider.SignInWithEmailAndPasswordAsync(UserEmail, UserPassword);
                var content = await auth.GetFreshAuthAsync();
                var serialisedContent = JsonConvert.SerializeObject(content);
                Preferences.Set("MyFirebaseToken", serialisedContent);
                Preferences.Set("UserEmail", auth.User.Email);
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
            await Shell.Current.GoToAsync("SignUpView");

        }
    }
}
