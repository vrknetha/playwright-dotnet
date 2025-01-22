using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using PlaywrightDemo.Infrastructure.Config.Models;
using NUnit.Framework;
using PlaywrightDemo.Infrastructure.Logging;

namespace PlaywrightDemo.Infrastructure.Reporting;

public class AzureDevOpsTestReporter
{
    private readonly ILogger _logger;
    private readonly AzureDevOpsSettings? _settings;
    private readonly HttpClient? _httpClient;

    public AzureDevOpsTestReporter(ILogger logger, AzureDevOpsSettings settings)
    {
        _logger = logger;
        _settings = settings;
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Basic",
            Convert.ToBase64String(Encoding.ASCII.GetBytes($":{_settings.PersonalAccessToken}"))
        );
    }

    public AzureDevOpsTestReporter()
    {
        _logger = LoggerManager.Current;
        _httpClient = new HttpClient();
    }
}