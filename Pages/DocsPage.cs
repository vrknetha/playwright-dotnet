using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using Microsoft.Extensions.Configuration;

namespace ParkPlaceSample.Pages;

public class DocsPage : BasePage
{
    private readonly IPage _page;
    private readonly ILogger _logger;
    private readonly IConfiguration _configuration;

    // Locators
    private ILocator InstallationHeading => _page.GetByRole(AriaRole.Heading, new() { Name = "Installation" });
    private ILocator ApiDocsLink => _page.GetByRole(AriaRole.Link, new() { Name = "API" });
    private ILocator SearchInput => _page.GetByPlaceholder("Search");

    public DocsPage(IPage page, ILogger logger, IConfiguration configuration)
        : base(page, logger, configuration)
    {
        _page = page;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task NavigateAsync()
    {
        _logger.LogInformation("Navigating to documentation page");
        await _page.GotoAsync($"{_configuration["Environment:BaseUrl"]}/docs/intro");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        _logger.LogInformation("Documentation page loaded successfully");
    }

    public async Task NavigateToApiDocsAsync()
    {
        _logger.LogInformation("Navigating to API documentation");
        await _page.GotoAsync($"{_configuration["Environment:BaseUrl"]}/docs/api/class-playwright");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        _logger.LogInformation("API documentation page loaded successfully");
    }

    public async Task<string> GetTitleAsync()
    {
        var title = await _page.TitleAsync();
        _logger.LogInformation("Retrieved page title: {Title}", title);
        return title;
    }

    public async Task<string> GetInstallationHeadingTextAsync()
    {
        await InstallationHeading.WaitForAsync();
        var text = await InstallationHeading.TextContentAsync();
        _logger.LogInformation("Retrieved installation heading text: {Text}", text);
        return text ?? string.Empty;
    }

    public async Task SearchAsync(string query)
    {
        _logger.LogInformation("Performing search for: {Query}", query);
        await _page.Keyboard.PressAsync("Control+k");
        await SearchInput.WaitForAsync();
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