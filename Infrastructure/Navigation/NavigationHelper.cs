using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using ParkPlaceSample.Infrastructure.Config;

namespace ParkPlaceSample.Infrastructure.Navigation;

public class NavigationHelper
{
    private readonly IPage _page;
    private readonly ILogger _logger;
    private readonly TestSettings _settings;

    public NavigationHelper(IPage page, ILogger logger, TestSettings settings)
    {
        _page = page;
        _logger = logger;
        _settings = settings;
    }

    public async Task NavigateToAsync(string path)
    {
        var url = BuildUrl(path);
        _logger.LogInformation("Navigating to {Url}", url);
        await _page.GotoAsync(url);
    }

    private string BuildUrl(string path)
    {
        var baseUrl = _settings.Environment.BaseUrl.TrimEnd('/');
        path = path.TrimStart('/');
        return $"{baseUrl}/{path}";
    }
}