using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute.ExceptionExtensions;
using WeatherApp.Core.Models;
using WeatherApp.Core.Services;
using WeatherApp.Core.Tests.TestDoubles;
using WeatherApp.Core.ViewModels;

namespace WeatherApp.Core.Tests.ViewModels;

public class YourLocationsViewModelTests
{
    private readonly IWeatherService _weather = Substitute.For<IWeatherService>();
    private readonly IGeocodingService _geocoding = Substitute.For<IGeocodingService>();
    private readonly ISavedLocationsService _store = Substitute.For<ISavedLocationsService>();
    private readonly ISettingsService _settings = Substitute.For<ISettingsService>();
    private readonly IDialogService _dialogs = Substitute.For<IDialogService>();
    private readonly INavigationService _navigation = Substitute.For<INavigationService>();

    public YourLocationsViewModelTests()
    {
        _geocoding.FindAsync("London", Arg.Any<CancellationToken>()).Returns(TestData.London);
        _weather.GetForecastAsync(Arg.Any<GeoLocation>(), Arg.Any<TemperatureUnit>(), Arg.Any<CancellationToken>())
            .Returns(TestData.Forecast(temperature: 15));
        _store.GetAllAsync(Arg.Any<CancellationToken>()).Returns([]);
    }

    private YourLocationsViewModel CreateViewModel() =>
        new(_weather, _geocoding, _store, _settings, _dialogs, _navigation, TimeProvider.System,
            NullLogger<YourLocationsViewModel>.Instance);

    [Fact]
    public async Task Appearing_loads_locations_in_the_chosen_unit()
    {
        _settings.TemperatureUnit.Returns(TemperatureUnit.Fahrenheit);
        _store.GetAllAsync(Arg.Any<CancellationToken>()).Returns([new SavedLocation("Paris") { TemperatureCelsius = 20 }]);
        var vm = CreateViewModel();

        await vm.OnAppearingAsync();

        var paris = Assert.Single(vm.SavedLocations);
        Assert.Equal(TemperatureUnit.Fahrenheit, paris.DisplayUnit);
    }

    [Fact]
    public async Task Appearing_while_signed_out_is_silent()
    {
        _store.GetAllAsync(Arg.Any<CancellationToken>()).ThrowsAsync(new AuthException(AuthError.NotSignedIn));
        var vm = CreateViewModel();

        await vm.OnAppearingAsync();

        Assert.Empty(vm.SavedLocations);
        await _dialogs.DidNotReceiveWithAnyArgs().ShowAlertAsync(default!, default!, default!);
    }

    [Fact]
    public async Task Save_stores_coordinates_and_celsius_then_clears_the_search()
    {
        var vm = CreateViewModel();
        vm.SearchQuery = "London";

        await vm.SaveLocationCommand.ExecuteAsync(null);

        var saved = Assert.Single(vm.SavedLocations);
        Assert.Equal(TestData.London, saved.Coordinates);
        Assert.Equal(15, saved.TemperatureCelsius);
        Assert.Equal(string.Empty, vm.SearchQuery);
        await _store.Received(1).AddAsync(saved, Arg.Any<CancellationToken>());
        await _weather.Received().GetForecastAsync(TestData.London, TemperatureUnit.Celsius, Arg.Any<CancellationToken>());
    }

    [Fact]
    public void Save_is_disabled_until_something_is_typed()
    {
        var vm = CreateViewModel();
        Assert.False(vm.SaveLocationCommand.CanExecute(null));

        vm.SearchQuery = "Rome";

        Assert.True(vm.SaveLocationCommand.CanExecute(null));
    }

    [Fact]
    public async Task Saving_a_duplicate_is_rejected()
    {
        _store.GetAllAsync(Arg.Any<CancellationToken>()).Returns([new SavedLocation("London")]);
        var vm = CreateViewModel();
        await vm.OnAppearingAsync();
        vm.SearchQuery = "london";

        await vm.SaveLocationCommand.ExecuteAsync(null);

        await _dialogs.Received().ShowAlertAsync("Already saved", Arg.Any<string>(), Arg.Any<string>());
        await _store.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
    }

    [Fact]
    public async Task Expired_session_sends_the_user_to_sign_in()
    {
        _store.AddAsync(Arg.Any<SavedLocation>(), Arg.Any<CancellationToken>()).ThrowsAsync(new AuthException(AuthError.NotSignedIn));
        var vm = CreateViewModel();
        vm.SearchQuery = "London";

        await vm.SaveLocationCommand.ExecuteAsync(null);

        await _navigation.Received().GoToAsync(Routes.SignIn);
        Assert.Empty(vm.SavedLocations);
    }

    [Fact]
    public async Task Refresh_reuses_stored_coordinates_and_geocodes_older_records_once()
    {
        var withCoordinates = new SavedLocation("Lagos") { Coordinates = new GeoLocation(6.45, 3.39) };
        var legacy = new SavedLocation("London"); // saved by the old app, no coordinates
        _store.GetAllAsync(Arg.Any<CancellationToken>()).Returns([withCoordinates, legacy]);
        var vm = CreateViewModel();
        await vm.OnAppearingAsync();

        await vm.RefreshCommand.ExecuteAsync(null);

        await _geocoding.DidNotReceive().FindAsync("Lagos", Arg.Any<CancellationToken>());
        Assert.Equal(TestData.London, legacy.Coordinates);
        Assert.All(vm.SavedLocations, l => Assert.Equal(15, l.TemperatureCelsius));
        await _store.Received(2).UpdateAsync(Arg.Any<SavedLocation>(), Arg.Any<CancellationToken>());
        Assert.False(vm.IsRefreshing);
    }

    [Fact]
    public async Task Delete_removes_the_location()
    {
        var paris = new SavedLocation("Paris") { Id = "-K1" };
        _store.GetAllAsync(Arg.Any<CancellationToken>()).Returns([paris]);
        var vm = CreateViewModel();
        await vm.OnAppearingAsync();

        await vm.DeleteCommand.ExecuteAsync(paris);

        Assert.Empty(vm.SavedLocations);
        await _store.Received(1).DeleteAsync(paris, Arg.Any<CancellationToken>());
    }
}
