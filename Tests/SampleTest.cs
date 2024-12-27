using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParkPlaceSample.Infrastructure.Tracing;
using ParkPlaceSample.Infrastructure.Utilities;

namespace ParkPlaceSample.Tests;

[TestClass]
public class SampleTest : BaseTest
{
    private IPage _page = null!;
    private TraceManager _traceManager = null!;

    [TestInitialize]
    public async Task TestInitialize()
    {
        await base.BaseTestInitialize();
        _page = await Context.NewPageAsync();

        // Initialize TraceManager with settings from base configuration
        var logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<TraceManager>();
        var traceSettings = new TraceSettings
        {
            Enabled = true,
            Directory = "TestResults/Traces",
            Mode = TracingMode.Always,
            Screenshots = true,
            Snapshots = true,
            Sources = true
        };

        _traceManager = new TraceManager(Context, logger, traceSettings, base.TestContext);
        await _traceManager.StartTracingAsync();
    }

    [TestCleanup]
    public async Task TestCleanup()
    {
        var testFailed = base.TestContext.CurrentTestOutcome != UnitTestOutcome.Passed;
        await _traceManager.StopTracingAsync(testFailed);
        await _page.CloseAsync();
        await base.BaseTestCleanup();
    }

    [TestMethod]
    public async Task SampleEndToEndTest()
    {
        // Test infrastructure components

        // 1. Test navigation and waiting
        await _page.GotoAsync("https://playwright.dev");
        await WaitHelper.WaitForElementVisibleAsync(_page, "text=Get Started");

        // 2. Test element interaction
        await _page.ClickAsync("text=Get Started");

        // 3. Test URL verification
        await WaitHelper.WaitForUrlAsync(_page, url => url.Contains("/docs/intro"));

        // 4. Test element state verification
        var heading = await _page.Locator("h1").First.TextContentAsync();
        Assert.IsTrue(heading?.Contains("Installation"),
            "Infrastructure components are working: Navigation, Waiting, Element Interaction, and State Verification");
    }
}