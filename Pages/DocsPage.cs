using Microsoft.Extensions.Logging;
using Microsoft.Playwright;

namespace ParkPlaceSample.Pages;

public class DocsPage : BasePage
{
    // Locators
    private ILocator InstallationHeading => Page.GetByRole(AriaRole.Heading, new() { Name = "Installation" });
    private ILocator ApiDocsLink => Page.GetByRole(AriaRole.Link, new() { Name = "API" });
    private ILocator SearchInput => Page.GetByPlaceholder("Search");

    public DocsPage(IPage page) : base(page) { }

    public async Task NavigateAsync()
    {
        Logger.LogInformation("Navigating to documentation page");
        await Page.GotoAsync(BuildUrl("/docs/intro"));
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        Logger.LogInformation("Documentation page loaded successfully");
    }

    public async Task NavigateToApiDocsAsync()
    {
        Logger.LogInformation("Navigating to API documentation");
        await Page.GotoAsync(BuildUrl("/docs/api/class-playwright"));
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        Logger.LogInformation("API documentation page loaded successfully");
    }

    public async Task<string> GetTitleAsync()
    {
        var title = await Page.TitleAsync();
        Logger.LogInformation("Retrieved page title: {Title}", title);
        return title;
    }

    public async Task<string> GetInstallationHeadingTextAsync()
    {
        await InstallationHeading.WaitForAsync();
        var text = await InstallationHeading.TextContentAsync();
        Logger.LogInformation("Retrieved installation heading text: {Text}", text);
        return text ?? string.Empty;
    }

    public async Task SearchAsync(string query)
    {
        Logger.LogInformation("Performing search for: {Query}", query);
        await Page.Keyboard.PressAsync("Control+k");
        await SearchInput.WaitForAsync();
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