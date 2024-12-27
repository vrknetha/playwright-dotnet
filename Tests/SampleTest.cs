using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParkPlaceSample.Infrastructure.Tracing;
using ParkPlaceSample.Infrastructure.Utilities;
using ParkPlaceSample.Infrastructure.Logging;
using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;
using System.Reflection;

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

    [ClassInitialize]
    public static void ClassInitialize(TestContext context)
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
        _extentReports.AttachReporter(htmlReporter);
    }

    [ClassCleanup]
    public static void ClassCleanup()
    {
        _extentReports.Flush();
    }

    [TestInitialize]
    public async Task TestInitialize()
    {
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

    [TestMethod]
    public async Task SampleEndToEndTest()
    {
        try
        {
            _logger.LogInformation("Starting end-to-end test");
            _test.Info("Starting end-to-end test");

            // 1. Test navigation and waiting
            _logger.LogInformation("Navigating to Playwright website");
            _test.Info("Navigating to Playwright website");
            await _page.GotoAsync("https://playwright.dev");
            await WaitHelper.WaitForElementVisibleAsync(_page, "text=Get Started");

            // 2. Test element interaction
            _logger.LogInformation("Clicking Get Started button");
            _test.Info("Clicking Get Started button");
            await _page.ClickAsync("text=Get Started");

            // 3. Test URL verification
            _logger.LogInformation("Verifying navigation to docs page");
            _test.Info("Verifying navigation to docs page");
            await WaitHelper.WaitForUrlAsync(_page, url => url.Contains("/docs/intro"));

            // 4. Test element state verification
            var heading = await _page.Locator("h1").First.TextContentAsync();
            _logger.LogInformation("Found page heading: {Heading}", heading);
            _test.Info($"Found page heading: {heading}");

            // 5. Intentionally fail the test to verify infrastructure
            _logger.LogWarning("About to verify a failing condition");
            _test.Warning("About to verify a failing condition");
            Assert.AreEqual("Wrong Title", heading,
                "This assertion is meant to fail to verify infrastructure components");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Test failed with error");
            _test.Fail(ex);
            throw;
        }
    }
}