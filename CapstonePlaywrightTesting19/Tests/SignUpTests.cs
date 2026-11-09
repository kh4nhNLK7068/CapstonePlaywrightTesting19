using System.Text.RegularExpressions;
using CapstonePlaywrightTesting19.Fixtures;
using CapstonePlaywrightTesting19.Models;
using CapstonePlaywrightTesting19.Pages;
using CapstonePlaywrightTesting19.Utilities;
using static Microsoft.Playwright.Assertions;

namespace CapstonePlaywrightTesting19.Tests;

[TestFixture]
public sealed class SignUpTests : BaseTest
{
    private static readonly string[] AllFields = { "taiKhoan", "matKhau", "confirmPassWord", "hoTen", "email" };

    private static IEnumerable<TestCaseData> ValidationCases() => JsonDataReader.Cases<SignUpCase>("signup_validation.json");

    [TestCaseSource(nameof(ValidationCases))]
    [Category(TestCategories.Validation)]
    [Description("Client-side validation of the sign-up form (data-driven: TestData/signup_validation.json)")]
    public async Task SignUp_ClientValidation(SignUpCase data)
    {
        var signUp = new SignUpPage(Page);
        await signUp.OpenAsync();

        await signUp.FillFormAsync(
            DataGenerator.Resolve(data.Username),
            DataGenerator.Resolve(data.Password),
            DataGenerator.Resolve(data.ConfirmPassword),
            DataGenerator.Resolve(data.FullName),
            DataGenerator.Resolve(data.Email));
        await signUp.SubmitAsync();

        foreach (var field in AllFields)
        {
            if (data.ExpectedErrors.TryGetValue(field, out var expected))
            {
                await Expect(signUp.FieldError(field)).ToHaveTextAsync(expected);
            }
            else
            {
                await Expect(signUp.FieldError(field)).ToBeHiddenAsync();
            }
        }

        // Invalid form must never reach the server.
        await Expect(signUp.Alert.Popup).ToBeHiddenAsync();
        await Expect(Page).ToHaveURLAsync(new Regex("/sign-up$"));
    }

    [Test(Description = "TC_SU_10 - Errors disappear once the field is corrected (validated on blur)")]
    [Category(TestCategories.Validation)]
    public async Task TC_SU_10_ErrorClearsAfterCorrection()
    {
        var signUp = new SignUpPage(Page);
        await signUp.OpenAsync();
        await signUp.SubmitAsync();
        await Expect(signUp.FieldError("hoTen")).ToBeVisibleAsync();

        await signUp.FullNameInput.FillAsync("Nguyen Van A");
        await signUp.FullNameInput.BlurAsync();

        await Expect(signUp.FieldError("hoTen")).ToBeHiddenAsync();
    }

    [Test(Description = "TC_SU_11 - Register a new account with valid data")]
    [Category(TestCategories.HappyPath)]
    [Category(TestCategories.RequiresAccount)]
    public async Task TC_SU_11_RegisterWithValidData()
    {
        var account = UserAccount.Random();
        var signUp = new SignUpPage(Page);
        await signUp.OpenAsync();

        await signUp.FillFormAsync(account.Username, account.Password, account.Password, account.FullName, account.Email);
        await signUp.SubmitAsync();

        await Expect(signUp.Alert.Title).ToHaveTextAsync(AppMessages.Get.SignUpSuccess);
        await Expect(signUp.Alert.SuccessIcon).ToBeVisibleAsync();
        await Expect(signUp.ServerError).ToBeHiddenAsync();
    }

    [Test(Description = "TC_SU_12 - Registering an existing username is rejected by the server")]
    [Category(TestCategories.Validation)]
    [Category(TestCategories.RequiresAccount)]
    public async Task TC_SU_12_RegisterDuplicateUsername()
    {
        var existing = await AccountProvider.GetAsync();
        var signUp = new SignUpPage(Page);
        await signUp.OpenAsync();

        await signUp.FillFormAsync(existing.Username, "Abc@123456", "Abc@123456", "Nguyen Van B", DataGenerator.Email());
        await signUp.SubmitAsync();

        await Expect(signUp.ServerError).ToBeVisibleAsync();
        await Expect(signUp.ServerError).ToContainTextAsync(AppMessages.Get.DuplicateAccount);
    }
}
