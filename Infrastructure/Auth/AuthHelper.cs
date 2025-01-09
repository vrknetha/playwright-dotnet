using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using ParkPlaceSample.Infrastructure.Config.Models;
using NUnit.Framework;

namespace ParkPlaceSample.Infrastructure.Auth;

public class AuthHelper
{
    private readonly ILogger _logger;
    private readonly TestSettings _settings;
    private readonly IAPIRequestContext _apiContext;
    private readonly JsonSerializerOptions _jsonOptions;
    private string? _cachedStorageState;

    public AuthHelper(ILogger logger, TestSettings settings, IAPIRequestContext apiContext)
    {
        _logger = logger;
        _settings = settings;
        _apiContext = apiContext;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };
    }

    /// <summary>
    /// Generates or retrieves a cached authentication state for the specified credentials.
    /// </summary>
    public async Task<string> GenerateAuthStateAsync(string username, string password)
    {
        try
        {
            if (!string.IsNullOrEmpty(_cachedStorageState))
            {
                _logger.LogInformation("Using cached authentication state");
                return _cachedStorageState;
            }

            _logger.LogInformation("Generating new authentication state for user: {Username}", username);

            var authData = new
            {
                username = username,
                password = password
            };

            var response = await _apiContext.PostAsync("/api/auth/login", new() { DataObject = authData });
            await LogResponseDetails(response);

            Assert.That((int)response.Status, Is.EqualTo(200), "Authentication request failed");

            var authResponse = await response.JsonAsync<AuthResponse>();
            Assert.That(authResponse, Is.Not.Null, "Authentication response is null");
            Assert.That(authResponse!.Token, Is.Not.Null.Or.Empty, "Authentication token is missing");

            // Create storage state with authentication data
            var storageState = new
            {
                cookies = new[]
                {
                    new
                    {
                        name = "auth_token",
                        value = authResponse.Token,
                        domain = new Uri(_settings.Environment.BaseUrl).Host,
                        path = "/"
                    }
                },
                origins = new[]
                {
                    new
                    {
                        origin = _settings.Environment.BaseUrl,
                        localStorage = new[]
                        {
                            new
                            {
                                name = "auth_token",
                                value = authResponse.Token
                            }
                        }
                    }
                }
            };

            _cachedStorageState = JsonSerializer.Serialize(storageState, _jsonOptions);
            _logger.LogInformation("Authentication state generated successfully");

            return _cachedStorageState;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate authentication state");
            throw new AuthenticationException("Failed to generate authentication state", ex);
        }
    }

    /// <summary>
    /// Clears the cached authentication state.
    /// </summary>
    public void ClearAuthState()
    {
        _logger.LogInformation("Clearing cached authentication state");
        _cachedStorageState = null;
    }

    /// <summary>
    /// Verifies if the current authentication state is valid.
    /// </summary>
    public async Task<bool> VerifyAuthStateAsync()
    {
        try
        {
            _logger.LogInformation("Verifying authentication state");
            var response = await _apiContext.GetAsync("/api/auth/verify");
            await LogResponseDetails(response);

            return response.Status == 200;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to verify authentication state");
            return false;
        }
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
                var jsonElement = JsonSerializer.Deserialize<JsonElement>(body);
                var formattedJson = JsonSerializer.Serialize(jsonElement, _jsonOptions);
                _logger.LogInformation("Response Body: {Body}", formattedJson);
            }
            catch
            {
                _logger.LogInformation("Response Body: {Body}", body);
            }
        }
    }
}

public class AuthenticationException : Exception
{
    public AuthenticationException(string message) : base(message) { }
    public AuthenticationException(string message, Exception innerException) : base(message, innerException) { }
}

public class AuthResponse
{
    public string? Token { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? ExpiresAt { get; set; }
}