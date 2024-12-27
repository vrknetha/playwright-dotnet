using Microsoft.Playwright;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParkPlaceSample.Infrastructure.Tracing;

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
        var projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
        var reportsPath = Path.GetFullPath(Path.Combine(projectRoot, "TestResults", "Reports"));

        var traceSettings = new TraceSettings
        {
            Enabled = true,
            Directory = Path.Combine(reportsPath, "Traces"),
            Mode = TracingMode.Always,
            Screenshots = true,
            Snapshots = true,
            Sources = true
        };

        _traceManager = new TraceManager(Context, Logger, traceSettings, TestContext);
        await _traceManager.StartTracingAsync();

        LogInfo($"Test initialized: {TestContext.TestName}");
    }

    [TestCleanup]
    public async Task TestCleanup()
    {
        var testFailed = TestContext.CurrentTestOutcome != UnitTestOutcome.Passed;

        // Save video if test failed
        if (testFailed && _page.Video != null)
        {
            var videoPath = await _page.Video.PathAsync();
            AddTestAttachment(videoPath, "Test Execution Video");
            LogInfo($"Video saved: {videoPath}");
        }

        // Stop tracing and get the trace path
        var tracePath = await _traceManager.StopTracingAsync(testFailed);
        if (!string.IsNullOrEmpty(tracePath))
        {
            AddTestAttachment(tracePath, "Test Execution Trace");
            LogInfo($"Trace saved: {tracePath}");
        }

        await _page.CloseAsync();
        await base.BaseTestCleanup();
    }

    [TestMethod]
    public async Task SampleEndToEndTest()
    {
        try
        {
            LogInfo("Starting end-to-end test - Initializing test execution");

            // 1. Test navigation and waiting
            LogInfo("Navigating to Playwright website - URL: https://playwright.dev");
            await _page.GotoAsync("https://playwright.dev");
            LogInfo("Navigation complete");
            LogInfo($"Current URL: {_page.Url}");

            LogInfo("Waiting for 'Get Started' button");
            await _page.WaitForSelectorAsync("text=Get Started", new() { State = WaitForSelectorState.Visible });
            LogInfo("'Get Started' button is visible");

            // 2. Test element interaction
            LogInfo("Clicking 'Get Started' button");
            await _page.ClickAsync("text=Get Started");
            LogInfo("'Get Started' button clicked");

            // 3. Test URL verification
            LogInfo("Verifying navigation to docs page");
            await _page.WaitForURLAsync("**/docs/intro");
            LogInfo($"URL verification complete - Current URL: {_page.Url}");

            // 4. Test element state verification
            LogInfo("Looking for page heading");
            var heading = await _page.Locator("h1").First.TextContentAsync();
            LogInfo($"Found page heading - Text content: {heading}");

            // 5. Intentionally fail the test to verify infrastructure
            LogInfo($"About to verify page heading - Expected: 'Wrong Title', Actual: '{heading}'");
            LogWarning("⚠️ Intentionally failing the test to verify infrastructure");

            Assert.AreEqual("Wrong Title", heading,
                "This assertion is meant to fail to verify infrastructure components");
        }
        catch (Exception ex)
        {
            LogError("Test failed with error", ex);
            throw;
        }
    }

    [TestMethod]
    public async Task SearchFunctionalityTest()
    {
        try
        {
            LogInfo("Starting search functionality test");

            // Navigate to docs
            LogInfo("Navigating to Playwright docs");
            await _page.GotoAsync("https://playwright.dev/docs/intro");
            LogInfo("Docs page loaded");

            // Click search button
            LogInfo("Opening search dialog");
            await _page.ClickAsync("button[type='button']:has-text('Search')");
            LogInfo("Search dialog opened");

            // Type search query
            var searchQuery = "assertions";
            LogInfo($"Entering search query - Query: {searchQuery}");
            await _page.FillAsync("input[type='search']", searchQuery);

            // Wait for search results
            LogInfo("Waiting for search results");
            await _page.WaitForSelectorAsync("mark:has-text('assertions')", new() { State = WaitForSelectorState.Visible });

            // Verify search results - This should fail intentionally
            var resultsCount = await _page.Locator("mark:has-text('assertions')").CountAsync();
            LogInfo($"Search results found - Results count: {resultsCount}");

            // Intentionally fail with a specific assertion
            LogInfo($"Verifying results count - Expected: 100, Actual: {resultsCount}");
            LogWarning("⚠️ Intentionally verifying incorrect results count");
            Assert.AreEqual(100, resultsCount, "Expected exactly 100 search results for 'assertions'");
        }
        catch (Exception ex)
        {
            LogError("Test failed with error", ex);
            throw;
        }
    }

    [TestMethod]
    public async Task NavigationAndTitleVerificationTest()
    {
        try
        {
            LogInfo("Starting navigation test - Verifying page title and navigation");

            // Navigate to Playwright
            LogInfo("Navigating to Playwright website");
            await _page.GotoAsync("https://playwright.dev");
            var title = await _page.TitleAsync();
            LogInfo($"Page loaded - Title: {title}");

            // Verify title - This should pass
            LogInfo($"Verifying page title - Expected: 'Playwright', Actual: '{title}'");
            Assert.IsTrue(title.Contains("Playwright"), "Page title should contain 'Playwright'");
            LogInfo("✅ Title verification passed");
        }
        catch (Exception ex)
        {
            LogError("Test failed with error", ex);
            throw;
        }
    }

    [TestMethod]
    public async Task APIDocsNavigationTest()
    {
        try
        {
            LogInfo("Starting API documentation navigation test");

            // Navigate to API docs
            LogInfo("Navigating to Playwright API docs");
            await _page.GotoAsync("https://playwright.dev/docs/api/class-playwright");
            LogInfo("API docs page loaded");

            // Verify navigation sections
            LogInfo("Verifying navigation sections");
            var sections = await _page.Locator("nav >> a").AllTextContentsAsync();
            LogInfo($"Navigation sections found - Total sections: {sections.Count}");

            // Verify specific sections exist
            var requiredSections = new[] { "Playwright", "Browser", "Page", "Locator" };
            foreach (var section in requiredSections)
            {
                LogInfo($"Checking for '{section}' section");
                Assert.IsTrue(sections.Any(s => s.Contains(section)), $"Navigation should include {section} section");
                LogInfo($"✅ '{section}' section found");
            }
        }
        catch (Exception ex)
        {
            LogError("Test failed with error", ex);
            throw;
        }
    }
}