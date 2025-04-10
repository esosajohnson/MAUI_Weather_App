using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Firebase.Auth;
using Newtonsoft.Json;
using Microsoft.Maui.Storage;

namespace WeatherApp_CW.NVVM.ViewModels
{
    public partial class AccountViewModel : ObservableObject
    {
        private const string APIkey = "AIzaSyCva-z-99c9dCpeVBpwkigDKWYQJnDBswE";

        [ObservableProperty]
        private string email;

        private string currentPassword;

        private string newPassword;

        public string CurrentPassword
        {
            get => currentPassword;
            set
            {
                SetProperty(ref currentPassword, value);
                ChangePasswordButton.NotifyCanExecuteChanged();
            }
        }

        public string NewPassword
        {
            get => newPassword;
            set
            {
                SetProperty(ref newPassword, value);
                ChangePasswordButton.NotifyCanExecuteChanged();
            }
        }

        public IRelayCommand ChangePasswordButton { get; }
        public IRelayCommand LogOutButton { get; }

        public AccountViewModel()
        {
            ChangePasswordButton = new RelayCommand(ChangePassword, CanChangePassword);
            LogOutButton = new RelayCommand(LogOut);
        }

        public async Task LoadUserEmail()
        {
            var token = Preferences.Get("MyFirebaseToken", string.Empty);
            if (!string.IsNullOrEmpty(token))
            {
                var auth = JsonConvert.DeserializeObject<FirebaseAuthLink>(token);
                Email = auth?.User?.Email;
            }
            else
            {
                // Load the email from preferences if available
                Email = Preferences.Get("UserEmail", string.Empty);  // Use this line
            }
        }


        private bool CanChangePassword()
        {
            return !string.IsNullOrWhiteSpace(CurrentPassword) && !string.IsNullOrWhiteSpace(NewPassword);
        }

        private async void ChangePassword()
        {
            try
            {
                var authProvider = new FirebaseAuthProvider(new FirebaseConfig(APIkey));
                var auth = await authProvider.SignInWithEmailAndPasswordAsync(Email, CurrentPassword);

                await authProvider.ChangeUserPassword(auth.FirebaseToken, NewPassword);

                await App.Current.MainPage.DisplayAlert("Success", "Password changed successfully!", "OK");
                await Shell.Current.GoToAsync("//SignInView");

                CurrentPassword = string.Empty;
                NewPassword = string.Empty;
            }
            catch
            {
                await App.Current.MainPage.DisplayAlert("Error", "Failed to change password. Please check your current password.", "OK");
            }
        }
        private async void LogOut()
        {
            bool confirmLogout = await App.Current.MainPage.DisplayAlert(
                "Log Out",
                "Are you sure you want to log out?",
                "Yes",
                "No");

            if (confirmLogout)
            {
                try
                {
                    Preferences.Remove("MyFirebaseToken");  // Remove the user's token from preference
                    await App.Current.MainPage.DisplayAlert("Logged Out", "You have been successfully logged out.", "OK");
                    await Shell.Current.GoToAsync("//SignInView");  // Navigate to the SignIn page
                }
                catch (Exception ex)
                {
                    await App.Current.MainPage.DisplayAlert("Error", $"Failed to log out: {ex.Message}", "OK");
                }
            }
        }


    }
}
