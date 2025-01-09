using NUnit.Framework;
using ParkPlaceSample.Infrastructure.Base;

namespace ParkPlaceSample.Tests.Auth;

[TestFixture]
[Category("Auth")]
public class UnauthenticatedTests : TestBase
{
    [Test]
    public async Task CanAccessPublicPages()
    {
        // Navigate to home page
        await Page.GotoAsync(Settings.Environment.BaseUrl);

        // Verify home page elements
        var homeTitle = await Page.TextContentAsync("h1");
        Assert.That(homeTitle, Is.Not.Null, "Home page should be accessible");

        // Navigate to about page
        await Page.ClickAsync("a[href='/about']");
        var aboutTitle = await Page.TextContentAsync("h1");
        Assert.That(aboutTitle, Does.Contain("About"), "About page should be accessible");
    }

    [Test]
    public async Task RedirectsToLoginForProtectedPages()
    {
        // Try to access protected profile page
        await Page.GotoAsync($"{Settings.Environment.BaseUrl}/profile");

        // Verify redirect to login
        Assert.That(Page.Url, Does.Contain("/login"), "Should be redirected to login page");

        // Verify login form elements
        var loginForm = await Page.QuerySelectorAsync("form.login-form");
        Assert.That(loginForm, Is.Not.Null, "Login form should be present");

        // Verify return URL is preserved
        var returnUrlInput = await Page.QuerySelectorAsync("input[name='returnUrl']");
        var returnUrlValue = await returnUrlInput!.GetAttributeAsync("value");
        Assert.That(returnUrlValue, Does.Contain("/profile"), "Return URL should be preserved");
    }

    [Test]
    public async Task CanRegisterNewAccount()
    {
        // Navigate to registration page
        await Page.GotoAsync($"{Settings.Environment.BaseUrl}/register");

        // Generate unique email
        var uniqueEmail = $"test_{DateTime.Now:yyyyMMddHHmmss}@example.com";

        // Fill registration form
        await Page.FillAsync("input[name='email']", uniqueEmail);
        await Page.FillAsync("input[name='password']", "TestPass123!");
        await Page.FillAsync("input[name='confirmPassword']", "TestPass123!");

        // Submit form
        await Page.ClickAsync("button[type='submit']");

        // Verify successful registration
        var successMessage = await Page.TextContentAsync(".success-message");
        Assert.That(successMessage, Does.Contain("Registration successful"), "Registration should complete successfully");

        // Verify redirect to login
        Assert.That(Page.Url, Does.Contain("/login"), "Should be redirected to login after registration");
    }

    [Test]
    public async Task ShowsValidationErrorsOnInvalidLogin()
    {
        // Navigate to login page
        await Page.GotoAsync($"{Settings.Environment.BaseUrl}/login");

        // Submit empty form
        await Page.ClickAsync("button[type='submit']");

        // Verify validation messages
        var emailError = await Page.TextContentAsync("input[name='email'] + .error-message");
        var passwordError = await Page.TextContentAsync("input[name='password'] + .error-message");

        Assert.Multiple(() =>
        {
            Assert.That(emailError, Does.Contain("required"), "Email validation message should be shown");
            Assert.That(passwordError, Does.Contain("required"), "Password validation message should be shown");
        });

        // Try invalid credentials
        await Page.FillAsync("input[name='email']", "invalid@example.com");
        await Page.FillAsync("input[name='password']", "wrongpass");
        await Page.ClickAsync("button[type='submit']");

        // Verify error message
        var loginError = await Page.TextContentAsync(".error-message");
        Assert.That(loginError, Does.Contain("Invalid"), "Invalid credentials message should be shown");
    }
}