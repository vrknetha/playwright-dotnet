using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using PlaywrightDemo.Infrastructure.Config;
using PlaywrightDemo.Infrastructure.Config.Models;
using PlaywrightDemo.Infrastructure.Logging;

namespace PlaywrightDemo.Pages;

public abstract class BasePage : PageTest
{
    protected readonly IPage Page;
    protected readonly ILogger Logger;
    protected readonly TestSettings Settings;

    protected BasePage(IPage page)
    {
        Page = page;
        Logger = LoggerManager.Current;
        Settings = ConfigurationLoader.GetSettings<TestSettings>();
    }

    protected string BuildUrl(string path)
    {
        var baseUrl = Settings.Environment.BaseUrl?.TrimEnd('/');
        path = path.TrimStart('/');
        return $"{baseUrl}/{path}";
    }
}