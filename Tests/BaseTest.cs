using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;
using System.Runtime.InteropServices;
using System.Web;

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
        htmlReporter.Config.EnableTimeline = true;

        // Add custom styles for dashboard enhancements and log formatting
        htmlReporter.Config.CSS = @"
            /* Reset ExtentReports default styles */
            .test-content { border: none !important; }
            .test-content .test-steps { border: none !important; margin: 0 !important; padding: 0 !important; }
            .test-content .test-step { border: none !important; margin: 0 !important; padding: 0 !important; }
            .test-content .test-step td { border: none !important; padding: 4px 8px !important; }
            .test-content .test-step .step-details { padding: 0 !important; margin: 0 !important; }
            .test-content .test-step .step-details > div { margin: 4px 0 !important; }
            
            /* Step status indicator styling */
            .test-content .test-step .status {
                display: inline-block !important;
                width: 20px !important;
                height: 20px !important;
                line-height: 20px !important;
                text-align: center !important;
                border-radius: 50% !important;
                margin-right: 8px !important;
                font-size: 12px !important;
                font-weight: bold !important;
            }
            .test-content .test-step .status.pass { background-color: #28a745 !important; color: white !important; }
            .test-content .test-step .status.fail { background-color: #dc3545 !important; color: white !important; }
            .test-content .test-step .status.warning { background-color: #ffc107 !important; color: #000 !important; }
            .test-content .test-step .status.info { background-color: #0d6efd !important; color: white !important; }
            .test-content .test-step .status.skip { background-color: #6c757d !important; color: white !important; }
            
            /* Step details styling */
            .test-content .test-step .step-details {
                display: flex !important;
                align-items: center !important;
                padding: 8px 12px !important;
                background-color: #f8f9fa !important;
                border-radius: 4px !important;
                margin: 4px 0 !important;
            }
            
            /* Step timestamp styling */
            .test-content .test-step .timestamp {
                color: #6c757d !important;
                font-size: 12px !important;
                margin-right: 12px !important;
                font-family: Consolas, monospace !important;
            }
            
            /* Step details text styling */
            .test-content .test-step .step-details .details {
                flex: 1 !important;
                font-family: 'Segoe UI', Arial, sans-serif !important;
                line-height: 1.5 !important;
            }
            
            /* Log message styling */
            .log-message {
                padding: 8px 12px;
                margin: 4px 0;
                border-radius: 4px;
                font-family: Consolas, monospace;
                line-height: 1.5;
                background-color: #f8f9fa;
                border-left: 4px solid #0d6efd;
            }
            .log-message.warning {
                background-color: #fff3cd;
                border-left-color: #ffc107;
            }
            .log-message.error {
                background-color: #f8d7da;
                border-left-color: #dc3545;
            }
            
            /* Error details styling */
            .error-details {
                margin: 8px 0 0 12px;
                padding: 8px;
                background: #fff3f3;
                border-radius: 4px;
            }
            .error-message { margin-bottom: 8px; }
            .stack-trace { margin-top: 8px; }
            .stack-trace pre {
                margin: 4px 0;
                padding: 8px;
                font-size: 12px;
                background: #f8f9fa;
                border-radius: 4px;
                overflow-x: auto;
            }
            
            /* Card styling */
            .card {
                margin: 10px 0;
                border: 1px solid #dee2e6;
                border-radius: 4px;
                background: #fff;
            }
            .card-header {
                padding: 10px 15px;
                background-color: #f8f9fa;
                border-bottom: 1px solid #dee2e6;
            }
            .card-body { padding: 15px; }
            
            /* Button styling */
            .btn {
                display: inline-block;
                padding: 6px 12px;
                margin-bottom: 15px;
                font-size: 14px;
                font-weight: 400;
                text-align: center;
                text-decoration: none;
                border-radius: 4px;
                cursor: pointer;
            }
            .btn-primary { background-color: #0d6efd; color: white !important; }
            .btn-success { background-color: #28a745; color: white !important; }
            .btn-secondary { background-color: #6c757d; color: white !important; }
            
            /* Alert styling */
            .alert {
                padding: 12px;
                margin: 8px 0;
                border-radius: 4px;
            }
            .alert-info {
                background-color: #cff4fc;
                border: 1px solid #b6effb;
                color: #055160;
            }
            
            /* Dashboard styling */
            .dashboard-view { padding: 20px; }
            .test-stats { margin-bottom: 30px; }
            .environment-info { background-color: #f8f9fa; padding: 15px; border-radius: 4px; margin-bottom: 20px; }
            .category-stats { display: flex; flex-wrap: wrap; gap: 20px; margin-bottom: 30px; }
            .category-item { flex: 1; min-width: 200px; padding: 15px; border-radius: 4px; background-color: #fff; box-shadow: 0 1px 3px rgba(0,0,0,0.12); }
            .timing-stats { margin-top: 20px; }
            .timing-item { margin-bottom: 10px; }
            .test-analysis { margin-top: 30px; }
            .failure-pattern { background-color: #fff3cd; padding: 10px; border-radius: 4px; margin-bottom: 10px; }
            
            /* Timeline styling */
            .timeline-item-container { border: none !important; }
            .timeline-item { margin: 4px 0 !important; padding: 8px !important; border-radius: 4px !important; }
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
            var allMetrics = _testMetrics.SelectMany(x => x.Value)
                                       .GroupBy(x => x.TestName)
                                       .Select(g => g.Last()) // Take only the last execution of each test
                                       .ToList();

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

        // Initialize Logger only if not already initialized
        if (Logger == null)
        {
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
            });
            Logger = loggerFactory.CreateLogger(GetType());
        }

        // Initialize Playwright only if not already initialized
        if (_playwright == null)
        {
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

            // Initialize Test Report - Check if test already exists
            var testName = TestContext.TestName;
            TestReport = _extentReports.CreateTest(testName);

            // Clear previous metrics for this test if it exists
            if (_testMetrics.ContainsKey(testName))
            {
                _testMetrics[testName].Clear();
            }

            LogInfo($"Test initialized: {testName}");
        }
    }

    [TestCleanup]
    public virtual async Task BaseTestCleanup()
    {
        var testFailed = TestContext.CurrentTestOutcome != UnitTestOutcome.Passed;
        var duration = DateTime.Now - _testStartTime;

        // Record test metrics only if not already recorded
        var testName = TestContext.TestName;
        if (!_testMetrics.ContainsKey(testName) || !_testMetrics[testName].Any())
        {
            var metric = new TestExecutionMetric
            {
                TestName = testName,
                Duration = duration,
                Passed = !testFailed,
                FailureStep = testFailed ? TestReport?.Status.ToString() : "",
                Category = GetTestCategory()
            };

            if (!_testMetrics.ContainsKey(testName))
            {
                _testMetrics[testName] = new List<TestExecutionMetric>();
            }
            _testMetrics[testName].Add(metric);

            LogInfo($"Test completed with status: {(testFailed ? "Failed" : "Passed")}");

            // Update test status in ExtentReports
            if (testFailed)
            {
                TestReport?.Fail("Test failed");
                if (TestContext.CurrentTestOutcome == UnitTestOutcome.Failed)
                {
                    var exception = TestContext.Properties["$Exception"] as Exception;
                    if (exception != null)
                    {
                        TestReport?.Log(AventStack.ExtentReports.Status.Error,
                            $"Test failed with error: {exception.Message}\n" +
                            $"Stack trace: {exception.StackTrace}");
                    }
                }
            }
            else
            {
                TestReport?.Pass("Test passed");
            }
        }

        if (Context != null)
        {
            await Context.DisposeAsync();
            Context = null!;
        }
        if (Browser != null)
        {
            await Browser.DisposeAsync();
            Browser = null!;
        }
        if (_playwright != null)
        {
            _playwright.Dispose();
            _playwright = null!;
        }
    }

    protected void LogInfo(string message)
    {
        Logger.LogInformation(message);
        var formattedMessage = $"<div class='log-message'>{message}</div>";
        TestReport?.Log(AventStack.ExtentReports.Status.Info, formattedMessage);
    }

    protected void LogWarning(string message)
    {
        Logger.LogWarning(message);
        var formattedMessage = $"<div class='log-message warning'>⚠️ {message}</div>";
        TestReport?.Log(AventStack.ExtentReports.Status.Warning, formattedMessage);
    }

    protected void LogError(string message, Exception? ex = null)
    {
        Logger.LogError(ex, message);
        var formattedMessage = new System.Text.StringBuilder();
        formattedMessage.AppendLine($"<div class='log-message error'>❌ {message}");

        if (ex != null)
        {
            formattedMessage.AppendLine("<div class='error-details'>");
            formattedMessage.AppendLine($"<div class='error-message'><b>Error:</b> {System.Web.HttpUtility.HtmlEncode(ex.Message)}</div>");
            formattedMessage.AppendLine("<div class='stack-trace'>");
            formattedMessage.AppendLine("<b>Stack Trace:</b>");
            formattedMessage.AppendLine($"<pre>{System.Web.HttpUtility.HtmlEncode(ex.StackTrace)}</pre>");
            formattedMessage.AppendLine("</div>");
            formattedMessage.AppendLine("</div>");
        }

        formattedMessage.AppendLine("</div>");
        TestReport?.Log(AventStack.ExtentReports.Status.Error, formattedMessage.ToString());
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

        TestReport?.Log(AventStack.ExtentReports.Status.Info, html);
    }

    private string CreateVideoAttachmentHtml(string fileName)
    {
        return $@"<div class='card' style='margin: 10px 0;'>
            <div class='card-header'>
                <h5 class='card-title'>Test Execution Video</h5>
            </div>
            <div class='card-body'>
                <a href='Videos/{fileName}' class='btn btn-primary mb-3' download>
                    <i class='fa fa-download'></i> Download Video
                </a>
                <div style='position: relative; padding-bottom: 56.25%; height: 0; overflow: hidden;'>
                    <video style='position: absolute; top: 0; left: 0; width: 100%; height: 100%;' controls>
                        <source src='Videos/{fileName}' type='video/webm'>
                        Your browser does not support the video tag.
                    </video>
                </div>
            </div>
        </div>";
    }

    private string CreateTraceAttachmentHtml(string fileName)
    {
        return $@"<div class='card' style='margin: 10px 0;'>
            <div class='card-header'>
                <h5 class='card-title'>Test Execution Trace</h5>
            </div>
            <div class='card-body'>
                <a href='Traces/{fileName}' class='btn btn-success mb-3' download>
                    <i class='fa fa-file-archive-o'></i> Download Trace
                </a>
                <div class='alert alert-info'>
                    <i class='fa fa-info-circle'></i> The trace file contains detailed information about the test execution, including network requests, console logs, and screenshots.
                </div>
            </div>
        </div>";
    }

    private string CreateGenericAttachmentHtml(string fileName, string description)
    {
        return $@"<div class='card' style='margin: 10px 0;'>
            <div class='card-header'>
                <h5 class='card-title'>{description}</h5>
            </div>
            <div class='card-body'>
                <a href='{fileName}' class='btn btn-secondary' download>
                    <i class='fa fa-download'></i> Download File
                </a>
            </div>
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