using System.Text.RegularExpressions;
using CapstonePlaywrightTesting19.Fixtures;
using CapstonePlaywrightTesting19.Models;
using CapstonePlaywrightTesting19.Pages;
using CapstonePlaywrightTesting19.Utilities;
using static Microsoft.Playwright.Assertions;

namespace CapstonePlaywrightTesting19.Tests;

[TestFixture]
public sealed class SignInTests : BaseTest
{
    private static IEnumerable<TestCaseData> ValidationCases() => JsonDataReader.Cases<SignInCase>("signin_validation.json");

    private static IEnumerable<TestCaseData> ServerCases() => JsonDataReader.Cases<SignInCase>("signin_server.json");

    [TestCaseSource(nameof(ValidationCases))]
    [Category(TestCategories.Validation)]
    [Description("Client-side validation of the sign-in form (data-driven: TestData/signin_validation.json)")]
    public async Task SignIn_ClientValidation(SignInCase data)
    {
        var signIn = new SignInPage(Page);
        await signIn.OpenAsync();

        await signIn.LoginAsync(DataGenerator.Resolve(data.Username), DataGenerator.Resolve(data.Password));

        await AssertFieldError(signIn.UsernameError, data, "taiKhoan");
        await AssertFieldError(signIn.PasswordError, data, "matKhau");
        await Expect(signIn.ServerError).ToBeHiddenAsync();
        await Expect(Page).ToHaveURLAsync(new Regex("/sign-in$"));
    }

    [TestCaseSource(nameof(ServerCases))]
    [Category(TestCategories.Validation)]
    [Description("Server-side validation: wrong credentials are rejected (data-driven: TestData/signin_server.json)")]
    public async Task SignIn_ServerValidation(SignInCase data)
    {
        var signIn = new SignInPage(Page);
        await signIn.OpenAsync();

        await signIn.LoginAsync(DataGenerator.Resolve(data.Username), DataGenerator.Resolve(data.Password));

        await Expect(signIn.ServerError).ToHaveTextAsync(data.ExpectedServerError);
        await Expect(signIn.Header.AccountLink).ToBeHiddenAsync();
        await Expect(Page).ToHaveURLAsync(new Regex("/sign-in$"));
    }

    [Test(Description = "TC_SI_07 - Password is masked and can be revealed with the eye icon")]
    [Category(TestCategories.Validation)]
    public async Task TC_SI_07_PasswordVisibilityToggle()
    {
        var signIn = new SignInPage(Page);
        await signIn.OpenAsync();
        await signIn.PasswordInput.FillAsync("Secret123");

        await Expect(signIn.PasswordInput).ToHaveAttributeAsync("type", "password");
        await signIn.PasswordToggle.ClickAsync();
        await Expect(signIn.PasswordInput).ToHaveAttributeAsync("type", "text");
        await signIn.PasswordToggle.ClickAsync();
        await Expect(signIn.PasswordInput).ToHaveAttributeAsync("type", "password");
    }

    [Test(Description = "TC_SI_08 - Login with a valid account")]
    [Category(TestCategories.Smoke)]
    [Category(TestCategories.HappyPath)]
    [Category(TestCategories.RequiresAccount)]
    public async Task TC_SI_08_LoginWithValidAccount()
    {
        var account = await AccountProvider.GetAsync();
        var signIn = new SignInPage(Page);
        await signIn.OpenAsync();

        await signIn.LoginAsync(account.Username, account.Password);

        await Expect(signIn.Alert.Title).ToHaveTextAsync(AppMessages.Get.SignInSuccess);
        await Expect(Page).ToHaveURLAsync(new Regex($"^{Regex.Escape(Settings.BaseUrl.TrimEnd('/'))}/?$"));
        await Expect(signIn.Header.AccountLink).ToBeVisibleAsync();
        await Expect(signIn.Header.SignOutLink).ToBeVisibleAsync();
        await Expect(signIn.Header.SignInLink).ToBeHiddenAsync();
    }

    [Test(Description = "TC_SI_09 - Logout after confirming the dialog")]
    [Category(TestCategories.HappyPath)]
    [Category(TestCategories.RequiresAccount)]
    public async Task TC_SI_09_LogoutConfirm()
    {
        await LoginAsync();
        var home = new HomePage(Page);

        await home.Header.ClickSignOutAsync();
        await Expect(home.Alert.Title).ToHaveTextAsync(AppMessages.Get.SignOutQuestion);
        await home.Alert.ConfirmAsync();

        await Expect(home.Alert.Title).ToHaveTextAsync(AppMessages.Get.SignOutDone);
        await Expect(home.Header.SignInLink).ToBeVisibleAsync();
        await Expect(home.Header.AccountLink).ToBeHiddenAsync();
    }

    [Test(Description = "TC_SI_10 - Cancelling the logout dialog keeps the user logged in")]
    [Category(TestCategories.RequiresAccount)]
    public async Task TC_SI_10_LogoutCancel()
    {
        await LoginAsync();
        var home = new HomePage(Page);

        await home.Header.ClickSignOutAsync();
        await home.Alert.CancelAsync();

        await Expect(home.Alert.Popup).ToBeHiddenAsync();
        await Expect(home.Header.AccountLink).ToBeVisibleAsync();
    }

    private static async Task AssertFieldError(Microsoft.Playwright.ILocator error, SignInCase data, string field)
    {
        if (data.ExpectedErrors.TryGetValue(field, out var expected))
        {
            await Expect(error).ToHaveTextAsync(expected);
        }
        else
        {
            await Expect(error).ToBeHiddenAsync();
        }
    }
}
