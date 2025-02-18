using NUnit.Framework;
using PlaywrightDemo.Infrastructure.Base;
using PlaywrightDemo.Infrastructure.Context;
using PlaywrightDemo.Infrastructure.Fixtures;
using PlaywrightDemo.Pages.UI;
using PlaywrightDemo.Pages.API;
using System.Threading.Tasks;

namespace PlaywrightDemo.Tests
{
    [TestFixture]
    [Parallelizable(ParallelScope.None)]
    public class FrameworkDemoTest : FixtureTestBase
    {
        [Test]
        [Description("Demonstrates full framework capabilities")]
        [Category("Demo")]
        [RequiredFixtures(typeof(GitHubDashboardPage), typeof(GitHubApiPage), typeof(DocsPage))]
        public async Task ShouldDemonstrateFullFrameworkCapabilities()
        {
            // 1. Basic logging demonstration
            LogInfo("Starting framework demonstration test");

            // 2. Using Page Fixtures
            LogInfo("Testing dashboard page navigation");
            await dashboardPage.GoToAsync();
            var isVisible = await dashboardPage.IsVisible();
            Assert.That(isVisible, Is.True, "Dashboard should be visible");

            // 3. Using API Fixtures
            LogInfo("Testing API integration");
            var repoName = "test-repo";
            var owner = "testuser";
            var response = await githubApi.GetRepositoryAsync(owner, repoName);
            Assert.That(response.Status, Is.EqualTo(200), "Repository should exist");

            // 4. Multiple Page Navigation
            LogInfo("Testing multiple page navigation");
            await docsPage.NavigateAsync();
            // Add assertions for docs page

            // 5. Creating a new context with different auth state
            LogInfo("Testing multiple contexts with different auth states");
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

            try
            {
                // 6. Using the new context
                var adminDashboardPage = (GitHubDashboardPage)adminContextResult.PageObjects["dashboardPage"];
                await adminDashboardPage.GoToAsync();
                var isAdminVisible = await adminDashboardPage.IsVisible();
                Assert.That(isAdminVisible, Is.True, "Admin should see dashboard");

                // 7. Error handling demonstration
                try
                {
                    LogWarning("Demonstrating error handling");
                    await adminDashboardPage.Page.Locator("non-existent-element").ClickAsync();
                }
                catch (Exception ex)
                {
                    LogError("Expected error occurred", ex);
                }

                // 8. API operations with admin context
                var adminApi = (GitHubApiPage)adminContextResult.ApiObjects["githubApi"];
                var adminResponse = await adminApi.GetRepositoryAsync(owner, repoName);
                Assert.That(adminResponse.Status, Is.EqualTo(200), "Admin should see repository");
            }
            finally
            {
                // Cleanup
                if (adminContextResult.Context != null)
                {
                    await adminContextResult.Context.CloseAsync();
                }
            }

            LogInfo("Framework demonstration completed successfully");
        }

        [Test]
        [Description("Demonstrates simple fixture usage")]
        [Category("Demo")]
        [RequiredFixtures(typeof(DocsPage))]
        public async Task ShouldDemonstrateSimpleFixtureUsage()
        {
            LogInfo("Starting simple fixture demonstration");

            await docsPage.NavigateAsync();
            // Add assertions for docs page

            LogInfo("Simple fixture demonstration completed");
        }
    }
}