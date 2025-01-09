using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using NUnit.Framework;
using ParkPlaceSample.Infrastructure.Config;
using ParkPlaceSample.Infrastructure.Config.Models;
using ParkPlaceSample.Infrastructure.Logging;

namespace ParkPlaceSample.Pages;

public abstract class BasePage
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