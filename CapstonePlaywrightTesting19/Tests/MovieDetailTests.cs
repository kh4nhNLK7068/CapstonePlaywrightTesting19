using System.Text.RegularExpressions;
using CapstonePlaywrightTesting19.Fixtures;
using CapstonePlaywrightTesting19.Pages;
using CapstonePlaywrightTesting19.Utilities;
using static Microsoft.Playwright.Assertions;

namespace CapstonePlaywrightTesting19.Tests;

[TestFixture]
public sealed class MovieDetailTests : BaseTest
{
    private List<MovieInfo> _movies = null!;
    private ShowtimeInfo _showtime = null!;

    [OneTimeSetUp]
    public async Task LoadCatalogueAsync()
    {
        using var api = new MovieApiClient();
        _movies = await api.GetMoviesAsync();
        _showtime = await api.FindAnyShowtimeAsync();
    }

    [Test(Description = "TC_MD_01 - Clicking a movie card opens its detail page with the right title")]
    [Category(TestCategories.Smoke)]
    [Category(TestCategories.HappyPath)]
    public async Task TC_MD_01_OpenMovieDetailFromHome()
    {
        var home = new HomePage(Page);
        await home.OpenAsync();
        var movieId = await home.OpenMovieDetailAsync(0);

        await Expect(Page).ToHaveURLAsync(new Regex($"/detail/{movieId}$"));
        var detail = new MovieDetailPage(Page, movieId);
        await detail.WaitUntilReadyAsync();

        var expected = _movies.Single(m => m.Id == movieId).Name.Trim();
        await Expect(detail.Title).ToHaveTextAsync(expected);
        await Expect(detail.BuyTicketLink).ToBeVisibleAsync();
    }

    [Test(Description = "TC_MD_02 - A showtime on the detail page opens the seat selection page of that movie")]
    [Category(TestCategories.HappyPath)]
    public async Task TC_MD_02_ShowtimeOpensPurchasePage()
    {
        var detail = new MovieDetailPage(Page, _showtime.Movie.Id);
        await detail.OpenAsync();

        Assert.That(await detail.CinemaSystemTabs.CountAsync(), Is.GreaterThan(0), "Cinema system tabs should be listed");
        var showtimeId = await detail.OpenFirstShowtimeAsync();

        await Expect(Page).ToHaveURLAsync(new Regex($"/purchase/{showtimeId}$"));
        var purchase = new PurchasePage(Page, showtimeId);
        await purchase.WaitUntilReadyAsync();
        await Expect(purchase.MovieNameSummary).ToHaveTextAsync(_showtime.Movie.Name.Trim());
    }

    [Test(Description = "TC_MD_03 - 'Mua vé' scrolls down to the showtime section")]
    [Category(TestCategories.Regression)]
    public async Task TC_MD_03_BuyTicketScrollsToShowtimes()
    {
        var detail = new MovieDetailPage(Page, _showtime.Movie.Id);
        await detail.OpenAsync();
        Assert.That(await Page.EvaluateAsync<double>("() => window.scrollY"), Is.EqualTo(0));

        await detail.BuyTicketLink.ClickAsync();

        // Smooth scroll is animated: wait for the page to actually move instead of sleeping.
        await Page.WaitForFunctionAsync("() => window.scrollY > 0");
        await Expect(detail.ShowtimeLinks.First).ToBeInViewportAsync();
    }
}
