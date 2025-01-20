namespace PlaywrightDemo.Infrastructure.Config.Models;

public class TestSettings
{
    public EnvironmentSettings Environment { get; set; } = new();
    public BrowserSettings Browser { get; set; } = new();
    public TimeoutSettings Timeouts { get; set; } = new();
    public TestDataSettings TestData { get; set; } = new();
    public ReportingSettings Reporting { get; set; } = new();
    public AuthSettings Auth { get; set; } = new();
}

public class TestDataSettings
{
    public string Locale { get; set; } = "en";
    public string DataDirectory { get; set; } = "TestData";
}

public class TimeoutSettings
{
    public int DefaultTimeout { get; set; } = 30000; // 30 seconds
    public int PageLoadTimeout { get; set; } = 30000; // 30 seconds
    public int NavigationTimeout { get; set; } = 30000; // 30 seconds
    public int FailureHoldTime { get; set; } = 5000; // 5 seconds to keep browser open after failure
}