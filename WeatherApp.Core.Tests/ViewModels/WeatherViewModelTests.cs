using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute.ExceptionExtensions;
using WeatherApp.Core.Models;
using WeatherApp.Core.Services;
using WeatherApp.Core.Tests.TestDoubles;
using WeatherApp.Core.ViewModels;

namespace WeatherApp.Core.Tests.ViewModels;

public class WeatherViewModelTests
{
    private readonly IWeatherService _weather = Substitute.For<IWeatherService>();
    private readonly IGeocodingService _geocoding = Substitute.For<IGeocodingService>();
    private readonly ISettingsService _settings = Substitute.For<ISettingsService>();
    private readonly IDialogService _dialogs = Substitute.For<IDialogService>();

    public WeatherViewModelTests()
    {
        _settings.DefaultLocation.Returns(string.Empty);
        _geocoding.FindAsync("London", Arg.Any<CancellationToken>()).Returns(TestData.London);
        _weather.GetForecastAsync(TestData.London, Arg.Any<TemperatureUnit>(), Arg.Any<CancellationToken>())
            .Returns(ci => TestData.Forecast(unit: ci.Arg<TemperatureUnit>()));
    }

    private WeatherViewModel CreateViewModel() =>
        new(_weather, _geocoding, _settings, _dialogs, NullLogger<WeatherViewModel>.Instance);

    [Fact]
    public async Task Search_shows_the_forecast()
    {
        var vm = CreateViewModel();

        await vm.SearchCommand.ExecuteAsync("  London ");

        Assert.Equal("London", vm.LocationName);
        Assert.EndsWith("°C", vm.CurrentTemperature);
        Assert.Equal("40%", vm.PrecipitationProbability);
        Assert.Equal(3, vm.WeatherCode);
        Assert.Equal(5, vm.FiveDayForecast.Count);
        Assert.Equal("Tuesday", vm.FiveDayForecast[0].DayOfWeek); // 24 Nov 2026
        Assert.NotNull(vm.SunriseTime);
        Assert.False(vm.IsBusy);
    }

    [Fact]
    public async Task Search_uses_the_unit_from_settings()
    {
        _settings.TemperatureUnit.Returns(TemperatureUnit.Fahrenheit);
        var vm = CreateViewModel();

        await vm.SearchCommand.ExecuteAsync("London");

        await _weather.Received().GetForecastAsync(TestData.London, TemperatureUnit.Fahrenheit, Arg.Any<CancellationToken>());
        Assert.EndsWith("°F", vm.CurrentTemperature);
    }

    [Fact]
    public async Task Unknown_place_shows_not_found()
    {
        var vm = CreateViewModel();

        await vm.SearchCommand.ExecuteAsync("Atlantis");

        await _dialogs.Received().ShowAlertAsync("Not found", Arg.Is<string>(m => m.Contains("Atlantis")), Arg.Any<string>());
        Assert.Null(vm.LocationName);
    }

    [Fact]
    public async Task Network_failure_shows_an_error_instead_of_crashing()
    {
        _weather.GetForecastAsync(default!, default, default).ThrowsAsyncForAnyArgs(new HttpRequestException("offline"));
        var vm = CreateViewModel();

        await vm.SearchCommand.ExecuteAsync("London");

        await _dialogs.Received().ShowAlertAsync("Error", Arg.Any<string>(), Arg.Any<string>());
        Assert.False(vm.IsBusy);
    }

    [Fact]
    public async Task Blank_search_does_nothing()
    {
        await CreateViewModel().SearchCommand.ExecuteAsync("   ");

        await _geocoding.DidNotReceiveWithAnyArgs().FindAsync(default!, default);
    }

    [Fact]
    public async Task First_appearance_loads_the_default_location()
    {
        _settings.DefaultLocation.Returns("London");
        var vm = CreateViewModel();

        await vm.OnAppearingAsync();

        Assert.Equal("London", vm.LocationName);
    }

    [Fact]
    public async Task Reappearing_reloads_only_when_the_unit_changed()
    {
        var vm = CreateViewModel();
        await vm.SearchCommand.ExecuteAsync("London");

        await vm.OnAppearingAsync(); // same unit: no new request
        _settings.TemperatureUnit.Returns(TemperatureUnit.Fahrenheit);
        await vm.OnAppearingAsync(); // unit changed: reload

        await _weather.Received(2).GetForecastAsync(TestData.London, Arg.Any<TemperatureUnit>(), Arg.Any<CancellationToken>());
        Assert.EndsWith("°F", vm.CurrentTemperature);
        await _geocoding.Received(1).FindAsync("London", Arg.Any<CancellationToken>()); // coordinates reused
    }
}
