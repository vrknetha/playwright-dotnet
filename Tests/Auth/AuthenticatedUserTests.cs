using NUnit.Framework;
using ParkPlaceSample.Infrastructure.Base;

namespace ParkPlaceSample.Tests.Auth;

[TestFixture]
[Category("Auth")]
public class AuthenticatedUserTests : TestBase
{
    private const string AUTH_STATE_FILE = "user1.json";
    private const string TEST_USERNAME = "testuser@example.com";
    private const string TEST_PASSWORD = "TestPass123!";

    [OneTimeSetUp]
    public async Task AuthSetup()
    {
        // Generate auth state for the test user if it doesn't exist or is invalid
        var authStatePath = Path.Combine(GetAuthStatePath(), AUTH_STATE_FILE);
        if (!File.Exists(authStatePath) || !await AuthHelper.VerifyAuthStateAsync(authStatePath))
        {
            LogInfo("Generating new authentication state for test user");
            await AuthHelper.GenerateAuthStateAsync(TEST_USERNAME, TEST_PASSWORD, AUTH_STATE_FILE);
        }

        // Set the auth state to use for all tests in this class
        AuthStateToUse = AUTH_STATE_FILE;
    }

    [Test]
    public async Task UserCanAccessProtectedPage()
    {
        // Navigate to a protected page
        await Page.GotoAsync($"{Settings.Environment.BaseUrl}/profile");

        // Verify we're not redirected to login
        Assert.That(Page.Url, Does.Not.Contain("/login"), "User should not be redirected to login page");

        // Verify profile page elements
        var welcomeText = await Page.TextContentAsync("h1");
        Assert.That(welcomeText, Does.Contain(TEST_USERNAME), "Profile page should display user's email");
    }

    [Test]
    public async Task UserCanUpdateProfile()
    {
        // Navigate to profile page
        await Page.GotoAsync($"{Settings.Environment.BaseUrl}/profile");

        // Update profile information
        await Page.FillAsync("input[name='displayName']", "Updated Name");
        await Page.ClickAsync("button[type='submit']");

        // Verify success message
        var successMessage = await Page.TextContentAsync(".success-message");
        Assert.That(successMessage, Does.Contain("Profile updated"), "Profile update should show success message");
    }

    [Test]
    public async Task UserCanViewOrderHistory()
    {
        // Navigate to orders page
        await Page.GotoAsync($"{Settings.Environment.BaseUrl}/orders");

        // Verify orders page loads
        var pageTitle = await Page.TextContentAsync("h1");
        Assert.That(pageTitle, Does.Contain("Order History"), "Order history page should be accessible");

        // Verify orders list is present
        var ordersList = await Page.QuerySelectorAsync(".orders-list");
        Assert.That(ordersList, Is.Not.Null, "Orders list should be present on the page");
    }
}