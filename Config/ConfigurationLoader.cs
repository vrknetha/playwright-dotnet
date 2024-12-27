using Microsoft.Extensions.Configuration;

namespace ParkPlaceSample.Config;

public static class ConfigurationLoader
{
    private static TestSettings? _settings;

    public static TestSettings Settings => _settings ??= LoadSettings();

    public static TestSettings LoadSettings()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{GetEnvironment()}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        var settings = new TestSettings();
        configuration.Bind(settings);
        return settings;
    }

    private static string GetEnvironment()
    {
        return Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
    }
}