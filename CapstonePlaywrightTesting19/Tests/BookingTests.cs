using System.Text.RegularExpressions;
using CapstonePlaywrightTesting19.Fixtures;
using CapstonePlaywrightTesting19.Models;
using CapstonePlaywrightTesting19.Pages;
using CapstonePlaywrightTesting19.Utilities;
using static Microsoft.Playwright.Assertions;

namespace CapstonePlaywrightTesting19.Tests;

/// <summary>Seat selection and ticket booking on /purchase/{maLichChieu}.</summary>
[TestFixture]
public sealed class BookingTests : BaseTest
{
    private const string SelectedSeatColor = "rgb(0, 128, 0)"; // "green"
    private int _showtimeId;

    [OneTimeSetUp]
    public async Task FindShowtimeAsync()
    {
        using var api = new MovieApiClient();
        var showtime = await api.FindShowtimeWithFreeSeatsAsync(minFreeSeats: 3, AppMessages.Get.PreferredShowtimeIds);
        _showtimeId = showtime.ShowtimeId;
    }

    private async Task<PurchasePage> OpenPurchasePageAsync()
    {
        var purchase = new PurchasePage(Page, _showtimeId);
        await purchase.OpenAsync();
        return purchase;
    }

    [Test(Description = "TC_BK_01 - Seat map and booking summary are displayed")]
    [Category(TestCategories.Smoke)]
    public async Task TC_BK_01_SeatMapDisplayed()
    {
        var purchase = await OpenPurchasePageAsync();

        Assert.That(await purchase.AvailableSeats.CountAsync(), Is.GreaterThan(0), "There should be free seats");
        await Expect(purchase.TotalPrice).ToHaveTextAsync("0VND");
        await Expect(purchase.MovieNameSummary).Not.ToBeEmptyAsync();
        await Expect(purchase.CinemaSummary).Not.ToBeEmptyAsync();
        await Expect(purchase.BookButton).ToBeEnabledAsync();
    }

    [Test(Description = "TC_BK_02 - Selecting a seat highlights it and updates the summary and total")]
    [Category(TestCategories.HappyPath)]
    public async Task TC_BK_02_SelectSeatUpdatesTotal()
    {
        var purchase = await OpenPurchasePageAsync();

        var seat = await purchase.SelectAvailableSeatAsync();

        await Expect(purchase.Seat(seat)).ToHaveCSSAsync("background-color", SelectedSeatColor);
        await Expect(purchase.SelectedSeatsSummary).ToContainTextAsync($"Ghế {seat}");
        await Expect(purchase.TotalPrice).Not.ToHaveTextAsync("0VND");
    }

    [Test(Description = "TC_BK_03 - Clicking a selected seat again deselects it and resets the total")]
    [Category(TestCategories.Regression)]
    public async Task TC_BK_03_DeselectSeatResetsTotal()
    {
        var purchase = await OpenPurchasePageAsync();
        var seat = await purchase.SelectAvailableSeatAsync();
        await Expect(purchase.TotalPrice).Not.ToHaveTextAsync("0VND");

        await purchase.ToggleSeatAsync(seat);

        await Expect(purchase.TotalPrice).ToHaveTextAsync("0VND");
        await Expect(purchase.SelectedSeatsSummary).Not.ToContainTextAsync($"Ghế {seat}");
        await Expect(purchase.Seat(seat)).Not.ToHaveCSSAsync("background-color", SelectedSeatColor);
    }

    [Test(Description = "TC_BK_04 - Total price is the sum of every selected seat")]
    [Category(TestCategories.Regression)]
    public async Task TC_BK_04_TotalIsSumOfSeats()
    {
        var purchase = await OpenPurchasePageAsync();

        var first = await purchase.SelectAvailableSeatAsync(0);
        await Expect(purchase.TotalPrice).Not.ToHaveTextAsync("0VND");
        var priceAfterFirst = PurchasePage.ParsePrice(await purchase.TotalPrice.InnerTextAsync());

        // index 1 = the next free seat (the selected one is still enabled, so skip it)
        var second = await purchase.SelectAvailableSeatAsync(1);
        await Expect(purchase.TotalPrice).Not.ToHaveTextAsync($"{priceAfterFirst}VND");
        var priceAfterSecond = PurchasePage.ParsePrice(await purchase.TotalPrice.InnerTextAsync());

        Assert.That(priceAfterSecond, Is.GreaterThan(priceAfterFirst));
        await Expect(purchase.SelectedSeatsSummary).ToContainTextAsync($"Ghế {first}");
        await Expect(purchase.SelectedSeatsSummary).ToContainTextAsync($"Ghế {second}");
    }

    [Test(Description = "TC_BK_05 - Booked seats are disabled and cannot be selected")]
    [Category(TestCategories.Regression)]
    public async Task TC_BK_05_BookedSeatsDisabled()
    {
        var purchase = await OpenPurchasePageAsync();
        if (await purchase.BookedSeats.CountAsync() == 0)
        {
            Assert.Inconclusive("The chosen showtime has no booked seat yet.");
        }

        await Expect(purchase.BookedSeats.First).ToBeDisabledAsync();
        await Expect(purchase.TotalPrice).ToHaveTextAsync("0VND");
    }

