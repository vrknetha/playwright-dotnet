using Microsoft.Extensions.Logging;

namespace PlaywrightDemo.Infrastructure.Logging;

/// <summary>
/// Manages the global logger instance for the test framework.
/// </summary>
public static class LoggerManager
{
    private static ILogger? _currentLogger;

    /// <summary>
    /// Gets the current logger instance.
    /// </summary>
    public static ILogger Current => _currentLogger ?? throw new InvalidOperationException("Logger not initialized");

    /// <summary>
    /// Sets the current logger instance.
    /// </summary>
    public static void SetLogger(ILogger logger)
    {
        _currentLogger = logger;
    }
}