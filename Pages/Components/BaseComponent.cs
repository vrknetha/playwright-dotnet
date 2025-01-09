using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using NUnit.Framework;
using ParkPlaceSample.Infrastructure.Config;
using ParkPlaceSample.Infrastructure.Config.Models;
using ParkPlaceSample.Infrastructure.Logging;

namespace ParkPlaceSample.Pages.Components;

public abstract class BaseComponent
{
    protected readonly IPage Page;
    protected readonly ILogger Logger;
    protected readonly TestSettings Settings;

    protected BaseComponent(IPage page)
    {
        Page = page;
        Logger = LoggerManager.Current;
        Settings = ConfigurationLoader.GetSettings<TestSettings>();
    }
}