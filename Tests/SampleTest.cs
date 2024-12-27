using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using ParkPlaceSample.Infrastructure.Base;
using ParkPlaceSample.Infrastructure.Reporting;

namespace ParkPlaceSample.Tests;

[TestClass]
public class SampleTest : TestBase
{
    [TestMethod]
    public async Task SampleEndToEndTest()
    {
        LogInfo("Starting end-to-end test - Initializing test execution");

        // Navigate to Playwright website
        LogInfo($"Navigating to Playwright website - URL: {Settings.Environment.BaseUrl}");
        await Page.GotoAsync(Settings.Environment.BaseUrl);
        LogInfo("Navigation complete");
        LogInfo($"Current URL: {Page.Url}");

        // Wait for and click the 'Get Started' button
        LogInfo("Waiting for 'Get Started' button");
        var getStartedButton = Page.GetByRole(AriaRole.Link, new() { Name = "Get Started" });
        await getStartedButton.WaitForAsync();
        LogInfo("'Get Started' button is visible");

        LogInfo("Clicking 'Get Started' button");
        await getStartedButton.ClickAsync();
        LogInfo("'Get Started' button clicked");

        // Verify navigation to docs page
        LogInfo("Verifying navigation to docs page");
        await Page.WaitForURLAsync("**/docs/intro");
        LogInfo($"URL verification complete - Current URL: {Page.Url}");

        // Find and verify the page heading
        LogInfo("Looking for page heading");
        var heading = Page.GetByRole(AriaRole.Heading, new() { Name = "Installation" });
        var headingText = await heading.TextContentAsync();
        LogInfo($"Found page heading - Text content: {headingText}");

        // Intentionally fail the test to verify infrastructure components
        LogInfo($"About to verify page heading - Expected: 'Wrong Title', Actual: '{headingText}'");
        LogWarning("⚠️ Intentionally failing the test to verify infrastructure");
        Assert.AreEqual("Wrong Title", headingText, "This assertion is meant to fail to verify infrastructure components");
    }

    [TestMethod]
    public async Task SearchFunctionalityTest()
    {
        LogInfo("Starting search functionality test");

        // Navigate to docs page
        LogInfo("Navigating to Playwright docs");
        await Page.GotoAsync($"{Settings.Environment.BaseUrl}/docs/intro");
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
        var searchResults = Page.Locator("[class*='searchResult']");
        var resultsCount = await searchResults.CountAsync();
        LogInfo($"Search results found - Results count: {resultsCount}");

        // Verify results count (intentionally failing)
        LogInfo($"Verifying results count - Expected: 100, Actual: {resultsCount}");
        LogWarning("⚠️ Intentionally verifying incorrect results count");
        Assert.AreEqual(100, resultsCount, $"Expected exactly 100 search results for '{searchQuery}'");
    }

    [TestMethod]
    public async Task APIDocsNavigationTest()
    {
        LogInfo("Starting API documentation navigation test");

        // Navigate to API docs
        LogInfo("Navigating to API documentation");
        await Page.GotoAsync($"{Settings.Environment.BaseUrl}/docs/api/class-playwright");
        LogInfo("API docs page loaded");

        // Verify page title
        var title = await Page.TitleAsync();
        LogInfo($"Page title: {title}");
        Assert.IsTrue(title.Contains("Playwright"), "Page title should contain 'Playwright'");

        // Take screenshot of API docs
        var screenshotPath = Path.Combine(AppContext.BaseDirectory, "api-docs.png");
        await Page.ScreenshotAsync(new() { Path = screenshotPath });
        TestReport?.Log(AventStack.ExtentReports.Status.Info, AttachmentHelper.CreateLogAttachment(screenshotPath, "API Documentation Screenshot"));
    }

    [TestMethod]
    public async Task NavigationAndTitleVerificationTest()
    {
        LogInfo("Starting navigation and title verification test");

        // Navigate to homepage
        LogInfo($"Navigating to homepage - URL: {Settings.Environment.BaseUrl}");
        await Page.GotoAsync(Settings.Environment.BaseUrl);
        LogInfo("Homepage loaded");

        // Verify title
        var title = await Page.TitleAsync();
        LogInfo($"Homepage title: {title}");
        Assert.IsTrue(title.Contains("Playwright"), "Homepage title should contain 'Playwright'");

        // Navigate to docs
        LogInfo("Navigating to documentation");
        await Page.GotoAsync($"{Settings.Environment.BaseUrl}/docs/intro");
        LogInfo("Documentation page loaded");

        // Verify docs title
        var docsTitle = await Page.TitleAsync();
        LogInfo($"Documentation title: {docsTitle}");
        Assert.IsTrue(docsTitle.Contains("Installation"), "Documentation title should contain 'Installation'");
    }
}