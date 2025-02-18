using NUnit.Framework;
using PlaywrightDemo.Infrastructure.Base;
using PlaywrightDemo.Infrastructure.Context;
using PlaywrightDemo.Infrastructure.Fixtures;
using PlaywrightDemo.Pages.UI;
using PlaywrightDemo.Pages.API;
using System.Threading.Tasks;
using System.IO;
using System.Linq;

namespace PlaywrightDemo.Tests
{
    [TestFixture]
    [Parallelizable(ParallelScope.None)]
    public class AuthStateManagementTest : FixtureTestBase
    {
        private const string ExistingAuthState = "AniketSelokar-CawTech_state.json";
        private const string TestUsername = "AniketSelokar-CawTech";

        [Test]
        [Description("Demonstrates login with user from json")]
        [Category("Auth")]
        [RequiredFixtures(typeof(LoginPage))]
        public async Task ShouldLoginWithUserFromJson()
        {
            LogInfo($"Testing login with user: {TestUsername}");
            var loginPage = GetFixture<LoginPage>("loginPage");
            await loginPage.LoginWithCredentialsAsync(TestUsername, Settings.Auth.CommonPassword);
            await loginPage.ExpectLoginSuccessfulAsync();
        }

        [Test]
        [Description("Demonstrates using existing auth state")]
        [Category("Auth")]
        [RequiredFixtures(typeof(GitHubDashboardPage))]
        public async Task ShouldUseExistingAuthState()
        {
            LogInfo("Testing with existing auth state");
            AuthStateToUse = ExistingAuthState;
            await dashboardPage.GoToAsync();
            var isVisible = await dashboardPage.IsVisible();
            Assert.That(isVisible, Is.True, "Dashboard should be visible for authenticated user");
        }

        [Test]
        [Description("Demonstrates creating new context with existing auth state")]
        [Category("Auth")]
        [RequiredFixtures(typeof(GitHubDashboardPage))]
        public async Task ShouldCreateNewContextWithExistingAuthState()
        {
            LogInfo("Testing context creation with existing auth state");

            var contextResult = await ContextManager.CreateContextAsync(new ContextOptions
            {
                BaseUrl = Settings.Environment.BaseUrl,
                ApiBaseUrl = Settings.Environment.ApiBaseUrl,
                AuthStateFile = ExistingAuthState,
                BrowserSettings = Settings.Browser,
                PageClasses = new()
                {
                    { "dashboardPage", typeof(GitHubDashboardPage) }
                }
            });

            try
            {
                var authenticatedDashboard = (GitHubDashboardPage)contextResult.PageObjects["dashboardPage"];
                await authenticatedDashboard.GoToAsync();
                var isVisible = await authenticatedDashboard.IsVisible();
                Assert.That(isVisible, Is.True, "Dashboard should be visible in new context");
            }
            finally
            {
                await contextResult.Context.CloseAsync();
            }
        }

        // [Test]
        // [Description("Demonstrates auth state persistence across navigation")]
        // [Category("Auth")]
        // [RequiredFixtures(typeof(GitHubDashboardPage), typeof(DocsPage))]
        // public async Task ShouldMaintainAuthStateAcrossNavigation()
        // {
        //     // Use existing auth state
        //     AuthStateToUse = ExistingAuthState;

        //     // Navigate through multiple pages while maintaining auth state
        //     await dashboardPage.GoToAsync();
        //     var isDashboardVisible = await dashboardPage.IsVisible();
        //     Assert.That(isDashboardVisible, Is.True, "Dashboard should be visible");

        //     await docsPage.NavigateAsync();
        //     // Verify the auth state is maintained
        //     var authState = await Context.StorageStateAsync();
        //     Assert.That(authState, Is.Not.Null, "Auth state should be maintained");

        //     // Verify user is still logged in by checking cookies
        //     var cookies = authState.Cookies;
        //     var loggedInCookie = cookies.FirstOrDefault(c => c.Name == "logged_in");
        //     Assert.That(loggedInCookie?.Value, Is.EqualTo("yes"), "User should still be logged in");

        //     var usernameCookie = cookies.FirstOrDefault(c => c.Name == "dotcom_user");
        //     Assert.That(usernameCookie?.Value, Is.EqualTo("AniketSelokar-CawTech"), "Username should match the auth state");
        // }
    }
}