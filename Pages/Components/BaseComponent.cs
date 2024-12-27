using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using ParkPlaceSample.Infrastructure.Config;

namespace ParkPlaceSample.Pages.Components;

public abstract class BaseComponent
{
    protected readonly IPage Page;
    protected readonly ILogger Logger;
    protected readonly TestSettings Settings;

    protected BaseComponent(IPage page, ILogger logger, TestSettings settings)
    {
        Page = page;
        Logger = logger;
        Settings = settings;
    }
}