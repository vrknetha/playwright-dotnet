using PlaywrightDemo.Pages.UI;
using PlaywrightDemo.Pages.API;
using System;

namespace PlaywrightDemo.Infrastructure.Fixtures
{
    public class GitHubDashboardFixture : ITestFixture
    {
        public string FixtureKey => "dashboardPage";
        public Type FixtureType => typeof(GitHubDashboardPage);
    }

    public class GitHubApiFixture : ITestFixture
    {
        public string FixtureKey => "githubApi";
        public Type FixtureType => typeof(GitHubApiPage);
    }

    public class DocsPageFixture : ITestFixture
    {
        public string FixtureKey => "docsPage";
        public Type FixtureType => typeof(DocsPage);
    }

    // Add more fixtures as needed
}