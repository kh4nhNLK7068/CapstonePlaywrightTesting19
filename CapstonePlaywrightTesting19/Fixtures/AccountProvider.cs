using CapstonePlaywrightTesting19.Configuration;
using CapstonePlaywrightTesting19.Utilities;

namespace CapstonePlaywrightTesting19.Fixtures;

/// <summary>
/// Supplies the account used by tests that need a logged-in user. Credentials are never hard-coded:
/// they come from configuration (appsettings.local.json / TEST_TestAccount__* env vars) or, if none is
/// configured, a throw-away account is registered once per run through the API.
/// </summary>
public static class AccountProvider
{
    private static readonly SemaphoreSlim Gate = new(1, 1);
    private static UserAccount? _account;

    public static async Task<UserAccount> GetAsync()
    {
        if (_account is not null) return _account;

        await Gate.WaitAsync();
        try
        {
            if (_account is not null) return _account;

            var configured = ConfigManager.Settings.TestAccount;
            if (configured.IsConfigured)
            {
                Logger.Info("Using test account from configuration");
                _account = new UserAccount(configured.Username, configured.Password, string.Empty, string.Empty);
            }
            else if (configured.AutoProvision)
            {
                var account = UserAccount.Random();
                using var api = new MovieApiClient();
                await api.RegisterAsync(account);
                _account = account;
            }
            else
            {
                Assert.Ignore("No test account configured and AutoProvision is disabled.");
            }

            return _account!;
        }
        finally
        {
            Gate.Release();
        }
    }
}
