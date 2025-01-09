using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using AventStack.ExtentReports;
using ParkPlaceSample.Infrastructure.Reporting;
using ParkPlaceSample.Infrastructure.Config;
using ParkPlaceSample.Infrastructure.Config.Models;
using ParkPlaceSample.Infrastructure.Tracing;
using ParkPlaceSample.Infrastructure.API;
using System.Text;
using System.Web;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Binder;

namespace ParkPlaceSample.Infrastructure.Base;

[TestFixture]
public class TestBase
{
    protected IBrowserContext Context { get; private set; } = null!;
    protected IBrowser Browser { get; private set; } = null!;
    protected ILogger Logger { get; private set; } = null!;
    protected IPage Page { get; private set; } = null!;
    protected IAPIRequestContext ApiContext { get; private set; } = null!;
    protected ApiTestHelper ApiHelper { get; private set; } = null!;
    private IPlaywright _playwright = null!;
    protected ExtentTest TestReport { get; private set; } = null!;
    private DateTime _testStartTime;
    private TraceManager _traceManager = null!;
    protected TestSettings Settings => ConfigurationLoader.GetSettings<TestSettings>();

    [OneTimeSetUp]
    public static async Task AssemblyInitialize()
    {
        await TestReportManager.InitializeReporting();
    }

    [OneTimeTearDown]
    public static void AssemblyCleanup()
    {
        TestReportManager.FinalizeReporting();
    }

    private string GetProjectRoot()
    {
        return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
    }

    private string GetTestResultsPath()
    {
        return Path.Combine(GetProjectRoot(), "TestResults");
    }

    private string GetReportsPath()
    {
        return Path.Combine(GetTestResultsPath(), "Reports");
    }

    private string GetVideoPath(string fileName)
    {
        return Path.Combine(GetReportsPath(), "Videos", fileName);
    }

    private string GetTempVideoPath()
    {
        return Path.Combine(GetReportsPath(), "Videos", "temp");
    }

