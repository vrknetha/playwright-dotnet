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
    public class FixtureTestBase : IAsyncDisposable
    {
        protected ILogger Logger { get; }
        protected TestSettings Settings { get; }
        protected ContextManager ContextManager { get; private set; } = null!;
        protected IBrowser Browser { get; private set; } = null!;
        protected ExtentTest TestReport { get; private set; } = null!;
        protected ContextResult CurrentContext { get; private set; } = null!;

        private readonly Dictionary<string, ITestFixture> _availableFixtures;

        public FixtureTestBase()
        {
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Information);
            });
            Logger = loggerFactory.CreateLogger<FixtureTestBase>();
            LoggerManager.SetLogger(Logger);

            ConfigurationLoader.Initialize(Logger);
            Settings = ConfigurationLoader.GetSettings<TestSettings>();

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

        [OneTimeSetUp]
        public static async Task AssemblyInitialize()
        {
            await TestReportManager.InitializeReporting();
        }

        [OneTimeTearDown]
        public static void AssemblyCleanup()
        {
            TestReportManager.FinalizeReporting();
        }

        [SetUp]
        public virtual async Task TestInitialize()
        {
            TestReport = TestReportManager.CreateTest(TestContext.CurrentContext.Test.Name);

            var playwright = await Playwright.CreateAsync();
            Browser = await playwright[Settings.Browser.Type].LaunchAsync(new()
            {
                Headless = Settings.Browser.Headless,
                SlowMo = Settings.Browser.SlowMo
            });

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

        [TearDown]
        public virtual async Task TestCleanup()
        {
            if (TestContext.CurrentContext.Result.Outcome.Status == NUnit.Framework.Interfaces.TestStatus.Failed)
            {
                var errorMessage = TestContext.CurrentContext.Result.Message;
                var stackTrace = TestContext.CurrentContext.Result.StackTrace;
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    TestReport?.Error($"Test Failed: {errorMessage}");
                    if (!string.IsNullOrEmpty(stackTrace))
                    {
                        TestReport?.Error($"Stack Trace: {stackTrace}");
                    }
                }
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (ContextManager != null)
            {
                await ContextManager.DisposeAsync();
            }
            if (Browser != null)
            {
                await Browser.DisposeAsync();
            }
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