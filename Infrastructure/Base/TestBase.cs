using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using AventStack.ExtentReports;
using ParkPlaceSample.Infrastructure.Reporting;
using ParkPlaceSample.Infrastructure.Config;
using ParkPlaceSample.Infrastructure.Tracing;

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
        if (_playwright == null)
        {
            _playwright = await Playwright.CreateAsync();
            Browser = await _playwright.Chromium.LaunchAsync(new()
            {
                Headless = Settings.Browser.Headless,
                SlowMo = Settings.Browser.SlowMo
            });

            // Initialize Browser Context with video recording
            var projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
            var videosPath = Path.Combine(projectRoot, "TestResults", "Reports", "Videos");
            Directory.CreateDirectory(videosPath);

            Context = await Browser.NewContextAsync(new()
            {
                ViewportSize = new ViewportSize
                {
                    Width = Settings.Browser.Viewport.Width,
                    Height = Settings.Browser.Viewport.Height
                },
                RecordVideoDir = videosPath,
                RecordVideoSize = new RecordVideoSize
                {
                    Width = Settings.Browser.Viewport.Width,
                    Height = Settings.Browser.Viewport.Height
                }
            });

            // Initialize trace manager
            var traceSettings = new Infrastructure.Tracing.TraceSettings
            {
                Enabled = Settings.Trace.Enabled,
                Directory = Settings.Trace.Directory,
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
                        File.Move(videoPath, destinationPath);
                        AddTestAttachment(destinationPath, "Test Execution Video");
                    }
                }
            }

            // Handle trace recording
            if (_traceManager != null)
            {
                var tracePath = await _traceManager.StopTracingAsync(testFailed);
                if (!string.IsNullOrEmpty(tracePath))
                {
                    AddTestAttachment(tracePath, "Test Execution Trace");
                }
            }

            // Update test status in ExtentReports
            if (testFailed)
            {
                TestReport?.Fail("Test failed");
                if (TestContext.CurrentTestOutcome == UnitTestOutcome.Failed)
                {
                    var exception = TestContext.Properties["$Exception"] as Exception;
                    if (exception != null)
                    {
                        TestReport?.Log(Status.Error,
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
        catch (Exception ex)
        {
            LogError("Error during test cleanup", ex);
        }
        finally
        {
            // Cleanup resources
            if (Page != null)
            {
                await Page.CloseAsync();
                Page = null!;
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
    }

    protected void LogInfo(string message)
    {
        Logger.LogInformation(message);
        var formattedMessage = $"<div class='log-message'>{message}</div>";
        TestReport?.Log(Status.Info, formattedMessage);
    }

    protected void LogWarning(string message)
    {
        Logger.LogWarning(message);
        var formattedMessage = $"<div class='log-message warning'>⚠️ {message}</div>";
        TestReport?.Log(Status.Warning, formattedMessage);
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
        TestReport?.Log(Status.Error, formattedMessage.ToString());
    }

    protected void AddTestAttachment(string filePath, string description)
    {
        if (!File.Exists(filePath))
        {
            LogWarning($"Attachment file not found: {filePath}");
            return;
        }

        TestContext.AddResultFile(filePath);
        var html = AttachmentHelper.CreateAttachmentHtml(filePath, description);
        TestReport?.Log(Status.Info, html);
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