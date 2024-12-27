using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;
using Microsoft.Playwright;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Runtime.InteropServices;

namespace ParkPlaceSample.Infrastructure.Reporting;

/// <summary>
/// Reporter that generates HTML reports using ExtentReports
/// </summary>
public class ExtentReporter : ITestReporter
{
    private readonly string _reportsPath;
    private AventStack.ExtentReports.ExtentReports _extentReports = null!;
    private ExtentTest _currentTest = null!;
    private readonly Dictionary<string, List<TestExecutionMetric>> _testMetrics = new();
    private DateTime _testStartTime;

    private class TestExecutionMetric
    {
        public string TestName { get; set; } = "";
        public TimeSpan Duration { get; set; }
        public bool Passed { get; set; }
        public string FailureStep { get; set; } = "";
        public string Category { get; set; } = "";
    }

    public ExtentReporter(string reportsPath)
    {
        _reportsPath = reportsPath;
    }

    public async Task InitializeAsync()
    {
        // Create reports directory and its subdirectories
        Directory.CreateDirectory(_reportsPath);
        Directory.CreateDirectory(Path.Combine(_reportsPath, "Videos"));
        Directory.CreateDirectory(Path.Combine(_reportsPath, "Traces"));

        var reportPath = Path.Combine(_reportsPath, "index.html");
        _extentReports = new AventStack.ExtentReports.ExtentReports();
        var htmlReporter = new ExtentHtmlReporter(reportPath);

        // Get Playwright version
        using var playwright = await Microsoft.Playwright.Playwright.CreateAsync();
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
            .test-video { margin: 20px 0; padding: 15px; border: 1px solid #ddd; border-radius: 4px; background-color: #f8f9fa; }
            .test-video video { width: 100%; max-width: 800px; border-radius: 4px; }
            .test-trace { margin: 20px 0; padding: 15px; border: 1px solid #ddd; border-radius: 4px; background-color: #f8f9fa; }
            .test-logs { margin: 20px 0; padding: 15px; border: 1px solid #ddd; border-radius: 4px; background-color: #f8f9fa; }
            .test-error { margin: 20px 0; padding: 15px; border: 1px solid #dc3545; border-radius: 4px; background-color: #fff5f5; }
            .file-info { color: #6c757d; margin-top: 10px; }
            .download-button { display: inline-block; padding: 6px 12px; background-color: #007bff; color: white; text-decoration: none; border-radius: 4px; margin-bottom: 15px; }
            .download-button:hover { background-color: #0056b3; color: white; text-decoration: none; }
            .download-button i { margin-right: 5px; }
        ";

        _extentReports.AttachReporter(htmlReporter);
    }

    public void StartTest(TestContext testContext)
    {
        _testStartTime = DateTime.Now;
        _currentTest = _extentReports.CreateTest(testContext.TestName);
    }

    public void LogInfo(string message)
    {
        _currentTest.Info(message);
    }

    public void LogWarning(string message)
    {
        _currentTest.Warning(message);
    }

    public void LogError(string message, Exception? exception = null)
    {
        var errorHtml = $@"
            <div class='test-error'>
                <p style='margin-bottom: 10px; color: #dc3545;'><strong>❌ Test Failure Details:</strong></p>
                <div style='margin-bottom: 10px;'>
                    <strong>Error Type:</strong> {exception?.GetType().Name ?? "Unknown"}
                </div>
                <div style='margin-bottom: 10px;'>
                    <strong>Message:</strong> {message}
                </div>
                {(exception != null ? $@"
                <div>
                    <strong>Stack Trace:</strong>
                    <pre style='background-color: #f8f9fa; padding: 10px; border-radius: 4px; color: #333; font-family: monospace; font-size: 13px; line-height: 1.5; overflow-x: auto; margin-top: 5px;'>{exception.StackTrace}</pre>
                </div>" : "")}
            </div>";
        _currentTest.Error(errorHtml);
    }

    public void AddFileAttachment(string filePath, string description)
    {
        var fileName = Path.GetFileName(filePath);
        var fileExtension = Path.GetExtension(filePath).ToLower();
        var relativePath = Path.GetRelativePath(_reportsPath, filePath);

        switch (fileExtension)
        {
            case ".webm":
                var videoHtml = $@"
                    <div class='test-video'>
                        <p><strong>Test Execution Video:</strong></p>
                        <p>
                            <a href='{relativePath}' download class='download-button'>
                                <i class='fa fa-download'></i> Download Video
                            </a>
                        </p>
                        <video controls>
                            <source src='{relativePath}' type='video/webm'>
                            Your browser does not support the video tag.
                        </video>
                    </div>";
                _currentTest.Info(videoHtml);
                break;

            case ".zip":
                var traceHtml = $@"
                    <div class='test-trace'>
                        <p><strong>Test Execution Trace:</strong></p>
                        <p>
                            <a href='{relativePath}' download class='download-button' style='background-color: #28a745;'>
                                <i class='fa fa-file-archive-o'></i> Download Trace
                            </a>
                        </p>
                        <div class='file-info'>
                            <i class='fa fa-info-circle'></i> The trace file contains detailed information about the test execution, including network requests, console logs, and screenshots.
                        </div>
                    </div>";
                _currentTest.Info(traceHtml);
                break;

            default:
                var attachmentHtml = $@"
                    <div class='test-attachment'>
                        <p><strong>{description}:</strong></p>
                        <p>
                            <a href='{relativePath}' download class='download-button'>
                                <i class='fa fa-file'></i> Download {description}
                            </a>
                        </p>
                    </div>";
                _currentTest.Info(attachmentHtml);
                break;
        }
    }

    public void EndTest(TestContext testContext)
    {
        var duration = DateTime.Now - _testStartTime;
        var testFailed = testContext.CurrentTestOutcome != UnitTestOutcome.Passed;

        // Record test metrics
        var metric = new TestExecutionMetric
        {
            TestName = testContext.TestName,
            Duration = duration,
            Passed = !testFailed,
            FailureStep = testFailed ? _currentTest.Status.ToString() : "",
            Category = GetTestCategory(testContext)
        };

        if (!_testMetrics.ContainsKey(testContext.TestName))
        {
            _testMetrics[testContext.TestName] = new List<TestExecutionMetric>();
        }
        _testMetrics[testContext.TestName].Add(metric);

        if (testFailed)
        {
            _currentTest.Fail("Test failed");
        }
        else
        {
            _currentTest.Pass("Test passed");
        }
    }

    public async Task FinalizeAsync()
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
        await Task.CompletedTask;
    }

    public void Dispose()
    {
        _extentReports?.Flush();
        GC.SuppressFinalize(this);
    }

    private static string GetTestCategory(TestContext testContext)
    {
        if (testContext.TestName.Contains("API"))
            return "API Tests";
        if (testContext.TestName.Contains("Navigation"))
            return "Navigation Tests";
        if (testContext.TestName.Contains("Search"))
            return "Search Tests";

        return "UI Tests";
    }
}