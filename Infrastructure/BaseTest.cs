using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParkPlaceSample.Infrastructure.Config;
using ParkPlaceSample.Infrastructure.Logging;
using ParkPlaceSample.Infrastructure.Navigation;

namespace ParkPlaceSample.Infrastructure;

[TestClass]
public class BaseTest
{
    protected IBrowserContext Context { get; private set; } = null!;
    private IBrowser _browser = null!;
    private IPlaywright _playwright = null!;
    protected TestSettings Settings = null!;
    protected ILogger Logger = null!;
    protected NavigationHelper Navigation = null!;

    public TestContext TestContext { get; set; } = null!;

    [TestInitialize]
    public async Task TestInitialize()
    {
        Settings = ConfigurationLoader.LoadSettings();
        InitializeLogger();

        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = Settings.Browser.Headless,
            SlowMo = Settings.Browser.SlowMo,
            Timeout = Settings.Browser.Timeout,
            Args = Settings.Browser.LaunchArgs.ToArray()
        });

        Context = await _browser.NewContextAsync(new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize
            {
                Width = Settings.Browser.Viewport.Width,
                Height = Settings.Browser.Viewport.Height
            }
        });

        Navigation = new NavigationHelper(Context.Pages[0], Logger, Settings);
    }

    private void InitializeLogger()
    {
        var factory = LoggerFactory.Create(builder =>
        {
            builder.AddTestContext(TestContext);
            builder.SetMinimumLevel(LogLevel.Information);
        });

        Logger = factory.CreateLogger(GetType());
    }

    [TestCleanup]
    public async Task TestCleanup()
    {
        if (Settings.Browser.Screenshots.Enabled &&
            Settings.Browser.Screenshots.TakeOnFailure &&
            TestContext.CurrentTestOutcome != UnitTestOutcome.Passed)
        {
            var screenshotFileName = $"{TestContext.TestName}_{DateTime.Now:yyyyMMdd_HHmmss}.png";
            var screenshotPath = Path.Combine(Settings.Browser.Screenshots.Directory, screenshotFileName);
            Directory.CreateDirectory(Settings.Browser.Screenshots.Directory);

            if (Context.Pages.Count > 0)
            {
                await Context.Pages[0].ScreenshotAsync(new PageScreenshotOptions
                {
                    Path = screenshotPath,
                    FullPage = true
                });

                if (File.Exists(screenshotPath))
                {
                    TestContext.AddResultFile(screenshotPath);
                }
            }
        }

        await Context.DisposeAsync();
        await _browser.DisposeAsync();
        _playwright.Dispose();
    }
}