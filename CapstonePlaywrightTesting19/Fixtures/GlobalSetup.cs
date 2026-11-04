using CapstonePlaywrightTesting19.Configuration;
using CapstonePlaywrightTesting19.Utilities;

[assembly: Parallelizable(ParallelScope.Fixtures)]

// Lives in the root namespace on purpose: a [SetUpFixture] only applies to tests in its namespace and below.
namespace CapstonePlaywrightTesting19;

/// <summary>Runs once for the whole test assembly: logger, HTML report, browser binaries.</summary>
[SetUpFixture]
public sealed class GlobalSetup
{
    public static string OutputDir { get; private set; } = string.Empty;

    [OneTimeSetUp]
    public void BeforeAll()
    {
        var settings = ConfigManager.Settings;
        OutputDir = System.IO.Path.IsPathRooted(settings.Artifacts.OutputDir)
            ? settings.Artifacts.OutputDir
            : System.IO.Path.Combine(AppContext.BaseDirectory, settings.Artifacts.OutputDir);

        Logger.Init(System.IO.Path.Combine(OutputDir, "Logs"));
        ReportManager.Init(System.IO.Path.Combine(OutputDir, "Reports"));
        Logger.Info($"Test run started – {settings.BaseUrl} on {settings.Browser.Type} (headless={settings.Browser.Headless})");

        // Idempotent: downloads the browser only if it's missing.
        var exitCode = Microsoft.Playwright.Program.Main(new[] { "install", settings.Browser.Type });
        if (exitCode != 0)
        {
            throw new InvalidOperationException($"Playwright browser install failed with exit code {exitCode}");
        }
    }

    [OneTimeTearDown]
    public void AfterAll()
    {
        ReportManager.Flush();
        Logger.Info($"Test run finished – HTML report: {ReportManager.ReportPath}");
        Logger.Close();
    }
}
