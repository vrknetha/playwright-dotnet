using Microsoft.Extensions.Logging;

namespace ParkPlaceSample.Infrastructure.Logging;

public class TestContextLogger : ILogger
{
    private readonly string _categoryName;
    private readonly TestContext _testContext;
    private readonly LogLevel _minLevel;

    public TestContextLogger(string categoryName, TestContext testContext, LogLevel minLevel = LogLevel.Information)
    {
        _categoryName = categoryName;
        _testContext = testContext;
        _minLevel = minLevel;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => default;

    public bool IsEnabled(LogLevel logLevel) => logLevel >= _minLevel;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;

        var message = formatter(state, exception);
        var formattedMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{logLevel}] [{_categoryName}] {message}";

        _testContext.WriteLine(formattedMessage);
        Console.WriteLine(formattedMessage);

        if (exception != null)
        {
            var exceptionMessage = $"Exception: {exception.Message}\nStackTrace: {exception.StackTrace}";
            _testContext.WriteLine(exceptionMessage);
            Console.WriteLine(exceptionMessage);
        }
    }
}

public class TestContextLoggerProvider : ILoggerProvider
{
    private readonly TestContext _testContext;
    private readonly LogLevel _minLevel;

    public TestContextLoggerProvider(TestContext testContext, LogLevel minLevel = LogLevel.Information)
    {
        _testContext = testContext;
        _minLevel = minLevel;
    }

    public ILogger CreateLogger(string categoryName)
        => new TestContextLogger(categoryName, _testContext, _minLevel);

    public void Dispose() { }
}

public static class TestContextLoggerExtensions
{
    public static ILoggingBuilder AddTestContext(
        this ILoggingBuilder builder,
        TestContext testContext,
        LogLevel minLevel = LogLevel.Information)
    {
        builder.AddProvider(new TestContextLoggerProvider(testContext, minLevel));
        return builder;
    }
}