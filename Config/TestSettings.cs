namespace ParkPlaceSample.Config;

/// <summary>
/// Represents the test configuration settings.
/// </summary>
public class TestSettings
{
    /// <summary>
    /// Gets or sets the environment configuration.
    /// </summary>
    public EnvironmentSettings Environment { get; set; } = new();

    /// <summary>
    /// Gets or sets the test data settings.
    /// </summary>
    public TestDataSettings TestData { get; set; } = new();

    /// <summary>
    /// Gets or sets the browser settings.
    /// </summary>
    public BrowserSettings Browser { get; set; } = new();

    /// <summary>
    /// Gets or sets the timeout settings.
    /// </summary>
    public TimeoutSettings Timeouts { get; set; } = new();

    /// <summary>
    /// Gets or sets the trace settings.
    /// </summary>
    public TraceSettings Trace { get; set; } = new();

    /// <summary>
    /// Gets or sets the reporting settings.
    /// </summary>
    public ReportingSettings Reporting { get; set; } = new();
}

/// <summary>
/// Represents environment-specific settings.
/// </summary>
public class EnvironmentSettings
{
    /// <summary>
    /// Gets or sets the environment name.
    /// </summary>
    public string Name { get; set; } = "Development";

    /// <summary>
    /// Gets or sets the base URL for the web application.
    /// </summary>
    public string BaseUrl { get; set; } = "http://localhost:5000";

    /// <summary>
    /// Gets or sets the base URL for the API.
    /// </summary>
    public string ApiBaseUrl { get; set; } = "http://localhost:5000/api";

    /// <summary>
    /// Gets or sets additional environment variables.
    /// </summary>
    public Dictionary<string, string> Variables { get; set; } = new();
}

/// <summary>
/// Represents test data configuration settings.
/// </summary>
public class TestDataSettings
{
    /// <summary>
    /// Gets or sets the locale for generating test data.
    /// </summary>
    public string Locale { get; set; } = "en";

    /// <summary>
    /// Gets or sets the test data directory path.
    /// </summary>
    public string DataDirectory { get; set; } = "TestData";
}

/// <summary>
/// Represents browser configuration settings.
/// </summary>
public class BrowserSettings
{
    /// <summary>
    /// Gets or sets the browser type (e.g., "chromium", "firefox", "webkit").
    /// </summary>
    public string Type { get; set; } = "chromium";

    /// <summary>
    /// Gets or sets whether to run in headless mode.
    /// </summary>
    public bool Headless { get; set; } = true;

    /// <summary>
    /// Gets or sets the browser window size.
    /// </summary>
    public string WindowSize { get; set; } = "1920x1080";

    /// <summary>
    /// Gets or sets the browser launch arguments.
    /// </summary>
    public List<string> LaunchArgs { get; set; } = new();

    /// <summary>
    /// Gets or sets whether to record traces.
    /// </summary>
    public bool RecordTrace { get; set; } = false;

    /// <summary>
    /// Gets or sets whether to record video.
    /// </summary>
    public bool RecordVideo { get; set; } = false;

    /// <summary>
    /// Gets or sets whether to record screenshots.
    /// </summary>
    public bool RecordScreenshots { get; set; } = true;

    /// <summary>
    /// Gets or sets the slow motion delay in milliseconds.
    /// </summary>
    public int SlowMo { get; set; } = 0;

    /// <summary>
    /// Gets or sets the browser timeout in milliseconds.
    /// </summary>
    public int Timeout { get; set; } = 30000;

    /// <summary>
    /// Gets or sets the viewport settings.
    /// </summary>
    public ViewportSettings Viewport { get; set; } = new();

    /// <summary>
    /// Gets or sets the screenshot settings.
    /// </summary>
    public ScreenshotSettings Screenshots { get; set; } = new();
}

/// <summary>
/// Represents viewport configuration settings.
/// </summary>
public class ViewportSettings
{
    /// <summary>
    /// Gets or sets the viewport width.
    /// </summary>
    public int Width { get; set; } = 1920;

    /// <summary>
    /// Gets or sets the viewport height.
    /// </summary>
    public int Height { get; set; } = 1080;
}

/// <summary>
/// Represents screenshot configuration settings.
/// </summary>
public class ScreenshotSettings
{
    /// <summary>
    /// Gets or sets whether screenshots are enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to take screenshots on test failure.
    /// </summary>
    public bool TakeOnFailure { get; set; } = true;

