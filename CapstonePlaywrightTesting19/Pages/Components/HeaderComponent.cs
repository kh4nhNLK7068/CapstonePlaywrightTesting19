using CapstonePlaywrightTesting19.Utilities;
using Microsoft.Playwright;

namespace CapstonePlaywrightTesting19.Pages.Components;

/// <summary>Top app bar shared by every page.</summary>
public sealed class HeaderComponent
{
    private readonly IPage _page;

    public HeaderComponent(IPage page)
    {
        _page = page;
    }

    private ILocator Root => _page.Locator("header");

    public ILocator Logo => Root.GetByRole(AriaRole.Img, new() { Name = "Logo" });

    public ILocator SignInLink => Root.GetByRole(AriaRole.Link, new() { Name = "Đăng Nhập", Exact = true });

    public ILocator SignUpLink => Root.GetByRole(AriaRole.Link, new() { Name = "Đăng Ký", Exact = true });

    /// <summary>Shown only when logged in – links to the account page and displays the user's full name.</summary>
    public ILocator AccountLink => Root.Locator("a[href='/account']");

    public ILocator SignOutLink => Root.GetByRole(AriaRole.Link, new() { Name = "Đăng xuất" });

    /// <summary>Menu anchors ("Lịch Chiếu", "Cụm Rạp"…) have no href, so they are matched by their heading text.</summary>
    public ILocator NavItem(string text) =>
        Root.Locator("a").Filter(new() { Has = _page.GetByRole(AriaRole.Heading, new() { Name = text, Exact = true }) });

    public async Task GoToSignInAsync()
    {
        Logger.Step("Click header link 'Đăng Nhập'");
        await SignInLink.ClickAsync();
    }

    public async Task GoToSignUpAsync()
    {
        Logger.Step("Click header link 'Đăng Ký'");
        await SignUpLink.ClickAsync();
    }

    public async Task ClickLogoAsync()
    {
        Logger.Step("Click header logo");
        await Logo.ClickAsync();
    }

    public async Task ClickSignOutAsync()
    {
        Logger.Step("Click header link 'Đăng xuất'");
        await SignOutLink.ClickAsync();
    }
}
