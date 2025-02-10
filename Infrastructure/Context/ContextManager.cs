using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using PlaywrightDemo.Infrastructure.Config.Models;
using PlaywrightDemo.Infrastructure.Fixtures;
using PlaywrightDemo.Pages.UI;
using PlaywrightDemo.Pages.API;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace PlaywrightDemo.Infrastructure.Context
{
    public class ContextOptions
    {
        public string? BaseUrl { get; set; }
        public string? ApiBaseUrl { get; set; }
        public string? AuthStateFile { get; set; }
        public BrowserSettings BrowserSettings { get; set; } = new();
        public Dictionary<string, Type> PageClasses { get; set; } = new();
        public Dictionary<string, Type> ApiClasses { get; set; } = new();
    }

    public class ContextResult
    {
        public IBrowserContext Context { get; set; } = null!;
        public IPage Page { get; set; } = null!;
        public IAPIRequestContext ApiContext { get; set; } = null!;
        public Dictionary<string, object> PageObjects { get; set; } = new();
        public Dictionary<string, object> ApiObjects { get; set; } = new();
    }

    public class ContextManager : IAsyncDisposable
    {
        private readonly IBrowser _browser;
        private readonly ILogger _logger;
        private readonly List<IBrowserContext> _contexts = new();
        private readonly List<IAPIRequestContext> _apiContexts = new();

        public ContextManager(IBrowser browser, ILogger logger)
        {
            _browser = browser;
            _logger = logger;
        }

        private string? GetStorageStatePath(string? authStateFile)
        {
            if (string.IsNullOrEmpty(authStateFile)) return null;

            var projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
            return Path.Combine(projectRoot, ".auth", authStateFile);
        }

        private object CreatePageInstance(Type pageType, IPage page)
        {
            return Activator.CreateInstance(pageType, page)
                ?? throw new InvalidOperationException($"Failed to create instance of {pageType.Name}");
        }

        private object CreateApiInstance(Type apiType, IAPIRequestContext apiContext)
        {
            return Activator.CreateInstance(apiType, apiContext)
                ?? throw new InvalidOperationException($"Failed to create instance of {apiType.Name}");
        }

        public async Task<ContextResult> CreateContextAsync(ContextOptions options)
        {
            var contextOptions = new BrowserNewContextOptions
            {
                BaseURL = options.BaseUrl,
                ViewportSize = new ViewportSize
                {
                    Width = options.BrowserSettings.Viewport.Width,
                    Height = options.BrowserSettings.Viewport.Height
                },
                StorageStatePath = GetStorageStatePath(options.AuthStateFile)
            };

            var context = await _browser.NewContextAsync(contextOptions);
            _contexts.Add(context);

            var page = await context.NewPageAsync();

            var playwright = await Playwright.CreateAsync();
            var apiContext = await playwright.APIRequest.NewContextAsync(new()
            {
                BaseURL = options.ApiBaseUrl,
                IgnoreHTTPSErrors = true,
                StorageStatePath = GetStorageStatePath(options.AuthStateFile)
            });
            _apiContexts.Add(apiContext);

            var pageObjects = new Dictionary<string, object>();
            foreach (var (key, pageType) in options.PageClasses)
            {
                pageObjects[key] = CreatePageInstance(pageType, page);
            }

            var apiObjects = new Dictionary<string, object>();
            foreach (var (key, apiType) in options.ApiClasses)
            {
                apiObjects[key] = CreateApiInstance(apiType, apiContext);
            }

            return new ContextResult
            {
                Context = context,
                Page = page,
                ApiContext = apiContext,
                PageObjects = pageObjects,
                ApiObjects = apiObjects
            };
        }

        public async ValueTask DisposeAsync()
        {
            foreach (var context in _contexts)
            {
                await context.DisposeAsync();
            }
            foreach (var apiContext in _apiContexts)
            {
                await apiContext.DisposeAsync();
            }
            _contexts.Clear();
            _apiContexts.Clear();
        }
    }
}