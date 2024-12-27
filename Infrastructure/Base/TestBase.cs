using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using AventStack.ExtentReports;
using ParkPlaceSample.Infrastructure.Reporting;
using ParkPlaceSample.Infrastructure.Config;
using ParkPlaceSample.Infrastructure.Tracing;
using System.Text;
using System.Web;

namespace ParkPlaceSample.Infrastructure.Base;

[TestClass]
public class TestBase
{
    protected IBrowserContext Context { get; private set; } = null!;
    protected IBrowser Browser { get; private set; } = null!;
    protected ILogger Logger { get; private set; } = null!;
    protected IPage Page { get; private set; } = null!;
    private IPlaywright _playwright = null!;
    protected ExtentTest TestReport { get; private set; } = null!;
    private DateTime _testStartTime;
    private TraceManager _traceManager = null!;
    protected TestSettings Settings => ConfigurationLoader.Settings;

    public TestContext TestContext { get; set; } = null!;

    [AssemblyInitialize]
    public static async Task AssemblyInitialize(TestContext context)
    {
        await TestReportManager.InitializeReporting();
    }

    [AssemblyCleanup]
    public static void AssemblyCleanup()
    {
        TestReportManager.FinalizeReporting();
    }

    [TestInitialize]
    public virtual async Task BaseTestInitialize()
    {
        _testStartTime = DateTime.Now;

        // Initialize Logger
        if (Logger == null)
        {
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
            });
            Logger = loggerFactory.CreateLogger(GetType());
        }

        // Initialize Configuration
        ConfigurationLoader.Initialize(Logger);

        // Initialize Playwright
        _playwright = await Playwright.CreateAsync();
        Browser = await _playwright.Chromium.LaunchAsync(new()
        {
            Headless = Settings.Browser.Headless,
            SlowMo = Settings.Browser.SlowMo
        });

        // Create test results directory structure
        var projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
        var testResultsRoot = Path.Combine(projectRoot, "TestResults");
        var reportsDir = Path.Combine(testResultsRoot, "Reports");
        var videosDir = Path.Combine(reportsDir, "Videos");
        var logsDir = Path.Combine(reportsDir, "Logs");
        var tracesDir = Path.Combine(reportsDir, "Traces");

        Directory.CreateDirectory(videosDir);
        Directory.CreateDirectory(logsDir);
        Directory.CreateDirectory(tracesDir);

        // Initialize Browser Context with video recording
        Context = await Browser.NewContextAsync(new()
        {
            ViewportSize = new ViewportSize
            {
                Width = Settings.Browser.Viewport.Width,
                Height = Settings.Browser.Viewport.Height
            },
            RecordVideoDir = videosDir,
            RecordVideoSize = new RecordVideoSize
            {
                Width = Settings.Browser.Viewport.Width,
                Height = Settings.Browser.Viewport.Height
            }
        });

        // Initialize trace manager with updated trace directory
        var traceSettings = new Infrastructure.Tracing.TraceSettings
        {
            Enabled = Settings.Trace.Enabled,
            Directory = tracesDir,
            Mode = Enum.Parse<Infrastructure.Tracing.TracingMode>(Settings.Trace.Mode),
            Screenshots = Settings.Trace.Screenshots,
            Snapshots = Settings.Trace.Snapshots,
            Sources = Settings.Trace.Sources
        };
        _traceManager = new TraceManager(Context, Logger, traceSettings, TestContext);
        await _traceManager.StartTracingAsync();

        // Create a new page for the test
        Page = await Context.NewPageAsync();

        // Initialize Test Report
        var testName = TestContext.TestName;
        TestReport = TestReportManager.CreateTest(testName);
        TestMetricsManager.InitializeTest(testName);

        LogInfo($"Test initialized: {testName}");
    }

    [TestCleanup]
    public virtual async Task BaseTestCleanup()
    {
        var testFailed = TestContext.CurrentTestOutcome != UnitTestOutcome.Passed;
        var duration = DateTime.Now - _testStartTime;

        try
        {
            // Record test result
            TestMetricsManager.RecordTestResult(
                TestContext.TestName,
                duration,
                !testFailed,
                testFailed ? TestReport?.Status.ToString() : "",
                GetTestCategory()
            );

            LogInfo($"Test completed with status: {(testFailed ? "Failed" : "Passed")}");

            // Create test results directory structure
            var projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
            var testResultsRoot = Path.Combine(projectRoot, "TestResults");
            var reportsDir = Path.Combine(testResultsRoot, "Reports");
            var logsDir = Path.Combine(reportsDir, "Logs");
            Directory.CreateDirectory(logsDir);

            // Start HTML report
            TestReport?.Log(Status.Info, AttachmentHelper.GetReportStyles());
            TestReport?.Log(Status.Info, @"<div class='test-report'>");

            // Add test header
            TestReport?.Log(Status.Info, $@"
                <div class='test-header'>
                    <h1 class='test-title'>{TestContext.TestName}</h1>
                    <span class='test-status {(testFailed ? "status-failed" : "status-passed")}'>
                        {(testFailed ? "Failed" : "Passed")}
                    </span>
                    <div class='test-info'>
                        <div class='info-item'>
                            <span class='info-label'>Duration</span>
                            <span class='info-value'>{duration.TotalSeconds:F2} seconds</span>
                        </div>
                        <div class='info-item'>
                            <span class='info-label'>Category</span>
                            <span class='info-value'>{GetTestCategory()}</span>
                        </div>
                        <div class='info-item'>
                            <span class='info-label'>Start Time</span>
                            <span class='info-value'>{_testStartTime:yyyy-MM-dd HH:mm:ss}</span>
                        </div>
                    </div>
                </div>");

            // Add test artifacts section
            TestReport?.Log(Status.Info, @"<div class='artifacts-section'>
                <h2>Test Artifacts</h2>
                <div class='artifacts-grid'>");

            // Handle video recording
            if (Page != null)
            {
                var video = Page.Video;
                if (video != null)
                {
                    var videoPath = await video.PathAsync();
                    if (!string.IsNullOrEmpty(videoPath))
                    {
                        var fileName = $"{TestContext.TestName}_{DateTime.Now:yyyyMMdd_HHmmss}.webm";
                        var destinationPath = Path.Combine(Path.GetDirectoryName(videoPath)!, fileName);
                        await video.SaveAsAsync(destinationPath);

                        // Make path relative to the HTML report
                        var relativePath = Path.Combine("Videos", fileName).Replace('\\', '/');
                        TestReport?.Log(Status.Info, AttachmentHelper.CreateVideoAttachment(relativePath));
                    }
                }
            }

            // Handle trace recording
            if (_traceManager != null)
            {
                var tracePath = await _traceManager.StopTracingAsync(testFailed);
                if (!string.IsNullOrEmpty(tracePath))
                {
                    // Make path relative to the HTML report
                    var fileName = Path.GetFileName(tracePath);
                    var relativePath = Path.Combine("Traces", fileName).Replace('\\', '/');
                    TestReport?.Log(Status.Info, AttachmentHelper.CreateTraceAttachment(relativePath));
                }
            }

            // Save and attach test logs
            var testOutput = GetTestLogContent();
            var logFileName = $"{TestContext.TestName}_{DateTime.Now:yyyyMMdd_HHmmss}.log";
            var logPath = Path.Combine(logsDir, logFileName);
            await File.WriteAllTextAsync(logPath, testOutput);

            // Make path relative to the HTML report
            var relativeLogPath = Path.Combine("Logs", logFileName).Replace('\\', '/');
            TestReport?.Log(Status.Info, AttachmentHelper.CreateLogAttachment(relativeLogPath, testOutput));

            TestReport?.Log(Status.Info, "</div></div>"); // Close artifacts section

            // Add error details if test failed
            if (testFailed && TestContext.CurrentTestOutcome == UnitTestOutcome.Failed)
            {
                var exception = TestContext.Properties["$Exception"] as Exception;
                if (exception != null)
                {
                    TestReport?.Log(Status.Info, $@"
                        <div class='error-section'>
                            <h2 class='error-title'>Error Details</h2>
                            <div class='error-message'>
                                <b>Message:</b> {HttpUtility.HtmlEncode(exception.Message)}
                            </div>
                            <div class='stack-trace'>
                                <b>Stack Trace:</b>
                                <pre>{HttpUtility.HtmlEncode(exception.StackTrace)}</pre>
                            </div>
                        </div>");
                }
            }

            TestReport?.Log(Status.Info, "</div>"); // Close test-report div

            if (testFailed)
            {
                TestReport?.Fail("Test failed");
            }
            else
            {
                TestReport?.Pass("Test passed");
            }
        }
        catch (Exception ex)
        {
            LogError("Error during test cleanup", ex);
        }
        finally
        {
            try
            {
                // Cleanup resources in the correct order
                if (Page != null)
                {
                    await Page.CloseAsync();
                    Page = null!;
                }

                if (Context != null)
                {
                    await Context.CloseAsync();
                    await Context.DisposeAsync();
                    Context = null!;
                }

                if (Browser != null)
                {
                    await Browser.CloseAsync();
                    await Browser.DisposeAsync();
                    Browser = null!;
                }

                if (_playwright != null)
                {
                    _playwright.Dispose();
                    _playwright = null!;
                }
            }
            catch (Exception ex)
            {
                LogError("Error during resource cleanup", ex);
            }
        }
    }

    private string GetTestLogContent()
    {
        var output = new StringBuilder();
        output.AppendLine($"Test Name: {TestContext.TestName}");
        output.AppendLine($"Test Status: {TestContext.CurrentTestOutcome}");
        output.AppendLine($"Test Start Time: {_testStartTime:yyyy-MM-dd HH:mm:ss}");
        output.AppendLine($"Test Duration: {(DateTime.Now - _testStartTime).TotalSeconds:F2} seconds");
        output.AppendLine($"Test Class: {TestContext.FullyQualifiedTestClassName}");
        output.AppendLine($"Test Category: {GetTestCategory()}");
        output.AppendLine("\nTest Log Messages:");

        // Get all log messages from TestContext
        var testOutput = TestContext.CurrentTestOutcome == UnitTestOutcome.Failed
            ? TestContext.FullyQualifiedTestClassName + "." + TestContext.TestName + " Failed"
            : TestContext.FullyQualifiedTestClassName + "." + TestContext.TestName + " Passed";
        output.AppendLine(testOutput);

        if (TestContext.CurrentTestOutcome == UnitTestOutcome.Failed)
        {
            var exception = TestContext.Properties["$Exception"] as Exception;
            if (exception != null)
            {
                output.AppendLine("\nError Details:");
                output.AppendLine($"Message: {exception.Message}");
                output.AppendLine($"Stack Trace:\n{exception.StackTrace}");
            }
        }

        return output.ToString();
    }

    protected void LogInfo(string message)
    {
        Logger.LogInformation(message);
        TestContext.WriteLine($"[INFO] {message}");
        var formattedMessage = $"<div class='log-message info'>{message}</div>";
        TestReport?.Log(Status.Info, formattedMessage);
    }

    protected void LogWarning(string message)
    {
        Logger.LogWarning(message);
        TestContext.WriteLine($"[WARNING] {message}");
        var formattedMessage = $"<div class='log-message warning'>⚠️ {message}</div>";
        TestReport?.Log(Status.Warning, formattedMessage);
    }

    protected void LogError(string message, Exception? ex = null)
    {
        Logger.LogError(ex, message);
        TestContext.WriteLine($"[ERROR] {message}");
        if (ex != null)
        {
            TestContext.WriteLine($"[ERROR] Exception: {ex.Message}");
            TestContext.WriteLine($"[ERROR] Stack Trace: {ex.StackTrace}");
        }

        var formattedMessage = new StringBuilder();
        formattedMessage.AppendLine($"<div class='log-message error'>");
        formattedMessage.AppendLine($"<div class='error-icon'>❌</div>");
        formattedMessage.AppendLine($"<div class='error-content'>");
        formattedMessage.AppendLine($"<div class='error-message'>{message}</div>");

        if (ex != null)
        {
            formattedMessage.AppendLine("<div class='error-details'>");
            formattedMessage.AppendLine($"<div class='exception-message'><b>Error:</b> {System.Web.HttpUtility.HtmlEncode(ex.Message)}</div>");
            formattedMessage.AppendLine("<div class='stack-trace'>");
            formattedMessage.AppendLine("<b>Stack Trace:</b>");
            formattedMessage.AppendLine($"<pre>{System.Web.HttpUtility.HtmlEncode(ex.StackTrace)}</pre>");
            formattedMessage.AppendLine("</div>");
            formattedMessage.AppendLine("</div>");
        }

        formattedMessage.AppendLine("</div>");
        formattedMessage.AppendLine("</div>");
        TestReport?.Log(Status.Error, formattedMessage.ToString());
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