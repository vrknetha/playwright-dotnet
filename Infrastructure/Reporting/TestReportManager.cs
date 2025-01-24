using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;
using System.Runtime.InteropServices;
using Microsoft.Playwright;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace PlaywrightDemo.Infrastructure.Reporting;

public static class TestReportManager
{
    private static AventStack.ExtentReports.ExtentReports _extentReports = null!;
    private static string _reportsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestResults");
    private static readonly Dictionary<string, ExtentTest> _testCache = new();

    public static async Task InitializeReporting()
    {
        if (_extentReports != null) return;

        // Create reports directory
        Directory.CreateDirectory(_reportsPath);
        Directory.CreateDirectory(Path.Combine(_reportsPath, "Screenshots"));
        Directory.CreateDirectory(Path.Combine(_reportsPath, "Videos"));
        Directory.CreateDirectory(Path.Combine(_reportsPath, "Traces"));

        // Initialize ExtentReports
        var htmlReporter = new ExtentHtmlReporter(Path.Combine(_reportsPath, "index.html"));
        ConfigureHtmlReporter(htmlReporter);

        _extentReports = new ExtentReports();
        _extentReports.AttachReporter(htmlReporter);

        // Add environment info
        _extentReports.AddSystemInfo("Environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development");
        _extentReports.AddSystemInfo("Machine", Environment.MachineName);
        _extentReports.AddSystemInfo("OS", Environment.OSVersion.ToString());
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

        var relativeTracePath = Path.GetRelativePath(_reportsPath, tracePath);

        var test = _extentReports.CreateTest(testName);
        test.Info($@"
            <h3>Trace Recording</h3>
            <div>
                <a href='{relativeTracePath}' download class='btn btn-secondary'>
                    <i class='fa fa-download'></i> Download Trace
                </a>
            </div>
            <div class='trace-note'>
                <p><strong>Note:</strong> The trace file contains a detailed recording of the test execution.</p>
            </div>
        ");
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

    private static void ConfigureHtmlReporter(ExtentHtmlReporter reporter)
    {
        reporter.Config.DocumentTitle = "Test Execution Report";
        reporter.Config.ReportName = "Automated Test Results";
        reporter.Config.Theme = AventStack.ExtentReports.Reporter.Configuration.Theme.Standard;
    }

    public static void FinalizeReporting()
    {
        _extentReports?.Flush();
    }
}