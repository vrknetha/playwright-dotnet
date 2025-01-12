using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using NUnit.Framework;
using PlaywrightDemo.Infrastructure.Config;
using PlaywrightDemo.Infrastructure.Config.Models;
using PlaywrightDemo.Infrastructure.Logging;

namespace PlaywrightDemo.Pages.Components;

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