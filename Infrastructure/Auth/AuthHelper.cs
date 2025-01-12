using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using PlaywrightDemo.Infrastructure.Config.Models;
using PlaywrightDemo.Infrastructure.Config;
using PlaywrightDemo.Infrastructure.Logging;
using NUnit.Framework;
using System.Runtime.CompilerServices;

namespace PlaywrightDemo.Infrastructure.Auth;

public class AuthHelper
{
    private readonly ILogger _logger;
    private readonly TestSettings _settings;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly string _authDirectory;
    private readonly Dictionary<string, string> _authStateCache;
    private readonly HashSet<string> _validatedStates;
    private IPlaywright? _playwright;
    private IBrowser? _browser;

    public AuthHelper()
    {
        _logger = LoggerManager.Current;
        _settings = ConfigurationLoader.GetSettings<TestSettings>();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };

        var projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
        _authDirectory = Path.Combine(projectRoot, "TestResults", "Reports", ".auth");
        _authStateCache = new Dictionary<string, string>();
        _validatedStates = new HashSet<string>();

        EnsureAuthDirectoryExists();
        InitializePlaywright().Wait();
        LogMessage(LogLevel.Information, "AuthHelper initialized successfully");
    }

    private void EnsureAuthDirectoryExists()
    {
        try
        {
            if (!Directory.Exists(_authDirectory))
            {
                Directory.CreateDirectory(_authDirectory);
                LogMessage(LogLevel.Information, $"Created auth directory: {_authDirectory}");
            }
        }
        catch (Exception ex)
        {
            LogMessage(LogLevel.Error, $"Failed to create auth directory: {_authDirectory}", ex);
            throw;
        }
    }

    private async Task InitializePlaywright()
    {
        try
        {
            _playwright = await Playwright.CreateAsync();
            _browser = await _playwright.Chromium.LaunchAsync(new()
            {
                Headless = _settings.Browser.Headless
            });
            LogMessage(LogLevel.Information, "Playwright initialized successfully");
        }
        catch (Exception ex)
        {
            LogMessage(LogLevel.Error, "Failed to initialize Playwright", ex);
            throw;
        }
    }

    /// <summary>
    /// Generates a new authentication state for the specified credentials and saves it to a file.
    /// </summary>
    public async Task<string> GenerateAuthStateAsync(string username, string password, [CallerMemberName] string filename = "")
    {
        LogMessage(LogLevel.Information, $"Starting authentication state generation for user: {username}");

        if (string.IsNullOrEmpty(filename))
        {
            filename = $"authstate-{Guid.NewGuid()}.json";
            LogMessage(LogLevel.Information, $"Generated filename: {filename}");
        }

        var filePath = Path.Combine(_authDirectory, filename);
        IBrowserContext? context = null;
        IPage? page = null;

        try
        {
            context = await _browser!.NewContextAsync();
            page = await context.NewPageAsync();

            await NavigateToLoginPage(page);
            await PerformLogin(page, username, password);
            await SaveAuthState(context, filePath);

            // Cache the successful auth state
            _authStateCache[filename] = filePath;
            _validatedStates.Add(filePath);

            return filePath;
        }
        catch (Exception ex)
        {
            LogMessage(LogLevel.Error, $"Failed to generate authentication state for user {username}", ex);
            throw new AuthenticationException($"Failed to generate authentication state for user {username}", ex);
        }
        finally
        {
            await CleanupResources(context, page);
        }
    }

    private async Task NavigateToLoginPage(IPage page)
    {
        try
        {
            LogMessage(LogLevel.Information, $"Navigating to login page: {_settings.Environment.BaseUrl}/login");
            await page.GotoAsync($"{_settings.Environment.BaseUrl}/login");

            // Wait for login form to be ready
            await page.WaitForSelectorAsync("input[name='username']");
            await page.WaitForSelectorAsync("input[name='password']");
        }
        catch (Exception ex)
        {
            LogMessage(LogLevel.Error, "Failed to navigate to login page", ex);
            throw;
        }
    }

    private async Task PerformLogin(IPage page, string username, string password)
    {
        try
        {
            LogMessage(LogLevel.Information, "Filling login credentials");
            await page.FillAsync("input[name='username']", username);
            await page.FillAsync("input[name='password']", password);

            LogMessage(LogLevel.Information, "Submitting login form");

            // Replace deprecated RunAndWaitForNavigationAsync with newer pattern
            var navigationTask = page.WaitForNavigationAsync();
            await page.ClickAsync("button[type='submit']");
            await navigationTask;

            if (page.Url.Contains("/login"))
            {
                var errorMessage = await GetLoginErrorMessage(page);
                throw new AuthenticationException($"Login failed for user: {username}. {errorMessage}");
            }

            LogMessage(LogLevel.Information, "Login successful");
        }
        catch (Exception ex)
        {
            LogMessage(LogLevel.Error, "Login attempt failed", ex);
            throw;
        }
    }

    private async Task<string> GetLoginErrorMessage(IPage page)
    {
        try
        {
            var errorElement = await page.QuerySelectorAsync(".error-message");
            if (errorElement != null)
            {
                return await errorElement.TextContentAsync() ?? "Unknown error occurred";
            }
        }
        catch
        {
            // Ignore error element reading failures
        }
        return "No error message available";
    }

    private async Task SaveAuthState(IBrowserContext context, string filePath)
    {
        try
        {
            LogMessage(LogLevel.Information, "Saving authentication state");
            await context.StorageStateAsync(new() { Path = filePath });
            LogMessage(LogLevel.Information, $"Authentication state saved to: {filePath}");
        }
        catch (Exception ex)
        {
            LogMessage(LogLevel.Error, "Failed to save authentication state", ex);
            throw;
        }
    }

    /// <summary>
    /// Retrieves a storage state from file or cache.
    /// </summary>
    public async Task<string> GetAuthStateAsync(string filename = "default")
    {
        // Check cache first
        if (_authStateCache.TryGetValue(filename, out var cachedPath))
        {
            if (_validatedStates.Contains(cachedPath))
            {
                LogMessage(LogLevel.Information, $"Using cached auth state for: {filename}");
                return await File.ReadAllTextAsync(cachedPath);
            }
        }

        var filePath = Path.Combine(_authDirectory, filename);
        LogMessage(LogLevel.Information, $"Retrieving auth state from: {filePath}");

        if (!File.Exists(filePath))
        {
            var errorMessage = $"Authentication state file not found: {filePath}";
            LogMessage(LogLevel.Error, errorMessage);
            throw new FileNotFoundException(errorMessage);
        }

        var content = await File.ReadAllTextAsync(filePath);

        // Cache the content
        _authStateCache[filename] = filePath;

        LogMessage(LogLevel.Information, $"Successfully retrieved auth state from: {filePath}");
        return content;
    }

    /// <summary>
    /// Verifies if the authentication state at the given path is valid.
    /// </summary>
    public async Task<bool> VerifyAuthStateAsync(string storageStatePath)
    {
        if (_validatedStates.Contains(storageStatePath))
        {
            LogMessage(LogLevel.Information, $"Auth state already validated: {storageStatePath}");
            return true;
        }

        LogMessage(LogLevel.Information, $"Verifying auth state from: {storageStatePath}");
        IBrowserContext? context = null;
        IPage? page = null;

        try
        {
            if (!File.Exists(storageStatePath))
            {
                LogMessage(LogLevel.Warning, $"Authentication state file not found: {storageStatePath}");
                return false;
            }

            context = await _browser!.NewContextAsync(new BrowserNewContextOptions
            {
                StorageStatePath = storageStatePath
            });
            page = await context.NewPageAsync();

            LogMessage(LogLevel.Information, "Navigating to protected page to verify auth state");
            await page.GotoAsync($"{_settings.Environment.BaseUrl}/profile");

            var isAuthenticated = !page.Url.Contains("/login");
            LogMessage(LogLevel.Information, $"Auth state verification result: {(isAuthenticated ? "Valid" : "Invalid")}");

            if (isAuthenticated)
            {
                var cookies = await context.CookiesAsync(new[] { _settings.Environment.BaseUrl });
                LogMessage(LogLevel.Information, $"Found {cookies.Count} cookies for domain");
                _validatedStates.Add(storageStatePath);
            }

            return isAuthenticated;
        }
        catch (Exception ex)
        {
            LogMessage(LogLevel.Error, "Error verifying authentication state", ex);
            return false;
        }
        finally
        {
            await CleanupResources(context, page);
        }
    }

    /// <summary>
    /// Deletes the specified authentication state file and removes it from cache.
    /// </summary>
    public void DeleteAuthState(string filename)
    {
        var filePath = Path.Combine(_authDirectory, filename);
        LogMessage(LogLevel.Information, $"Attempting to delete auth state file: {filePath}");

        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                _authStateCache.Remove(filename);
                _validatedStates.Remove(filePath);
                LogMessage(LogLevel.Information, $"Successfully deleted auth state file: {filePath}");
            }
            else
            {
                LogMessage(LogLevel.Warning, $"Attempted to delete non-existent auth state file: {filePath}");
            }
        }
        catch (Exception ex)
        {
            LogMessage(LogLevel.Error, $"Failed to delete auth state file: {filePath}", ex);
            throw;
        }
    }

    private async Task CleanupResources(IBrowserContext? context, IPage? page)
    {
        try
        {
            if (page != null) await page.CloseAsync();
            if (context != null) await context.DisposeAsync();
        }
        catch (Exception ex)
        {
            LogMessage(LogLevel.Warning, "Error during resource cleanup", ex);
        }
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            if (_browser != null) await _browser.DisposeAsync();
            _playwright?.Dispose();
            LogMessage(LogLevel.Information, "AuthHelper resources disposed successfully");
        }
        catch (Exception ex)
        {
            LogMessage(LogLevel.Error, "Error disposing AuthHelper resources", ex);
        }
    }

    private void LogMessage(LogLevel level, string message, Exception? ex = null)
    {
        var prefix = "[AUTH]";
        var formattedMessage = $"{prefix} {message}";

        switch (level)
        {
            case LogLevel.Information:
                _logger.LogInformation(formattedMessage);
                TestContext.WriteLine(formattedMessage);
                break;
            case LogLevel.Warning:
                _logger.LogWarning(formattedMessage);
                TestContext.WriteLine($"{prefix} WARNING: {message}");
                break;
            case LogLevel.Error:
                _logger.LogError(ex, formattedMessage);
                TestContext.WriteLine($"{prefix} ERROR: {message}");
                if (ex != null)
                {
                    TestContext.WriteLine($"{prefix} Exception: {ex.Message}");
                    TestContext.WriteLine($"{prefix} Stack Trace: {ex.StackTrace}");
                }
                break;
        }
    }
}

public class AuthenticationException : Exception
{
    public AuthenticationException(string message) : base(message) { }
    public AuthenticationException(string message, Exception innerException) : base(message, innerException) { }
}