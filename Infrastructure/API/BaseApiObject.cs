using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using PlaywrightDemo.Infrastructure.Config.Models;

namespace PlaywrightDemo.Infrastructure.API;

public abstract class BaseApiObject
{
    protected readonly IAPIRequestContext ApiContext;
    protected readonly ILogger Logger;
    protected readonly TestSettings Settings;

    protected BaseApiObject(IAPIRequestContext apiContext, ILogger logger, TestSettings settings)
    {
        ApiContext = apiContext;
        Logger = logger;
        Settings = settings;
    }
}