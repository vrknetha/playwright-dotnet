using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using PlaywrightDemo.Infrastructure.Config.Models;

namespace PlaywrightDemo.Infrastructure.API;

/// <summary>
/// Manages the global API context instance for the test framework.
/// </summary>
public static class ApiContextManager
{
    private static IAPIRequestContext? _currentContext;
    private static readonly object _lock = new();

    /// <summary>
    /// Gets the current API context instance.
    /// </summary>
    public static IAPIRequestContext Current => _currentContext ?? throw new InvalidOperationException("API context not initialized");

    /// <summary>
    /// Initializes a new API context with the specified settings.
    /// </summary>
    public static async Task<IAPIRequestContext> InitializeAsync(IPlaywright playwright, TestSettings settings, string? authStatePath = null)
    {
        var options = new APIRequestNewContextOptions
        {
            BaseURL = settings.Environment.ApiBaseUrl,
            IgnoreHTTPSErrors = true
        };

        // If auth state exists, load it
        if (!string.IsNullOrEmpty(authStatePath))
        {
            options.StorageStatePath = authStatePath;
        }

        var apiContext = await playwright.APIRequest.NewContextAsync(options);
        return apiContext;
    }

    /// <summary>
    /// Disposes of the current API context.
    /// </summary>
    public static async Task DisposeAsync()
    {
        if (_currentContext != null)
        {
            await _currentContext.DisposeAsync();
            _currentContext = null;
        }
    }
}