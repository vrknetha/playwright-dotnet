using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using Microsoft.Extensions.Configuration;

namespace ParkPlaceSample.Pages.Components;

public abstract class BaseComponent
{
    protected readonly IPage Page;
    protected readonly ILogger Logger;
    protected readonly IConfiguration Configuration;

    protected BaseComponent(IPage page, ILogger logger, IConfiguration configuration)
    {
        Page = page;
        Logger = logger;
        Configuration = configuration;
    }
}