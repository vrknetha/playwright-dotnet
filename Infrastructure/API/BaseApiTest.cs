using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using ParkPlaceSample.Config;
using ParkPlaceSample.Infrastructure.Logging;

namespace ParkPlaceSample.Infrastructure.API;

/// <summary>
/// Base class for API tests providing common functionality and setup.
/// </summary>
[TestClass]
public abstract class BaseApiTest
{
    protected HttpClient HttpClient { get; private set; } = null!;
    protected TestSettings Settings => ConfigurationLoader.Settings;
    protected ILogger Logger { get; private set; } = null!;
    protected string BaseUrl => GetBaseUrl();

    public TestContext TestContext { get; set; } = null!;

    [TestInitialize]
    public virtual async Task TestInitialize()
    {
        InitializeLogger();
        Logger.LogInformation("Starting API test: {TestName}", TestContext.TestName);

        try
        {
            HttpClient = CreateHttpClient();
            await OnTestInitialize();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to initialize API test");
            throw;
        }
    }

    [TestCleanup]
    public virtual async Task TestCleanup()
    {
        try
        {
            await OnTestCleanup();

            if (TestContext.CurrentTestOutcome != UnitTestOutcome.Passed)
            {
                Logger.LogWarning("API test failed: {TestOutcome}", TestContext.CurrentTestOutcome);
            }
            else
            {
                Logger.LogInformation("API test passed successfully");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error during API test cleanup");
            throw;
        }
        finally
        {
            HttpClient.Dispose();
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
            builder.AddTestContext(TestContext);
            builder.SetMinimumLevel(LogLevel.Information);
        });

        Logger = factory.CreateLogger(GetType());
    }
}