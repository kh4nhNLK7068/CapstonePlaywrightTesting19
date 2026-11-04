using System.Text.RegularExpressions;
using CapstonePlaywrightTesting19.Fixtures;
using CapstonePlaywrightTesting19.Models;
using CapstonePlaywrightTesting19.Pages;
using static Microsoft.Playwright.Assertions;

namespace CapstonePlaywrightTesting19.Tests;

[TestFixture]
[Category(TestCategories.Smoke)]
public sealed class SmokeTests : BaseTest
{
    [Test(Description = "TC_SMK_01 - Home page loads with header, quick-booking tool and movie list")]
    public async Task TC_SMK_01_HomePageLoads()
    {
        var home = new HomePage(Page);
        await home.OpenAsync();

        await Expect(Page).ToHaveTitleAsync(AppMessages.Get.HomePageTitle);
        await Expect(home.Header.Logo).ToBeVisibleAsync();
        await Expect(home.Header.SignInLink).ToBeVisibleAsync();
        await Expect(home.Header.SignUpLink).ToBeVisibleAsync();
        await Expect(home.MovieSelect).ToBeVisibleAsync();
        await Expect(home.BuyNowButton).ToBeEnabledAsync();
        Assert.That(await home.MovieCards.CountAsync(), Is.GreaterThan(0), "Movie list should not be empty");
    }

    [TestCase("Lịch Chiếu")]
    [TestCase("Cụm Rạp")]
    [TestCase("Tin Tức")]
    [TestCase("Ứng Dụng")]
    [Description("TC_SMK_02 - Header menu items are displayed")]
    public async Task TC_SMK_02_HeaderMenuItemVisible(string menu)
    {
        var home = new HomePage(Page);
        await home.OpenAsync();

        await Expect(home.Header.NavItem(menu)).ToBeVisibleAsync();
    }

    [Test(Description = "TC_SMK_03 - Header 'Đăng Nhập' navigates to the sign-in page")]
    public async Task TC_SMK_03_HeaderSignInNavigates()
    {
        var home = new HomePage(Page);
        await home.OpenAsync();
        await home.Header.GoToSignInAsync();

        var signIn = new SignInPage(Page);
        await Expect(Page).ToHaveURLAsync(new Regex("/sign-in$"));
        await Expect(signIn.Heading).ToBeVisibleAsync();
        await Expect(signIn.UsernameInput).ToBeVisibleAsync();
        await Expect(signIn.PasswordInput).ToHaveAttributeAsync("type", "password");
    }

    [Test(Description = "TC_SMK_04 - Header 'Đăng Ký' navigates to the sign-up page")]
    public async Task TC_SMK_04_HeaderSignUpNavigates()
    {
        var home = new HomePage(Page);
        await home.OpenAsync();
        await home.Header.GoToSignUpAsync();

        var signUp = new SignUpPage(Page);
        await Expect(Page).ToHaveURLAsync(new Regex("/sign-up$"));
        await Expect(signUp.Heading).ToBeVisibleAsync();
        await Expect(signUp.SubmitButton).ToBeEnabledAsync();
    }

    [Test(Description = "TC_SMK_05 - Sign-in and sign-up pages link to each other")]
    public async Task TC_SMK_05_AuthPagesCrossLinks()
    {
        var signIn = new SignInPage(Page);
        await signIn.OpenAsync();
        await signIn.SignUpLink.ClickAsync();
        await Expect(Page).ToHaveURLAsync(new Regex("/sign-up$"));

        var signUp = new SignUpPage(Page);
        await signUp.SignInLink.ClickAsync();
        await Expect(Page).ToHaveURLAsync(new Regex("/sign-in$"));
    }

    [Test(Description = "TC_SMK_06 - Clicking the logo returns to the home page")]
    public async Task TC_SMK_06_LogoReturnsHome()
    {
        var signIn = new SignInPage(Page);
        await signIn.OpenAsync();
        await signIn.Header.ClickLogoAsync();

        var home = new HomePage(Page);
        await Expect(Page).ToHaveURLAsync(new Regex($"^{Regex.Escape(Settings.BaseUrl.TrimEnd('/'))}/?$"));
        await home.WaitUntilReadyAsync();
    }
}
