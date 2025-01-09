using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using Microsoft.Extensions.Configuration;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace ParkPlaceSample.Pages;

public abstract class BasePage : PageTest
{
    protected new readonly IPage Page;
    protected readonly ILogger Logger;
    protected readonly IConfiguration Configuration;

    protected BasePage(IPage page, ILogger logger, IConfiguration configuration)
    {
        Page = page;
        Logger = logger;
        Configuration = configuration;
    }

    protected string BuildUrl(string path)
    {
        var baseUrl = Configuration["Environment:BaseUrl"]?.TrimEnd('/');
        path = path.TrimStart('/');
        return $"{baseUrl}/{path}";
    }
}