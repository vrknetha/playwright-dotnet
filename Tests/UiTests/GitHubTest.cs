using NUnit.Framework;
using PlaywrightDemo.Infrastructure.Base;
using PlaywrightDemo.Infrastructure.Context;
using PlaywrightDemo.Infrastructure.Fixtures;
using PlaywrightDemo.Pages.UI;
using PlaywrightDemo.Pages.API;
using System.Threading.Tasks;
using static PlaywrightDemo.Infrastructure.Fixtures.RequiredFixturesAttribute;

namespace PlaywrightDemo.Tests.UiTests
{
    [TestFixture]
    public class GitHubTest : FixtureTestBase
    {
        [Test]
        [RequiredFixtures(typeof(GitHubDashboardPage), typeof(GitHubApiPage))]
        public async Task ShouldDemonstrateWorkflowBetweenDifferentUsers()
        {
            // Use fixtures directly through properties
            await dashboardPage.GoToAsync();
            var isVisible = await dashboardPage.IsVisible();
            Assert.That(isVisible, Is.True, "Create repository button should be visible");

            // Use API fixtures
            var repoName = "test-repo";
            var owner = "testuser";
            var response = await githubApi.GetRepositoryAsync(owner, repoName);
            Assert.That(response.Status, Is.EqualTo(200), "Repository should exist");

            // Create a new context with different auth state
            var adminContextResult = await ContextManager.CreateContextAsync(new ContextOptions
            {
                BaseUrl = Settings.Environment.BaseUrl,
                ApiBaseUrl = Settings.Environment.ApiBaseUrl,
                AuthStateFile = "admin.json",
                BrowserSettings = Settings.Browser,
                PageClasses = new()
                {
                    { "dashboardPage", typeof(GitHubDashboardPage) }
                },
                ApiClasses = new()
                {
                    { "githubApi", typeof(GitHubApiPage) }
                }
            });

            // Use the admin context
            var adminDashboardPage = (GitHubDashboardPage)adminContextResult.PageObjects["dashboardPage"];
            await adminDashboardPage.GoToAsync();
            var isAdminVisible = await adminDashboardPage.IsVisible();
            Assert.That(isAdminVisible, Is.True, "Admin should see create repository button");
        }

        [Test]
        [RequiredFixtures(typeof(DocsPage))]
        public async Task ShouldNavigateToDocumentation()
        {
            // Use docs page fixture directly
            await docsPage.NavigateAsync();
            // Add your test assertions here
        }
    }
}