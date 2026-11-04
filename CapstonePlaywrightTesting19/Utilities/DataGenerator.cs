namespace CapstonePlaywrightTesting19.Utilities;

/// <summary>Generates unique, valid test data at runtime so nothing is hard-coded in tests.</summary>
public static class DataGenerator
{
    private static string Suffix() => Guid.NewGuid().ToString("N")[..8];

    public static string Username(string prefix = "autotest") => $"{prefix}{Suffix()}";

    public static string Email(string prefix = "autotest") => $"{prefix}.{Suffix()}@mailinator.com";

    /// <summary>Random password that satisfies the app rule (min 6 chars).</summary>
    public static string Password() => $"Pw@{Suffix()}";

    public static string FullName() => "Nguyen Van Kiem Thu";

    /// <summary>
    /// Replaces "{{random}}" placeholders inside data-file values with a unique token,
    /// letting JSON test data describe "a fresh, unique value".
    /// </summary>
    public static string Resolve(string? value) =>
        string.IsNullOrEmpty(value) ? string.Empty : value.Replace("{{random}}", Suffix());
}
