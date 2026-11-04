using Microsoft.Playwright;

namespace CapstonePlaywrightTesting19.Pages;

public sealed class SignUpPage : BasePage
{
    public SignUpPage(IPage page) : base(page)
    {
    }

    protected override string Path => "/sign-up";

    protected override ILocator ReadyIndicator => SubmitButton;

    public ILocator Heading => Page.GetByRole(AriaRole.Heading, new() { Name = "Đăng ký", Exact = true });

    public ILocator UsernameInput => Page.Locator("#taiKhoan");

    public ILocator PasswordInput => Page.Locator("#matKhau");

    public ILocator ConfirmPasswordInput => Page.Locator("#confirmPassWord");

    public ILocator FullNameInput => Page.Locator("#hoTen");

    public ILocator EmailInput => Page.Locator("#email");

    public ILocator SubmitButton => Page.GetByRole(AriaRole.Button, new() { Name = "Đăng ký", Exact = true });

    public ILocator ServerError => Page.Locator("form").GetByRole(AriaRole.Alert);

    public ILocator SignInLink => Page.Locator("form").GetByRole(AriaRole.Link, new() { Name = "Bạn đã có tài khoản? Đăng nhập" });

    /// <summary>MUI renders field errors in "#{fieldId}-helper-text".</summary>
    public ILocator FieldError(string fieldId) => Page.Locator($"#{fieldId}-helper-text");

    public async Task FillFormAsync(string? username, string? password, string? confirmPassword, string? fullName, string? email)
    {
        await FillAsync(UsernameInput, username, "Username");
        await FillAsync(PasswordInput, password, "Password");
        await FillAsync(ConfirmPasswordInput, confirmPassword, "Confirm password");
        await FillAsync(FullNameInput, fullName, "Full name");
        await FillAsync(EmailInput, email, "Email");
    }

    public Task SubmitAsync() => ClickAsync(SubmitButton, "'Đăng ký' button");
}
