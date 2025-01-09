using NUnit.Framework;
using ParkPlaceSample.Infrastructure.Base;

namespace ParkPlaceSample.Tests.Auth;

[TestFixture]
[Category("Auth")]
[Category("Admin")]
public class AdminUserTests : TestBase
{
    private const string AUTH_STATE_FILE = "admin.json";
    private const string ADMIN_USERNAME = "admin@example.com";
    private const string ADMIN_PASSWORD = "AdminPass123!";

    [OneTimeSetUp]
    public async Task AuthSetup()
    {
        // Generate auth state for admin user if it doesn't exist or is invalid
        var authStatePath = Path.Combine(GetAuthStatePath(), AUTH_STATE_FILE);
        if (!File.Exists(authStatePath) || !await AuthHelper.VerifyAuthStateAsync(authStatePath))
        {
            LogInfo("Generating new authentication state for admin user");
            await AuthHelper.GenerateAuthStateAsync(ADMIN_USERNAME, ADMIN_PASSWORD, AUTH_STATE_FILE);
        }

        // Set the auth state to use for all tests in this class
        AuthStateToUse = AUTH_STATE_FILE;
    }

    [Test]
    public async Task AdminCanAccessDashboard()
    {
        // Navigate to admin dashboard
        await Page.GotoAsync($"{Settings.Environment.BaseUrl}/admin/dashboard");

        // Verify we're not redirected to login
        Assert.That(Page.Url, Does.Not.Contain("/login"), "Admin should not be redirected to login page");

        // Verify admin dashboard elements
        var dashboardTitle = await Page.TextContentAsync("h1");
        Assert.That(dashboardTitle, Does.Contain("Admin Dashboard"), "Admin dashboard should be accessible");
    }

    [Test]
    public async Task AdminCanManageUsers()
    {
        // Navigate to user management page
        await Page.GotoAsync($"{Settings.Environment.BaseUrl}/admin/users");

        // Verify user management elements
        var usersList = await Page.QuerySelectorAsync(".users-list");
        Assert.That(usersList, Is.Not.Null, "Users list should be present");

        // Test user search functionality
        await Page.FillAsync("input[name='search']", "test@example.com");
        await Page.ClickAsync("button[type='submit']");

        // Verify search results
        var searchResults = await Page.QuerySelectorAllAsync(".user-item");
        Assert.That(searchResults.Count, Is.GreaterThan(0), "Search should return results");
    }

    [Test]
    public async Task AdminCanViewSystemLogs()
    {
        // Navigate to system logs page
        await Page.GotoAsync($"{Settings.Environment.BaseUrl}/admin/logs");

        // Verify logs page elements
        var logsTitle = await Page.TextContentAsync("h1");
        Assert.That(logsTitle, Does.Contain("System Logs"), "System logs should be accessible");

        // Verify log entries are present
        var logEntries = await Page.QuerySelectorAllAsync(".log-entry");
        Assert.That(logEntries.Count, Is.GreaterThan(0), "Log entries should be present");

        // Test log filtering
        await Page.SelectOptionAsync("select[name='logLevel']", "error");
        await Page.ClickAsync("button.apply-filter");

        // Verify filtered results
        var filteredLogs = await Page.QuerySelectorAllAsync(".log-entry.error");
        Assert.That(filteredLogs.Count, Is.GreaterThan(0), "Filtered error logs should be present");
    }
}