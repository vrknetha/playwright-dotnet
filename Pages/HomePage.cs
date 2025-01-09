using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using Microsoft.Extensions.Configuration;

namespace ParkPlaceSample.Pages;

public class HomePage : BasePage
{
    private readonly IPage _page;
    private readonly ILogger _logger;
    private readonly IConfiguration _configuration;

    // Locators
    private ILocator GetStartedButton => _page.GetByRole(AriaRole.Link, new() { Name = "Get Started" });
    private ILocator SearchButton => _page.GetByRole(AriaRole.Button, new() { Name = "Search" });
    private ILocator SearchInput => _page.GetByPlaceholder("Search");

    public HomePage(IPage page, ILogger logger, IConfiguration configuration)
        : base(page, logger, configuration)
    {
        _page = page;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task NavigateAsync()
    {
        _logger.LogInformation("Navigating to homepage");
        await _page.GotoAsync(_configuration["Environment:BaseUrl"]);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        _logger.LogInformation("Homepage loaded successfully");
    }

    public async Task<string> GetTitleAsync()
    {
        var title = await _page.TitleAsync();
        _logger.LogInformation("Retrieved page title: {Title}", title);
        return title;
    }

    public async Task ClickGetStartedAsync()
    {
        _logger.LogInformation("Clicking 'Get Started' button");
        await GetStartedButton.WaitForAsync();
        await GetStartedButton.ClickAsync();
        await _page.WaitForURLAsync("**/docs/intro");
        _logger.LogInformation("Navigated to documentation page");
    }

    public async Task OpenSearchAsync()
    {
        _logger.LogInformation("Opening search dialog");
        await _page.Keyboard.PressAsync("Control+k");
        await SearchInput.WaitForAsync();
        _logger.LogInformation("Search dialog opened");
    }

    public async Task SearchAsync(string query)
    {
        _logger.LogInformation("Performing search for: {Query}", query);
        await OpenSearchAsync();
        await SearchInput.FillAsync(query);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        _logger.LogInformation("Search completed");
    }

    public async Task<int> GetSearchResultsCountAsync()
    {
        var searchResults = _page.Locator("[class*='searchResult']");
        var count = await searchResults.CountAsync();
        _logger.LogInformation("Found {Count} search results", count);
        return count;
    }

    public async Task TakeScreenshotAsync(string path)
    {
        _logger.LogInformation("Taking screenshot: {Path}", path);
        await _page.ScreenshotAsync(new() { Path = path });
        _logger.LogInformation("Screenshot saved successfully");
    }
}