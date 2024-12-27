using Microsoft.Extensions.Logging;
using ParkPlaceSample.Config;

namespace ParkPlaceSample.Infrastructure.API;

public abstract class BaseApiObject
{
    protected readonly HttpClient HttpClient;
    protected readonly ILogger Logger;
    protected readonly TestSettings Settings;
    protected abstract string BasePath { get; }

    protected BaseApiObject(HttpClient httpClient, ILogger logger, TestSettings settings)
    {
        HttpClient = httpClient;
        Logger = logger;
        Settings = settings;
    }

    protected string BuildEndpoint(string path)
    {
        var cleanBasePath = BasePath.TrimStart('/').TrimEnd('/');
        var cleanPath = path.TrimStart('/');
        return $"{cleanBasePath}/{cleanPath}";
    }

    protected async Task<T?> GetAsync<T>(string path)
    {
        var endpoint = BuildEndpoint(path);
        Logger.LogInformation("GET request to: {Endpoint}", endpoint);
        var response = await HttpClient.GetAsync(endpoint);
        return await HandleResponseAsync<T>(response);
    }

    protected async Task<T?> PostAsync<T>(string path, object? data = null)
    {
        var endpoint = BuildEndpoint(path);
        Logger.LogInformation("POST request to: {Endpoint}", endpoint);
        var content = CreateJsonContent(data);
        var response = await HttpClient.PostAsync(endpoint, content);
        return await HandleResponseAsync<T>(response);
    }

    protected async Task<T?> PutAsync<T>(string path, object? data = null)
    {
        var endpoint = BuildEndpoint(path);
        Logger.LogInformation("PUT request to: {Endpoint}", endpoint);
        var content = CreateJsonContent(data);
        var response = await HttpClient.PutAsync(endpoint, content);
        return await HandleResponseAsync<T>(response);
    }

    protected async Task<T?> DeleteAsync<T>(string path)
    {
        var endpoint = BuildEndpoint(path);
        Logger.LogInformation("DELETE request to: {Endpoint}", endpoint);
        var response = await HttpClient.DeleteAsync(endpoint);
        return await HandleResponseAsync<T>(response);
    }

    protected async Task<T?> PatchAsync<T>(string path, object? data = null)
    {
        var endpoint = BuildEndpoint(path);
        Logger.LogInformation("PATCH request to: {Endpoint}", endpoint);
        var content = CreateJsonContent(data);
        var response = await HttpClient.PatchAsync(endpoint, content);
        return await HandleResponseAsync<T>(response);
    }

    private async Task<T?> HandleResponseAsync<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        LogResponse(response, content);

        response.EnsureSuccessStatusCode();

        if (string.IsNullOrEmpty(content))
        {
            return default;
        }

        return System.Text.Json.JsonSerializer.Deserialize<T>(content, new System.Text.Json.JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    private void LogResponse(HttpResponseMessage response, string content)
    {
        var logLevel = response.IsSuccessStatusCode ? LogLevel.Information : LogLevel.Error;
        Logger.Log(logLevel, "Response Status: {StatusCode} ({StatusPhrase})",
            (int)response.StatusCode, response.ReasonPhrase);
        Logger.Log(logLevel, "Response Content: {Content}", content);
    }

    private static StringContent CreateJsonContent(object? data)
    {
        var json = data != null ? System.Text.Json.JsonSerializer.Serialize(data) : "";
        return new StringContent(json, System.Text.Encoding.UTF8, "application/json");
    }
}