using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ParkPlaceSample.Infrastructure.Config;

public static class ConfigurationLoader
{
    private static TestSettings? _settings;
    private static ILogger? _logger;

    public static TestSettings Settings => _settings ??= LoadSettings();

    public static void Initialize(ILogger logger)
    {
        _logger = logger;
        _settings = LoadSettings();
        LogConfigurationInfo();
    }

    public static TestSettings LoadSettings()
    {
        var environment = GetEnvironment();
        var projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
        var configPaths = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "appsettings.json"),
            Path.Combine(projectRoot, "appsettings.json")
        };

        var configPath = configPaths.FirstOrDefault(File.Exists)
            ?? throw new FileNotFoundException("Could not find appsettings.json in any of the expected locations.");

        _logger?.LogInformation("Using configuration file: {ConfigPath}", configPath);

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.GetDirectoryName(configPath)!)
            .AddJsonFile(Path.GetFileName(configPath), optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        var settings = new TestSettings();
        configuration.Bind(settings);

        // Ensure environment name is set
        settings.Environment.Name = environment;

        return settings;
    }

    private static string GetEnvironment()
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            ?? Environment.GetEnvironmentVariable("TEST_ENVIRONMENT")
            ?? "Development";

        _logger?.LogInformation("Using environment: {Environment}", environment);
        return environment;
    }

    private static void LogConfigurationInfo()
    {
        if (_logger == null || _settings == null) return;

        _logger.LogInformation("Configuration loaded successfully:");
        _logger.LogInformation("Environment: {Name}", _settings.Environment.Name);
        _logger.LogInformation("Base URL: {BaseUrl}", _settings.Environment.BaseUrl);
        _logger.LogInformation("Browser: {Type} (Headless: {Headless})",
            _settings.Browser.Type,
            _settings.Browser.Headless);
        _logger.LogInformation("Trace Enabled: {TraceEnabled} (Mode: {TraceMode})",
            _settings.Trace.Enabled,
            _settings.Trace.Mode);
    }
}