using Microsoft.Playwright;

namespace CapstonePlaywrightTesting19.Pages;

public sealed class HomePage : BasePage
{
    public HomePage(IPage page) : base(page)
    {
    }

    protected override string Path => "/";

    protected override ILocator ReadyIndicator => MovieCards;

    // ---- Quick booking tool (Phim / Rạp / Ngày giờ chiếu / MUA VÉ NGAY) ----
    public ILocator MovieSelect => Page.Locator("select[name='film']");

    public ILocator CinemaSelect => Page.Locator("select[name='cinema']");

    public ILocator ShowtimeSelect => Page.Locator("select[name='date']");

    public ILocator BuyNowButton => Page.GetByRole(AriaRole.Button, new() { Name = "MUA VÉ NGAY" });

    // ---- Movie list ----
    public ILocator MovieCards => Page.Locator("a[href^='/detail/']");

    public ILocator MovieCard(int movieId) => Page.Locator($"a[href='/detail/{movieId}']");

    public async Task SelectMovieAsync(string movieName)
    {
        Utilities.Logger.Step($"Select movie '{movieName}'");
        await MovieSelect.SelectOptionAsync(new SelectOptionValue { Label = movieName });
    }

    /// <summary>Cinema options are loaded asynchronously after a movie is picked; SelectOption auto-waits for them.</summary>
    public async Task SelectCinemaAsync(string cinemaName)
    {
        Utilities.Logger.Step($"Select cinema '{cinemaName}'");
        await CinemaSelect.SelectOptionAsync(new SelectOptionValue { Label = cinemaName });
    }

    public async Task SelectShowtimeAsync(int showtimeId)
    {
        Utilities.Logger.Step($"Select showtime #{showtimeId}");
        await ShowtimeSelect.SelectOptionAsync(new SelectOptionValue { Value = showtimeId.ToString() });
    }

    public Task ClickBuyNowAsync() => ClickAsync(BuyNowButton, "'MUA VÉ NGAY' button");

    public async Task<int> OpenMovieDetailAsync(int index = 0)
    {
        var card = MovieCards.Nth(index);
        var href = await card.GetAttributeAsync("href") ?? string.Empty;
        await ClickAsync(card, $"movie card #{index} ({href})");
        return int.Parse(href.Split('/').Last());
    }
}
