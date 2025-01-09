using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using ParkPlaceSample.Infrastructure.Config.Models;

namespace ParkPlaceSample.Infrastructure.Config;

public static class ConfigurationLoader
{
    private static IConfiguration? _configuration;
    private static ILogger? _logger;

    public static IConfiguration Configuration
    {
        get
        {
            if (_configuration == null)
            {
                LoadConfiguration();
            }
            return _configuration!;
        }
    }

    public static void Initialize(ILogger logger)
    {
        _logger = logger;
        LoadConfiguration();
        LogConfigurationInfo();
    }

    private static void LoadConfiguration()
    {
        try
        {
            var environment = GetEnvironment();
            var projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
            var configPaths = new[]
            {
                Path.Combine(projectRoot, "appsettings.json"),
                Path.Combine(AppContext.BaseDirectory, "appsettings.json")
            };

            _logger?.LogInformation("Searching for configuration file in:");
            foreach (var path in configPaths)
            {
                _logger?.LogInformation("  - {Path}", path);
                if (File.Exists(path))
                {
                    _logger?.LogInformation("Found configuration file at: {Path}", path);
                }
            }

            var configPath = configPaths.FirstOrDefault(File.Exists);
            if (configPath == null)
            {
                _logger?.LogError("Could not find appsettings.json in any of the expected locations.");
                throw new FileNotFoundException("Could not find appsettings.json in any of the expected locations.");
            }

            _logger?.LogInformation("Using configuration file: {ConfigPath}", configPath);
            var configContent = File.ReadAllText(configPath);
            _logger?.LogInformation("Configuration content: {Content}", configContent);

            _configuration = new ConfigurationBuilder()
                .SetBasePath(Path.GetDirectoryName(configPath)!)
                .AddJsonFile(Path.GetFileName(configPath), optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            ValidateConfiguration();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to load configuration");
            throw new InvalidOperationException("Failed to load configuration. See inner exception for details.", ex);
        }
    }

    private static void ValidateConfiguration()
    {
        var requiredSettings = new[]
        {
            "Environment:BaseUrl",
            "Environment:ApiBaseUrl",
            "Browser:Type",
            "Browser:Viewport:Width",
            "Browser:Viewport:Height"
        };

        var missingSettings = requiredSettings
            .Where(setting => string.IsNullOrEmpty(Configuration[setting]))
            .ToList();

        if (missingSettings.Any())
        {
            var message = $"Missing required configuration settings: {string.Join(", ", missingSettings)}";
            _logger?.LogError(message);
            throw new InvalidOperationException(message);
        }

        // Validate browser settings
        var browserType = Configuration["Browser:Type"]?.ToLower();
        if (browserType != "chromium" && browserType != "firefox" && browserType != "webkit")
        {
            var message = $"Invalid browser type: {browserType}. Must be one of: chromium, firefox, webkit";
            _logger?.LogError(message);
            throw new InvalidOperationException(message);
        }

        // Validate viewport settings
        if (!int.TryParse(Configuration["Browser:Viewport:Width"], out var width) || width <= 0)
        {
            throw new InvalidOperationException("Browser:Viewport:Width must be a positive integer");
        }

        if (!int.TryParse(Configuration["Browser:Viewport:Height"], out var height) || height <= 0)
        {
            throw new InvalidOperationException("Browser:Viewport:Height must be a positive integer");
        }

        // Validate reporting settings
        var settings = GetSettings<TestSettings>();
        if (settings.Reporting.Trace.Enabled)
        {
            if (string.IsNullOrEmpty(settings.Reporting.Trace.Directory))
            {
                throw new InvalidOperationException("Reporting:Trace:Directory must be specified when tracing is enabled");
            }

            if (!Enum.IsDefined(typeof(TraceMode), settings.Reporting.Trace.Mode))
            {
                throw new InvalidOperationException($"Invalid Reporting:Trace:Mode value: {settings.Reporting.Trace.Mode}");
            }
        }
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
        var settings = GetSettings<TestSettings>();
        _logger?.LogInformation("Environment: {Name} ({BaseUrl})",
            settings.Environment.Name,
            settings.Environment.BaseUrl);

        _logger?.LogInformation("Browser: {Type} (Headless: {Headless})",
            settings.Browser.Type,
            settings.Browser.Headless);

        _logger?.LogInformation("Trace Enabled: {TraceEnabled} (Mode: {TraceMode})",
            settings.Reporting.Trace.Enabled,
            settings.Reporting.Trace.Mode);
    }

    public static T GetSettings<T>() where T : new()
    {
        var settings = Configuration.Get<T>();
        if (settings == null)
        {
            _logger?.LogWarning("Could not bind configuration to type {Type}. Using default values.", typeof(T).Name);
            return new T();
        }
        return settings;
    }
}