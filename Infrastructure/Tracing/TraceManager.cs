using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ParkPlaceSample.Infrastructure.Tracing;

public class TraceManager
{
    private readonly IBrowserContext _context;
    private readonly ILogger _logger;
    private readonly TraceSettings _settings;
    private readonly TestContext _testContext;

    public TraceManager(IBrowserContext context, ILogger logger, TraceSettings settings, TestContext testContext)
    {
        _context = context;
        _logger = logger;
        _settings = settings;
        _testContext = testContext;
    }

    public async Task StartTracingAsync()
    {
        if (!_settings.Enabled) return;

        _logger.LogInformation("Started tracing for test: {TestName}", _testContext.TestName);
        await _context.Tracing.StartAsync(new()
        {
            Screenshots = _settings.Screenshots,
            Snapshots = _settings.Snapshots,
            Sources = _settings.Sources
        });
    }

    public async Task StopTracingAsync(bool testFailed)
    {
        if (!_settings.Enabled) return;
        if (_settings.Mode == TracingMode.OnFailure && !testFailed) return;

        var traceFileName = $"{_testContext.TestName}_{DateTime.Now:yyyyMMdd_HHmmss}.zip";
        var tracePath = Path.Combine(_settings.Directory, traceFileName);

        Directory.CreateDirectory(_settings.Directory);
        await _context.Tracing.StopAsync(new() { Path = tracePath });

        _logger.LogInformation("Trace saved to: {TracePath}", tracePath);
        _testContext.AddResultFile(tracePath);
    }
}