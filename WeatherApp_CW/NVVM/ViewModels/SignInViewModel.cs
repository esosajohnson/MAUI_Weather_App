using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Firebase.Auth;
using Newtonsoft.Json;
using WeatherApp_CW.NVVM.Views;

namespace WeatherApp_CW.NVVM.ViewModels
{
    internal partial class SignInViewModel : INotifyPropertyChanged
    {
        public string APIkey = "AIzaSyCva-z-99c9dCpeVBpwkigDKWYQJnDBswE";

        private INavigation _navigation;
        private string userName;
        private string userPassword;

        public event PropertyChangedEventHandler? PropertyChanged;

        public Command RegisterButton { get; }
        public Command SignInButton { get; }

        public string UserName
        {
            get => userName;
            set { userName = value; OnPropertyChanged("UserName"); }
        }

        public string UserPassword
        {
            get => userPassword;
            set { userPassword = value; OnPropertyChanged("UserPassword"); }
        }

        public SignInViewModel(INavigation navigation)
        {
            this._navigation = navigation;
            RegisterButton = new Command(Register);
            SignInButton = new Command(SignIn);
        }
        private async void SignIn(object obj)
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
                    await this._navigation.PushAsync(new WeatherView());
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Log In Failed", "Invalid Email or Password", "OK"); 
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async void Register(object obj)
        {
            await this._navigation.PushAsync(new SignUpView());
        }
    }
}
