using System.Collections.Concurrent;
using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;
using AventStack.ExtentReports.Reporter.Config;
using CapstonePlaywrightTesting19.Configuration;

namespace CapstonePlaywrightTesting19.Utilities;

/// <summary>
/// Thread-safe wrapper around ExtentReports. One report node per NUnit test,
/// looked up by the NUnit test id so it works with parallel fixtures.
/// </summary>
public static class ReportManager
{
    private static readonly object Sync = new();
    private static readonly ConcurrentDictionary<string, ExtentTest> Tests = new();
    private static ExtentReports? _extent;

    public static string ReportPath { get; private set; } = string.Empty;

    public static void Init(string reportDirectory)
    {
        Directory.CreateDirectory(reportDirectory);
        ReportPath = Path.Combine(reportDirectory, "TestReport.html");

        var spark = new ExtentSparkReporter(ReportPath);
        spark.Config.DocumentTitle = "Capstone Playwright - Automation Report";
        spark.Config.ReportName = "Movie Booking (demo1.cybersoft.edu.vn)";
        spark.Config.Theme = Theme.Standard;
        spark.Config.TimeStampFormat = "dd/MM/yyyy HH:mm:ss";

        var settings = ConfigManager.Settings;
        _extent = new ExtentReports();
        _extent.AttachReporter(spark);
        _extent.AddSystemInfo("Base URL", settings.BaseUrl);
        _extent.AddSystemInfo("Browser", settings.Browser.Type);
        _extent.AddSystemInfo("Headless", settings.Browser.Headless.ToString());
        _extent.AddSystemInfo("OS", Environment.OSVersion.ToString());
        _extent.AddSystemInfo(".NET", Environment.Version.ToString());
    }

    public static void StartTest(string testId, string name, string description, IEnumerable<string> categories)
    {
        if (_extent is null) return;
        lock (Sync)
        {
            var test = _extent.CreateTest(name, description);
            foreach (var category in categories) test.AssignCategory(category);
            Tests[testId] = test;
        }
    }

    private static ExtentTest? Current
    {
        get
        {
            try
            {
                return Tests.TryGetValue(TestContext.CurrentContext.Test.ID, out var test) ? test : null;
            }
            catch
            {
                return null;
            }
        }
    }

    public static void LogInfo(string message)
    {
        lock (Sync) Current?.Info(message);
    }

    public static void LogWarning(string message)
    {
        lock (Sync) Current?.Warning(message);
    }

    public static void Pass(string message)
    {
        lock (Sync) Current?.Pass(message);
    }

    public static void Skip(string message)
    {
        lock (Sync) Current?.Skip(message);
    }

    public static void Fail(string message, string? stackTrace, string? screenshotBase64)
    {
        lock (Sync)
        {
            var test = Current;
            if (test is null) return;

            test.Fail(message);
            if (!string.IsNullOrWhiteSpace(stackTrace))
            {
                test.Fail($"<pre>{System.Net.WebUtility.HtmlEncode(stackTrace)}</pre>");
            }

            if (screenshotBase64 is not null)
            {
                test.Fail("Screenshot at failure",
                    MediaEntityBuilder.CreateScreenCaptureFromBase64String(screenshotBase64, "failure").Build());
            }
        }
    }

    public static void AttachFile(string label, string relativePath)
    {
        lock (Sync) Current?.Info($"{label}: <a href='{relativePath}' target='_blank'>{relativePath}</a>");
    }

    public static void Flush()
    {
        lock (Sync) _extent?.Flush();
    }
}
