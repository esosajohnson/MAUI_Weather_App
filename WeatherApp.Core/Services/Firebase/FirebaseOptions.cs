namespace WeatherApp.Core.Services.Firebase;

/// <param name="ApiKey">Firebase Web API key. It identifies the project; access is enforced by the database rules.</param>
/// <param name="DatabaseUrl">Realtime Database URL, without a trailing slash.</param>
public sealed record FirebaseOptions(string ApiKey, string DatabaseUrl);
