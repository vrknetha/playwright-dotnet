using Microsoft.Extensions.Logging;
using AventStack.ExtentReports;
using AventStack.ExtentReports.MarkupUtils;
using System.Text;

namespace PlaywrightDemo.Infrastructure.Logging;

public class TestLogger
{
    private readonly ILogger _logger;
    private readonly ExtentTest _test;
    private readonly List<LogEntry> _logs = new();

    public TestLogger(ILogger logger, ExtentTest test)
    {
        _logger = logger;
        _test = test;
    }

    public void LogStep(string step, LogStatus status, string? details = null, string? screenshot = null)
    {
        var logEntry = new LogEntry
        {
            Timestamp = DateTime.Now,
            Step = step,
            Status = status,
            Details = details
        };

        string statusEmoji = status switch
        {
            LogStatus.Pass => "✅",
            LogStatus.Fail => "❌",
            LogStatus.Warning => "⚠️",
            LogStatus.Info => "ℹ️",
            _ => "📝"
        };

        string logMessage = $"{statusEmoji} {step}";
        if (details != null)
            logMessage += $"\n   Details: {details}";

        // Log to console/file
        switch (status)
        {
            case LogStatus.Pass:
                _logger.LogInformation(logMessage);
                _test.Pass(CreateMarkup(logEntry, screenshot));
                break;
            case LogStatus.Fail:
                _logger.LogError(logMessage);
                _test.Fail(CreateMarkup(logEntry, screenshot));
                break;
            case LogStatus.Warning:
                _logger.LogWarning(logMessage);
                _test.Warning(CreateMarkup(logEntry, screenshot));
                break;
            default:
                _logger.LogInformation(logMessage);
                _test.Info(CreateMarkup(logEntry, screenshot));
                break;
        }

        _logs.Add(logEntry);
    }

    private IMarkup CreateMarkup(LogEntry entry, string? screenshot)
    {
        var data = new string[,] {
            { "Time", entry.Timestamp.ToString("HH:mm:ss.fff") },
            { "Step", entry.Step },
            { "Status", entry.Status.ToString() },
            { "Details", entry.Details ?? "-" }
        };

        var table = MarkupHelper.CreateTable(data);

        if (screenshot != null)
        {
            _test.AddScreenCaptureFromPath(screenshot);
        }

        return table;
    }

    public void LogTestSummary()
    {
        var summary = new StringBuilder();
        summary.AppendLine("<h3>Test Execution Summary</h3>");
        summary.AppendLine("<ul>");

        foreach (var log in _logs)
        {
            string color = log.Status switch
            {
                LogStatus.Pass => "green",
                LogStatus.Fail => "red",
                LogStatus.Warning => "orange",
                _ => "black"
            };

            summary.AppendLine($"<li style='color: {color}'>{log.Timestamp:HH:mm:ss.fff} - {log.Step}</li>");
        }

        summary.AppendLine("</ul>");
        _test.Info(summary.ToString());
    }
}

public class LogEntry
{
    public DateTime Timestamp { get; set; }
    public string Step { get; set; } = string.Empty;
    public LogStatus Status { get; set; }
    public string? Details { get; set; }
}

public enum LogStatus
{
    Pass,
    Fail,
    Warning,
    Info
}