using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;
using System.Runtime.InteropServices;
using Microsoft.Playwright;

namespace ParkPlaceSample.Infrastructure.Reporting;

public static class TestReportManager
{
    private static AventStack.ExtentReports.ExtentReports _extentReports = null!;
    private static string _reportsPath = null!;

    public static async Task InitializeReporting()
    {
        var projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
        var testResultsPath = Path.GetFullPath(Path.Combine(projectRoot, "TestResults"));
        _reportsPath = Path.GetFullPath(Path.Combine(testResultsPath, "Reports"));

        // Create reports directory and its subdirectories
        Directory.CreateDirectory(_reportsPath);
        Directory.CreateDirectory(Path.Combine(_reportsPath, "Videos"));
        Directory.CreateDirectory(Path.Combine(_reportsPath, "Traces"));
        Directory.CreateDirectory(Path.Combine(_reportsPath, "Logs"));

        var reportPath = Path.Combine(_reportsPath, "index.html");
        _extentReports = new AventStack.ExtentReports.ExtentReports();
        var htmlReporter = new ExtentHtmlReporter(reportPath);

        // Configure HTML reporter
        htmlReporter.Config.DocumentTitle = "Test Execution Report";
        htmlReporter.Config.ReportName = "Playwright Test Results";
        htmlReporter.Config.Theme = AventStack.ExtentReports.Reporter.Configuration.Theme.Standard;
        htmlReporter.Config.CSS = AttachmentHelper.GetReportStyles();
        htmlReporter.Config.EnableTimeline = true;

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

        _extentReports.AttachReporter(htmlReporter);
    }

    public static ExtentTest CreateTest(string testName)
    {
        return _extentReports.CreateTest(testName);
    }

    public static void FinalizeReporting()
    {
        var metricsReport = TestMetricsManager.GenerateMetricsReport();
        if (!string.IsNullOrEmpty(metricsReport))
        {
            _extentReports.AddTestRunnerLogs(metricsReport);
        }

        _extentReports.Flush();
    }
}