using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using PlaywrightDemo.Infrastructure.Config;
using PlaywrightDemo.Infrastructure.Logging;
using Microsoft.Playwright;
using PlaywrightDemo.Infrastructure.Config.Models;
using PlaywrightDemo.Infrastructure.Base;

namespace PlaywrightDemo.Infrastructure.API;

/// <summary>
/// Base class for API tests providing common functionality and setup.
/// </summary>
[TestFixture]
public abstract class BaseApiTest : TestBase
{
    protected HttpClient HttpClient { get; private set; } = null!;
    protected TestSettings Settings { get; private set; } = null!;
    protected ILogger Logger { get; private set; } = null!;
    protected string BaseUrl => GetBaseUrl();
    protected IAPIRequestContext ApiContext { get; private set; } = null!;
    private IPlaywright _playwright = null!;

    [SetUp]
    public override async Task BaseTestInitialize()
    {
        await base.BaseTestInitialize();
        Logger.LogInformation("Starting API test: {TestName}", TestContext.CurrentContext.Test.Name);
    }

    [TearDown]
    public override async Task BaseTestCleanup()
    {
        try
        {
            // Cleanup API resources
            if (ApiContext != null)
            {
                await ApiContext.DisposeAsync();
                Logger.LogInformation("API context disposed successfully");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error during API test cleanup");
            throw;
        }
        finally
        {
            await base.BaseTestCleanup();
        }
    }

    protected virtual Task OnTestInitialize() => Task.CompletedTask;
    protected virtual Task OnTestCleanup() => Task.CompletedTask;

    protected virtual HttpClient CreateHttpClient()
    {
        var client = new HttpClient
        {
            BaseAddress = new Uri(BaseUrl)
        };

        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        ConfigureHttpClient(client);

        return client;
    }

    protected virtual void ConfigureHttpClient(HttpClient client)
    {
        // Override to add custom headers, authentication, etc.
    }

    private string GetBaseUrl()
    {
        var environment = Settings.Environment.Name ?? "Development";
        var apiBaseUrl = Settings.Environment.ApiBaseUrl;

        if (string.IsNullOrEmpty(apiBaseUrl))
        {
            throw new InvalidOperationException($"API base URL not configured for environment: {environment}");
        }

        return apiBaseUrl;
    }

    private void InitializeLogger()
    {
        var factory = LoggerFactory.Create(builder =>
        {
            builder.AddTestContext(TestContext.CurrentContext);
            builder.SetMinimumLevel(LogLevel.Information);
        });

        Logger = factory.CreateLogger(GetType());
    }
}