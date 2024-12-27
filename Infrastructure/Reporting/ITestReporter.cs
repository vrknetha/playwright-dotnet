using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ParkPlaceSample.Infrastructure.Reporting;

/// <summary>
/// Interface for test reporters
/// </summary>
public interface ITestReporter : IDisposable
{
    /// <summary>
    /// Initializes the reporter
    /// </summary>
    Task InitializeAsync();

    /// <summary>
    /// Starts a new test
    /// </summary>
    /// <param name="testContext">The test context</param>
    void StartTest(TestContext testContext);

    /// <summary>
    /// Logs an informational message
    /// </summary>
    /// <param name="message">The message to log</param>
    void LogInfo(string message);

    /// <summary>
    /// Logs a warning message
    /// </summary>
    /// <param name="message">The message to log</param>
    void LogWarning(string message);

    /// <summary>
    /// Logs an error message
    /// </summary>
    /// <param name="message">The message to log</param>
    /// <param name="exception">Optional exception details</param>
    void LogError(string message, Exception? exception = null);

    /// <summary>
    /// Adds a file attachment to the test report
    /// </summary>
    /// <param name="filePath">The path to the file</param>
    /// <param name="description">A description of the file</param>
    void AddFileAttachment(string filePath, string description);

    /// <summary>
    /// Ends the current test
    /// </summary>
    /// <param name="testContext">The test context</param>
    void EndTest(TestContext testContext);

    /// <summary>
    /// Finalizes the reporter
    /// </summary>
    Task FinalizeAsync();
}