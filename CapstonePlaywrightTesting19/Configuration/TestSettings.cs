namespace CapstonePlaywrightTesting19.Configuration;

/// <summary>Strongly-typed view over appsettings.json (+ local file + environment variables).</summary>
public sealed class TestSettings
{
    public string BaseUrl { get; set; } = string.Empty;
    public string ApiBaseUrl { get; set; } = string.Empty;
    public string GroupCode { get; set; } = "GP09";
    public BrowserSettings Browser { get; set; } = new();
    public TimeoutSettings Timeouts { get; set; } = new();
    public ArtifactSettings Artifacts { get; set; } = new();
    public TestAccountSettings TestAccount { get; set; } = new();
}

public sealed class BrowserSettings
{
    /// <summary>chromium | firefox | webkit</summary>
    public string Type { get; set; } = "chromium";

    /// <summary>Optional branded channel, e.g. "chrome" or "msedge".</summary>
    public string Channel { get; set; } = string.Empty;

    public bool Headless { get; set; } = true;
    public float SlowMo { get; set; }
    public int ViewportWidth { get; set; } = 1366;
    public int ViewportHeight { get; set; } = 900;
    public string Locale { get; set; } = "vi-VN";
}

public sealed class TimeoutSettings
{
    public float DefaultMs { get; set; } = 15000;
    public float NavigationMs { get; set; } = 30000;
    public float ExpectMs { get; set; } = 15000;
}

public sealed class ArtifactSettings
{
    public string OutputDir { get; set; } = "TestResults";
    public bool ScreenshotOnFailure { get; set; } = true;
    public bool TraceOnFailure { get; set; } = true;
    public bool VideoOnFailure { get; set; }
}

public sealed class TestAccountSettings
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    /// <summary>When no account is configured, register a throw-away account through the API.</summary>
    public bool AutoProvision { get; set; } = true;

    public bool IsConfigured => !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password);
}
