using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using PlaywrightDemo.Infrastructure.Base;
using PlaywrightDemo.Pages.API;
using PlaywrightDemo.Pages.UI;
using NUnit.Framework;
using PlaywrightDemo.Infrastructure.Auth;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using PlaywrightDemo.Infrastructure.Config.Models;

namespace PlaywrightDemo.Tests.UiTests;

[TestFixture]
[Category("UI")]
public class SampleTest : TestBase
{
    private GitHubDashboardPage _dashboardPage = null!;

    [SetUp]
    public override async Task BaseTestInitialize()
    {
        // AuthStateToUse = "AniketSelokar-CawTech_state.json";
        await base.BaseTestInitialize();
        _dashboardPage = new GitHubDashboardPage(Page);
        // log
        // _githubApi = new GitHubApiPage(ApiContext);
    }

    [Test]
    [Category("GitHubRepo")]
    public async Task CreateRepoAndVerifyInUI()
    {
        //     // First verify we can access an existing repository
        //     const string OWNER = "AniketSelokar-CawTech";
        //     const string EXISTING_REPO = "TestDemo2";

        //     Logger.LogInformation("Verifying access to existing repository");
        //     await _githubApi.VerifyRepositoryAccessAsync(OWNER, EXISTING_REPO);
        //     Logger.LogInformation("Successfully verified API access with existing repository");

        //     // Create a new repository using GitHubApiPage
        //     var repoName = $"test-repo-{Guid.NewGuid()}";
        //     var description = "Test repository created via Playwright";

        //     // Create repository via API and verify
        //     await _githubApi.CreateRepositoryAsync(repoName, description);
        //     await _githubApi.VerifyRepositoryCreatedAsync(repoName);
        //     Logger.LogInformation($"Repository '{repoName}' created successfully via API");

        //     // Verify repository in UI
        //     await _dashboardPage.NavigateToDashboardAsync();
        //     await _dashboardPage.ExpectRepoToBeVisibleAsync(repoName);
        //     Logger.LogInformation($"Repository '{repoName}' verified in UI");
        LogInfo("Navigating to homepage");
        await Page.GotoAsync(Settings.Environment.BaseUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        LogInfo("Homepage loaded successfully");

    }

    // [Test]
    // public async Task LoginAndSaveSessionStorage()
    // {
    //     // ... your existing UI login code ...

    //     // Assuming you have a page object for the login page
    //     // var loginPage = new LoginPage(Page);
    //     // await loginPage.LoginAsync("username", "password");

    //     // Save session storage after login
    //     await AuthHelper.SaveSessionStorageAsync(Page, ".auth/session.json");
    // }

    // [Test]
    // public async Task VerifyApiAccessWithSessionCookie()
    // {
    //     // 1. Log in via UI
    //     await Page.GotoAsync(Configuration["Environment:BaseUrl"] + "/login");
    //     await Page.FillAsync("input[name='login']", Configuration["GitHubUsername"]);
    //     await Page.FillAsync("input[name='password']", Configuration["GitHubPassword"]);
    //     await Page.ClickAsync("button[type='submit']");
    //     await Page.WaitForURLAsync(Configuration["Environment:BaseUrl"] + "/"); // Wait for login redirect

    //     // 2. Get the _gh_sess cookie
    //     var cookies = await Page.Context.CookiesAsync();
    //     var ghSessCookie = cookies.FirstOrDefault(c => c.Name == "_gh_sess");

    //     if (ghSessCookie == null)
    //     {
    //         Assert.Fail("Could not find _gh_sess cookie");
    //     }

    //     // 3. Call GetUserAsync, passing the cookie value
    //     var userResponse = await _githubApi.GetUserAsync(ghSessCookie.Value);

    //     // 4. Verify the response
    //     await _githubApi.ExpectResponseToBeSuccessfulAsync(userResponse);
    //     var userJson = await userResponse.JsonAsync<JsonElement>();
    //     var username = userJson.GetProperty("login").GetString();
    //     Assert.That(username, Is.EqualTo(Configuration["GitHubUsername"]), "Authenticated user mismatch");
    // }
}
