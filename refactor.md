refactor.md:

# Refactoring: Playwright .NET Test Automation Framework

This document outlines the steps to refactor the Playwright .NET test automation framework to achieve the following goals:

1. **Universal Playwright Assertions:** Use Playwright's `Expect` assertions for both UI and API tests, where applicable.
2. **Refine Page Object Model:**
    *   Reorganize page objects into `UI` and `API` subfolders under `Pages`.
    *   Create a `BaseApiPage` class for common API-related logic and assertions.
    *   Ensure `GitHubDashboardPage` inherits from `BasePage` and receives the `IPage` instance from `TestBase`.
    *   Ensure `GitHubApiPage` inherits from `BaseApiPage` and uses Playwright assertions for API responses.
3. **Simplify `SampleTest.cs`:**
    *   Assume session storage is pre-generated, removing explicit login logic from the test.
    *   Focus the test on creating a repository via API and verifying it in the UI.
4. **Move existing `BaseApiObject.cs` into `Pages/Api/` folder:**
     *    Rename it as `BaseApiPage.cs` and update the namespace, usings and references accordingly.
5. **Remove Legacy Code:**
    *   Remove the obsolete `User` class from the `Pages` directory.
    *   Remove `MultiUserLoginTest` from the `SampleTest` class.

## Prerequisites

*   The Azure DevOps pipeline's `GenerateSessionStorage` stage successfully generates session storage files in `TestResults/Reports/.auth/`.

## Constraints

*   Do not use `Assert.That` or any other assertion library besides Playwright's `Expect` within the test methods.
*   Do not add error handling within the tests for potentially invalid auth states. Assume the session storage is valid.

## Refactoring Steps

### Step 1: Restructure Folders and Files

1. **Create Directories:**

    *   Create a new directory: `Pages/UI`
    *   Create a new directory: `Pages/API`
    *   Create a new directory: `Tests/ApiTests`
    *   Create a new directory: `Tests/UiTests`

2. **Move Files:**

    *   Move `Pages/GitHubApiPage.cs` to `Pages/API/GitHubApiPage.cs`.
    *   Move the `Infrastructure/API/BaseApiObject.cs` to `Pages/API/BaseApiPage.cs`.
    *   Move `Tests/GitHubApiTests.cs` to `Tests/ApiTests/GitHubApiTests.cs`.
    *   Move `Tests/SampleTest.cs` to `Tests/UiTests/SampleTest.cs`.

3. **Rename File and Class:**

    *   Rename `Infrastructure/API/BaseApiObject.cs` to `Infrastructure/API/BaseApiPage.cs`.
    *   Rename the class `BaseApiObject` to `BaseApiPage` within the `BaseApiPage.cs` file.

### Step 2: Update `BaseApiPage.cs`

1. **File Location:** `Pages/API/BaseApiPage.cs`
2. **Namespace:**

    *   Change the namespace to:

        ```csharp
        namespace PlaywrightDemo.Pages.API;
        ```
3. **Inheritance:**
     * Make `BaseApiPage` inherit from `PageTest`.

        ```csharp
        public abstract class BaseApiPage : PageTest
        ```

4. **Add using statement:**

    *   Add `using Microsoft.Playwright.NUnit;` to access `PageTest` and Playwright assertion methods.
    *   Add `using System.Text.Json;`
5. **Constructor:**
    *   Ensure the constructor initializes the base class with `ApiContext`

