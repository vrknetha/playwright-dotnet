using Microsoft.Playwright;
using PlaywrightDemo.Pages.UI;
using System.Threading.Tasks;

namespace PlaywrightDemo.Infrastructure.Fixtures
{
    public class PageFixtures
    {
        private readonly IPage _page;

        public PageFixtures(IPage page)
        {
            _page = page;
        }

        private DocsPage? _docsPage;
        public DocsPage DocsPage => _docsPage ??= new DocsPage(_page);

        private GitHubDashboardPage? _gitHubDashboardPage;
        public GitHubDashboardPage GitHubDashboardPage => _gitHubDashboardPage ??= new GitHubDashboardPage(_page);

        // Add other page objects as needed with lazy initialization
    }
}