using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Firebase.Auth;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;


namespace WeatherApp_CW.NVVM.ViewModels
{
    public partial class AccountViewModel : ObservableObject
    {
        private const string APIkey = "AIzaSyCva-z-99c9dCpeVBpwkigDKWYQJnDBswE";

        [ObservableProperty]
        private string email;

        [ObservableProperty]
        private string currentPassword;

        [ObservableProperty]
        private string newPassword;

        public IRelayCommand ChangePasswordCommand { get; }

        public AccountViewModel()
        {
            LoadUserEmail();
            ChangePasswordCommand = new RelayCommand(async () => await ChangePasswordAsync());
        }

        private void LoadUserEmail()
        {
            var token = Preferences.Get("MyFirebaseToken", string.Empty);
            if (!string.IsNullOrEmpty(token))
            {
                var auth = JsonConvert.DeserializeObject<FirebaseAuthLink>(token);
                Email = auth?.User?.Email;
            }
        }

        private bool CanChangePassword()
        {
            return !string.IsNullOrWhiteSpace(NewPassword) && !string.IsNullOrWhiteSpace(CurrentPassword);
        }

        private async Task ChangePasswordAsync()
        {
            try
            {
                var authProvider = new FirebaseAuthProvider(new FirebaseConfig(APIkey));
                var auth = await authProvider.SignInWithEmailAndPasswordAsync(Email, CurrentPassword);

                await authProvider.ChangeUserPassword(auth.FirebaseToken, NewPassword);

                await App.Current.MainPage.DisplayAlert("Success", "Password changed successfully!", "OK");

                // Clear inputs
                CurrentPassword = string.Empty;
                NewPassword = string.Empty;
            }
            catch
            {
                await App.Current.MainPage.DisplayAlert("Error", "Failed to change password. Please check your current password.", "OK");
            }
        }
    }
}