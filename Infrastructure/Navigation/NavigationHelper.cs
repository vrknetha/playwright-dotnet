using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using ParkPlaceSample.Infrastructure.Config.Models;

namespace ParkPlaceSample.Infrastructure.Navigation;

public class NavigationHelper
{
    private readonly ILogger _logger;
    private readonly TestSettings _settings;
    private readonly IPage _page;

    public NavigationHelper(ILogger logger, TestSettings settings, IPage page)
    {
        _logger = logger;
        _settings = settings;
        _page = page;
    }

    public async Task NavigateToAsync(string path)
    {
        var url = BuildUrl(path);
        _logger.LogInformation("Navigating to: {Url}", url);
        await _page.GotoAsync(url);
    }

    private string BuildUrl(string path)
    {
        var baseUrl = _settings.Environment.BaseUrl.TrimEnd('/');
        path = path.TrimStart('/');
        return $"{baseUrl}/{path}";
    }
}