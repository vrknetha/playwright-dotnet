using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using System.Text.Json;
using PlaywrightDemo.Infrastructure.Config;
using PlaywrightDemo.Infrastructure.Config.Models;
using PlaywrightDemo.Infrastructure.Logging;
using NUnit.Framework;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace PlaywrightDemo.Pages.API;

public abstract class BaseApiPage : PageTest
{
    protected readonly IAPIRequestContext ApiContext;
    protected readonly ILogger Logger;
    protected TestSettings Settings { get; }

    protected BaseApiPage(IAPIRequestContext apiContext)
    {
        ApiContext = apiContext;
        Logger = LoggerManager.Current;
        Settings = ConfigurationLoader.GetSettings<TestSettings>();
    }

    // Reusable assertion methods using Playwright's Expect
    protected async Task ExpectResponseToContainKeyValueAsync(IAPIResponse response, string key, string value)
    {
        var json = await response.JsonAsync<JsonElement>();
        await Expect(response).ToBeOKAsync();
        try
        {
            var propertyValue = json.GetProperty(key).GetString();
            Assert.That(propertyValue, Is.EqualTo(value),
                $"Expected JSON response to have key '{key}' with value '{value}'");
        }
        catch (KeyNotFoundException)
        {
            Assert.Fail($"Key '{key}' not found in the JSON response.");
        }
    }

    protected async Task ExpectResponseToBeSuccessfulAsync(IAPIResponse response)
    {
        await Expect(response).ToBeOKAsync();
        Assert.That(response.Status, Is.EqualTo(200));
    }

    protected async Task ExpectResponseToContainArrayAsync(IAPIResponse response)
    {
        await Expect(response).ToBeOKAsync();
        var json = await response.JsonAsync<JsonElement>();
        Assert.That(json.ValueKind, Is.EqualTo(JsonValueKind.Array),
            "Expected response to be a JSON array");
        Assert.That(json.EnumerateArray().Any(), Is.True,
            "Expected array to not be empty");
    }
}