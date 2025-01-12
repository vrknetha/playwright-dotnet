using Microsoft.Extensions.Logging;
using Microsoft.Playwright;

namespace PlaywrightDemo.Pages;

public class HomePage : BasePage
{
    // Locators
    private ILocator GetStartedButton => Page.GetByRole(AriaRole.Link, new() { Name = "Get Started" });
    private ILocator SearchButton => Page.GetByRole(AriaRole.Button, new() { Name = "Search" });
    private ILocator SearchInput => Page.GetByPlaceholder("Search");

    public HomePage(IPage page) : base(page) { }

    public async Task NavigateAsync()
    {
        Logger.LogInformation("Navigating to homepage");
        await Page.GotoAsync(Settings.Environment.BaseUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        Logger.LogInformation("Homepage loaded successfully");
    }

    public async Task<string> GetTitleAsync()
    {
        var title = await Page.TitleAsync();
        Logger.LogInformation("Retrieved page title: {Title}", title);
        return title;
    }

    public async Task ClickGetStartedAsync()
    {
        Logger.LogInformation("Clicking 'Get Started' button");
        await GetStartedButton.WaitForAsync();
        await GetStartedButton.ClickAsync();
        await Page.WaitForURLAsync("**/docs/intro");
        Logger.LogInformation("Navigated to documentation page");
    }

    public async Task OpenSearchAsync()
    {
        Logger.LogInformation("Opening search dialog");
        await Page.Keyboard.PressAsync("Control+k");
        await SearchInput.WaitForAsync();
        Logger.LogInformation("Search dialog opened");
    }

    public async Task SearchAsync(string query)
    {
        Logger.LogInformation("Performing search for: {Query}", query);
        await OpenSearchAsync();
        await SearchInput.FillAsync(query);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        Logger.LogInformation("Search completed");
    }

    public async Task<int> GetSearchResultsCountAsync()
    {
        var searchResults = Page.Locator("[class*='searchResult']");
        var count = await searchResults.CountAsync();
        Logger.LogInformation("Found {Count} search results", count);
        return count;
    }

    public async Task TakeScreenshotAsync(string path)
    {
        Logger.LogInformation("Taking screenshot: {Path}", path);
        await Page.ScreenshotAsync(new() { Path = path });
        Logger.LogInformation("Screenshot saved successfully");
    }
}