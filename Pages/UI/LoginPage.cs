using System;
using System.Threading.Tasks;
using Microsoft.Playwright;
using System.Text.Json;
using System.IO;
using Microsoft.Extensions.Logging;
using PlaywrightDemo.Infrastructure.Base;
using PlaywrightDemo.Infrastructure.TestData.Models;
using PlaywrightDemo.Infrastructure.Logging;
using PlaywrightDemo.Pages.UI;
using PlaywrightDemo.Infrastructure.Config;

namespace PlaywrightDemo.Pages.UI;

public class LoginPage : BasePage
{
    private readonly IPage _page;
    private ILocator UsernameInput => _page.Locator("#login_field");
    private ILocator PasswordInput => _page.Locator("#password");
    private ILocator SignInButton => _page.GetByRole(AriaRole.Button, new() { Name = "Sign in" }).First;

    public LoginPage(IPage page) : base(page)
    {
        _page = page;
    }

    public async Task LoginWithCredentialsAsync(string username, string password)
    {
        Logger.LogInformation($"Logging in as user: {username}");
        await GoToLoginPage();
        await Expect(SignInButton).ToBeVisibleAsync();
        await UsernameInput.FillAsync(username);
        await PasswordInput.FillAsync(password);
        await SignInButton.ClickAsync();
        await Page.WaitForURLAsync("**/dashboard");
        Logger.LogInformation("Login successful");
    }

    private async Task<string> TakeScreenshot(string name)
    {
        var path = Path.Combine(Settings.Reporting.Screenshots.Directory,
            $"{name}_{DateTime.Now:yyyyMMddHHmmss}.png");
        await Page.ScreenshotAsync(new() { Path = path, FullPage = true });
        return path;
    }

    public async Task NavigateToDashboardAsync()
    {
        await _page.GotoAsync(Settings.Environment.BaseUrl);
    }

    public async Task ExpectLoginSuccessfulAsync()
    {
        await Expect(_page.Locator("#copilot-dashboard-entrypoint-textarea").First).ToBeVisibleAsync(new() { Timeout = 30000 });
    }

    private async Task GoToLoginPage()
    {
        var loginUrl = $"{Settings.Environment.BaseUrl}/login";
        await Page.GotoAsync(loginUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }
}

