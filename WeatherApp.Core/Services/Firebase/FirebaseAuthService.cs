using Firebase.Auth;
using Microsoft.Extensions.Logging;

namespace WeatherApp.Core.Services.Firebase;

/// <summary>Email/password sign-in with Firebase Authentication.</summary>
public sealed class FirebaseAuthService(
    FirebaseOptions options,
    ISessionStore sessionStore,
    TimeProvider time,
    ILogger<FirebaseAuthService> logger) : IAuthService
{
    private readonly FirebaseAuthProvider _provider = new(new FirebaseConfig(options.ApiKey));
    private readonly SemaphoreSlim _refreshLock = new(1, 1);
    private UserSession? _session;
    private bool _loaded;

    public async Task<UserSession> SignInAsync(string email, string password)
    {
        var link = await CallFirebaseAsync(() => _provider.SignInWithEmailAndPasswordAsync(email, password));
        var session = ToSession(link);
        await StoreAsync(session);
        return session;
    }

    public async Task RegisterAsync(string email, string password) =>
        await CallFirebaseAsync(() => _provider.CreateUserWithEmailAndPasswordAsync(email, password));

    public async Task ChangePasswordAsync(string currentPassword, string newPassword)
    {
        var session = await GetSessionAsync() ?? throw new AuthException(AuthError.NotSignedIn);

        // Signing in again proves the user knows the current password
        var link = await CallFirebaseAsync(() => _provider.SignInWithEmailAndPasswordAsync(session.Email, currentPassword));
        var updated = await CallFirebaseAsync(() => _provider.ChangeUserPassword(link.FirebaseToken, newPassword));
        await StoreAsync(ToSession(updated));
    }

    public async Task<UserSession?> GetSessionAsync()
    {
        await _refreshLock.WaitAsync();
        try
        {
            if (!_loaded)
            {
                _session = await sessionStore.LoadAsync();
                _loaded = true;
            }

            if (_session is null || !_session.IsExpired(time.GetUtcNow()))
                return _session;

            return _session = await RefreshAsync(_session);
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    public async Task SignOutAsync()
    {
        _session = null;
        _loaded = true;
        await sessionStore.ClearAsync();
    }

    private async Task<UserSession?> RefreshAsync(UserSession expired)
    {
        try
        {
            var auth = new FirebaseAuth
            {
                FirebaseToken = expired.IdToken,
                RefreshToken = expired.RefreshToken,
                User = new User { LocalId = expired.UserId, Email = expired.Email }
            };

            var refreshed = ToSession(await _provider.RefreshAuthAsync(auth));
            await sessionStore.SaveAsync(refreshed);
            return refreshed;
        }
        catch (FirebaseAuthException ex)
        {
            // Refresh token revoked or account deleted: the user has to sign in again
            logger.LogWarning(ex, "Refreshing the session failed; signing out");
            await sessionStore.ClearAsync();
            return null;
        }
        catch (HttpRequestException ex)
        {
            // Offline: keep the session so it can be refreshed later
            throw new AuthException(AuthError.Network, ex);
        }
    }

    private async Task StoreAsync(UserSession session)
    {
        _session = session;
        _loaded = true;
        await sessionStore.SaveAsync(session);
    }

    private UserSession ToSession(FirebaseAuthLink link) => new(
        link.User.LocalId,
        link.User.Email,
        link.FirebaseToken,
        link.RefreshToken,
        time.GetUtcNow().AddSeconds(link.ExpiresIn));

    // Translates the library's exceptions into AuthException so callers only handle one type
    private async Task<T> CallFirebaseAsync<T>(Func<Task<T>> call)
    {
        try
        {
            return await call();
        }
        catch (FirebaseAuthException ex)
        {
            logger.LogInformation(ex, "Firebase rejected the request: {Reason}", ex.Reason);
            throw new AuthException(MapReason(ex.Reason), ex);
        }
        catch (HttpRequestException ex)
        {
            throw new AuthException(AuthError.Network, ex);
        }
    }

    internal static AuthError MapReason(AuthErrorReason reason) => reason switch
    {
        AuthErrorReason.EmailExists => AuthError.EmailAlreadyInUse,
        AuthErrorReason.WeakPassword => AuthError.WeakPassword,
        AuthErrorReason.InvalidEmailAddress or AuthErrorReason.MissingEmail => AuthError.InvalidEmail,
        AuthErrorReason.WrongPassword or AuthErrorReason.UnknownEmailAddress or AuthErrorReason.UserNotFound
            or AuthErrorReason.MissingPassword => AuthError.InvalidCredentials,
        // Newer Firebase projects return INVALID_LOGIN_CREDENTIALS, which this library reports as Undefined
        AuthErrorReason.Undefined => AuthError.InvalidCredentials,
        _ => AuthError.Unknown
    };
}
