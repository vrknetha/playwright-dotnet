using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParkPlaceSample.Infrastructure.Base;

namespace ParkPlaceSample.Tests;

[TestClass]
public class SampleTest : TestBase
{
    [TestMethod]
    public async Task SampleEndToEndTest()
    {
        LogInfo("Starting end-to-end test - Initializing test execution");

        // Navigate to Playwright website
        var url = "https://playwright.dev";
        LogInfo($"Navigating to Playwright website - URL: {url}");
        await Page.GotoAsync(url);
        LogInfo("Navigation complete");
        LogInfo($"Current URL: {Page.Url}");

        // Click on Get Started button
        LogInfo("Waiting for 'Get Started' button");
        var getStartedButton = await Page.WaitForSelectorAsync("text=Get Started");
        LogInfo("'Get Started' button is visible");
        LogInfo("Clicking 'Get Started' button");
        await getStartedButton.ClickAsync();
        LogInfo("'Get Started' button clicked");

        // Verify navigation to docs page
        LogInfo("Verifying navigation to docs page");
        var currentUrl = Page.Url;
        LogInfo($"URL verification complete - Current URL: {currentUrl}");
        Assert.IsTrue(currentUrl.Contains("/docs/intro"), "Expected URL to contain '/docs/intro'");

        // Find and verify page heading
        LogInfo("Looking for page heading");
        var heading = await Page.TextContentAsync("h1");
        LogInfo($"Found page heading - Text content: {heading}");
        LogInfo($"About to verify page heading - Expected: 'Wrong Title', Actual: '{heading}'");
        LogWarning("⚠️ Intentionally failing the test to verify infrastructure");
        Assert.AreEqual("Wrong Title", heading, "This assertion is meant to fail to verify infrastructure components");
    }

    [TestMethod]
    public async Task SearchFunctionalityTest()
    {
        LogInfo("Starting search functionality test");

        // Navigate to Playwright docs
        LogInfo("Navigating to Playwright docs");
        await Page.GotoAsync("https://playwright.dev/docs/intro");
        LogInfo("Docs page loaded");

        // Open search dialog
        LogInfo("Opening search dialog");
        await Page.Keyboard.PressAsync("Control+k");
        LogInfo("Search dialog opened");

        // Enter search query
        var searchQuery = "assertions";
        LogInfo($"Entering search query - Query: {searchQuery}");
        await Page.Keyboard.TypeAsync(searchQuery);

        // Wait for search results
        LogInfo("Waiting for search results");
        var searchResults = await Page.QuerySelectorAllAsync("[class*='searchResult']");
        LogInfo($"Search results found - Results count: {searchResults.Count}");

        // Verify results count
        LogInfo($"Verifying results count - Expected: 100, Actual: {searchResults.Count}");
        LogWarning("⚠️ Intentionally verifying incorrect results count");
        Assert.AreEqual(100, searchResults.Count, $"Expected exactly 100 search results for '{searchQuery}'");
    }

    [TestMethod]
    public async Task APIDocsNavigationTest()
    {
        LogInfo("Starting API documentation navigation test");

        // Navigate to Playwright API docs
        await Page.GotoAsync("https://playwright.dev/docs/api/class-playwright");
        LogInfo("API documentation page loaded");

        // Verify page title
        var title = await Page.TitleAsync();
        Assert.IsTrue(title.Contains("Playwright"), "Expected page title to contain 'Playwright'");
        LogInfo("Page title verified");
    }

    [TestMethod]
    public async Task NavigationAndTitleVerificationTest()
    {
        LogInfo("Starting navigation and title verification test");

        // Navigate to homepage
        await Page.GotoAsync("https://playwright.dev");
        LogInfo("Homepage loaded");

        // Take screenshot
        var screenshotPath = Path.Combine(TestContext.TestResultsDirectory, "homepage.png");
        await Page.ScreenshotAsync(new() { Path = screenshotPath });
        AddTestAttachment(screenshotPath, "Homepage Screenshot");
        LogInfo("Screenshot captured");

        // Verify title
        var title = await Page.TitleAsync();
        Assert.IsTrue(title.Contains("Playwright"), "Expected page title to contain 'Playwright'");
        LogInfo("Title verification complete");
    }
}