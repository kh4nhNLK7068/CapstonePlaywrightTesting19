using CapstonePlaywrightTesting19.Configuration;
using CapstonePlaywrightTesting19.Pages.Components;
using CapstonePlaywrightTesting19.Utilities;
using Microsoft.Playwright;

namespace CapstonePlaywrightTesting19.Pages;

/// <summary>
/// Common behaviour for every page object. Pages expose intent-level actions (LoginAsync, SelectSeat…)
/// and locators; assertions live in the tests. All waits rely on Playwright auto-waiting / web-first
/// assertions – there is no Thread.Sleep / WaitForTimeout anywhere in the framework.
/// </summary>
public abstract class BasePage
{
    protected BasePage(IPage page)
    {
        Page = page;
        Header = new HeaderComponent(page);
        Alert = new SweetAlertComponent(page);
    }

    protected IPage Page { get; }

    protected static TestSettings Settings => ConfigManager.Settings;

    /// <summary>Relative route of the page, e.g. "/sign-in".</summary>
    protected abstract string Path { get; }

    /// <summary>An element that only exists once the page has finished rendering its data.</summary>
    protected abstract ILocator ReadyIndicator { get; }

    public HeaderComponent Header { get; }

    public SweetAlertComponent Alert { get; }

    public string Url => Page.Url;

    public async Task OpenAsync(string? path = null)
    {
        var target = path ?? Path;
        Logger.Step($"Open page '{target}'");
        await Page.GotoAsync(target, new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        await WaitUntilReadyAsync();
    }

    public async Task WaitUntilReadyAsync()
    {
        await ReadyIndicator.First.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        Logger.Debug($"{GetType().Name} is ready ({Page.Url})");
    }

    protected async Task FillAsync(ILocator locator, string? value, string fieldName)
    {
        Logger.Step($"Type '{Mask(fieldName, value)}' into {fieldName}");
        await locator.FillAsync(value ?? string.Empty);
    }

    protected async Task ClickAsync(ILocator locator, string elementName)
    {
        Logger.Step($"Click {elementName}");
        await locator.ClickAsync();
    }

    private static string Mask(string fieldName, string? value) =>
        fieldName.Contains("password", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(value)
            ? new string('*', value.Length)
            : value ?? string.Empty;
}
