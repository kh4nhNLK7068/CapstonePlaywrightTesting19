using System.Text.RegularExpressions;
using CapstonePlaywrightTesting19.Fixtures;
using CapstonePlaywrightTesting19.Models;
using CapstonePlaywrightTesting19.Pages;
using CapstonePlaywrightTesting19.Utilities;
using static Microsoft.Playwright.Assertions;

namespace CapstonePlaywrightTesting19.Tests;

/// <summary>Quick-booking tool on the home page: Phim → Rạp → Ngày giờ chiếu → MUA VÉ NGAY.</summary>
[TestFixture]
public sealed class HomeQuickBookingTests : BaseTest
{
    private ShowtimeInfo _showtime = null!;

    private static IEnumerable<TestCaseData> Cases() => JsonDataReader.Cases<QuickBookingCase>("home_quick_booking.json");

    [OneTimeSetUp]
    public async Task FindShowtimeAsync()
    {
        // Test data is looked up from the live catalogue instead of being hard-coded,
        // because movies/showtimes on the demo server change over time.
        using var api = new MovieApiClient();
        _showtime = await api.FindAnyShowtimeAsync();
    }

    [TestCaseSource(nameof(Cases))]
    [Category(TestCategories.Validation)]
    [Description("Quick booking requires movie, cinema and showtime (data-driven: TestData/home_quick_booking.json)")]
    public async Task QuickBooking_MissingSelection(QuickBookingCase data)
    {
        var home = new HomePage(Page);
        await home.OpenAsync();

        if (data.SelectMovie) await home.SelectMovieAsync(_showtime.Movie.Name);
        if (data.SelectCinema) await home.SelectCinemaAsync(_showtime.CinemaName);
        if (data.SelectShowtime) await home.SelectShowtimeAsync(_showtime.ShowtimeId);
        await home.ClickBuyNowAsync();

        await Expect(home.Alert.Title).ToHaveTextAsync(data.ExpectedTitle);
        await Expect(home.Alert.Text).ToHaveTextAsync(data.ExpectedText);
        await home.Alert.ConfirmAsync();
        await Expect(home.Alert.Popup).ToBeHiddenAsync();
        await Expect(Page).ToHaveURLAsync(new Regex($"^{Regex.Escape(Settings.BaseUrl.TrimEnd('/'))}/?$"));
    }

    [Test(Description = "TC_HS_04 - Selecting movie, cinema and showtime opens the seat selection page")]
    [Category(TestCategories.Smoke)]
    [Category(TestCategories.HappyPath)]
    public async Task TC_HS_04_QuickBookingNavigatesToPurchase()
    {
        var home = new HomePage(Page);
        await home.OpenAsync();

        await home.SelectMovieAsync(_showtime.Movie.Name);
        await home.SelectCinemaAsync(_showtime.CinemaName);
        await home.SelectShowtimeAsync(_showtime.ShowtimeId);
        await home.ClickBuyNowAsync();

        await Expect(Page).ToHaveURLAsync(new Regex($"/purchase/{_showtime.ShowtimeId}$"));
        var purchase = new PurchasePage(Page);
        await purchase.WaitUntilReadyAsync();
        await Expect(purchase.MovieNameSummary).ToHaveTextAsync(_showtime.Movie.Name.Trim());
    }

    [Test(Description = "TC_HS_05 - Cinema list is loaded according to the selected movie")]
    [Category(TestCategories.Regression)]
    public async Task TC_HS_05_CinemaListDependsOnMovie()
    {
        var home = new HomePage(Page);
        await home.OpenAsync();

        // Before choosing a movie only the placeholder option exists.
        await Expect(home.CinemaSelect.Locator("option")).ToHaveCountAsync(1);
        await Expect(home.ShowtimeSelect.Locator("option")).ToHaveCountAsync(1);

        await home.SelectMovieAsync(_showtime.Movie.Name);

        await Expect(home.CinemaSelect.Locator("option", new() { HasTextString = _showtime.CinemaName }).First).ToBeAttachedAsync();
    }
}
