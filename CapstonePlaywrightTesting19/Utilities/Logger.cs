using Serilog;
using Serilog.Events;

namespace CapstonePlaywrightTesting19.Utilities;

/// <summary>
/// Central logger: every step goes to console, to a rolling log file and to the HTML report
/// node of the currently running test.
/// </summary>
public static class Logger
{
    private static ILogger _log = Serilog.Core.Logger.None;

    public static void Init(string logDirectory)
    {
        Directory.CreateDirectory(logDirectory);
        _log = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Information,
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.File(Path.Combine(logDirectory, "test-run-.log"),
                rollingInterval: RollingInterval.Day,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();
    }

    private static string Prefix => $"[{CurrentTestName}]";

    private static string CurrentTestName
    {
        get
        {
            try { return TestContext.CurrentContext.Test.Name; }
            catch { return "global"; }
        }
    }

    /// <summary>A user-visible test step (also written to the HTML report).</summary>
    public static void Step(string message)
    {
        _log.Information("{Prefix} STEP: {Message}", Prefix, message);
        ReportManager.LogInfo(message);
    }

    public static void Info(string message) => _log.Information("{Prefix} {Message}", Prefix, message);

    public static void Debug(string message) => _log.Debug("{Prefix} {Message}", Prefix, message);

    public static void Warn(string message)
    {
        _log.Warning("{Prefix} {Message}", Prefix, message);
        ReportManager.LogWarning(message);
    }

    public static void Error(string message, Exception? ex = null) =>
        _log.Error(ex, "{Prefix} {Message}", Prefix, message);

    public static void Close() => (_log as IDisposable)?.Dispose();
}
