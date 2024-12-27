using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using ParkPlaceSample.Config;
using ParkPlaceSample.Infrastructure.UI;

namespace ParkPlaceSample.Pages.Components;

public abstract class BaseComponent : ElementInteractionBase
{
    protected BaseComponent(IPage page, ILogger logger, TestSettings settings, string rootSelector)
        : base(page, logger, settings, rootSelector)
    {
    }

    public async Task<bool> IsVisibleAsync()
    {
        try
        {
            await WaitForElementAsync(BaseSelector!, "visible");
            return true;
        }
        catch (TimeoutException)
        {
            return false;
        }
    }

    public async Task WaitUntilVisibleAsync()
    {
        Logger.LogDebug("Waiting for component to be visible: {Selector}", BaseSelector);
        await WaitForElementAsync(BaseSelector!, "visible");
    }
}