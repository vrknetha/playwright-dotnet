using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using NUnit.Framework;
using PlaywrightDemo.Infrastructure.Config.Models;

namespace PlaywrightDemo.Infrastructure.Tracing;

public class TraceManager
{
    private readonly IBrowserContext _context;
    private readonly ILogger _logger;
    private readonly TraceSettings _settings;
    private readonly TestContext _testContext;
    private readonly string _baseDirectory;

    public TraceManager(IBrowserContext context, ILogger logger, TraceSettings settings, string baseDirectory, TestContext testContext)
    {
        _context = context;
        _logger = logger;
        _settings = settings;
        _baseDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", baseDirectory, "Reports"));
        _testContext = testContext;

        // Ensure trace directory exists
        if (_settings.Enabled)
        {
            var traceDir = Path.Combine(_baseDirectory, _settings.Directory);
            Directory.CreateDirectory(traceDir);
            _logger.LogInformation("Trace directory created at: {TraceDir}", traceDir);
        }
    }

    public async Task StartTracingAsync()
    {
        if (!_settings.Enabled) return;

        _logger.LogInformation("Started tracing for test: {TestName}", _testContext.Test.Name);
        await _context.Tracing.StartAsync(new()
        {
            Screenshots = _settings.Options.Screenshots,
            Snapshots = _settings.Options.Snapshots,
            Sources = _settings.Options.Sources
        });
    }

    public async Task<string> StopTracingAsync(bool testFailed)
    {
        if (!_settings.Enabled) return string.Empty;

        var shouldSaveTrace = _settings.Mode switch
        {
            TraceMode.All => true,
            TraceMode.RetainOnFailure => !testFailed,
            TraceMode.OnFailure => testFailed,
            _ => false
        };

        if (!shouldSaveTrace)
        {
            await _context.Tracing.StopAsync();
            return string.Empty;
        }

        var traceFileName = $"{_testContext.Test.Name}_{DateTime.Now:yyyyMMdd_HHmmss}.zip";
        var tracePath = Path.Combine(_baseDirectory, _settings.Directory, traceFileName);

        Directory.CreateDirectory(Path.GetDirectoryName(tracePath)!);
        await _context.Tracing.StopAsync(new() { Path = tracePath });

        _logger.LogInformation("Trace saved to: {TracePath}", tracePath);
        TestContext.AddTestAttachment(tracePath);

        return tracePath;
    }
}