using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using ParkPlaceSample.Config;

namespace ParkPlaceSample.Infrastructure.UI;

public abstract class ElementInteractionBase
{
    protected readonly IPage Page;
    protected readonly ILogger Logger;
    protected readonly TestSettings Settings;
    protected readonly string? BaseSelector;

    protected ElementInteractionBase(IPage page, ILogger logger, TestSettings settings, string? baseSelector = null)
    {
        Page = page;
        Logger = logger;
        Settings = settings;
        BaseSelector = baseSelector;
    }

    protected string GetScopedSelector(string selector) =>
        BaseSelector != null ? $"{BaseSelector} {selector}" : selector;

    protected async Task<ILocator> GetElementAsync(string selector)
    {
        var scopedSelector = GetScopedSelector(selector);
        await WaitForElementAsync(scopedSelector);
        return Page.Locator(scopedSelector);
    }

    protected async Task WaitForElementAsync(string selector, string state = "visible")
    {
        var scopedSelector = GetScopedSelector(selector);
        Logger.LogDebug("Waiting for element: {Selector} with state: {State}", scopedSelector, state);
        var options = new LocatorWaitForOptions
        {
            State = state switch
            {
                "visible" => WaitForSelectorState.Visible,
                "hidden" => WaitForSelectorState.Hidden,
                "attached" => WaitForSelectorState.Attached,
                "detached" => WaitForSelectorState.Detached,
                _ => WaitForSelectorState.Visible
            },
            Timeout = Settings.Timeouts.Element
        };

        await Page.Locator(scopedSelector).WaitForAsync(options);
    }

    protected async Task ClickAsync(string selector)
    {
        var scopedSelector = GetScopedSelector(selector);
        Logger.LogDebug("Clicking element: {Selector}", scopedSelector);
        var element = await GetElementAsync(selector);
        await element.ClickAsync();
    }

    protected async Task TypeTextAsync(string selector, string text)
    {
        var scopedSelector = GetScopedSelector(selector);
        Logger.LogDebug("Typing text into element: {Selector}", scopedSelector);
        var element = await GetElementAsync(selector);
        await element.FillAsync(text);
    }

    protected async Task<string> GetTextAsync(string selector)
    {
        var element = await GetElementAsync(selector);
        return await element.TextContentAsync() ?? string.Empty;
    }

    protected async Task<bool> IsElementVisibleAsync(string selector)
    {
        var scopedSelector = GetScopedSelector(selector);
        try
        {
            await WaitForElementAsync(scopedSelector, "visible");
            return true;
        }
        catch (TimeoutException)
        {
            return false;
        }
    }

    protected async Task<bool> IsElementPresentAsync(string selector)
    {
        var scopedSelector = GetScopedSelector(selector);
        try
        {
            await WaitForElementAsync(scopedSelector, "attached");
            return true;
        }
        catch (TimeoutException)
        {
            return false;
        }
    }

    protected async Task SelectOptionAsync(string selector, string value)
    {
        var scopedSelector = GetScopedSelector(selector);
        Logger.LogDebug("Selecting option in dropdown: {Selector}", scopedSelector);
        var element = await GetElementAsync(selector);
        await element.SelectOptionAsync(value);
    }

    protected async Task HoverAsync(string selector)
    {
        var scopedSelector = GetScopedSelector(selector);
        Logger.LogDebug("Hovering over element: {Selector}", scopedSelector);
        var element = await GetElementAsync(selector);
        await element.HoverAsync();
    }

    protected async Task ScrollIntoViewAsync(string selector)
    {
        var scopedSelector = GetScopedSelector(selector);
        Logger.LogDebug("Scrolling element into view: {Selector}", scopedSelector);
        var element = await GetElementAsync(selector);
        await element.ScrollIntoViewIfNeededAsync();
    }

    protected async Task DragAndDropAsync(string sourceSelector, string targetSelector)
    {
        var scopedSourceSelector = GetScopedSelector(sourceSelector);
        var scopedTargetSelector = GetScopedSelector(targetSelector);
        Logger.LogDebug("Performing drag and drop from {Source} to {Target}", scopedSourceSelector, scopedTargetSelector);
        var sourceElement = await GetElementAsync(sourceSelector);
        var targetElement = await GetElementAsync(targetSelector);
        await sourceElement.DragToAsync(targetElement);
    }

    protected async Task<string> GetAttributeAsync(string selector, string attributeName)
    {
        var element = await GetElementAsync(selector);
        return await element.GetAttributeAsync(attributeName) ?? string.Empty;
    }

    protected async Task<bool> HasClassAsync(string selector, string className)
    {
        var classAttribute = await GetAttributeAsync(selector, "class");
        return classAttribute.Split(' ').Contains(className);
    }

    protected async Task WaitForNetworkIdleAsync()
    {
        Logger.LogDebug("Waiting for network idle");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    protected async Task WaitForDomContentLoadedAsync()
    {
        Logger.LogDebug("Waiting for DOM content loaded");
        await Page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
    }

    protected async Task<bool> WaitForConditionAsync(Func<Task<bool>> condition, int timeout = 10000)
    {
        var startTime = DateTime.Now;
        while (DateTime.Now.Subtract(startTime).TotalMilliseconds < timeout)
        {
            if (await condition())
            {
                return true;
            }
            await Task.Delay(100);
        }
        return false;
    }
}