using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using Microsoft.Extensions.Configuration;

namespace PlaywrightDemo.Infrastructure.UI;

public abstract class ElementInteractionBase
{
    protected readonly IPage Page;
    protected readonly ILogger Logger;
    protected readonly IConfiguration Configuration;
    protected readonly string BaseSelector;

    protected ElementInteractionBase(IPage page, ILogger logger, IConfiguration configuration, string baseSelector)
    {
        Page = page;
        Logger = logger;
        Configuration = configuration;
        BaseSelector = baseSelector;
    }

    protected async Task<ILocator> GetElementAsync(string selector)
    {
        return Page.Locator(selector);
    }

    protected async Task WaitForElementAsync(string selector, string state = "visible")
    {
        Logger.LogDebug("Waiting for element: {Selector} to be {State}", selector, state);
        var waitState = state.ToLowerInvariant() switch
        {
            "visible" => WaitForSelectorState.Visible,
            "hidden" => WaitForSelectorState.Hidden,
            "attached" => WaitForSelectorState.Attached,
            "detached" => WaitForSelectorState.Detached,
            _ => WaitForSelectorState.Visible
        };
        await Page.WaitForSelectorAsync(selector, new() { State = waitState });
    }

    protected async Task<bool> IsElementVisibleAsync(string selector)
    {
        try
        {
            await WaitForElementAsync(selector);
            return true;
        }
        catch (TimeoutException)
        {
            return false;
        }
    }
}