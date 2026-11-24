using System.Text.Json;
using Firebase.Auth;
using WeatherApp.Core.Models;
using WeatherApp.Core.Services;
using WeatherApp.Core.Services.Firebase;
using WeatherApp.Core.Tests.TestDoubles;

namespace WeatherApp.Core.Tests.Services;

public class FirebaseSavedLocationsServiceTests
{
    private readonly FakeHttpMessageHandler _http = new();
    private readonly IAuthService _auth = Substitute.For<IAuthService>();
    private readonly FirebaseOptions _options = new("key", "https://example-db.firebaseio.com/");

    public FirebaseSavedLocationsServiceTests()
    {
        _auth.GetSessionAsync().Returns(TestData.Session());
    }

    private FirebaseSavedLocationsService CreateService() => new(_http.CreateClient(), _auth, _options);

    [Fact]
    public async Task Loads_records_written_by_the_original_app()
    {
        // Exactly what the coursework version stored with Newtonsoft.Json (no coordinates)
        _http.RespondWith("""
            {
              "-Nb2": { "LocationName": "Paris", "Temperature": 14.5, "WeatherCode": 3,
                        "SavedAt": "2025-04-02T14:22:33.1234567+01:00", "UserId": "user-1", "FirebaseKey": null },
              "-Na1": { "LocationName": "Lagos", "Temperature": 31.0, "WeatherCode": 0,
                        "SavedAt": "2025-04-01T09:00:00+01:00", "UserId": "user-1",
                        "Latitude": 6.45, "Longitude": 3.39 }
            }
            """);

        var locations = await CreateService().GetAllAsync();

        Assert.Equal(["Lagos", "Paris"], locations.Select(l => l.Name)); // sorted by name
        var paris = locations[1];
        Assert.Equal("-Nb2", paris.Id);
        Assert.Equal(14.5, paris.TemperatureCelsius);
        Assert.Null(paris.Coordinates);
        Assert.Equal(new GeoLocation(6.45, 3.39), locations[0].Coordinates);
    }

    [Fact]
    public async Task Returns_an_empty_list_when_nothing_is_saved()
    {
        _http.RespondWith("null");

        Assert.Empty(await CreateService().GetAllAsync());
    }

    [Fact]
    public async Task Requests_the_users_own_path_with_their_token()
    {
        _http.RespondWith("null");

        await CreateService().GetAllAsync();

        Assert.Equal("https://example-db.firebaseio.com/savedLocations/user-1.json?auth=id-token", _http.Requests.Single().Url);
    }

    [Fact]
    public async Task Add_posts_the_record_and_stores_the_generated_key()
    {
        _http.RespondWith("""{"name": "-NewKey"}""");
        var location = new SavedLocation("Oslo")
        {
            Coordinates = new GeoLocation(59.9, 10.7),
            TemperatureCelsius = -2,
            WeatherCode = 71
        };

        await CreateService().AddAsync(location);

        Assert.Equal("-NewKey", location.Id);
        var request = _http.Requests.Single();
        Assert.Equal(HttpMethod.Post, request.Method);

        using var body = JsonDocument.Parse(request.Body!);
        Assert.Equal("Oslo", body.RootElement.GetProperty("LocationName").GetString());
        Assert.Equal(59.9, body.RootElement.GetProperty("Latitude").GetDouble());
        Assert.Equal("user-1", body.RootElement.GetProperty("UserId").GetString());
    }

    [Fact]
    public async Task Delete_targets_the_record_key()
    {
        _http.RespondWith("null");

        await CreateService().DeleteAsync(new SavedLocation("Oslo") { Id = "-K1" });

        var request = _http.Requests.Single();
        Assert.Equal(HttpMethod.Delete, request.Method);
        Assert.Contains("/savedLocations/user-1/-K1.json", request.Url);
    }

    [Fact]
    public async Task Throws_NotSignedIn_without_a_session()
    {
        _auth.GetSessionAsync().Returns((UserSession?)null);

        var ex = await Assert.ThrowsAsync<AuthException>(() => CreateService().GetAllAsync());

        Assert.Equal(AuthError.NotSignedIn, ex.Error);
        Assert.Empty(_http.Requests);
    }
}

public class FirebaseAuthServiceTests
{
    [Theory]
    [InlineData(AuthErrorReason.EmailExists, AuthError.EmailAlreadyInUse)]
    [InlineData(AuthErrorReason.WeakPassword, AuthError.WeakPassword)]
    [InlineData(AuthErrorReason.InvalidEmailAddress, AuthError.InvalidEmail)]
    [InlineData(AuthErrorReason.WrongPassword, AuthError.InvalidCredentials)]
    [InlineData(AuthErrorReason.UnknownEmailAddress, AuthError.InvalidCredentials)]
    [InlineData(AuthErrorReason.Undefined, AuthError.InvalidCredentials)]
    [InlineData(AuthErrorReason.TooManyAttemptsTryLater, AuthError.Unknown)]
    public void Maps_firebase_errors(AuthErrorReason reason, AuthError expected) =>
        Assert.Equal(expected, FirebaseAuthService.MapReason(reason));

    [Fact]
    public void Session_counts_as_expired_a_minute_early()
    {
        var expiresAt = new DateTimeOffset(2026, 11, 24, 12, 0, 0, TimeSpan.Zero);
        var session = TestData.Session(expiresAt);

        Assert.False(session.IsExpired(expiresAt.AddMinutes(-2)));
        Assert.True(session.IsExpired(expiresAt.AddSeconds(-30)));
    }

    [Fact]
    public async Task Returns_a_valid_stored_session_without_calling_firebase()
    {
        var store = Substitute.For<ISessionStore>();
        var stored = TestData.Session(DateTimeOffset.UtcNow.AddMinutes(30));
        store.LoadAsync().Returns(stored);
        var service = new FirebaseAuthService(new FirebaseOptions("key", "https://db"), store, TimeProvider.System,
            Microsoft.Extensions.Logging.Abstractions.NullLogger<FirebaseAuthService>.Instance);

        Assert.Same(stored, await service.GetSessionAsync());
        Assert.Same(stored, await service.GetSessionAsync());
        await store.Received(1).LoadAsync(); // loaded once, then cached
    }

    [Fact]
    public async Task Sign_out_clears_the_store()
    {
        var store = Substitute.For<ISessionStore>();
        store.LoadAsync().Returns(TestData.Session());
        var service = new FirebaseAuthService(new FirebaseOptions("key", "https://db"), store, TimeProvider.System,
            Microsoft.Extensions.Logging.Abstractions.NullLogger<FirebaseAuthService>.Instance);

        await service.SignOutAsync();

        await store.Received(1).ClearAsync();
        Assert.Null(await service.GetSessionAsync());
    }
}
