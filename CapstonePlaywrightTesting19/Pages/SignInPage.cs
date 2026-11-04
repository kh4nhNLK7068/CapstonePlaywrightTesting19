using Microsoft.Playwright;

namespace CapstonePlaywrightTesting19.Pages;

public sealed class SignInPage : BasePage
{
    public SignInPage(IPage page) : base(page)
    {
    }

    protected override string Path => "/sign-in";

    protected override ILocator ReadyIndicator => SubmitButton;

    public ILocator Heading => Page.GetByRole(AriaRole.Heading, new() { Name = "Đăng nhập", Exact = true });

    public ILocator UsernameInput => Page.Locator("#taiKhoan");

    public ILocator PasswordInput => Page.Locator("#matKhau");

    public ILocator UsernameError => Page.Locator("#taiKhoan-helper-text");

    public ILocator PasswordError => Page.Locator("#matKhau-helper-text");

    /// <summary>Eye icon button inside the password field.</summary>
    public ILocator PasswordToggle => Page.Locator(".MuiInputAdornment-root").GetByRole(AriaRole.Button);

    public ILocator RememberMeCheckbox => Page.GetByRole(AriaRole.Checkbox);

    public ILocator SubmitButton => Page.GetByRole(AriaRole.Button, new() { Name = "Đăng nhập", Exact = true });

    /// <summary>MUI &lt;Alert&gt; that renders the server-side error message.</summary>
    public ILocator ServerError => Page.Locator("form").GetByRole(AriaRole.Alert);

    public ILocator SignUpLink => Page.Locator("form").GetByRole(AriaRole.Link, new() { Name = "Bạn chưa có tài khoản? Đăng ký" });

    public async Task FillFormAsync(string? username, string? password)
    {
        await FillAsync(UsernameInput, username, "Username");
        await FillAsync(PasswordInput, password, "Password");
    }

    public Task SubmitAsync() => ClickAsync(SubmitButton, "'Đăng nhập' button");

    public async Task LoginAsync(string username, string password)
    {
        await FillFormAsync(username, password);
        await SubmitAsync();
    }

    /// <summary>Logs in and waits until the app redirects home with the user shown in the header.</summary>
    public async Task LoginSuccessfullyAsync(string username, string password)
    {
        await LoginAsync(username, password);
        await Header.AccountLink.WaitForAsync();
    }
}