    /// <summary>
    /// Gets or sets the screenshot directory path.
    /// </summary>
    public string Directory { get; set; } = "TestResults/Screenshots";
}

/// <summary>
/// Represents timeout configuration settings.
/// </summary>
public class TimeoutSettings
{
    /// <summary>
    /// Gets or sets the page load timeout in milliseconds.
    /// </summary>
    public int PageLoad { get; set; } = 30000;

    /// <summary>
    /// Gets or sets the navigation timeout in milliseconds.
    /// </summary>
    public int Navigation { get; set; } = 30000;

    /// <summary>
    /// Gets or sets the element timeout in milliseconds.
    /// </summary>
    public int Element { get; set; } = 10000;

    /// <summary>
    /// Gets or sets the script timeout in milliseconds.
    /// </summary>
    public int Script { get; set; } = 10000;
}

/// <summary>
/// Represents trace configuration settings.
/// </summary>
public class TraceSettings
{
    /// <summary>
    /// Gets or sets whether tracing is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the trace directory path.
    /// </summary>
    public string Directory { get; set; } = "TestResults/Traces";

    /// <summary>
    /// Gets or sets the tracing mode.
    /// </summary>
    public TracingMode Mode { get; set; } = TracingMode.OnFailure;

    /// <summary>
    /// Gets or sets whether to capture screenshots in traces.
    /// </summary>
    public bool Screenshots { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to capture snapshots in traces.
    /// </summary>
    public bool Snapshots { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to capture sources in traces.
    /// </summary>
    public bool Sources { get; set; } = true;
}

/// <summary>
/// Represents reporting configuration settings.
/// </summary>
public class ReportingSettings
{
    /// <summary>
    /// Gets or sets the ExtentReports settings.
    /// </summary>
    public ExtentReportSettings ExtentReports { get; set; } = new();

    /// <summary>
    /// Gets or sets the Azure DevOps settings.
    /// </summary>
    public AzureDevOpsSettings AzureDevOps { get; set; } = new();
}

/// <summary>
/// Represents ExtentReports configuration settings.
/// </summary>
public class ExtentReportSettings
{
    /// <summary>
    /// Gets or sets whether ExtentReports is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the output directory path.
    /// </summary>
    public string OutputDirectory { get; set; } = "TestResults/ExtentReports";

    /// <summary>
    /// Gets or sets the document title.
    /// </summary>
    public string DocumentTitle { get; set; } = "Test Execution Report";

    /// <summary>
    /// Gets or sets the report name.
    /// </summary>
    public string ReportName { get; set; } = "UI Test Automation Report";

    /// <summary>
    /// Gets or sets the report theme.
    /// </summary>
    public string Theme { get; set; } = "Standard";

    /// <summary>
    /// Gets or sets the environment name.
    /// </summary>
    public string Environment { get; set; } = "Development";

    /// <summary>
    /// Gets or sets the browser name.
    /// </summary>
    public string Browser { get; set; } = "Chromium";

    /// <summary>
    /// Gets or sets whether to add screenshots to the report.
    /// </summary>
    public bool AddScreenshots { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to add test logs to the report.
    /// </summary>
    public bool AddTestLogs { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to add system info to the report.
    /// </summary>
    public bool AddSystemInfo { get; set; } = true;
}

/// <summary>
/// Represents Azure DevOps configuration settings.
/// </summary>
public class AzureDevOpsSettings
{
    /// <summary>
    /// Gets or sets whether Azure DevOps integration is enabled.
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// Gets or sets the Azure DevOps organization URL.
    /// </summary>
    public string OrganizationUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the project name.
    /// </summary>
    public string ProjectName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the personal access token.
    /// </summary>
    public string PersonalAccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the test plan ID.
    /// </summary>
    public string TestPlanId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the test suite ID.
    /// </summary>
    public string TestSuiteId { get; set; } = string.Empty;
}

/// <summary>
/// Represents tracing mode options.
/// </summary>
public enum TracingMode
{
    /// <summary>
    /// Always capture traces.
    /// </summary>
    Always,

    /// <summary>
    /// Capture traces only on test failure.
    /// </summary>
    OnFailure,

    /// <summary>
    /// Never capture traces.
    /// </summary>
    Never
}