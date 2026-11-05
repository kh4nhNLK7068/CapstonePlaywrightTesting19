using Microsoft.Playwright;

namespace CapstonePlaywrightTesting19.Pages;

public sealed class MovieDetailPage : BasePage
{
    private readonly int _movieId;

    public MovieDetailPage(IPage page, int movieId = 0) : base(page)
    {
        _movieId = movieId;
    }

    protected override string Path => $"/detail/{_movieId}";

    protected override ILocator ReadyIndicator => Title;

    public ILocator Title => Page.GetByRole(AriaRole.Heading, new() { Level = 1 });

    /// <summary>react-scroll anchor without href (so it has no "link" role) - matched by its exact text.</summary>
    public ILocator BuyTicketLink => Page.Locator("a").GetByText("Mua vé", new() { Exact = true });

    public ILocator CinemaSystemTabs => Page.GetByRole(AriaRole.Tab);

    public ILocator VisibleShowtimePanel => Page.GetByRole(AriaRole.Tabpanel).Locator("visible=true");

    public ILocator ShowtimeLinks => Page.Locator("a[href^='/purchase/']").Locator("visible=true");

    public async Task<int> OpenFirstShowtimeAsync()
    {
        var link = ShowtimeLinks.First;
        var href = await link.GetAttributeAsync("href") ?? string.Empty;
        await ClickAsync(link, $"showtime link ({href})");
        return int.Parse(href.Split('/').Last());
    }
}
