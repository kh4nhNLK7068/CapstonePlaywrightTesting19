using CapstonePlaywrightTesting19.Utilities;

namespace CapstonePlaywrightTesting19.Models;

public sealed class SignUpCase : IDataCase
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    /// <summary>field id → expected helper text.</summary>
    public Dictionary<string, string> ExpectedErrors { get; set; } = new();

    public override string ToString() => Id;
}

public sealed class SignInCase : IDataCase
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public Dictionary<string, string> ExpectedErrors { get; set; } = new();
    public string ExpectedServerError { get; set; } = string.Empty;

    public override string ToString() => Id;
}

public sealed class QuickBookingCase : IDataCase
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public bool SelectMovie { get; set; }
    public bool SelectCinema { get; set; }
    public bool SelectShowtime { get; set; }
    public string ExpectedTitle { get; set; } = string.Empty;
    public string ExpectedText { get; set; } = string.Empty;

    public override string ToString() => Id;
}

/// <summary>Expected UI messages, kept out of the test code (TestData/messages.json).</summary>
public sealed class AppMessages
{
    private static readonly Lazy<AppMessages> Instance = new(() => JsonDataReader.Read<AppMessages>("messages.json"));

    public static AppMessages Get => Instance.Value;

    public string SignInSuccess { get; set; } = string.Empty;
    public string SignUpSuccess { get; set; } = string.Empty;
    public string SignOutQuestion { get; set; } = string.Empty;
    public string SignOutDone { get; set; } = string.Empty;
    public string DuplicateAccount { get; set; } = string.Empty;
    public string BookingNotLoggedInTitle { get; set; } = string.Empty;
    public string BookingNotLoggedInText { get; set; } = string.Empty;
    public string BookingNoSeatTitle { get; set; } = string.Empty;
    public string BookingNoSeatText { get; set; } = string.Empty;
    public string BookingSuccessTitle { get; set; } = string.Empty;
    public string BookingSuccessText { get; set; } = string.Empty;
    public string HomePageTitle { get; set; } = string.Empty;
    public List<int> PreferredShowtimeIds { get; set; } = new();
}
