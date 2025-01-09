using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using NUnit.Framework;
using ParkPlaceSample.Infrastructure.Config;
using ParkPlaceSample.Infrastructure.Config.Models;
using ParkPlaceSample.Infrastructure.Logging;
using ParkPlaceSample.Infrastructure.TestData.Models;

namespace ParkPlaceSample.Infrastructure.API;

public class ApiTestHelper
{
    private readonly ILogger _logger;
    private readonly TestSettings _settings;
    private readonly IAPIRequestContext _apiContext;
    private readonly List<string> _createdResources;
    private readonly JsonSerializerOptions _jsonOptions;

    public ApiTestHelper(IAPIRequestContext apiContext)
    {
        _apiContext = apiContext;
        _logger = LoggerManager.Current;
        _settings = ConfigurationLoader.GetSettings<TestSettings>();
        _createdResources = new List<string>();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };
    }

    /// <summary>
    /// Sends a GET request to the specified endpoint.
    /// </summary>
    public async Task<T?> GetAsync<T>(string endpoint, IDictionary<string, string>? queryParams = null)
    {
        try
        {
            _logger.LogInformation("Sending GET request to {Endpoint}", endpoint);
            var url = BuildUrl(endpoint, queryParams);
            var response = await _apiContext.GetAsync(url);

            await LogResponseDetails(response);
            Assert.That((int)response.Status, Is.InRange(200, 299), $"GET request to {url} failed with status {response.Status}");

            return await response.JsonAsync<T>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GET request to {Endpoint} failed", endpoint);
            throw;
        }
    }

    /// <summary>
    /// Sends a POST request to the specified endpoint.
    /// </summary>
    public async Task<T?> PostAsync<T>(string endpoint, object? data = null)
    {
        try
        {
            _logger.LogInformation("Sending POST request to {Endpoint}", endpoint);
            var response = await _apiContext.PostAsync(endpoint, new() { DataObject = data });

            await LogResponseDetails(response);
            Assert.That((int)response.Status, Is.InRange(200, 299), $"POST request to {endpoint} failed with status {response.Status}");

            var result = await response.JsonAsync<T>();
            TrackCreatedResource(endpoint, result);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "POST request to {Endpoint} failed", endpoint);
            throw;
        }
    }

    /// <summary>
    /// Sends a PUT request to the specified endpoint.
    /// </summary>
    public async Task<T?> PutAsync<T>(string endpoint, object data)
    {
        try
        {
            _logger.LogInformation("Sending PUT request to {Endpoint}", endpoint);
            var response = await _apiContext.PutAsync(endpoint, new() { DataObject = data });

            await LogResponseDetails(response);
            Assert.That((int)response.Status, Is.InRange(200, 299), $"PUT request to {endpoint} failed with status {response.Status}");

            return await response.JsonAsync<T>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PUT request to {Endpoint} failed", endpoint);
            throw;
        }
    }

    /// <summary>
    /// Sends a DELETE request to the specified endpoint.
    /// </summary>
    public async Task DeleteAsync(string endpoint)
    {
        try
        {
            _logger.LogInformation("Sending DELETE request to {Endpoint}", endpoint);
            var response = await _apiContext.DeleteAsync(endpoint);

            await LogResponseDetails(response);
            Assert.That((int)response.Status, Is.InRange(200, 299), $"DELETE request to {endpoint} failed with status {response.Status}");

            RemoveTrackedResource(endpoint);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DELETE request to {Endpoint} failed", endpoint);
            throw;
        }
    }

    /// <summary>
    /// Verifies that a resource exists and matches the expected data.
    /// </summary>
    public async Task VerifyResourceAsync<T>(string endpoint, Action<T> verifyAction)
    {
        var resource = await GetAsync<T>(endpoint);
        Assert.That(resource, Is.Not.Null, $"Resource at {endpoint} not found");
        verifyAction(resource!);
    }

    /// <summary>
    /// Cleans up all resources created during the test.
    /// </summary>
    public async Task CleanupResourcesAsync()
    {
        _logger.LogInformation("Cleaning up {Count} resources", _createdResources.Count);

        foreach (var resource in _createdResources.ToList())
        {
            try
            {
                await DeleteAsync(resource);
                _logger.LogInformation("Successfully deleted resource: {Resource}", resource);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete resource: {Resource}", resource);
            }
        }

        _createdResources.Clear();
    }

    private async Task LogResponseDetails(IAPIResponse response)
    {
        var statusCode = (int)response.Status;
        var headers = response.Headers;
        var body = await response.TextAsync();

        _logger.LogInformation("Response Status: {StatusCode}", statusCode);
        _logger.LogInformation("Response Headers: {Headers}", JsonSerializer.Serialize(headers, _jsonOptions));

        if (!string.IsNullOrEmpty(body))
        {
            try
            {
                // Try to format JSON response
                var jsonElement = JsonSerializer.Deserialize<JsonElement>(body);
                var formattedJson = JsonSerializer.Serialize(jsonElement, _jsonOptions);
                _logger.LogInformation("Response Body: {Body}", formattedJson);
            }
            catch
            {
                // If not JSON, log as plain text
                _logger.LogInformation("Response Body: {Body}", body);
            }
        }
    }

    private string BuildUrl(string endpoint, IDictionary<string, string>? queryParams)
    {
        if (queryParams == null || !queryParams.Any())
            return endpoint;

        var queryString = string.Join("&", queryParams.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
        return $"{endpoint}?{queryString}";
    }

    private void TrackCreatedResource(string endpoint, object? resource)
    {
        if (resource == null) return;

        // Extract ID or other identifier from the resource based on common property names
        var resourceId = ExtractResourceIdentifier(resource);
        if (!string.IsNullOrEmpty(resourceId))
        {
            var resourcePath = $"{endpoint.TrimEnd('/')}/{resourceId}";
            _createdResources.Add(resourcePath);
            _logger.LogInformation("Tracking created resource: {ResourcePath}", resourcePath);
        }
    }

    private string? ExtractResourceIdentifier(object resource)
    {
        // Common property names for resource identifiers
        var idPropertyNames = new[] { "Id", "ID", "id", "Identifier", "identifier", "Key", "key" };

        var resourceType = resource.GetType();
        foreach (var propertyName in idPropertyNames)
        {
            var property = resourceType.GetProperty(propertyName);
            if (property != null)
            {
                var value = property.GetValue(resource)?.ToString();
                if (!string.IsNullOrEmpty(value))
                    return value;
            }
        }

        return null;
    }

    private void RemoveTrackedResource(string endpoint)
    {
        var removed = _createdResources.RemoveAll(r => r.StartsWith(endpoint, StringComparison.OrdinalIgnoreCase));
        if (removed > 0)
        {
            _logger.LogInformation("Removed {Count} tracked resources for endpoint: {Endpoint}", removed, endpoint);
        }
    }
}