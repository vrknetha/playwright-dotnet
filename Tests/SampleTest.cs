using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using PlaywrightDemo.Infrastructure.Base;
using PlaywrightDemo.Infrastructure.Reporting;
using PlaywrightDemo.Infrastructure.API;
using PlaywrightDemo.Pages;
using PlaywrightDemo.Infrastructure.Config;
using NUnit.Framework;
using Microsoft.Playwright.NUnit;
namespace PlaywrightDemo.Tests;

[TestFixture]
[Category("UI")]
public class SampleTest : TestBase
{
    private LoginPage _loginPage = null!;

    [SetUp]
    public override async Task BaseTestInitialize()
    {
        AuthStateToUse = "AniketSelokar-CawTech_state.json";
        await base.BaseTestInitialize();
        _loginPage = new LoginPage(Page);

        // Load session storage if it exists
    }

    [Test]
    [Category("Login")]
    public async Task LoginTest()
    {
        var user = new User("AniketSelokar-CawTech", "Aniket@0464");
        await _loginPage.LoginAsync(user);
        // Add assertions to verify successful login
    }

    [Test]
    [Category("Login")]
    public async Task MultiUserLoginTest()
    {
        var users = new[]
        {
            new User("Username", "Password"),
        };

        foreach (var user in users)
        {
            await _loginPage.LoginAsync(user);
            // Add assertions or further actions as needed
        }
    }

    [Test]
    [Category("SessionStorage")]
    public async Task UseSessionStorageTest()
    {
        // Now you can navigate to a page that requires the user to be logged in
        await _loginPage.NavitageToDashBoard(); // Example URL that requires login

        // Add assertions to verify that the user is logged in
        await _loginPage.AssertLoginSuccessfulAsync();
    }


}


// [TestFixture]
// [Category("UI")]
// public class GitHubTests : PageTest
// {
//     [SetUp]
//     public async Task SetupAsync()
//     {
//         // No need to create a new Playwright instance here
//         // _playwright = await Playwright.CreateAsync(); // Remove this line
//     }

//     [TearDown]
//     public async Task TeardownAsync()
//     {
//         // No need to dispose of Playwright here
//         // await _playwright?.Dispose(); // Remove this line
//     }

//     [Test]
//     public async Task LoginAndStoreSessionAsync()
//     {
//         // Create a new browser instance
//         var browser = await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
//         {
//             Headless = false // Set to false if you want to see the browser
//         });

//         // Create a new context from the browser
//         var context = await browser.NewContextAsync();

//         var page = await context.NewPageAsync();

//         await page.GotoAsync("https://github.com/login");
//         await page.FillAsync("input[name='login']", "AniketSelokar-CawTech");
//         await page.FillAsync("input[name='password']", "Aniket@0464");
//         await page.GetByRole(AriaRole.Button, new() { Name = "Sign in" }).First.ClickAsync();

//         // Wait for successful login (replace with appropriate selector)
//         await Expect(page.GetByText("Dashboard").First).ToBeAttachedAsync();


//         // Store session state correctly
//         await context.StorageStateAsync(new BrowserContextStorageStateOptions
//         {
//             Path = "C:\\Users\\caw_qa\\Documents\\Projects\\lastest-modification\\playwright-dotnet\\playwright\\.auth\\github_session.json" // Specify the path to save the session state
//         });

//         await context.CloseAsync(); // Close the context instead of the browser
//         await browser.CloseAsync(); // Close the browser after the test
//     }

//     [Test]
//     public async Task RunTestsWithStoredSessionAsync()
//     {
//         // Create a new browser instance
//         var browser = await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
//         {
//             Headless = false // Set to false if you want to see the browser
//         });

//         // Create a new context from the browser
//         var context = await browser.NewContextAsync(new BrowserNewContextOptions
//         {
//             StorageStatePath = "C:\\Users\\caw_qa\\Documents\\Projects\\lastest-modification\\playwright-dotnet\\playwright\\.auth\\github_session.json"
//         });

//         var page = await context.NewPageAsync();

//         await page.GotoAsync("https://github.com");
//         // ... other test actions ... 

//         await context.CloseAsync(); // Close the context instead of the browser
//         await browser.CloseAsync(); // Close the browser after the test
//     }
// }
