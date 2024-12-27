using Microsoft.Playwright;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ParkPlaceSample.Tests;

[TestClass]
public class BaseTest
{
    protected IBrowserContext Context { get; private set; } = null!;
    private IBrowser _browser = null!;
    private IPlaywright _playwright = null!;

    public TestContext TestContext { get; set; } = null!;

    [TestInitialize]
    public async Task BaseTestInitialize()
    {
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new()
        {
            Headless = false,
            SlowMo = 50
        });
        Context = await _browser.NewContextAsync(new()
        {
            ViewportSize = new ViewportSize
            {
                Width = 1920,
                Height = 1080
            }
        });
    }

    [TestCleanup]
    public async Task BaseTestCleanup()
    {
        if (Context != null)
        {
            await Context.DisposeAsync();
        }
        if (_browser != null)
        {
            await _browser.DisposeAsync();
        }
        _playwright?.Dispose();
    }
}