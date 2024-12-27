using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ParkPlaceSample.Infrastructure.Reporting;

/// <summary>
/// Service to manage test reporting
/// </summary>
public class TestReportingService : IDisposable
{
    private readonly List<ITestReporter> _reporters;
    private readonly ILogger<TestReportingService> _logger;
    private bool _isInitialized;

    public TestReportingService(ILogger<TestReportingService> logger, params ITestReporter[] reporters)
    {
        _reporters = new List<ITestReporter>(reporters);
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        if (_isInitialized) return;

        _logger.LogInformation("Initializing test reporters");
        foreach (var reporter in _reporters)
        {
            try
            {
                await reporter.InitializeAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize reporter: {ReporterType}", reporter.GetType().Name);
            }
        }

        _isInitialized = true;
    }

    public void StartTest(TestContext testContext)
    {
        foreach (var reporter in _reporters)
        {
            try
            {
                reporter.StartTest(testContext);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start test in reporter: {ReporterType}", reporter.GetType().Name);
            }
        }
    }

    public void LogInfo(string message)
    {
        foreach (var reporter in _reporters)
        {
            try
            {
                reporter.LogInfo(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log info in reporter: {ReporterType}", reporter.GetType().Name);
            }
        }
    }

    public void LogWarning(string message)
    {
        foreach (var reporter in _reporters)
        {
            try
            {
                reporter.LogWarning(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log warning in reporter: {ReporterType}", reporter.GetType().Name);
            }
        }
    }

    public void LogError(string message, Exception? exception = null)
    {
        foreach (var reporter in _reporters)
        {
            try
            {
                reporter.LogError(message, exception);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log error in reporter: {ReporterType}", reporter.GetType().Name);
            }
        }
    }

    public void AddFileAttachment(string filePath, string description)
    {
        foreach (var reporter in _reporters)
        {
            try
            {
                reporter.AddFileAttachment(filePath, description);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add file attachment in reporter: {ReporterType}", reporter.GetType().Name);
            }
        }
    }

    public void EndTest(TestContext testContext)
    {
        foreach (var reporter in _reporters)
        {
            try
            {
                reporter.EndTest(testContext);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to end test in reporter: {ReporterType}", reporter.GetType().Name);
            }
        }
    }

    public async Task FinalizeAsync()
    {
        foreach (var reporter in _reporters)
        {
            try
            {
                await reporter.FinalizeAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to finalize reporter: {ReporterType}", reporter.GetType().Name);
            }
        }
    }

    public void Dispose()
    {
        foreach (var reporter in _reporters)
        {
            try
            {
                reporter.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to dispose reporter: {ReporterType}", reporter.GetType().Name);
            }
        }

        GC.SuppressFinalize(this);
    }
}