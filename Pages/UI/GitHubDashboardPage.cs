using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace PlaywrightDemo.Pages.UI;

public class GitHubDashboardPage : BasePage
{
    public GitHubDashboardPage(IPage page) : base(page)
    {
        Logger.LogInformation("Initializing GitHub Dashboard Page");
    }

    // Locators
    private ILocator RepoLink(string repoName) => Page.Locator($"a[href*='{repoName}']");
    private ILocator NewRepoButton => Page.Locator("a[href='/new']");
    private ILocator ReposList => Page.Locator("div[data-testid='repositories-list']");
    private ILocator UserNavMenu => Page.Locator("button[aria-label='Open user account menu']");
    private ILocator UserProfileLink => Page.Locator("a[href*='/settings/profile']");

    // Navigation methods
    public async Task NavigateToDashboardAsync()
    {
        Logger.LogInformation("Navigating to GitHub dashboard");
        await Page.GotoAsync(BuildUrl("/"));
        await Expect(ReposList).ToBeVisibleAsync();
    }

    public async Task NavigateToNewRepoPageAsync()
    {
        Logger.LogInformation("Navigating to new repository page");
        await NewRepoButton.ClickAsync();
        await Page.WaitForURLAsync("**/new");
    }

    // Verification methods using Playwright's Expect
    public async Task ExpectRepoToBeVisibleAsync(string repoName)
    {
        Logger.LogInformation($"Verifying repository '{repoName}' is visible");
        await Expect(RepoLink(repoName)).ToBeVisibleAsync(new LocatorAssertionsToBeVisibleOptions
        {
            Timeout = 10000
        });
    }

    public async Task ExpectUserProfileToBeAccessibleAsync()
    {
        Logger.LogInformation("Verifying user profile is accessible");
        await UserNavMenu.ClickAsync();
        await Expect(UserProfileLink).ToBeVisibleAsync();
    }

    // Helper methods
    public async Task RefreshPageAsync()
    {
        Logger.LogInformation("Refreshing dashboard page");
        await Page.ReloadAsync();
        await Expect(ReposList).ToBeVisibleAsync();
    }
}