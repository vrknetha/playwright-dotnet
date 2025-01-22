using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using AventStack.ExtentReports;
using PlaywrightDemo.Infrastructure.Reporting;
using PlaywrightDemo.Infrastructure.Config;
using PlaywrightDemo.Infrastructure.Config.Models;
using PlaywrightDemo.Infrastructure.Tracing;
using PlaywrightDemo.Infrastructure.API;
using PlaywrightDemo.Infrastructure.Logging;
using PlaywrightDemo.Infrastructure.Auth;
using System.Text;
using System.Web;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Binder;
using PlaywrightDemo.Infrastructure.TestData;
using PlaywrightDemo.Infrastructure.TestData.Models;

namespace PlaywrightDemo.Infrastructure.Base;

[TestFixture]
public class TestBase : IAsyncDisposable
{
    protected IBrowserContext Context { get; private set; } = null!;
    protected IBrowser Browser { get; private set; } = null!;
    protected ILogger Logger { get; }
    protected IPage Page { get; private set; } = null!;
    protected IAPIRequestContext ApiContext { get; private set; } = null!;
    private IPlaywright _playwright = null!;
    protected ExtentTest TestReport { get; private set; } = null!;
    private DateTime _testStartTime;
    private TraceManager _traceManager = null!;
    protected TestSettings Settings { get; }
    protected AuthHelper? AuthHelper { get; private set; }
    protected string? AuthStateToUse { get; set; }
    protected IUserData UserDataHelper { get; }
    public static TestLogger TestLogger { get; private set; } = null!;
    protected string? SessionStoragePath => Configuration["SessionStoragePath"];
    protected IConfiguration Configuration { get; }

    public TestBase()
    {
        // Initialize logger
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });
        Logger = loggerFactory.CreateLogger<TestBase>();
        LoggerManager.SetLogger(Logger);

        // Initialize settings
        ConfigurationLoader.Initialize(Logger);
        Settings = ConfigurationLoader.GetSettings<TestSettings>();

        // Initialize UserDataHelper
        UserDataHelper = new UserDataGenerator();

        // Initialize Configuration
        Configuration = ConfigurationLoader.LoadConfiguration();
    }

    public static string GetAuthStatePath()
    {
        var projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
        return Path.Combine(projectRoot, ".auth");
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

    [SetUp]
    public virtual async Task BaseTestInitialize()
    {
        if (!TestSharding.ShardingStrategy.ShouldRunTest())
        {
            Assert.Ignore("Test not assigned to this shard");
            return;
        }

        _testStartTime = DateTime.Now;
        TestMetricsManager.InitializeTest(TestContext.CurrentContext.Test.Name);

        var testName = TestContext.CurrentContext.Test.Name;

        // Initialize test reporting
        TestReport = TestReportManager.CreateTest(testName);
        TestLogger = new TestLogger(Logger, TestReport);

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

        // Initialize and verify auth state if specified
        if (!string.IsNullOrEmpty(AuthStateToUse))
        {
            try
            {
                AuthHelper = new AuthHelper();
                var authStatePath = Path.Combine(GetAuthStatePath(), AuthStateToUse);

                if (await AuthHelper.VerifyAuthStateAsync(authStatePath))
                {
                    LogInfo($"Using authentication state: {AuthStateToUse}");
                    contextOptions.StorageStatePath = authStatePath;
                }
                else
                {
                    LogWarning($"Authentication state is invalid or expired: {AuthStateToUse}");
                }
            }
            catch (Exception ex)
            {
                LogError($"Failed to apply authentication state: {AuthStateToUse}", ex);
            }
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
            var authStatePath = !string.IsNullOrEmpty(AuthStateToUse)
                ? Path.Combine(GetAuthStatePath(), AuthStateToUse)
                : null;
            ApiContext = await ApiContextManager.InitializeAsync(_playwright, Settings, authStatePath);
        }
    }

    [TearDown]
    public virtual async Task BaseTestCleanup()
    {
        var testFailed = TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Failed;
        var testName = TestContext.CurrentContext.Test.Name;

        try
        {
            // First: Handle all logs, test results, and error messages
            // Add error details to report if test failed
            if (testFailed)
            {
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

            // Clean up API context if needed
            if (!string.IsNullOrEmpty(Settings.Environment.ApiBaseUrl))
            {
                try
                {
                    LogInfo("Cleaning up API resources...");
                    await ApiContextManager.DisposeAsync();
                }
                catch (Exception ex)
                {
                    LogError($"Error during API resource cleanup: {ex.Message}", ex);
                }
            }

            // Record test result
            await TestMetricsManager.RecordTestResultAsync(
                TestContext.CurrentContext.Test.Name,
                DateTime.Now - _testStartTime,
                !testFailed,
                testFailed ? TestContext.CurrentContext.Result.Message ?? "Unknown failure" : "",
                TestContext.CurrentContext.Test.Properties["Category"]?.Cast<string>().FirstOrDefault() ?? ""
            );

            if (Page != null)
            {
                try
                {
                    // First: Handle trace recording (before closing context)
                    var tracePath = await _traceManager.StopTracingAsync(testFailed);
                    if (!string.IsNullOrEmpty(tracePath))
                    {
                        TestReportManager.AddTestTrace(testName, tracePath);
                    }

                    // Second: Handle video recording
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
                                            File.Move(videoPath, destinationPath, true);
                                        }
                                        catch
                                        {
                                            File.Copy(videoPath, destinationPath, true);
                                            try { File.Delete(videoPath); } catch { }
                                        }
                                        TestReportManager.AddTestVideo(testName, destinationPath);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            LogError($"Error handling test video: {ex.Message}", ex);
                        }
                    }

                    // Finally: Handle failure screenshots
                    if (testFailed)
                    {
                        var screenshotFileName = $"{testName}_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                        var screenshotPath = Path.Combine(GetReportsPath(), "Screenshots", screenshotFileName);
                        Directory.CreateDirectory(Path.GetDirectoryName(screenshotPath)!);
                        await Page.ScreenshotAsync(new() { Path = screenshotPath, FullPage = true });
                        TestReportManager.AddTestScreenshot(testName, screenshotPath, "Failure Screenshot");
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
            await ApiContextManager.DisposeAsync();
            if (Context != null)
            {
                await Context.CloseAsync();
                Context = null!;
            }
            if (Browser != null)
            {
                await Browser.CloseAsync();
                Browser = null!;
            }
            if (_playwright != null)
            {
                _playwright.Dispose();
                _playwright = null!;
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (AuthHelper != null)
        {
            await AuthHelper.DisposeAsync();
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