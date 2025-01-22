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

namespace PlaywrightDemo.Pages.UI;

public class LoginPage : BasePage
{
    private readonly IPage _page;
    private string CommonPassword => Settings.Auth.CommonPassword;

    public LoginPage(IPage page) : base(page)
    {
        _page = page;
    }

    public async Task LoginAsync(User user)
    {
        Logger.LogInformation("Navigating to login page");
        await Page.GotoAsync($"{Settings.Environment.BaseUrl}/login");

        Logger.LogInformation("Filling login credentials");
        await Page.FillAsync("input[name='login']", user.Username);
        await Page.FillAsync("input[name='password']", user.Password);

        Logger.LogInformation("Submitting login form");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Sign in" }).ClickAsync();

        await SaveSessionStorageAsync(user.Username);
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
        await Expect(_page.GetByText("Dashboard").First).ToBeAttachedAsync();
    }

    private async Task SaveSessionStorageAsync(string username)
    {
        var authPath = TestBase.GetAuthStatePath();
        var filePath = Path.Combine(authPath, $"{username}_state.json");

        await _page.Context.StorageStateAsync(new BrowserContextStorageStateOptions
        {
            Path = filePath
        });
    }
}

