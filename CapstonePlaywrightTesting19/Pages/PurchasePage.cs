using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace CapstonePlaywrightTesting19.Pages;

/// <summary>Seat map + booking summary for one showtime (/purchase/{maLichChieu}).</summary>
public sealed class PurchasePage : BasePage
{
    private static readonly Regex SeatNumber = new(@"^\d{1,3}$");
    private readonly int _showtimeId;

    public PurchasePage(IPage page, int showtimeId = 0) : base(page)
    {
        _showtimeId = showtimeId;
    }

    protected override string Path => $"/purchase/{_showtimeId}";

    protected override ILocator ReadyIndicator => BookButton;

    public ILocator BookButton => Page.GetByRole(AriaRole.Button, new() { Name = "ĐẶT VÉ", Exact = true });

    /// <summary>Free seats are enabled buttons labelled with their number ("01", "02"…).</summary>
    public ILocator AvailableSeats => Page.GetByRole(AriaRole.Button, new() { NameRegex = SeatNumber, Disabled = false });

    /// <summary>Booked seats are disabled and labelled "X".</summary>
    public ILocator BookedSeats => Page.GetByRole(AriaRole.Button, new() { Name = "X", Exact = true, Disabled = true });

    public ILocator Seat(string number) => Page.GetByRole(AriaRole.Button, new() { Name = number, Exact = true });

    public ILocator TotalPrice => Page.GetByText(new Regex(@"^\d+VND$"));

    /// <summary>
    /// Each summary row is a &lt;div&gt; holding a label heading and a value heading. The innermost div that
    /// contains the label is the last match in document order; its second heading is the value.
    /// </summary>
    public ILocator SummaryValue(string label) =>
        Page.Locator("div")
            .Filter(new() { Has = Page.GetByRole(AriaRole.Heading, new() { Name = label, Exact = true }) })
            .Last
            .GetByRole(AriaRole.Heading)
            .Nth(1);

    public ILocator SelectedSeatsSummary => SummaryValue("Chọn:");

    public ILocator MovieNameSummary => SummaryValue("Tên Phim:");

    public ILocator CinemaSummary => SummaryValue("Cụm Rạp:");

    /// <summary>Clicks the n-th currently free seat and returns its number.</summary>
    public async Task<string> SelectAvailableSeatAsync(int index = 0)
    {
        var seat = AvailableSeats.Nth(index);
        var number = (await seat.InnerTextAsync()).Trim();
        await ClickAsync(seat, $"seat {number}");
        return number;
    }

    public Task ToggleSeatAsync(string number) => ClickAsync(Seat(number), $"seat {number}");

    public Task ClickBookAsync() => ClickAsync(BookButton, "'ĐẶT VÉ' button");

    public static int ParsePrice(string text) => int.Parse(Regex.Match(text, @"\d+").Value);
}
