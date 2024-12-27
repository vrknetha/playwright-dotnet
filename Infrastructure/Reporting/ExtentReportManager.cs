using Microsoft.Extensions.Logging;

namespace ParkPlaceSample.Infrastructure.Reporting;

public class ExtentReportManager
{
    private readonly ILogger _logger;
    private readonly string _reportPath;

    public ExtentReportManager(ILogger logger, string reportPath)
    {
        _logger = logger;
        _reportPath = reportPath;
    }

    public void InitializeReport()
    {
        _logger.LogInformation("Initializing test report at: {ReportPath}", _reportPath);
    }

    public void StartTest(string testName, string description = "")
    {
        _logger.LogInformation("Starting test: {TestName}", testName);
    }

    public void EndTest(bool passed, string errorMessage = "")
    {
        if (passed)
        {
            _logger.LogInformation("Test passed");
        }
        else
        {
            _logger.LogError("Test failed: {ErrorMessage}", errorMessage);
        }
    }

    public void LogInfo(string message)
    {
        _logger.LogInformation(message);
    }

    public void LogError(string message)
    {
        _logger.LogError(message);
    }

    public void LogWarning(string message)
    {
        _logger.LogWarning(message);
    }

    public void AddScreenshot(string screenshotPath, string title = "")
    {
        _logger.LogInformation("Screenshot captured: {Title} at {Path}", title, screenshotPath);
    }

    public void FlushReport()
    {
        _logger.LogInformation("Flushing test report");
    }
}