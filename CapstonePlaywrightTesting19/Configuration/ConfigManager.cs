using Microsoft.Extensions.Configuration;

namespace CapstonePlaywrightTesting19.Configuration;

/// <summary>
/// Loads settings in priority order (last wins):
///   1. Configuration/appsettings.json            – committed defaults
///   2. Configuration/appsettings.{TEST_ENV}.json – optional per-environment file
///   3. Configuration/appsettings.local.json      – optional, git-ignored (secrets)
///   4. Environment variables prefixed TEST_      – e.g. TEST_Browser__Headless=false
/// </summary>
public static class ConfigManager
{
    private static readonly Lazy<TestSettings> LazySettings = new(Load, isThreadSafe: true);

    public static TestSettings Settings => LazySettings.Value;

    private static TestSettings Load()
    {
        var baseDir = Path.Combine(AppContext.BaseDirectory, "Configuration");
        var environment = Environment.GetEnvironmentVariable("TEST_ENV");

        var builder = new ConfigurationBuilder()
            .SetBasePath(baseDir)
            .AddJsonFile("appsettings.json", optional: false);

        if (!string.IsNullOrWhiteSpace(environment))
        {
            builder.AddJsonFile($"appsettings.{environment}.json", optional: true);
        }

        var configuration = builder
            .AddJsonFile("appsettings.local.json", optional: true)
            .AddEnvironmentVariables(prefix: "TEST_")
            .Build();

        var settings = new TestSettings();
        configuration.Bind(settings);
        return settings;
    }
}
