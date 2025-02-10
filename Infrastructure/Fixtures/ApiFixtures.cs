using Microsoft.Playwright;
using PlaywrightDemo.Pages.API;
using System.Threading.Tasks;

namespace PlaywrightDemo.Infrastructure.Fixtures
{
    public class ApiFixtures
    {
        private readonly IAPIRequestContext _apiContext;

        public ApiFixtures(IAPIRequestContext apiContext)
        {
            _apiContext = apiContext;
        }

        private GitHubApiPage? _gitHubApiPage;
        public GitHubApiPage GitHubApiPage => _gitHubApiPage ??= new GitHubApiPage(_apiContext);

        // Add other API clients as needed with lazy initialization
    }
}