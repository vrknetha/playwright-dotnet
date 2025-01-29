using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using PlaywrightDemo.Infrastructure.Config;
using PlaywrightDemo.Infrastructure.Config.Models;
using PlaywrightDemo.Infrastructure.Logging;

namespace AuthSetup.Infrastructure.Base;

public class PlaywrightConsoleBase : IAsyncDisposable
{
    protected IPage Page { get; private set; } = null!;
    protected IBrowser Browser { get; private set; } = null!;
    protected IBrowserContext Context { get; private set; } = null!;
    private IPlaywright _playwright = null!;
    protected ILogger Logger { get; }
    protected TestSettings Settings { get; }

    public PlaywrightConsoleBase()
    {
        // Initialize logger
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });
        Logger = loggerFactory.CreateLogger<PlaywrightConsoleBase>();
        LoggerManager.SetLogger(Logger);

        // Initialize settings
        ConfigurationLoader.Initialize(Logger);
        Settings = ConfigurationLoader.GetSettings<TestSettings>();
    }

    protected async Task InitializeAsync()
    {
        // Initialize Playwright
        _playwright = await Playwright.CreateAsync();

        // Configure browser options
        var launchOptions = new BrowserTypeLaunchOptions
        {
            Headless = Settings.Browser.Headless,
            SlowMo = Settings.Browser.SlowMo
        };

        // Launch browser
        Browser = await _playwright[Settings.Browser.Type].LaunchAsync(launchOptions);

        // Create context
        var contextOptions = new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize
            {
                Width = Settings.Browser.Viewport.Width,
                Height = Settings.Browser.Viewport.Height
            }
        };

        Context = await Browser.NewContextAsync(contextOptions);
        Page = await Context.NewPageAsync();
    }

    protected async Task CleanupAsync()
    {
        if (Context != null)
        {
            await Context.CloseAsync();
        }
        if (Browser != null)
        {
            await Browser.CloseAsync();
        }
        if (_playwright != null)
        {
            _playwright.Dispose();
        }
    }

    public async ValueTask DisposeAsync()
    {
        await CleanupAsync();
    }
}