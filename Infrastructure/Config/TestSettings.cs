namespace ParkPlaceSample.Infrastructure.Config;

public class TestSettings
{
    public EnvironmentSettings Environment { get; set; } = new();
    public TestDataSettings TestData { get; set; } = new();
    public BrowserSettings Browser { get; set; } = new();
    public TimeoutSettings Timeouts { get; set; } = new();
    public TraceSettings Trace { get; set; } = new();
    public ReportingSettings Reporting { get; set; } = new();
}

public class EnvironmentSettings
{
    public string Name { get; set; } = "Development";
    public string BaseUrl { get; set; } = "http://localhost:5000";
    public string ApiBaseUrl { get; set; } = "http://localhost:5000/api";
    public Dictionary<string, string> Variables { get; set; } = new();
}

public class TestDataSettings
{
    public string Locale { get; set; } = "en";
    public string DataDirectory { get; set; } = "TestData";
}

public class BrowserSettings
{
    public string Type { get; set; } = "chromium";
    public bool Headless { get; set; } = true;
    public int SlowMo { get; set; } = 0;
    public int Timeout { get; set; } = 30000;
    public ViewportSettings Viewport { get; set; } = new();
    public List<string> LaunchArgs { get; set; } = new();
    public ScreenshotSettings Screenshots { get; set; } = new();
}

public class ViewportSettings
{
    public int Width { get; set; } = 1920;
    public int Height { get; set; } = 1080;
}

public class ScreenshotSettings
{
    public bool Enabled { get; set; } = true;
    public bool TakeOnFailure { get; set; } = true;
    public string Directory { get; set; } = "TestResults/Screenshots";
}

public class TimeoutSettings
{
    public int PageLoad { get; set; } = 30000;
    public int Navigation { get; set; } = 30000;
    public int Element { get; set; } = 10000;
    public int Script { get; set; } = 10000;
}

public class TraceSettings
{
    public bool Enabled { get; set; } = true;
    public string Directory { get; set; } = "TestResults/Traces";
    public string Mode { get; set; } = "OnFailure";
    public bool Screenshots { get; set; } = true;
    public bool Snapshots { get; set; } = true;
    public bool Sources { get; set; } = true;
}

public class ReportingSettings
{
    public ExtentReportSettings ExtentReports { get; set; } = new();
    public AzureDevOpsSettings AzureDevOps { get; set; } = new();
}

public class ExtentReportSettings
{
    public bool Enabled { get; set; } = true;
    public string OutputDirectory { get; set; } = "TestResults/ExtentReports";
    public string DocumentTitle { get; set; } = "Test Execution Report";
    public string ReportName { get; set; } = "UI Test Automation Report";
    public string Theme { get; set; } = "Standard";
    public string Environment { get; set; } = "Development";
    public string Browser { get; set; } = "Chromium";
    public bool AddScreenshots { get; set; } = true;
    public bool AddTestLogs { get; set; } = true;
    public bool AddSystemInfo { get; set; } = true;
}

public class AzureDevOpsSettings
{
    public bool Enabled { get; set; } = false;
    public string OrganizationUrl { get; set; } = "";
    public string ProjectName { get; set; } = "";
    public string PersonalAccessToken { get; set; } = "";
    public string TestPlanId { get; set; } = "";
    public string TestSuiteId { get; set; } = "";
}