using Firebase.Auth;
using Newtonsoft.Json;

namespace WeatherApp_CW.Services
{
    // Central place for the stored Firebase session.
    // Firebase ID tokens expire after an hour, so callers should always go through
    // GetValidAuthAsync() rather than reading the saved token directly.
    public static class AuthSession
    {
        // Firebase Web API key (identifies the project; access is enforced by Realtime Database rules)
        public const string ApiKey = "AIzaSyCva-z-99c9dCpeVBpwkigDKWYQJnDBswE";

        private const string TokenKey = "MyFirebaseToken";

        public static FirebaseAuthProvider CreateProvider() => new(new FirebaseConfig(ApiKey));

        public static void Save(FirebaseAuth auth) =>
            Preferences.Set(TokenKey, JsonConvert.SerializeObject(auth));

        public static void Clear() => Preferences.Remove(TokenKey);

        // Returns the stored session, refreshing the ID token first if it has expired.
        // Returns null when nobody is signed in or the refresh token is no longer valid.
        public static async Task<FirebaseAuth?> GetValidAuthAsync()
        {
            var json = Preferences.Get(TokenKey, string.Empty);
            if (string.IsNullOrEmpty(json))
                return null;

            var auth = JsonConvert.DeserializeObject<FirebaseAuth>(json);
            if (auth?.User == null || string.IsNullOrEmpty(auth.RefreshToken))
                return null;

            if (!auth.IsExpired())
                return auth;

            try
            {
                var refreshed = await CreateProvider().RefreshAuthAsync(auth);
                Save(refreshed);
                return refreshed;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Token refresh failed: {ex.Message}");
                return null;
            }
        }
    }
}
