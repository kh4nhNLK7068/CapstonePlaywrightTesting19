namespace CapstonePlaywrightTesting19.Tests;

/// <summary>Category names used for filtering, e.g. dotnet test --filter "TestCategory=Smoke".</summary>
public static class TestCategories
{
    public const string Smoke = "Smoke";
    public const string Regression = "Regression";
    public const string Validation = "Validation";
    public const string HappyPath = "HappyPath";

    /// <summary>Needs a real user account (logs in and/or registers users on the demo server).</summary>
    public const string RequiresAccount = "RequiresAccount";

    /// <summary>Asserts the specified behaviour of a confirmed application bug – expected to fail until it is fixed.</summary>
    public const string KnownDefect = "KnownDefect";
}
