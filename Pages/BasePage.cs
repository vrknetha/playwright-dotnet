using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using ParkPlaceSample.Infrastructure.Config;

namespace ParkPlaceSample.Pages;

public abstract class BasePage
{
    protected readonly IPage Page;
    protected readonly ILogger Logger;
    protected readonly TestSettings Settings;

    protected BasePage(IPage page, ILogger logger, TestSettings settings)
    {
        Page = page;
        Logger = logger;
        Settings = settings;
    }

    protected string BuildUrl(string path)
    {
        var baseUrl = Settings.Environment.BaseUrl.TrimEnd('/');
        path = path.TrimStart('/');
        return $"{baseUrl}/{path}";
    }
}