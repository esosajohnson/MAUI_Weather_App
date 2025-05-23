﻿using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Firebase.Auth;


// This code was inspired by the following Youtube Video: https://www.youtube.com/watch?v=dGrh-z1G8lI&feature=youtu.be
// This code was used as motivation to get a better understanding
// of how to implement Firebase Authentication in a .NET MAUI application.
namespace WeatherApp_CW.NVVM.ViewModels
{
    internal partial class SignUpViewModel : ObservableObject
    {
        private const string APIkey = "AIzaSyCva-z-99c9dCpeVBpwkigDKWYQJnDBswE";

        private string email;
        private string password;
        public IRelayCommand RegisterUserButton { get; }

        public string Email
        {
            get => email;
            set
            {
                SetProperty(ref email, value);
                OnPropertyChanged(nameof(Email));
                RegisterUserButton.NotifyCanExecuteChanged();
            }
        }

        public string Password
        {
            get => password;
            set
            {
                SetProperty(ref password, value);
                OnPropertyChanged(nameof(Password));
                RegisterUserButton.NotifyCanExecuteChanged();
            }
        }

        public SignUpViewModel()
        {
            RegisterUserButton = new RelayCommand(Register, CanRegister);
        }

        private bool CanRegister()
        {
            return !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Password);
        }

        // Create a new user with email and password
        private async void Register()
        {
            try
            {
                // Create a new FirebaseAuthProvider instance
                var authProvider = new FirebaseAuthProvider(new FirebaseConfig(APIkey));
                var auth = await authProvider.CreateUserWithEmailAndPasswordAsync(Email, Password);
                var token = auth.FirebaseToken;
                if (token == null) return;
                await App.Current.MainPage.DisplayAlert("Success", "User Registered", "OK");
                await Shell.Current.GoToAsync("//SignInView");
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
