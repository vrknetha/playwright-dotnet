using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using ParkPlaceSample.Infrastructure.Base;
using ParkPlaceSample.Infrastructure.Reporting;
using ParkPlaceSample.Infrastructure.API;
using ParkPlaceSample.Pages;
using ParkPlaceSample.Infrastructure.Config;
using NUnit.Framework;

namespace ParkPlaceSample.Tests;

[TestFixture]
[Category("UI")]
public class SampleTest : TestBase
{
    private HomePage _homePage = null!;
    private DocsPage _docsPage = null!;
    private ApiTestHelper _apiHelper = null!;

    [SetUp]
    public override async Task BaseTestInitialize()
    {
        await base.BaseTestInitialize();

        _homePage = new HomePage(Page);
        _docsPage = new DocsPage(Page);
        _apiHelper = new ApiTestHelper(ApiContextManager.Current);
    }

    [Test]
    [Category("Smoke")]
    public async Task SampleEndToEndTest()
    {
        LogInfo("Starting end-to-end test - Initializing test execution");

        // Navigate to homepage and click Get Started
        await _homePage.NavigateAsync();
        await _homePage.ClickGetStartedAsync();

        // Verify the installation heading on docs page
        var headingText = await _docsPage.GetInstallationHeadingTextAsync();
        Assert.That(headingText, Is.EqualTo("Installation"));
    }

    [Test]
    [Category("Search")]
    public async Task SearchFunctionalityTest()
    {
        LogInfo("Starting search functionality test");

        // Navigate to docs page and perform search
        await _docsPage.NavigateAsync();
        await _docsPage.SearchAsync("assertions");

        // Verify results count
        var resultsCount = await _docsPage.GetSearchResultsCountAsync();
        LogInfo($"Found {resultsCount} search results for 'assertions'");
        Assert.That(resultsCount, Is.GreaterThan(0), "Expected at least one search result for 'assertions'");
    }

    [Test]
    [Category("API")]
    public async Task APIDocsNavigationTest()
    {
        LogInfo("Starting API documentation navigation test");

        // Navigate to API docs
        await _docsPage.NavigateToApiDocsAsync();

        // Verify page title
        var title = await _docsPage.GetTitleAsync();
        LogInfo($"Page title: {title}");
        Assert.That(title, Does.Contain("Playwright"), "Page title should contain 'Playwright'");

        // Take screenshot of API docs
        var screenshotPath = Path.Combine(AppContext.BaseDirectory, "api-docs.png");
        await _docsPage.TakeScreenshotAsync(screenshotPath);
        TestReport?.Log(AventStack.ExtentReports.Status.Info, AttachmentHelper.CreateLogAttachment(screenshotPath, "API Documentation Screenshot"));
    }

    [Test]
    [Category("Navigation")]
    public async Task NavigationAndTitleVerificationTest()
    {
        LogInfo("Starting navigation and title verification test");

        // Navigate to homepage and verify title
        await _homePage.NavigateAsync();
        var title = await _homePage.GetTitleAsync();
        Assert.That(title, Does.Contain("Playwright"), "Homepage title should contain 'Playwright'");

        // Navigate to docs and verify title
        await _docsPage.NavigateAsync();
        var docsTitle = await _docsPage.GetTitleAsync();
        Assert.That(docsTitle, Does.Contain("Installation"), "Documentation title should contain 'Installation'");
    }

    [Test]
    [Category("API")]
    public async Task ApiHealthCheckTest()
    {
        LogInfo("Starting API health check test");

        var response = await _apiHelper.GetAsync<object>("/");
        Assert.That(response, Is.Not.Null, "Health check response should not be null");
        LogInfo("Successfully received response from the website");
    }
}