    [Test(Description = "TC_BK_06 - Booking without being logged in asks the user to log in")]
    [Category(TestCategories.Validation)]
    public async Task TC_BK_06_BookWithoutLoginPromptsLogin()
    {
        var purchase = await OpenPurchasePageAsync();
        await purchase.SelectAvailableSeatAsync();

        await purchase.ClickBookAsync();

        await Expect(purchase.Alert.Title).ToHaveTextAsync(AppMessages.Get.BookingNotLoggedInTitle);
        await Expect(purchase.Alert.Text).ToHaveTextAsync(AppMessages.Get.BookingNotLoggedInText);
        await purchase.Alert.ConfirmAsync();
        await Expect(Page).ToHaveURLAsync(new Regex("/sign-in$"));
    }

    [Test(Description = "TC_BK_07 - Declining the login prompt keeps the user on the seat page")]
    [Category(TestCategories.Validation)]
    public async Task TC_BK_07_BookWithoutLoginDecline()
    {
        var purchase = await OpenPurchasePageAsync();

        await purchase.ClickBookAsync();
        await Expect(purchase.Alert.Title).ToHaveTextAsync(AppMessages.Get.BookingNotLoggedInTitle);
        await purchase.Alert.DenyAsync();

        await Expect(purchase.Alert.Popup).ToBeHiddenAsync();
        await Expect(Page).ToHaveURLAsync(new Regex($"/purchase/{_showtimeId}$"));
    }

    [Test(Description = "TC_BK_08 - Logged-in user must choose a seat before booking")]
    [Category(TestCategories.Validation)]
    [Category(TestCategories.RequiresAccount)]
    public async Task TC_BK_08_BookWithoutSeat()
    {
        await LoginAsync();
        var purchase = await OpenPurchasePageAsync();

        await purchase.ClickBookAsync();

        await Expect(purchase.Alert.Title).ToHaveTextAsync(AppMessages.Get.BookingNoSeatTitle);
        await Expect(purchase.Alert.Text).ToHaveTextAsync(AppMessages.Get.BookingNoSeatText);
    }

    [Test(Description = "TC_BK_09 - Logged-in user books a ticket successfully; the seat becomes booked")]
    [Category(TestCategories.Smoke)]
    [Category(TestCategories.HappyPath)]
    [Category(TestCategories.RequiresAccount)]
    public async Task TC_BK_09_BookTicketSuccessfully()
    {
        await LoginAsync();
        var purchase = await OpenPurchasePageAsync();
        var seat = await purchase.SelectAvailableSeatAsync();

        await purchase.ClickBookAsync();

        await Expect(purchase.Alert.Title).ToHaveTextAsync(AppMessages.Get.BookingSuccessTitle);
        await Expect(purchase.Alert.Text).ToHaveTextAsync(AppMessages.Get.BookingSuccessText);
        await purchase.Alert.ConfirmAsync();

        // The seat map is reloaded: the booked seat is no longer offered.
        await purchase.WaitUntilReadyAsync();
        await Expect(purchase.Seat(seat)).ToHaveCountAsync(0);
    }

    [Test(Description = "TC_BK_10 - Logged-in user books several seats in one booking")]
    [Category(TestCategories.HappyPath)]
    [Category(TestCategories.RequiresAccount)]
    public async Task TC_BK_10_BookMultipleSeatsAtOnce()
    {
        await LoginAsync();
        var purchase = await OpenPurchasePageAsync();

        var first = await purchase.SelectAvailableSeatAsync(0);
        await Expect(purchase.TotalPrice).Not.ToHaveTextAsync("0VND");
        var firstPrice = PurchasePage.ParsePrice(await purchase.TotalPrice.InnerTextAsync());

        // index 1 = the next free seat (the selected one is still enabled, so skip it)
        var second = await purchase.SelectAvailableSeatAsync(1);
        await Expect(purchase.TotalPrice).Not.ToHaveTextAsync($"{firstPrice}VND");
        await Expect(purchase.SelectedSeatsSummary).ToContainTextAsync($"Ghế {first}");
        await Expect(purchase.SelectedSeatsSummary).ToContainTextAsync($"Ghế {second}");

        await purchase.ClickBookAsync();

        await Expect(purchase.Alert.Title).ToHaveTextAsync(AppMessages.Get.BookingSuccessTitle);
        await purchase.Alert.ConfirmAsync();

        // Both seats are booked by the single request.
        await purchase.WaitUntilReadyAsync();
        await Expect(purchase.Seat(first)).ToHaveCountAsync(0);
        await Expect(purchase.Seat(second)).ToHaveCountAsync(0);
    }

    /// <summary>
    /// Known defect: after a successful booking the app reloads the seat map but never clears the
    /// selected-seat list (movieShowtimesReducer keeps bookingChairList), so the summary still shows
    /// the booked seat and its price. Expected by spec: total "0VND", no seat selected.
    /// </summary>
    [Test(Description = "TC_BK_11 - After a successful booking the selection and total are reset")]
    [Category(TestCategories.Regression)]
    [Category(TestCategories.RequiresAccount)]
    [Category(TestCategories.KnownDefect)]
    public async Task TC_BK_11_SummaryResetAfterBooking()
    {
        await LoginAsync();
        var purchase = await OpenPurchasePageAsync();
        var seat = await purchase.SelectAvailableSeatAsync();

        await purchase.ClickBookAsync();
        await Expect(purchase.Alert.Title).ToHaveTextAsync(AppMessages.Get.BookingSuccessTitle);
        await purchase.Alert.ConfirmAsync();
        await purchase.WaitUntilReadyAsync();
        await Expect(purchase.Seat(seat)).ToHaveCountAsync(0);

        await Expect(purchase.TotalPrice).ToHaveTextAsync("0VND");
        await Expect(purchase.SelectedSeatsSummary).Not.ToContainTextAsync($"Ghế {seat}");
    }
}
