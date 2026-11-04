using CapstonePlaywrightTesting19.Configuration;
using CapstonePlaywrightTesting19.Utilities;
using Microsoft.Playwright;
using NUnit.Framework.Interfaces;
using PropertyNames = NUnit.Framework.Internal.PropertyNames;

namespace CapstonePlaywrightTesting19.Fixtures;

/// <summary>
/// Owns the Playwright lifecycle:
///   - one browser per fixture (fixtures run in parallel, tests inside a fixture run sequentially),
///   - a fresh, isolated BrowserContext + Page per test,
///   - screenshot + trace on failure, attached to the HTML report.
/// </summary>
public abstract class BaseTest
{
    private IPlaywright _playwright = null!;
    private IBrowser _browser = null!;

    protected IBrowserContext Context { get; private set; } = null!;

    protected IPage Page { get; private set; } = null!;

    protected static TestSettings Settings => ConfigManager.Settings;

    [OneTimeSetUp]
    public async Task LaunchBrowserAsync()
    {
        _playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        var browserType = Settings.Browser.Type.ToLowerInvariant() switch
        {
            "firefox" => _playwright.Firefox,
            "webkit" => _playwright.Webkit,
            _ => _playwright.Chromium
        };

        _browser = await browserType.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = Settings.Browser.Headless,
            SlowMo = Settings.Browser.SlowMo,
            Channel = string.IsNullOrWhiteSpace(Settings.Browser.Channel) ? null : Settings.Browser.Channel
        });

        Assertions.SetDefaultExpectTimeout(Settings.Timeouts.ExpectMs);
        Logger.Info($"Browser launched for fixture {GetType().Name}");
    }

    [SetUp]
    public async Task CreateContextAsync()
    {
        var test = TestContext.CurrentContext.Test;
        ReportManager.StartTest(test.ID, test.Name, Description(test), Categories(test));
        Logger.Info("===== START =====");

        Context = await _browser.NewContextAsync(new BrowserNewContextOptions
        {
            BaseURL = Settings.BaseUrl,
            Locale = Settings.Browser.Locale,
            ViewportSize = new ViewportSize { Width = Settings.Browser.ViewportWidth, Height = Settings.Browser.ViewportHeight },
            RecordVideoDir = Settings.Artifacts.VideoOnFailure ? ArtifactDir("Videos") : null
        });
        Context.SetDefaultTimeout(Settings.Timeouts.DefaultMs);
        Context.SetDefaultNavigationTimeout(Settings.Timeouts.NavigationMs);

        if (Settings.Artifacts.TraceOnFailure)
        {
            await Context.Tracing.StartAsync(new TracingStartOptions { Screenshots = true, Snapshots = true, Sources = true });
        }

        Page = await Context.NewPageAsync();
    }

    [TearDown]
    public async Task CloseContextAsync()
    {
        var result = TestContext.CurrentContext.Result;
        var failed = result.Outcome.Status == TestStatus.Failed;
        var safeName = SafeFileName(TestContext.CurrentContext.Test.Name);

        try
        {
            string? screenshot = null;
            if (failed && Settings.Artifacts.ScreenshotOnFailure && !Page.IsClosed)
            {
                var path = System.IO.Path.Combine(ArtifactDir("Screenshots"), $"{safeName}.png");
                var bytes = await Page.ScreenshotAsync(new PageScreenshotOptions { Path = path, FullPage = true });
                screenshot = Convert.ToBase64String(bytes);
                TestContext.AddTestAttachment(path, "Screenshot on failure");
                Logger.Info($"Screenshot saved: {path}");
            }

            if (Settings.Artifacts.TraceOnFailure)
            {
                var tracePath = failed ? System.IO.Path.Combine(ArtifactDir("Traces"), $"{safeName}.zip") : null;
                await Context.Tracing.StopAsync(new TracingStopOptions { Path = tracePath });
                if (tracePath is not null)
                {
                    TestContext.AddTestAttachment(tracePath, "Playwright trace (npx playwright show-trace <file>)");
                    ReportManager.AttachFile("Trace", $"../Traces/{safeName}.zip");
                }
            }

            switch (result.Outcome.Status)
            {
                case TestStatus.Passed:
                    ReportManager.Pass("Test passed");
                    Logger.Info("===== PASSED =====");
                    break;
                case TestStatus.Skipped:
                case TestStatus.Inconclusive:
                    ReportManager.Skip(result.Message ?? "Skipped");
                    Logger.Warn($"===== SKIPPED: {result.Message} =====");
                    break;
                default:
                    ReportManager.Fail(result.Message ?? "Test failed", result.StackTrace, screenshot);
                    Logger.Error($"===== FAILED: {result.Message} =====");
                    break;
            }
        }
        finally
        {
            await Context.CloseAsync();
        }
    }

    [OneTimeTearDown]
    public async Task CloseBrowserAsync()
    {
        await _browser.CloseAsync();
        _playwright.Dispose();
    }

    /// <summary>Opens the sign-in page and logs in with the provided test account.</summary>
    protected async Task<UserAccount> LoginAsync()
    {
        var account = await AccountProvider.GetAsync();
        var signIn = new Pages.SignInPage(Page);
        await signIn.OpenAsync();
        await signIn.LoginSuccessfullyAsync(account.Username, account.Password);
        return account;
    }

    private static string ArtifactDir(string name)
    {
        var dir = System.IO.Path.Combine(GlobalSetup.OutputDir, name);
        Directory.CreateDirectory(dir);
        return dir;
    }

    private static string SafeFileName(string name) =>
        string.Concat(name.Select(c => System.IO.Path.GetInvalidFileNameChars().Contains(c) || c is '"' or '\'' or ' ' ? '_' : c));

    private static string Description(TestContext.TestAdapter test) =>
        test.Properties.Get(PropertyNames.Description) as string ?? test.MethodName ?? string.Empty;

    private static IEnumerable<string> Categories(TestContext.TestAdapter test) =>
        test.Properties[PropertyNames.Category].Cast<string>()
            .Concat(new[] { test.ClassName?.Split('.').Last() ?? "Tests" });
}
