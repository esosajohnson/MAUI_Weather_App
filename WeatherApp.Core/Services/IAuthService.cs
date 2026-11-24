namespace WeatherApp.Core.Services;

/// <summary>A signed-in user. Firebase ID tokens expire after an hour.</summary>
public sealed record UserSession(string UserId, string Email, string IdToken, string RefreshToken, DateTimeOffset ExpiresAt)
{
    // Refresh a minute early so a token never expires mid-request
    public bool IsExpired(DateTimeOffset now) => now >= ExpiresAt - TimeSpan.FromMinutes(1);
}

public enum AuthError
{
    InvalidCredentials,
    EmailAlreadyInUse,
    WeakPassword,
    InvalidEmail,
    NotSignedIn,
    Network,
    Unknown
}

public sealed class AuthException(AuthError error, Exception? innerException = null)
    : Exception($"Authentication failed: {error}", innerException)
{
    public AuthError Error { get; } = error;
}

public interface IAuthService
{
    /// <exception cref="AuthException"/>
    Task<UserSession> SignInAsync(string email, string password);

    /// <exception cref="AuthException"/>
    Task RegisterAsync(string email, string password);

    /// <summary>Re-checks the current password, then sets the new one.</summary>
    /// <exception cref="AuthException"/>
    Task ChangePasswordAsync(string currentPassword, string newPassword);

    /// <summary>The signed-in user, with the token refreshed if it expired; null if nobody is signed in.</summary>
    Task<UserSession?> GetSessionAsync();

    Task SignOutAsync();
}

public static class AuthServiceExtensions
{
    /// <summary>Like <see cref="IAuthService.GetSessionAsync"/>, but throws <see cref="AuthException"/> (NotSignedIn) instead of returning null.</summary>
    public static async Task<UserSession> RequireSessionAsync(this IAuthService auth) =>
        await auth.GetSessionAsync() ?? throw new AuthException(AuthError.NotSignedIn);
}

/// <summary>Persists the session between app launches (secure storage in the app).</summary>
public interface ISessionStore
{
    Task<UserSession?> LoadAsync();

    Task SaveAsync(UserSession session);

    Task ClearAsync();
}
