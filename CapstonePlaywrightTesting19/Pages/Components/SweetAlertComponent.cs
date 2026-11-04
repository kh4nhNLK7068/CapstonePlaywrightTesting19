using CapstonePlaywrightTesting19.Utilities;
using Microsoft.Playwright;

namespace CapstonePlaywrightTesting19.Pages.Components;

/// <summary>
/// The app reports most outcomes through SweetAlert2 pop-ups (success/error/confirm).
/// SweetAlert2 renders stable class names, which are the most robust hook available.
/// </summary>
public sealed class SweetAlertComponent
{
    private readonly IPage _page;

    public SweetAlertComponent(IPage page)
    {
        _page = page;
    }

    public ILocator Popup => _page.Locator(".swal2-popup.swal2-show");

    public ILocator Title => Popup.Locator(".swal2-title");

    /// <summary>Body text (".swal2-html-container" in SweetAlert2 v10+, "#swal2-content" in older versions).</summary>
    public ILocator Text => Popup.Locator(".swal2-html-container, #swal2-content");

    public ILocator SuccessIcon => Popup.Locator(".swal2-icon.swal2-success");

    public ILocator ErrorIcon => Popup.Locator(".swal2-icon.swal2-error");

    public ILocator ConfirmButton => Popup.Locator("button.swal2-confirm");

    public ILocator DenyButton => Popup.Locator("button.swal2-deny");

    public ILocator CancelButton => Popup.Locator("button.swal2-cancel");

    public async Task ConfirmAsync()
    {
        Logger.Step("Confirm the pop-up");
        await ConfirmButton.ClickAsync();
    }

    public async Task DenyAsync()
    {
        Logger.Step("Deny the pop-up");
        await DenyButton.ClickAsync();
    }

    public async Task CancelAsync()
    {
        Logger.Step("Cancel the pop-up");
        await CancelButton.ClickAsync();
    }
}
