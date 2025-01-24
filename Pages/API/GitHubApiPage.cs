using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using Microsoft.Extensions.Configuration;

namespace PlaywrightDemo.Pages.API;

public class GitHubApiPage : BaseApiPage
{
    private record CreateRepoRequest(
        string name,
        string? description = null,
        bool? @private = null,
        bool? auto_init = null,
        string? gitignore_template = null,
        string? license_template = null
    );

    private readonly Dictionary<string, string> _defaultHeaders;

    public GitHubApiPage(IAPIRequestContext apiContext) : base(apiContext)
    {
        Logger.LogInformation("Initializing GitHub API Page");
        _defaultHeaders = new Dictionary<string, string>
        {
            ["Accept"] = "application/vnd.github.v3+json",
        };
    }

    public async Task<IAPIResponse> GetUserProfileAsync()
    {
        try
        {
            Logger.LogInformation("Fetching user profile");
            var response = await ApiContext.GetAsync("/user", new APIRequestContextOptions
            {
                Headers = _defaultHeaders
            });
            await ExpectResponseToBeSuccessfulAsync(response);
            return response;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to fetch user profile");
            throw;
        }
    }

    public async Task<IAPIResponse> ListRepositoriesAsync()
    {
        try
        {
            Logger.LogInformation("Fetching repositories");
            var response = await ApiContext.GetAsync("/user/repos", new APIRequestContextOptions
            {
                Headers = _defaultHeaders
            });
            await ExpectResponseToBeSuccessfulAsync(response);
            await ExpectResponseToContainArrayAsync(response);
            return response;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to fetch repositories");
            throw;
        }
    }

    public async Task<IAPIResponse> CreateRepositoryAsync(string name, string? description = null, bool autoInit = true)
    {
        try
        {
            Logger.LogInformation("Creating repository: {Name}", name);

            var request = new CreateRepoRequest(
                name: name,
                description: description,
                auto_init: autoInit
            );

            var response = await ApiContext.PostAsync("/user/repos", new APIRequestContextOptions
            {
                DataObject = request,
                Headers = _defaultHeaders
            });

            // GitHub returns 201 Created for successful repository creation
            if (response.Status == 201)
            {
                Logger.LogInformation("Repository created successfully");
                return response;
            }

            // Check for specific error cases
            if (response.Status == 422)
            {
                var error = await response.JsonAsync<JsonElement>();
                var message = error.GetProperty("message").GetString();
                throw new Exception($"Repository creation failed: {message}");
            }

            await ExpectResponseToBeSuccessfulAsync(response);
            return response;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to create repository: {Name}", name);
            throw;
        }
    }

    public async Task<IAPIResponse> GetRepositoryAsync(string owner, string repoName)
    {
        try
        {
            Logger.LogInformation("Fetching repository: {Owner}/{Name}", owner, repoName);
            var response = await ApiContext.GetAsync($"/repos/{owner}/{repoName}", new APIRequestContextOptions
            {
                Headers = _defaultHeaders
            });

            if (response.Status == 404)
            {
                throw new Exception($"Repository not found: {owner}/{repoName}");
            }

            await ExpectResponseToBeSuccessfulAsync(response);
            return response;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to fetch repository: {Owner}/{Name}", owner, repoName);
            throw;
        }
    }

    public async Task VerifyRepositoryAccessAsync(string owner, string repoName)
    {
        var response = await GetRepositoryAsync(owner, repoName);
        var json = await response.JsonAsync<JsonElement>();

        // Verify essential properties
        var actualOwner = json.GetProperty("owner").GetProperty("login").GetString();
        var actualName = json.GetProperty("name").GetString();

        Assert.That(actualOwner, Is.EqualTo(owner), "Repository owner mismatch");
        Assert.That(actualName, Is.EqualTo(repoName), "Repository name mismatch");

        Logger.LogInformation("Successfully verified access to repository: {Owner}/{Name}", owner, repoName);
    }

    public async Task VerifyRepositoryExistsAsync(string repoName)
    {
        var response = await ListRepositoriesAsync();
        var json = await response.JsonAsync<JsonElement>();
        var exists = json.EnumerateArray()
            .Any(r => r.GetProperty("name").GetString() == repoName);
        Assert.That(exists, Is.True, $"Repository '{repoName}' was not found");
    }

    public async Task VerifyRepositoryCreatedAsync(string repoName)
    {
        await VerifyRepositoryExistsAsync(repoName);

        // Get user info to verify repository ownership
        var userResponse = await GetUserProfileAsync();
        var userJson = await userResponse.JsonAsync<JsonElement>();
        var username = userJson.GetProperty("login").GetString();

        // Additional verification of repository settings
        var response = await ApiContext.GetAsync($"/repos/{username}/{repoName}", new APIRequestContextOptions
        {
            Headers = _defaultHeaders
        });
        await ExpectResponseToBeSuccessfulAsync(response);
        await ExpectResponseToContainKeyValueAsync(response, "name", repoName);
    }

    // public async Task<IAPIResponse> GetUserAsync(string ghSessCookieValue)
    // {
    //     // Add the _gh_sess cookie to the existing ApiContext
    //     await AddExtraHeadersToApiContextAsync(new Dictionary<string, string>
    //     {
    //         { "Cookie", $"_gh_sess={ghSessCookieValue}" }
    //     });

    //     // Use the existing ApiContext to make the API call
    //     var userResponse = await ApiContext.GetAsync("/user");
    //     return userResponse;
    // }
}