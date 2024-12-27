using Microsoft.Playwright;
using ParkPlaceSample.Config;

namespace ParkPlaceSample.Infrastructure.Utilities;

public static class WaitHelper
{
    private static readonly RetrySettings DefaultWaitSettings = new()
    {
        MaxAttempts = 60,
        RetryIntervalSeconds = 1
    };

    public static async Task WaitForElementVisibleAsync(IPage page, string selector, int timeoutSeconds = 30)
    {
        await page.WaitForSelectorAsync(selector, new() { State = WaitForSelectorState.Visible, Timeout = timeoutSeconds * 1000 });
    }

    public static async Task WaitForElementNotVisibleAsync(IPage page, string selector, int timeoutSeconds = 30)
    {
        await RetryUtility.RetryAsync(async () =>
        {
            var element = await page.Locator(selector).IsVisibleAsync();
            if (element)
            {
                throw new Exception($"Element with selector '{selector}' is still visible");
            }
        },
        new RetrySettings
        {
            MaxAttempts = timeoutSeconds,
            RetryIntervalSeconds = 1
        });
    }

    public static async Task WaitForUrlAsync(IPage page, Func<string, bool> urlPredicate, int timeoutSeconds = 30)
    {
        var startTime = DateTime.Now;
        while (DateTime.Now.Subtract(startTime).TotalSeconds < timeoutSeconds)
        {
            if (urlPredicate(page.Url))
            {
                return;
            }
            await Task.Delay(100);
        }
        throw new TimeoutException($"URL condition not met within {timeoutSeconds} seconds");
    }

    public static async Task WaitForConditionAsync(Func<Task<bool>> condition, string description, int timeoutSeconds = 30)
    {
        await RetryUtility.RetryAsync(async () =>
        {
            if (!await condition())
            {
                throw new Exception($"Condition not met: {description}");
            }
        },
        new RetrySettings
        {
            MaxAttempts = timeoutSeconds,
            RetryIntervalSeconds = 1
        });
    }
}