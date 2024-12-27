using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;
using System.Runtime.InteropServices;
using ParkPlaceSample.Infrastructure.Tracing;

namespace ParkPlaceSample.Tests;

[TestClass]
public class BaseTest
{
    protected IBrowserContext Context { get; private set; } = null!;
    protected IBrowser Browser { get; private set; } = null!;
    protected ILogger Logger { get; private set; } = null!;
    private IPlaywright _playwright = null!;
    private static AventStack.ExtentReports.ExtentReports _extentReports = null!;
    protected ExtentTest TestReport { get; private set; } = null!;
    private DateTime _testStartTime;
    private static Dictionary<string, List<TestExecutionMetric>> _testMetrics = new();

    public TestContext TestContext { get; set; } = null!;

    private class TestExecutionMetric
    {
        public string TestName { get; set; } = "";
        public TimeSpan Duration { get; set; }
        public bool Passed { get; set; }
        public string FailureStep { get; set; } = "";
        public string Category { get; set; } = "";
    }

    [AssemblyInitialize]
    public static async Task AssemblyInitialize(TestContext context)
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

    [AssemblyCleanup]
    public static void AssemblyCleanup()
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
    public virtual async Task BaseTestInitialize()
    {
        _testStartTime = DateTime.Now;

        // Initialize Logger
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
        });
        Logger = loggerFactory.CreateLogger(GetType());

        // Initialize Playwright
        _playwright = await Playwright.CreateAsync();
        Browser = await _playwright.Chromium.LaunchAsync(new()
        {
            Headless = true,
            SlowMo = 50
        });

        // Initialize Browser Context with video recording
        var projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
        var videosPath = Path.Combine(projectRoot, "TestResults", "Reports", "Videos");

        Context = await Browser.NewContextAsync(new()
        {
            ViewportSize = new ViewportSize { Width = 1920, Height = 1080 },
            RecordVideoDir = videosPath
        });

        // Initialize Test Report
        TestReport = _extentReports.CreateTest(TestContext.TestName);
        LogInfo($"Test initialized: {TestContext.TestName}");
    }

    [TestCleanup]
    public virtual async Task BaseTestCleanup()
    {
        var testFailed = TestContext.CurrentTestOutcome != UnitTestOutcome.Passed;
        var duration = DateTime.Now - _testStartTime;

        // Record test metrics
        var metric = new TestExecutionMetric
        {
            TestName = TestContext.TestName,
            Duration = duration,
            Passed = !testFailed,
            FailureStep = testFailed ? TestReport.Status.ToString() : "",
            Category = GetTestCategory()
        };

        if (!_testMetrics.ContainsKey(TestContext.TestName))
        {
            _testMetrics[TestContext.TestName] = new List<TestExecutionMetric>();
        }
        _testMetrics[TestContext.TestName].Add(metric);

        if (Context != null)
        {
            await Context.DisposeAsync();
        }
        if (Browser != null)
        {
            await Browser.DisposeAsync();
        }
        _playwright?.Dispose();

        LogInfo($"Test completed with status: {TestContext.CurrentTestOutcome}");

        if (testFailed)
        {
            TestReport.Fail("Test failed");
        }
        else
        {
            TestReport.Pass("Test passed");
        }
    }

    protected void LogInfo(string message)
    {
        Logger.LogInformation(message);
        TestReport.Info(message);
    }

    protected void LogWarning(string message)
    {
        Logger.LogWarning(message);
        TestReport.Warning(message);
    }

    protected void LogError(string message, Exception? ex = null)
    {
        Logger.LogError(ex, message);
        TestReport.Error(message);
    }

    protected void AddTestAttachment(string filePath, string description)
    {
        TestContext.AddResultFile(filePath);
        var fileName = Path.GetFileName(filePath);
        var fileExtension = Path.GetExtension(filePath).ToLower();
        var isVideo = fileExtension == ".webm";
        var isTrace = fileExtension == ".zip";

        var html = isVideo ? CreateVideoAttachmentHtml(fileName) :
                  isTrace ? CreateTraceAttachmentHtml(fileName) :
                  CreateGenericAttachmentHtml(fileName, description);

        TestReport.Info(html);
    }

    private string CreateVideoAttachmentHtml(string fileName)
    {
        return $@"
            <div class='test-video' style='margin: 20px 0; padding: 15px; border: 1px solid #ddd; border-radius: 4px; background-color: #f8f9fa;'>
                <p style='margin-bottom: 10px;'><strong>Test Execution Video:</strong></p>
                <p style='margin-bottom: 15px;'>
                    <a href='Videos/{fileName}' download style='display: inline-block; padding: 6px 12px; background-color: #007bff; color: white; text-decoration: none; border-radius: 4px;'>
                        <i class='fa fa-download'></i> Download Video
                    </a>
                </p>
                <div style='position: relative; padding-bottom: 56.25%; height: 0; overflow: hidden;'>
                    <video style='position: absolute; top: 0; left: 0; width: 100%; height: 100%; border-radius: 4px;' controls>
                        <source src='Videos/{fileName}' type='video/webm'>
                        Your browser does not support the video tag.
                    </video>
                </div>
            </div>";
    }

    private string CreateTraceAttachmentHtml(string fileName)
    {
        return $@"
            <div class='test-trace' style='margin: 20px 0; padding: 15px; border: 1px solid #ddd; border-radius: 4px; background-color: #f8f9fa;'>
                <p style='margin-bottom: 10px;'><strong>Test Execution Trace:</strong></p>
                <p style='margin-bottom: 15px;'>
                    <a href='Traces/{fileName}' download style='display: inline-block; padding: 6px 12px; background-color: #28a745; color: white; text-decoration: none; border-radius: 4px;'>
                        <i class='fa fa-file-archive-o'></i> Download Trace
                    </a>
                </p>
                <div class='file-info' style='color: #6c757d;'>
                    <i class='fa fa-info-circle'></i> The trace file contains detailed information about the test execution, including network requests, console logs, and screenshots.
                </div>
            </div>";
    }

    private string CreateGenericAttachmentHtml(string fileName, string description)
    {
        return $@"
            <div class='test-attachment' style='margin: 20px 0; padding: 15px; border: 1px solid #ddd; border-radius: 4px; background-color: #f8f9fa;'>
                <p style='margin-bottom: 10px;'><strong>{description}:</strong></p>
                <p style='margin-bottom: 15px;'>
                    <a href='{fileName}' download style='display: inline-block; padding: 6px 12px; background-color: #6c757d; color: white; text-decoration: none; border-radius: 4px;'>
                        <i class='fa fa-download'></i> Download File
                    </a>
                </p>
            </div>";
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
}