    [SetUp]
    public virtual async Task BaseTestInitialize()
    {
        var testName = TestContext.CurrentContext.Test.Name;
        _testStartTime = DateTime.Now;

        // Initialize logger
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });
        Logger = loggerFactory.CreateLogger<TestBase>();
        ConfigurationLoader.Initialize(Logger);

        // Initialize test reporting
        TestReport = TestReportManager.CreateTest(testName);
        TestMetricsManager.InitializeTest(testName);

        // Initialize Playwright
        _playwright = await Playwright.CreateAsync();

        // Configure browser options
        var launchOptions = new BrowserTypeLaunchOptions
        {
            Headless = Settings.Browser.Headless,
            SlowMo = Settings.Browser.SlowMo
        };

        // Launch browser
        Browser = await _playwright[Settings.Browser.Type].LaunchAsync(launchOptions);

        // Create context with tracing enabled
        var contextOptions = new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize
            {
                Width = Settings.Browser.Viewport.Width,
                Height = Settings.Browser.Viewport.Height
            }
        };

        // Configure video recording if enabled
        if (Settings.Reporting.Video.Enabled)
        {
            var tempVideoDir = GetTempVideoPath();
            Directory.CreateDirectory(tempVideoDir);

            contextOptions.RecordVideoDir = tempVideoDir;
            contextOptions.RecordVideoSize = new RecordVideoSize
            {
                Width = Settings.Reporting.Video.Size.Width,
                Height = Settings.Reporting.Video.Size.Height
            };
        }

        Context = await Browser.NewContextAsync(contextOptions);

        // Initialize trace manager
        _traceManager = new TraceManager(
            Context,
            Logger,
            Settings.Reporting.Trace,
            Settings.Reporting.BaseDirectory,
            TestContext.CurrentContext
        );
        await _traceManager.StartTracingAsync();

        // Create new page
        Page = await Context.NewPageAsync();

        // Initialize API context if needed
        if (!string.IsNullOrEmpty(Settings.Environment.ApiBaseUrl))
        {
            ApiContext = await _playwright.APIRequest.NewContextAsync(new()
            {
                BaseURL = Settings.Environment.ApiBaseUrl,
                IgnoreHTTPSErrors = true
            });

            ApiHelper = new ApiTestHelper(Logger, Settings, ApiContext);
        }
    }

    [TearDown]
    public virtual async Task BaseTestCleanup()
    {
        var testFailed = TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Failed;
        var testName = TestContext.CurrentContext.Test.Name;

        try
        {
            // Clean up API resources first
            if (ApiHelper != null)
            {
                try
                {
                    LogInfo("Cleaning up API resources...");
                    await ApiHelper.CleanupResourcesAsync();
                }
                catch (Exception ex)
                {
                    LogError($"Error during API resource cleanup: {ex.Message}", ex);
                }
            }

            // Record test result
            TestMetricsManager.RecordTestResult(
                TestContext.CurrentContext.Test.Name,
                DateTime.Now - _testStartTime,
                !testFailed,
                testFailed ? TestContext.CurrentContext.Result.Message : "",
                TestContext.CurrentContext.Test.Properties["Category"]?.Cast<string>().FirstOrDefault() ?? ""
            );

            if (Page != null)
            {
                try
                {
                    // Save trace
                    var tracePath = await _traceManager.StopTracingAsync(testFailed);
                    if (!string.IsNullOrEmpty(tracePath))
                    {
                        TestReportManager.AddTestTrace(testName, tracePath);
                    }

                    // Save video
                    if (Context != null && Settings.Reporting.Video.Enabled)
                    {
                        try
                        {
                            var video = Page.Video;
                            if (video != null)
                            {
                                var videoPath = await video.PathAsync();
                                if (!string.IsNullOrEmpty(videoPath))
                                {
                                    // Wait for video to be saved
                                    await Page.CloseAsync();
                                    await Context.CloseAsync();

                                    // Create final video path
                                    var videoFileName = $"{testName}_{DateTime.Now:yyyyMMdd_HHmmss}.webm";
                                    var destinationPath = GetVideoPath(videoFileName);
                                    Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);

                                    // Ensure source video exists and copy it
                                    if (File.Exists(videoPath))
                                    {
                                        try
                                        {
                                            // First try to move the file
                                            File.Move(videoPath, destinationPath, true);
                                        }
                                        catch
                                        {
                                            // If move fails, try to copy and then delete
                                            File.Copy(videoPath, destinationPath, true);
                                            try
                                            {
                                                File.Delete(videoPath);
                                            }
                                            catch (Exception ex)
                                            {
                                                LogWarning($"Could not delete temporary video file: {ex.Message}");
                                            }
                                        }

                                        TestReportManager.AddTestVideo(testName, destinationPath);
                                    }
                                    else
                                    {
                                        LogWarning($"Video file not found at path: {videoPath}");
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            LogError($"Error handling test video: {ex.Message}", ex);
                        }
                    }

                    // Take screenshot on failure
                    if (testFailed)
                    {
                        var screenshotFileName = $"{testName}_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                        var screenshotPath = Path.Combine(GetReportsPath(), "Screenshots", screenshotFileName);
                        Directory.CreateDirectory(Path.GetDirectoryName(screenshotPath)!);
                        await Page.ScreenshotAsync(new() { Path = screenshotPath, FullPage = true });
                        TestReportManager.AddTestScreenshot(testName, screenshotPath, "Failure Screenshot");

                        // Add error details to report
                        var errorMessage = TestContext.CurrentContext.Result.Message;
                        var stackTrace = TestContext.CurrentContext.Result.StackTrace;
                        if (!string.IsNullOrEmpty(errorMessage))
                        {
                            TestReport?.Error($"Test Failed: {errorMessage}");
                            if (!string.IsNullOrEmpty(stackTrace))
                            {
                                TestReport?.Error($"Stack Trace: {stackTrace}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogError($"Error during artifact collection: {ex.Message}", ex);
                }
            }

            // Update final test status in report
            if (testFailed)
            {
                TestReport?.Fail($"Test failed: {TestContext.CurrentContext.Result.Message}");
            }
            else
            {
                TestReport?.Pass("Test passed successfully");
            }

            LogInfo($"Test completed with status: {(testFailed ? "Failed" : "Passed")}");
        }
        catch (Exception ex)
        {
            LogError($"Error during test cleanup: {ex.Message}", ex);
            throw;
        }
        finally
        {
            // Dispose of resources in reverse order of creation
            if (ApiContext != null)
            {
                await ApiContext.DisposeAsync();
                ApiContext = null;
            }
            if (Context != null)
            {
                await Context.CloseAsync();
                Context = null;
            }
            if (Browser != null)
            {
                await Browser.CloseAsync();
                Browser = null;
            }
            if (_playwright != null)
            {
                _playwright.Dispose();
                _playwright = null;
            }
        }
    }

    protected void LogInfo(string message)
    {
        Logger.LogInformation(message);
        TestContext.WriteLine($"[INFO] {message}");
        TestReport?.Info(message);
    }

    protected void LogWarning(string message)
    {
        Logger.LogWarning(message);
        TestContext.WriteLine($"[WARNING] {message}");
        TestReport?.Warning(message);
    }

    protected void LogError(string message, Exception? ex = null)
    {
        Logger.LogError(ex, message);
        TestContext.WriteLine($"[ERROR] {message}");
        TestReport?.Error(message);
        if (ex != null)
        {
            TestContext.WriteLine($"[ERROR] Exception: {ex.Message}");
            TestContext.WriteLine($"[ERROR] Stack Trace: {ex.StackTrace}");
            TestReport?.Error(ex.ToString());
        }
    }
}