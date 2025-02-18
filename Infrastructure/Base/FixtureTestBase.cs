using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using NUnit.Framework;
using PlaywrightDemo.Infrastructure.Config;
using PlaywrightDemo.Infrastructure.Config.Models;
using PlaywrightDemo.Infrastructure.Context;
using PlaywrightDemo.Infrastructure.Fixtures;
using PlaywrightDemo.Infrastructure.Logging;
using PlaywrightDemo.Infrastructure.Reporting;
using PlaywrightDemo.Pages.UI;
using PlaywrightDemo.Pages.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AventStack.ExtentReports;

namespace PlaywrightDemo.Infrastructure.Base
{
    [TestFixture]
    public class FixtureTestBase : TestBase
    {
        protected ContextManager ContextManager { get; private set; } = null!;
        protected ContextResult CurrentContext { get; private set; } = null!;
        private readonly Dictionary<string, ITestFixture> _availableFixtures;

        public FixtureTestBase() : base()
        {
            // Initialize available fixtures
            _availableFixtures = InitializeAvailableFixtures();
        }

        private Dictionary<string, ITestFixture> InitializeAvailableFixtures()
        {
            return Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => !t.IsAbstract && typeof(ITestFixture).IsAssignableFrom(t))
                .Select(t => (ITestFixture)Activator.CreateInstance(t)!)
                .ToDictionary(f => f.FixtureKey);
        }

        [SetUp]
        public override async Task BaseTestInitialize()
        {
            await base.BaseTestInitialize();

            ContextManager = new ContextManager(Browser, Logger);

            // Initialize fixtures based on test method attributes
            await InitializeFixturesForCurrentTest();
        }

        private async Task InitializeFixturesForCurrentTest()
        {
            var testMethod = GetType().GetMethod(TestContext.CurrentContext.Test.Name);
            if (testMethod == null) return;

            var fixtureAttribute = testMethod.GetCustomAttribute<RequiredFixturesAttribute>();
            if (fixtureAttribute == null) return;

            var requiredFixtures = fixtureAttribute.Fixtures
                .Select(t => _availableFixtures.Values.FirstOrDefault(f => f.FixtureType == t))
                .Where(f => f != null)
                .ToDictionary(f => f!.FixtureKey, f => f!.FixtureType);

            var pageClasses = requiredFixtures
                .Where(f => f.Value.IsSubclassOf(typeof(BasePage)))
                .ToDictionary(f => f.Key, f => f.Value);

            var apiClasses = requiredFixtures
                .Where(f => f.Value.IsSubclassOf(typeof(BaseApiPage)))
                .ToDictionary(f => f.Key, f => f.Value);

            CurrentContext = await ContextManager.CreateContextAsync(new ContextOptions
            {
                BaseUrl = Settings.Environment.BaseUrl,
                ApiBaseUrl = Settings.Environment.ApiBaseUrl,
                BrowserSettings = Settings.Browser,
                PageClasses = pageClasses,
                ApiClasses = apiClasses
            });
        }

        public override async ValueTask DisposeAsync()
        {
            if (ContextManager != null)
            {
                await ContextManager.DisposeAsync();
            }
            await base.DisposeAsync();
        }

        // Helper method to get a strongly-typed page object
        protected T GetFixture<T>(string key) where T : class
        {
            if (CurrentContext?.PageObjects.TryGetValue(key, out var page) == true)
            {
                return (T)page;
            }
            if (CurrentContext?.ApiObjects.TryGetValue(key, out var api) == true)
            {
                return (T)api;
            }
            throw new KeyNotFoundException($"Fixture with key '{key}' not found");
        }

        // Properties for commonly used fixtures
        protected GitHubDashboardPage dashboardPage => GetFixture<GitHubDashboardPage>("dashboardPage");
        protected GitHubApiPage githubApi => GetFixture<GitHubApiPage>("githubApi");
        protected DocsPage docsPage => GetFixture<DocsPage>("docsPage");
    }
}