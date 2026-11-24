namespace WeatherApp.Core.Services;

/// <summary>Shows alerts without the view models depending on MAUI.</summary>
public interface IDialogService
{
    Task ShowAlertAsync(string title, string message, string cancel = "OK");

    Task<bool> ConfirmAsync(string title, string message, string accept = "Yes", string cancel = "No");
}
