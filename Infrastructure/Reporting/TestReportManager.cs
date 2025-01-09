using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;
using System.Runtime.InteropServices;
using Microsoft.Playwright;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace ParkPlaceSample.Infrastructure.Reporting;

public static class TestReportManager
{
    private static AventStack.ExtentReports.ExtentReports _extentReports = null!;
    private static string _reportsPath = null!;
    private static readonly Dictionary<string, ExtentTest> _testCache = new();

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
        Directory.CreateDirectory(Path.Combine(_reportsPath, "Screenshots"));

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
        _extentReports.AddSystemInfo("Test Framework", "NUnit");

        _extentReports.AttachReporter(htmlReporter);
    }

    public static ExtentTest CreateTest(string testName)
    {
        var test = _extentReports.CreateTest(testName);
        var currentTest = TestContext.CurrentContext.Test;

        // Add test categories
        var categories = currentTest.Properties["Category"]?.Cast<string>();
        if (categories != null)
        {
            foreach (var category in categories)
            {
                test.AssignCategory(category);
            }
        }

        // Add test description if available
        var description = currentTest.Properties["Description"]?.Cast<string>().FirstOrDefault();
        if (!string.IsNullOrEmpty(description))
        {
            test.Info(description);
        }

        // Cache the test for later use
        _testCache[testName] = test;

        return test;
    }

    public static ExtentTest GetTest(string testName)
    {
        return _testCache.TryGetValue(testName, out var test) ? test : CreateTest(testName);
    }

    public static void UpdateTestStatus(string testName)
    {
        var test = GetTest(testName);
        var result = TestContext.CurrentContext.Result;

        switch (result.Outcome.Status)
        {
            case TestStatus.Failed:
                test.Fail(result.Message);
                if (!string.IsNullOrEmpty(result.StackTrace))
                {
                    test.Info($"Stack Trace:<br><pre>{result.StackTrace}</pre>");
                }
                break;

            case TestStatus.Passed:
                test.Pass("Test passed successfully");
                break;

            case TestStatus.Skipped:
                test.Skip(result.Message ?? "Test was skipped");
                break;

            case TestStatus.Warning:
                test.Warning(result.Message ?? "Test completed with warnings");
                break;

            default:
                test.Info($"Test completed with status: {result.Outcome.Status}");
                break;
        }
    }

    public static void AddTestLog(string testName, Status status, string message)
    {
        var test = GetTest(testName);
        test.Log(status, message);
    }

    public static void AddTestScreenshot(string testName, string screenshotPath, string title = "Screenshot")
    {
        var test = GetTest(testName);
        var mediaEntity = MediaEntityBuilder.CreateScreenCaptureFromPath(screenshotPath).Build();
        test.Info(title, mediaEntity);
    }

    public static void AddTestTrace(string testName, string tracePath)
    {
        if (!File.Exists(tracePath)) return;

        var test = GetTest(testName);
        var relativeTracePath = Path.GetRelativePath(_reportsPath, tracePath);

        test.Info($@"<div class='artifact-item'>
            <h3>Trace Recording</h3>
            <p>To view the test execution trace:</p>
            <ol class='trace-instructions'>
                <li>Download the trace file using the link below</li>
                <li>Visit <a href='https://trace.playwright.dev' target='_blank'>trace.playwright.dev</a></li>
                <li>Upload the downloaded trace file to view the step-by-step test execution</li>
            </ol>
            <div class='artifact-actions'>
                <a href='{relativeTracePath}' download class='btn btn-primary'>
                    <i class='fa fa-download'></i> Download Trace
                </a>
            </div>
            <div class='trace-note'>
                <p><strong>Note:</strong> The trace viewer provides a detailed view of each step in the test, including:</p>
                <ul>
                    <li>Screenshots at each action</li>
                    <li>DOM snapshots</li>
                    <li>Network requests</li>
                    <li>Console logs</li>
                </ul>
            </div>
        </div>");
    }

    public static void AddTestVideo(string testName, string videoPath)
    {
        if (!File.Exists(videoPath)) return;

        var test = GetTest(testName);
        var relativeVideoPath = Path.GetRelativePath(_reportsPath, videoPath);
        var fileInfo = new FileInfo(videoPath);
        var fileSizeInMb = Math.Round(fileInfo.Length / (1024.0 * 1024.0), 2);

        // Add video player with metadata
        test.Info($@"<div class='artifact-item video-artifact'>
            <div class='artifact-header'>
                <h3>Test Recording</h3>
                <div class='video-metadata'>
                    <span class='metadata-item'>
                        <i class='fa fa-clock-o'></i> {DateTime.Now:HH:mm:ss}
                    </span>
                    <span class='metadata-item'>
                        <i class='fa fa-file-video-o'></i> {fileSizeInMb} MB
                    </span>
                </div>
            </div>
            <div class='video-container'>
                <video controls preload='metadata'>
                    <source src='{relativeVideoPath}' type='video/webm'>
                    Your browser does not support the video tag.
                </video>
            </div>
            <div class='artifact-actions'>
                <a href='{relativeVideoPath}' download class='btn btn-primary'>
                    <i class='fa fa-download'></i> Download Video
                </a>
                <button onclick='toggleFullscreen(this)' class='btn btn-secondary'>
                    <i class='fa fa-expand'></i> Toggle Fullscreen
                </button>
            </div>
        </div>");

        // Add video-specific CSS and JavaScript
        test.Info(@"<style>
            .video-artifact {
                background: #f8f9fa;
                border-radius: 8px;
                overflow: hidden;
                transition: all 0.3s ease;
            }

            .video-artifact.fullscreen {
                position: fixed;
                top: 0;
                left: 0;
                width: 100vw !important;
                height: 100vh !important;
                z-index: 9999;
                margin: 0;
                padding: 0;
                background: rgba(0, 0, 0, 0.9);
            }

            .video-artifact.fullscreen .video-container {
                height: calc(100vh - 120px);
                display: flex;
                align-items: center;
                justify-content: center;
            }

            .video-artifact.fullscreen video {
                max-width: 100%;
                max-height: 100%;
                margin: 0;
            }

            .artifact-header {
                display: flex;
                justify-content: space-between;
                align-items: center;
                padding: 16px;
                background: #fff;
                border-bottom: 1px solid #e9ecef;
            }

            .video-metadata {
                display: flex;
                gap: 16px;
                font-size: 14px;
                color: #6c757d;
            }

            .metadata-item {
                display: flex;
                align-items: center;
                gap: 6px;
            }

            .metadata-item i {
                font-size: 14px;
            }

            .video-container {
                position: relative;
                width: 100%;
                background: #000;
                overflow: hidden;
            }

            video {
                display: block;
                width: 100%;
                max-width: 1200px;
                margin: 0 auto;
                background: #000;
            }

            .artifact-actions {
                padding: 16px;
                background: #fff;
                border-top: 1px solid #e9ecef;
            }

            .btn {
                padding: 8px 16px;
                border-radius: 4px;
                font-size: 14px;
                font-weight: 500;
                cursor: pointer;
                display: inline-flex;
                align-items: center;
                gap: 8px;
                transition: all 0.2s;
            }

            .btn-primary {
                background: #0d6efd;
                color: #fff;
                border: none;
            }

            .btn-primary:hover {
                background: #0b5ed7;
            }

            .btn-secondary {
                background: #6c757d;
                color: #fff;
                border: none;
            }

            .btn-secondary:hover {
                background: #5c636a;
            }
        </style>
        <script>
            if (typeof toggleFullscreen === 'undefined') {
                function toggleFullscreen(button) {
                    var container = button.closest('.video-artifact');
                    if (container.classList.contains('fullscreen')) {
                        container.classList.remove('fullscreen');
                        button.innerHTML = '<i class=\'fa fa-expand\'></i> Toggle Fullscreen';
                    } else {
                        container.classList.add('fullscreen');
                        button.innerHTML = '<i class=\'fa fa-compress\'></i> Exit Fullscreen';
                    }
                }
            }
        </script>");
    }

    public static void ConfigureHtmlReporter(ExtentHtmlReporter htmlReporter)
    {
        // Add custom CSS for artifacts
        var existingCss = htmlReporter.Config.CSS ?? "";
        htmlReporter.Config.CSS = existingCss + @"
            .artifact-item {
                background: #f8f9fa;
                border-radius: 8px;
                padding: 16px;
                margin: 16px 0;
            }

            .artifact-item h3 {
                margin: 0 0 12px 0;
                font-size: 18px;
                color: #333;
            }

            .trace-instructions {
                margin: 16px 0;
                padding-left: 24px;
            }

            .trace-instructions li {
                margin: 8px 0;
                line-height: 1.5;
            }

            .trace-note {
                margin-top: 16px;
                padding: 12px;
                background: #e9ecef;
                border-radius: 4px;
            }

            .trace-note p {
                margin: 0 0 8px 0;
                font-size: 14px;
            }

            .trace-note ul {
                margin: 0;
                padding-left: 20px;
            }

            .trace-note li {
                margin: 4px 0;
                font-size: 14px;
                color: #666;
            }

            .artifact-actions {
                margin-top: 16px;
                display: flex;
                gap: 8px;
            }

            .btn {
                display: inline-flex;
                align-items: center;
                gap: 6px;
                padding: 8px 16px;
                border-radius: 4px;
                text-decoration: none;
                font-size: 14px;
                font-weight: 500;
                cursor: pointer;
                border: none;
                transition: background-color 0.2s;
            }

            .btn-primary {
                background: #0d6efd;
                color: white;
            }

            .btn-primary:hover {
                background: #0b5ed7;
            }

            .btn-secondary {
                background: #6c757d;
                color: white;
            }

            .btn-secondary:hover {
                background: #5c636a;
            }

            .btn i {
                font-size: 14px;
            }

            video {
                border-radius: 4px;
                background: #000;
                width: 100%;
                max-width: 800px;
                margin: 12px 0;
            }";
    }

    public static void FinalizeReporting()
    {
        var metricsReport = TestMetricsManager.GenerateMetricsReport();
        if (!string.IsNullOrEmpty(metricsReport))
        {
            _extentReports.AddTestRunnerLogs(metricsReport);
        }

        // Add summary information
        var results = TestContext.CurrentContext.Result;
        _extentReports.AddTestRunnerLogs($@"
            <div class='test-summary'>
                <h2>Test Run Summary</h2>
                <div class='summary-grid'>
                    <div class='summary-item'>
                        <span class='label'>Total Tests:</span>
                        <span class='value'>{_testCache.Count}</span>
                    </div>
                    <div class='summary-item'>
                        <span class='label'>Start Time:</span>
                        <span class='value'>{DateTime.Now:yyyy-MM-dd HH:mm:ss}</span>
                    </div>
                    <div class='summary-item'>
                        <span class='label'>Duration:</span>
                        <span class='value'>{TestContext.CurrentContext.TestDirectory}</span>
                    </div>
                </div>
            </div>");

        _extentReports.Flush();
        _testCache.Clear();
    }
}