```csharp
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using System.Text.Json;
using PlaywrightDemo.Infrastructure.Config;
using PlaywrightDemo.Infrastructure.Config.Models;
using PlaywrightDemo.Infrastructure.Logging;

namespace PlaywrightDemo.Pages.API;

public abstract class BaseApiPage : PageTest
{
    protected readonly IAPIRequestContext ApiContext;
    protected readonly ILogger Logger;
    protected readonly TestSettings Settings;

    protected BaseApiPage(IAPIRequestContext apiContext)
    {
        ApiContext = apiContext;
        Logger = LoggerManager.Current;
        Settings = ConfigurationLoader.GetSettings<TestSettings>();
    }
    
    // Reusable assertion method
    public async Task ExpectResponseToContainKeyValueAsync(IAPIResponse response, string key, string value)
    {
        var json = await response.JsonAsync<JsonElement>();
        try
        {
            var propertyValue = json.GetProperty(key).GetString();
            // Using NUnit's Assert.That for string comparison
            Assert.That(propertyValue, Is.EqualTo(value), $"Expected JSON response to have key '{key}' with value '{value}', but found '{propertyValue}'");
        }
        catch (KeyNotFoundException)
        {
            Assert.Fail($"Key '{key}' not found in the JSON response.");
        }
        catch (InvalidOperationException)
        {
            Assert.Fail($"Property '{key}' in JSON response is not a string.");
        }
    }
    public async Task ExpectResponseToHaveJsonContentAsync(IAPIResponse response)
    {
        var json = await response.JsonAsync();
        // Here you might want to add a specific check, for now just ensuring it can parse JSON is a basic check
        Assert.That(json, Is.Not.Null, "Response does not contain valid JSON content.");
    }
}
content_copy
download
Use code with caution.
Markdown
Step 3: Update GitHubApiPage.cs

File Location: Pages/API/GitHubApiPage.cs

Namespace:

Change the namespace to:

namespace PlaywrightDemo.Pages.API;
content_copy
download
Use code with caution.
C#

Inheritance:

Make GitHubApiPage inherit from BaseApiPage.

using Directives:

Remove: using PlaywrightDemo.Infrastructure.API;

Add: using Microsoft.Playwright.NUnit;

Add: using System.Threading.Tasks;

using System.Text.Json;
using PlaywrightDemo.Infrastructure.API;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using System.Threading.Tasks;

namespace PlaywrightDemo.Pages.API;
content_copy
download
Use code with caution.
C#

Remove existing assertion helper methods:
* Remove the existing assertion helper methods.

Update Existing Methods:

Modify GetUserProfileAsync, ListRepositoriesAsync, and CreateRepositoryAsync so that they only return Task<IAPIResponse> and do not parse JSON or perform assertions within these methods. Ensure these methods use the ApiContext for making API requests and use await Expect(response).ToBeOKAsync(); for basic response validation.

Create new methods for Verification:

Add new methods like VerifyUserProfileAsync, VerifyRepositoryExistsAsync, and VerifyRepositoryCreatedAsync that will utilize the assertion capabilities from BaseApiPage.

Step 4: Create GitHubDashboardPage.cs

File Location: Pages/UI/GitHubDashboardPage.cs

Create the file with the following content:

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

    // Reusable assertion method (using Playwright's Expect)
    public async Task ExpectRepoToBeVisibleAsync(string repoName)
    {
        Logger.LogInformation("Checking if repository '{RepoName}' is visible on the dashboard", repoName);
        await Expect(RepoLink(repoName)).ToBeVisibleAsync();
    }

    // ... other methods for interacting with the dashboard ...
}
content_copy
download
Use code with caution.
C#
Step 5: Refactor SampleTest.cs

File Location: Tests/UiTests/SampleTest.cs

Update using Directives:

Remove any unnecessary using directives.

Add: using PlaywrightDemo.Pages.UI;

Add: using PlaywrightDemo.Pages.API;

Remove unnecessary test:
* Remove the existing LoginTest and UseSessionStorageTest methods.

Refactor BaseTestInitialize:

Modify BaseTestInitialize to initialize GitHubDashboardPage and GitHubApiPage objects.

[SetUp]
 public override async Task BaseTestInitialize()
 {
     await base.BaseTestInitialize();
     _dashboardPage = new GitHubDashboardPage(Page);
     _githubApi = new GitHubApiPage(ApiContext);
 }
content_copy
download
Use code with caution.
C#

Update CreateRepoAndVerifyInUI Test Method:

Make the test method perform the following actions:

Assume that the user is already logged in (session storage is applied in TestBase).

Use _githubApi.CreateRepositoryAsync to create a new repository via the API with a unique name.

Use _githubApi.Verify... methods to assert the successful creation of repo

Use _dashboardPage.NavigateToDashboardAsync (or similar method) to navigate to the GitHub dashboard.

Use _dashboardPage.ExpectRepoToBeVisibleAsync to verify that the newly created repository is visible in the UI.

[Test]
    [Category("GitHubRepo")]
    public async Task CreateRepoAndVerifyInUI()
    {
         // API: Create a new repository using GitHubApiPage
        var repoName = $"TestRepo-{Guid.NewGuid()}";
        var createRepoData = new
        {
            name = repoName,
            description = "Test Repo created by Playwright",
            auto_init = true // Add this to avoid 409
        };
         await _githubApi.VerifyRepositoryCreatedAsync(repoName);

        // UI: Navigate to the dashboard and verify the repo's existence
         await Page.GotoAsync($"{Settings.Environment.BaseUrl}"); // Go to dashboard
         await _dashboardPage.ExpectRepoToBeVisibleAsync(repoName);

    }
content_copy
download
Use code with caution.
C#

Refactor TestBase class

Remove Authhelper Initialisation code from class constructor

Move that code to BaseTestIntialise method, just after where var contextOptions created

[SetUp]
 public virtual async Task BaseTestInitialize()
 {
     // existing code 
     // Apply auth state if specified
        if (!string.IsNullOrEmpty(AuthStateToUse))
        {
            try
            {
                var authStatePath = Path.Combine(GetAuthStatePath(), AuthStateToUse);
                AuthHelper = new AuthHelper();

                if (await AuthHelper.VerifyAuthStateAsync(authStatePath))
                {
                    LogInfo($"Using authentication state: {AuthStateToUse}");
                    contextOptions.StorageStatePath = authStatePath;
                }
                else
                {
                    LogWarning($"Authentication state is invalid or expired: {AuthStateToUse}");
                }
            }
            catch (Exception ex)
            {
                LogError($"Failed to apply authentication state: {AuthStateToUse}", ex);
            }
        }

     // rest of the existing code
 }
 public TestBase()
 {
     // Initialize logger
     var loggerFactory = LoggerFactory.Create(builder =>
     {
         builder.AddConsole();
         builder.SetMinimumLevel(LogLevel.Information);
     });
     Logger = loggerFactory.CreateLogger<TestBase>();
     LoggerManager.SetLogger(Logger);

     // Initialize settings
     ConfigurationLoader.Initialize(Logger);
     Settings = ConfigurationLoader.GetSettings<TestSettings>();

     // Initialize UserDataHelper
     UserDataHelper = new UserDataGenerator();
 }
content_copy
download
Use code with caution.
C#
Step 6: Remove Pages/User.cs

Delete the file Pages/User.cs as it is no longer needed.

Update LoginPage class, replace User class reference from Pages to Infrastructure/TestData/Models, for this update using statement of User model

Replace hard coded username with User model object reference everywhere in the project.

Step 7: Clean Up and Review

Remove Unused Code: Go through the codebase and remove any unused methods or code that is no longer needed after the refactoring.

Review: Carefully review all the changes to make sure the code is well-structured, readable, and follows the intended design.

Update Namespaces: Ensure that the namespaces in all moved files are updated correctly to reflect their new locations.

Update using Directives: Update using directives in all files to reflect the moved files and the use of Playwright assertions.

Comments and Documentation: Update comments and documentation as needed to reflect the changes made during refactoring.

Step 8: Build and Test

Clean: Run dotnet clean in the root directory of the project.

Restore: Run dotnet restore to restore packages.

Build: Run dotnet build to build the solution and check for any compilation errors.

Test: Run dotnet test to execute the tests and ensure that they pass.

This refactor.md provides very specific instructions for the AI agent, which should help it perform the refactoring accurately and efficiently. I have created this file by keeping in mind our conversation history.