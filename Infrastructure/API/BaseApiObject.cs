using Microsoft.Extensions.Logging;
using ParkPlaceSample.Infrastructure.Config;

namespace ParkPlaceSample.Infrastructure.API;

public abstract class BaseApiObject
{
    protected readonly ILogger Logger;
    protected readonly TestSettings Settings;
    protected readonly HttpClient HttpClient;

    protected BaseApiObject(ILogger logger, TestSettings settings, HttpClient httpClient)
    {
        Logger = logger;
        Settings = settings;
        HttpClient = httpClient;
    }

    protected string BuildUrl(string path)
    {
        var baseUrl = Settings.Environment.ApiBaseUrl.TrimEnd('/');
        path = path.TrimStart('/');
        return $"{baseUrl}/{path}";
    }
}