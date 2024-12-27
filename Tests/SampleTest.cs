using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParkPlaceSample.Infrastructure.Tracing;
using ParkPlaceSample.Infrastructure.Utilities;
using ParkPlaceSample.Infrastructure.Logging;
using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;
using System.Reflection;
using System.Runtime.InteropServices;

namespace ParkPlaceSample.Tests;

[TestClass]
public class SampleTest : BaseTest
{
    private IPage _page = null!;
    private TraceManager _traceManager = null!;
    private ILogger<SampleTest> _logger = null!;
    private IBrowserContext _context = null!;
    private static AventStack.ExtentReports.ExtentReports _extentReports = null!;
    private ExtentTest _test = null!;
    private static Dictionary<string, List<TestExecutionMetric>> _testMetrics = new();
    private DateTime _testStartTime;

    private class TestExecutionMetric
    {
        public string TestName { get; set; } = "";
        public TimeSpan Duration { get; set; }
        public bool Passed { get; set; }
        public string FailureStep { get; set; } = "";
        public string Category { get; set; } = "";
    }

    [ClassInitialize]
    public static async Task ClassInitialize(TestContext context)
    {
        var projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
        var testResultsPath = Path.GetFullPath(Path.Combine(projectRoot, "TestResults"));
        var reportsPath = Path.GetFullPath(Path.Combine(testResultsPath, "Reports"));

        // Create reports directory and its subdirectories
        Directory.CreateDirectory(reportsPath);
        Directory.CreateDirectory(Path.Combine(reportsPath, "Videos"));
        Directory.CreateDirectory(Path.Combine(reportsPath, "Traces"));

        var reportPath = Path.Combine(reportsPath, "index.html");
        _extentReports = new AventStack.ExtentReports.ExtentReports();
        var htmlReporter = new ExtentHtmlReporter(reportPath);

        // Get Playwright version
        using var playwright = await Playwright.CreateAsync();
        var browser = await playwright.Chromium.LaunchAsync();
        var browserVersion = browser.Version;
        await browser.CloseAsync();

        // Configure system info
        _extentReports.AddSystemInfo("Operating System", RuntimeInformation.OSDescription);
        _extentReports.AddSystemInfo("Browser", "Chromium");
        _extentReports.AddSystemInfo("Browser Version", browserVersion);
        _extentReports.AddSystemInfo("Playwright Version", "Latest");
        _extentReports.AddSystemInfo("Machine Name", Environment.MachineName);
        _extentReports.AddSystemInfo(".NET Version", Environment.Version.ToString());
        _extentReports.AddSystemInfo("Test Framework", "MSTest v2");

        // Configure HTML reporter settings
        htmlReporter.Config.DocumentTitle = "Playwright Test Execution Report";
        htmlReporter.Config.ReportName = "Test Automation Results";
        htmlReporter.Config.Theme = AventStack.ExtentReports.Reporter.Configuration.Theme.Standard;

        // Add custom styles for dashboard enhancements
        htmlReporter.Config.CSS = @"
            .dashboard-view { padding: 20px; }
            .test-stats { margin-bottom: 30px; }
            .environment-info { background-color: #f8f9fa; padding: 15px; border-radius: 4px; margin-bottom: 20px; }
            .category-stats { display: flex; flex-wrap: wrap; gap: 20px; margin-bottom: 30px; }
            .category-item { flex: 1; min-width: 200px; padding: 15px; border-radius: 4px; background-color: #fff; box-shadow: 0 1px 3px rgba(0,0,0,0.12); }
            .timing-stats { margin-top: 20px; }
            .timing-item { margin-bottom: 10px; }
            .test-analysis { margin-top: 30px; }
            .failure-pattern { background-color: #fff3cd; padding: 10px; border-radius: 4px; margin-bottom: 10px; }
        ";

        _extentReports.AttachReporter(htmlReporter);
    }

    [ClassCleanup]
    public static void ClassCleanup()
    {
        // Add test execution metrics to the dashboard
        if (_testMetrics.Any())
        {
            var dashboardHtml = new System.Text.StringBuilder();
            dashboardHtml.AppendLine(@"<div class='test-analysis'>");

            // Add timing statistics
            var allMetrics = _testMetrics.SelectMany(x => x.Value).ToList();
            var avgDuration = allMetrics.Average(m => m.Duration.TotalSeconds);
            var slowestTest = allMetrics.OrderByDescending(m => m.Duration).First();
            var fastestTest = allMetrics.OrderBy(m => m.Duration).First();

            dashboardHtml.AppendLine(@"<div class='timing-stats'>");
            dashboardHtml.AppendLine("<h4>📊 Test Execution Analysis</h4>");
            dashboardHtml.AppendLine($"<div class='timing-item'>Average Duration: {avgDuration:F2} seconds</div>");
            dashboardHtml.AppendLine($"<div class='timing-item'>Slowest Test: {slowestTest.TestName} ({slowestTest.Duration.TotalSeconds:F2}s)</div>");
            dashboardHtml.AppendLine($"<div class='timing-item'>Fastest Test: {fastestTest.TestName} ({fastestTest.Duration.TotalSeconds:F2}s)</div>");
            dashboardHtml.AppendLine("</div>");

            // Add failure patterns if any tests failed
            var failedTests = allMetrics.Where(m => !m.Passed).ToList();
            if (failedTests.Any())
            {
                dashboardHtml.AppendLine(@"<div class='failure-patterns'>");
                dashboardHtml.AppendLine("<h4>❌ Failure Analysis</h4>");
                foreach (var failedTest in failedTests)
                {
                    dashboardHtml.AppendLine($@"<div class='failure-pattern'>
                        <strong>{failedTest.TestName}</strong><br/>
                        Failed at step: {failedTest.FailureStep}<br/>
                        Category: {failedTest.Category}
                    </div>");
                }
                dashboardHtml.AppendLine("</div>");
            }

            dashboardHtml.AppendLine("</div>");
            _extentReports.AddTestRunnerLogs(dashboardHtml.ToString());
        }

        _extentReports.Flush();
    }

    [TestInitialize]
    public async Task TestInitialize()
    {
        _testStartTime = DateTime.Now;
        await base.BaseTestInitialize();

        var projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
        var testResultsPath = Path.GetFullPath(Path.Combine(projectRoot, "TestResults"));
        var reportsPath = Path.GetFullPath(Path.Combine(testResultsPath, "Reports"));
        var videosPath = Path.Combine(reportsPath, "Videos");

        // Create context with video recording
        _context = await base.Browser.NewContextAsync(new()
        {
            RecordVideoDir = videosPath
        });
        _page = await _context.NewPageAsync();

        // Initialize Logger
        _logger = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            builder.AddTestContext(TestContext);
        }).CreateLogger<SampleTest>();

        // Initialize TraceManager with settings from base configuration
        var traceSettings = new TraceSettings
        {
            Enabled = true,
            Directory = Path.Combine(reportsPath, "Traces"),
            Mode = TracingMode.Always,
            Screenshots = true,
            Snapshots = true,
            Sources = true
        };

        _traceManager = new TraceManager(_context, _logger, traceSettings, TestContext);
        await _traceManager.StartTracingAsync();

        _logger.LogInformation("Test initialized: {TestName}", TestContext.TestName);
        _test = _extentReports.CreateTest(TestContext.TestName);
    }

    [TestCleanup]
    public async Task TestCleanup()
    {
        var testFailed = TestContext.CurrentTestOutcome != UnitTestOutcome.Passed;
        var duration = DateTime.Now - _testStartTime;

        // Record test metrics
        var metric = new TestExecutionMetric
        {
            TestName = TestContext.TestName,
            Duration = duration,
            Passed = !testFailed,
            FailureStep = testFailed ? _test.Status.ToString() : "",
            Category = GetTestCategory()
        };

        if (!_testMetrics.ContainsKey(TestContext.TestName))
        {
            _testMetrics[TestContext.TestName] = new List<TestExecutionMetric>();
        }
        _testMetrics[TestContext.TestName].Add(metric);

        // Save video if test failed
        if (testFailed && _page.Video != null)
        {
            var videoPath = await _page.Video.PathAsync();
            TestContext.AddResultFile(videoPath);
            _logger.LogInformation("Video saved: {VideoPath}", videoPath);

            // Add video to the HTML report with a download link
            var videoFileName = Path.GetFileName(videoPath);
            var videoHtml = $@"
                <div class='test-video' style='margin: 20px 0; padding: 15px; border: 1px solid #ddd; border-radius: 4px; background-color: #f8f9fa;'>
                    <p style='margin-bottom: 10px;'><strong>Test Execution Video:</strong></p>
                    <p style='margin-bottom: 15px;'>
                        <a href='Videos/{videoFileName}' download style='display: inline-block; padding: 6px 12px; background-color: #007bff; color: white; text-decoration: none; border-radius: 4px;'>
                            <i class='fa fa-download'></i> Download Video
                        </a>
                    </p>
                    <div style='position: relative; padding-bottom: 56.25%; height: 0; overflow: hidden;'>
                        <video style='position: absolute; top: 0; left: 0; width: 100%; height: 100%; border-radius: 4px;' controls>
                            <source src='Videos/{videoFileName}' type='video/webm'>
                            Your browser does not support the video tag.
                        </video>
                    </div>
                </div>";
            _test.Info(videoHtml);
        }

        // Stop tracing and get the trace path
        var tracePath = await _traceManager.StopTracingAsync(testFailed);
        if (!string.IsNullOrEmpty(tracePath))
        {
            var traceFileName = Path.GetFileName(tracePath);
            var traceHtml = $@"
                <div class='test-trace' style='margin: 20px 0; padding: 15px; border: 1px solid #ddd; border-radius: 4px; background-color: #f8f9fa;'>
                    <p style='margin-bottom: 10px;'><strong>Test Execution Trace:</strong></p>
                    <p style='margin-bottom: 15px;'>
                        <a href='Traces/{traceFileName}' download style='display: inline-block; padding: 6px 12px; background-color: #28a745; color: white; text-decoration: none; border-radius: 4px;'>
                            <i class='fa fa-file-archive-o'></i> Download Trace
                        </a>
                    </p>
                    <div class='file-info' style='color: #6c757d;'>
                        <i class='fa fa-info-circle'></i> The trace file contains detailed information about the test execution, including network requests, console logs, and screenshots.
                    </div>
                </div>";
            _test.Info(traceHtml);
            _logger.LogInformation("Trace saved: {TracePath}", tracePath);
        }

        await _page.CloseAsync();
        await _context.CloseAsync();
        await base.BaseTestCleanup();

        _logger.LogInformation("Test completed with status: {TestOutcome}", TestContext.CurrentTestOutcome);

        if (testFailed)
        {
            _test.Fail("Test failed");
        }
        else
        {
            _test.Pass("Test passed");
        }
    }

    private string GetTestCategory()
    {
        var methodInfo = GetType().GetMethod(TestContext.TestName);
        if (methodInfo == null) return "Unknown";

        if (TestContext.TestName.Contains("API"))
            return "API Tests";
        if (TestContext.TestName.Contains("Navigation"))
            return "Navigation Tests";
        if (TestContext.TestName.Contains("Search"))
            return "Search Tests";

        return "UI Tests";
    }

    [TestMethod]
    public async Task SampleEndToEndTest()
    {
        try
        {
            // Create a StringBuilder to collect detailed logs
            var detailedLogs = new System.Text.StringBuilder();
            void LogStep(string step, string details = "")
            {
                var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
                var logMessage = $"[{timestamp}] {step}";
                if (!string.IsNullOrEmpty(details))
                {
                    logMessage += $"\n    Details: {details}";
                }
                detailedLogs.AppendLine(logMessage);
                _logger.LogInformation(step);
                _test.Info(step);
            }

            LogStep("Starting end-to-end test", "Initializing test execution");

            // 1. Test navigation and waiting
            LogStep("Navigating to Playwright website", "URL: https://playwright.dev");
            await _page.GotoAsync("https://playwright.dev");
            var url = _page.Url;
            LogStep("Navigation complete", $"Current URL: {url}");

            LogStep("Waiting for 'Get Started' button");
            await WaitHelper.WaitForElementVisibleAsync(_page, "text=Get Started");
            LogStep("'Get Started' button is visible");

            // 2. Test element interaction
            LogStep("Clicking 'Get Started' button");
            await _page.ClickAsync("text=Get Started");
            LogStep("'Get Started' button clicked");

            // 3. Test URL verification
            LogStep("Verifying navigation to docs page");
            await WaitHelper.WaitForUrlAsync(_page, url => url.Contains("/docs/intro"));
            url = _page.Url;
            LogStep("URL verification complete", $"Current URL: {url}");

            // 4. Test element state verification
            LogStep("Looking for page heading");
            var heading = await _page.Locator("h1").First.TextContentAsync();
            LogStep("Found page heading", $"Text content: {heading}");

            // 5. Intentionally fail the test to verify infrastructure
            LogStep("About to verify page heading", $"Expected: 'Wrong Title', Actual: '{heading}'");
            _test.Warning("⚠️ Intentionally failing the test to verify infrastructure");

            // Before failing, add the detailed logs to the report
            var logsHtml = $@"
                <div class='test-logs' style='margin: 20px 0; padding: 15px; border: 1px solid #ddd; border-radius: 4px; background-color: #f8f9fa;'>
                    <p style='margin-bottom: 10px;'><strong>📋 Detailed Test Execution Log:</strong></p>
                    <pre style='background-color: #f8f9fa; padding: 10px; border-radius: 4px; color: #333; font-family: monospace; font-size: 13px; line-height: 1.5; overflow-x: auto; margin: 0;'>{detailedLogs}</pre>
                </div>";
            _test.Info(logsHtml);

            Assert.AreEqual("Wrong Title", heading,
                "This assertion is meant to fail to verify infrastructure components");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Test failed with error");

            // Add exception details in a readable format
            var errorHtml = $@"
                <div class='test-error' style='margin: 20px 0; padding: 15px; border: 1px solid #dc3545; border-radius: 4px; background-color: #fff5f5;'>
                    <p style='margin-bottom: 10px; color: #dc3545;'><strong>❌ Test Failure Details:</strong></p>
                    <div style='margin-bottom: 10px;'>
                        <strong>Error Type:</strong> {ex.GetType().Name}
                    </div>
                    <div style='margin-bottom: 10px;'>
                        <strong>Message:</strong> {ex.Message}
                    </div>
                    <div>
                        <strong>Stack Trace:</strong>
                        <pre style='background-color: #f8f9fa; padding: 10px; border-radius: 4px; color: #333; font-family: monospace; font-size: 13px; line-height: 1.5; overflow-x: auto; margin-top: 5px;'>{ex.StackTrace}</pre>
                    </div>
                </div>";
            _test.Error(errorHtml);
            throw;
        }
    }

    [TestMethod]
    public async Task NavigationAndTitleVerificationTest()
    {
        try
        {
            var detailedLogs = new System.Text.StringBuilder();
            void LogStep(string step, string details = "")
            {
                var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
                var logMessage = $"[{timestamp}] {step}";
                if (!string.IsNullOrEmpty(details))
                {
                    logMessage += $"\n    Details: {details}";
                }
                detailedLogs.AppendLine(logMessage);
                _logger.LogInformation(step);
                _test.Info(step);
            }

            LogStep("Starting navigation test", "Verifying page title and navigation");

            // Navigate to Playwright
            LogStep("Navigating to Playwright website");
            await _page.GotoAsync("https://playwright.dev");
            var title = await _page.TitleAsync();
            LogStep("Page loaded", $"Title: {title}");

            // Verify title - This should pass
            LogStep("Verifying page title", $"Expected: 'Playwright', Actual: '{title}'");
            Assert.IsTrue(title.Contains("Playwright"), "Page title should contain 'Playwright'");
            LogStep("✅ Title verification passed");

            // Add the detailed logs to the report
            var logsHtml = $@"
                <div class='test-logs' style='margin: 20px 0; padding: 15px; border: 1px solid #ddd; border-radius: 4px; background-color: #f8f9fa;'>
                    <p style='margin-bottom: 10px;'><strong>📋 Detailed Test Execution Log:</strong></p>
                    <pre style='background-color: #f8f9fa; padding: 10px; border-radius: 4px; color: #333; font-family: monospace; font-size: 13px; line-height: 1.5; overflow-x: auto; margin: 0;'>{detailedLogs}</pre>
                </div>";
            _test.Info(logsHtml);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Test failed with error");
            var errorHtml = $@"
                <div class='test-error' style='margin: 20px 0; padding: 15px; border: 1px solid #dc3545; border-radius: 4px; background-color: #fff5f5;'>
                    <p style='margin-bottom: 10px; color: #dc3545;'><strong>❌ Test Failure Details:</strong></p>
                    <div style='margin-bottom: 10px;'><strong>Error Type:</strong> {ex.GetType().Name}</div>
                    <div style='margin-bottom: 10px;'><strong>Message:</strong> {ex.Message}</div>
                    <div><strong>Stack Trace:</strong><pre style='background-color: #f8f9fa; padding: 10px; border-radius: 4px; color: #333; font-family: monospace; font-size: 13px; line-height: 1.5; overflow-x: auto; margin-top: 5px;'>{ex.StackTrace}</pre></div>
                </div>";
            _test.Error(errorHtml);
            throw;
        }
    }

    [TestMethod]
    public async Task SearchFunctionalityTest()
    {
        try
        {
            var detailedLogs = new System.Text.StringBuilder();
            void LogStep(string step, string details = "")
            {
                var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
                var logMessage = $"[{timestamp}] {step}";
                if (!string.IsNullOrEmpty(details))
                {
                    logMessage += $"\n    Details: {details}";
                }
                detailedLogs.AppendLine(logMessage);
                _logger.LogInformation(step);
                _test.Info(step);
            }

            LogStep("Starting search functionality test");

            // Navigate to docs
            LogStep("Navigating to Playwright docs");
            await _page.GotoAsync("https://playwright.dev/docs/intro");
            LogStep("Docs page loaded");

            // Click search button
            LogStep("Opening search dialog");
            await _page.ClickAsync("button[type='button']:has-text('Search')");
            LogStep("Search dialog opened");

            // Type search query
            var searchQuery = "assertions";
            LogStep("Entering search query", $"Query: {searchQuery}");
            await _page.FillAsync("input[type='search']", searchQuery);

            // Wait for search results
            LogStep("Waiting for search results");
            await _page.WaitForSelectorAsync("mark:has-text('assertions')", new() { State = WaitForSelectorState.Visible });

            // Verify search results - This should fail intentionally
            var resultsCount = await _page.Locator("mark:has-text('assertions')").CountAsync();
            LogStep("Search results found", $"Results count: {resultsCount}");

            // Intentionally fail with a specific assertion
            LogStep("Verifying results count", $"Expected: 100, Actual: {resultsCount}");
            _test.Warning("⚠️ Intentionally verifying incorrect results count");
            Assert.AreEqual(100, resultsCount, "Expected exactly 100 search results for 'assertions'");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Test failed with error");
            var errorHtml = $@"
                <div class='test-error' style='margin: 20px 0; padding: 15px; border: 1px solid #dc3545; border-radius: 4px; background-color: #fff5f5;'>
                    <p style='margin-bottom: 10px; color: #dc3545;'><strong>❌ Test Failure Details:</strong></p>
                    <div style='margin-bottom: 10px;'><strong>Error Type:</strong> {ex.GetType().Name}</div>
                    <div style='margin-bottom: 10px;'><strong>Message:</strong> {ex.Message}</div>
                    <div><strong>Stack Trace:</strong><pre style='background-color: #f8f9fa; padding: 10px; border-radius: 4px; color: #333; font-family: monospace; font-size: 13px; line-height: 1.5; overflow-x: auto; margin-top: 5px;'>{ex.StackTrace}</pre></div>
                </div>";
            _test.Error(errorHtml);
            throw;
        }
    }

    [TestMethod]
    public async Task APIDocsNavigationTest()
    {
        try
        {
            var detailedLogs = new System.Text.StringBuilder();
            void LogStep(string step, string details = "")
            {
                var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
                var logMessage = $"[{timestamp}] {step}";
                if (!string.IsNullOrEmpty(details))
                {
                    logMessage += $"\n    Details: {details}";
                }
                detailedLogs.AppendLine(logMessage);
                _logger.LogInformation(step);
                _test.Info(step);
            }

            LogStep("Starting API documentation navigation test");

            // Navigate to API docs
            LogStep("Navigating to Playwright API docs");
            await _page.GotoAsync("https://playwright.dev/docs/api/class-playwright");
            LogStep("API docs page loaded");

            // Verify navigation sections
            LogStep("Verifying navigation sections");
            var sections = await _page.Locator("nav >> a").AllTextContentsAsync();
            LogStep("Navigation sections found", $"Total sections: {sections.Count}");

            // Verify specific sections exist
            var requiredSections = new[] { "Playwright", "Browser", "Page", "Locator" };
            foreach (var section in requiredSections)
            {
                LogStep($"Checking for '{section}' section");
                Assert.IsTrue(sections.Any(s => s.Contains(section)), $"Navigation should include {section} section");
                LogStep($"✅ '{section}' section found");
            }

            // Add the detailed logs to the report
            var logsHtml = $@"
                <div class='test-logs' style='margin: 20px 0; padding: 15px; border: 1px solid #ddd; border-radius: 4px; background-color: #f8f9fa;'>
                    <p style='margin-bottom: 10px;'><strong>📋 Detailed Test Execution Log:</strong></p>
                    <pre style='background-color: #f8f9fa; padding: 10px; border-radius: 4px; color: #333; font-family: monospace; font-size: 13px; line-height: 1.5; overflow-x: auto; margin: 0;'>{detailedLogs}</pre>
                </div>";
            _test.Info(logsHtml);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Test failed with error");
            var errorHtml = $@"
                <div class='test-error' style='margin: 20px 0; padding: 15px; border: 1px solid #dc3545; border-radius: 4px; background-color: #fff5f5;'>
                    <p style='margin-bottom: 10px; color: #dc3545;'><strong>❌ Test Failure Details:</strong></p>
                    <div style='margin-bottom: 10px;'><strong>Error Type:</strong> {ex.GetType().Name}</div>
                    <div style='margin-bottom: 10px;'><strong>Message:</strong> {ex.Message}</div>
                    <div><strong>Stack Trace:</strong><pre style='background-color: #f8f9fa; padding: 10px; border-radius: 4px; color: #333; font-family: monospace; font-size: 13px; line-height: 1.5; overflow-x: auto; margin-top: 5px;'>{ex.StackTrace}</pre></div>
                </div>";
            _test.Error(errorHtml);
            throw;
        }
    }
}