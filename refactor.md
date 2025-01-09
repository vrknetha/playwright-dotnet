Okay, here's a detailed, step-by-step refactoring guide specifically tailored for the Cursor IDE, designed to be directly usable within its environment.

Instructions for Cursor IDE: Refactoring to NUnit and Framework Enhancements

This guide outlines the steps to refactor the existing Playwright/MSTest framework to use NUnit and improve its core components.

Phase 1: NUnit Conversion and Project Setup

Step 1: Install NUnit Packages

Cursor Action: Open the integrated terminal in Cursor.

Commands:

dotnet add package NUnit
dotnet add package NUnit3TestAdapter
dotnet add package Microsoft.NET.Test.Sdk

Bash

Step 2: Remove MSTest References

Cursor Action: Open the .csproj file in the editor.

Actions:

Delete the MSTestSettings.cs file from the project.

Remove the following lines from the .csproj file (use Cursor's multi-cursor editing or find and replace for efficiency):

<PackageReference Include="MSTest.TestAdapter" Version="..." />
<PackageReference Include="MSTest.TestFramework" Version="..." />

Xml

Cursor Action: Open all code files (use Cursor's project-wide search - Ctrl+Shift+F or Cmd+Shift+F).

Action: Replace all instances of using Microsoft.VisualStudio.TestTools.UnitTesting; with using NUnit.Framework; (use Cursor's project-wide find and replace).

Step 3: Update Project File (.csproj)

Cursor Action: Open the .csproj file.

Actions:

Ensure the TargetFramework is set to a compatible .NET version (e.g., net8.0):

<TargetFramework>net8.0</TargetFramework>

Xml
*   Verify the following package references are present (add them if they are missing):

<PackageReference Include="ExtentReports" Version="4.1.0" />
<PackageReference Include="Bogus" Version="35.0.1" />
<PackageReference Include="Microsoft.Extensions.Configuration" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.Configuration.Binder" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.Configuration.EnvironmentVariables" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.Logging" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.Logging.Console" Version="8.0.0" />
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
<PackageReference Include="Microsoft.Playwright" Version="1.40.0" />
<PackageReference Include="NUnit" Version="3.13.3" />
<PackageReference Include="NUnit3TestAdapter" Version="4.5.0" />

Xml
*   (Optional) Add `InternalsVisibleTo` if you need to access internal members from your test project:

<ItemGroup>
    <InternalsVisibleTo Include="YourProjectName.Tests" />
</ItemGroup>

Xml

Step 4: Update Namespace and Usings

Cursor Action: Use Cursor's project-wide search (Ctrl+Shift+F or Cmd+Shift+F) to find all .cs files.

Actions:

Replace using Microsoft.VisualStudio.TestTools.UnitTesting; with using NUnit.Framework;.

Adjust namespaces if you want to follow a different convention (e.g., YourProjectName.Tests, YourProjectName.Pages, etc.). Use Cursor's refactoring features to rename namespaces safely.

Step 5: Convert Attributes

Cursor Action: Use project-wide find and replace (Ctrl+Shift+H or Cmd+Shift+H) with regular expressions to efficiently replace attributes.

Actions:

MSTest	NUnit	Regex Find	Regex Replace
[TestClass]	[TestFixture]	\[TestClass]	[TestFixture]
[TestMethod]	[Test]	\[TestMethod]	[Test]
[TestInitialize]	[SetUp]	\[TestInitialize]	[SetUp]
[TestCleanup]	[TearDown]	\[TestCleanup]	[TearDown]
[ClassInitialize]	[OneTimeSetUp]	\[ClassInitialize]	[OneTimeSetUp]
[ClassCleanup]	[OneTimeTearDown]	\[ClassCleanup]	[OneTimeTearDown]
[DataTestMethod]	[TestCase] or [Test, TestCaseSource]	\[DataTestMethod]	[TestCase]
[ExpectedException]	Assert.Throws<>() or Assert.ThrowsAsync<>()	\[ExpectedException]	Assert.Throws<>()

Step 6: Update Assertions (Gradual)

Cursor Action: You can gradually replace MSTest assertions with NUnit assertions as you work on individual test files.

Action: Use Cursor's code completion and suggestions to help you use NUnit.Framework.Assert methods.

Step 7: Create AssemblyInfo.cs (Optional)

Cursor Action: Right-click on your test project in the Solution Explorer and select "Add" > "New Item...". Choose "Class" and name it AssemblyInfo.cs.

Action: Add the following code to AssemblyInfo.cs for parallel execution:

using NUnit.Framework;

[assembly: Parallelizable(ParallelScope.Fixtures)] // Run test fixtures in parallel
[assembly: LevelOfParallelism(4)] // Optional: Set the number of worker threads (adjust as needed)

C#

Phase 2: Core Framework Enhancements

Step 8: Refactor ConfigurationLoader

Cursor Action: Open Infrastructure/Config/ConfigurationLoader.cs.

Actions:

Replace the existing code with the following updated version that uses IConfiguration directly and has improved error handling:

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace ParkPlaceSample.Infrastructure.Config;

public static class ConfigurationLoader
{
    private static IConfiguration? _configuration;
    private static ILogger? _logger;

    public static IConfiguration Configuration
    {
        get
        {
            if (_configuration == null)
            {
                LoadConfiguration();
            }
            return _configuration!;
        }
    }

    public static void Initialize(ILogger logger)
    {
        _logger = logger;
        LoadConfiguration();
        LogConfigurationInfo();
    }

    private static void LoadConfiguration()
    {
        var environment = GetEnvironment();
        var projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
        var configPaths = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "appsettings.json"),
            Path.Combine(projectRoot, "appsettings.json")
        };

        var configPath = configPaths.FirstOrDefault(File.Exists);
        if (configPath == null)
        {
            _logger?.LogError("Could not find appsettings.json in any of the expected locations.");
            throw new FileNotFoundException("Could not find appsettings.json in any of the expected locations.");
        }

        _logger?.LogInformation("Using configuration file: {ConfigPath}", configPath);

        _configuration = new ConfigurationBuilder()
            .SetBasePath(Path.GetDirectoryName(configPath)!)
            .AddJsonFile(Path.GetFileName(configPath), optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        ValidateConfiguration();
    }

    private static void ValidateConfiguration()
    {
        // Example validation: Check for required settings
        if (string.IsNullOrEmpty(Configuration["Environment:BaseUrl"]))
        {
            _logger?.LogError("Environment:BaseUrl is missing in the configuration.");
            throw new InvalidOperationException("Environment:BaseUrl is missing in the configuration.");
        }

        // Add more validation as needed...
    }

    private static string GetEnvironment()
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            ?? Environment.GetEnvironmentVariable("TEST_ENVIRONMENT")
            ?? "Development";

        _logger?.LogInformation("Using environment: {Environment}", environment);
        return environment;
    }

    private static void LogConfigurationInfo()
    {
        if (_logger == null) return;

        _logger.LogInformation("Configuration loaded successfully:");
        _logger.LogInformation("Environment: {Name}", Configuration["Environment:Name"]);
        _logger.LogInformation("Base URL: {BaseUrl}", Configuration["Environment:BaseUrl"]);
        // ... log other important settings
    }

    // Helper method to get settings
    public static T GetSettings<T>() where T : new()
    {
        var settings = new T();
        Configuration.Bind(settings);
        return settings;
    }
}

C#

Step 9: Create TestLoggerProvider

Cursor Action: Create a new class in the Infrastructure/Logging folder named TestLoggerProvider.cs.

Action: Implement the ILoggerProvider interface to create a logger that writes to the NUnit TestContext and potentially to a file:

using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace ParkPlaceSample.Infrastructure.Logging;

public class TestLogger : ILogger
{
    private readonly string _categoryName;

    public TestLogger(string categoryName)
    {
        _categoryName = categoryName;
    }

    public IDisposable BeginScope<TState>(TState state) => default!;

    public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        var message = formatter(state, exception);
        var formattedMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{logLevel}] [{_categoryName}] {message}";

        TestContext.WriteLine(formattedMessage);

        if (exception != null)
        {
            TestContext.WriteLine(exception.ToString());
        }
    }
}

public class TestLoggerProvider : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName)
    {
        return new TestLogger(categoryName);
    }

    public void Dispose() { }
}

public static class TestLoggerExtensions
{
    public static ILoggingBuilder AddTestLogger(this ILoggingBuilder builder)
    {
        builder.AddProvider(new TestLoggerProvider());
        return builder;
    }
}

C#

Step 10: Refactor BaseTest

Cursor Action: Open Infrastructure/Base/TestBase.cs.

Actions:

Convert to NUnit attributes ([TestFixture], [OneTimeSetUp], [OneTimeTearDown], [SetUp], [TearDown]).

Use the new TestLoggerProvider.

Initialize IConfiguration using the ConfigurationLoader.

Implement the shared context logic (authentication and storage state generation - you'll create the AuthHelper in a later step).

Refactor the video recording and cleanup logic to be compatible with NUnit.

using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using NUnit.Framework;
using ParkPlaceSample.Infrastructure.Config;
using ParkPlaceSample.Infrastructure.Reporting;
using System.Text;
using System.Web;
using Microsoft.Extensions.Configuration;

namespace ParkPlaceSample.Infrastructure.Base;

[TestFixture]
public class TestBase
{
    protected IBrowserContext Context { get; private set; } = null!;
    protected IBrowser Browser { get; private set; } = null!;
    protected ILogger Logger { get; private set; } = null!;
    protected IPage Page { get; private set; } = null!;
    protected IAPIRequestContext ApiContext { get; private set; } = null!;
    private IPlaywright _playwright = null!;
    private DateTime _testStartTime;
    protected TestSettings Settings => ConfigurationLoader.GetSettings<TestSettings>();

    [OneTimeSetUp]
    public async Task AssemblyInitialize()
    {
        // Initialize Reporting
        await TestReportManager.InitializeReporting();

        // Initialize Logger
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            builder.AddTestLogger();
            builder.SetMinimumLevel(LogLevel.Debug);
        });
        Logger = loggerFactory.CreateLogger(GetType());

        // Initialize Configuration
        ConfigurationLoader.Initialize(Logger);
    }

    [OneTimeTearDown]
    public void AssemblyCleanup()
    {
        TestReportManager.FinalizeReporting();
    }

    [SetUp]
    public virtual async Task BaseTestInitialize()
    {
        _testStartTime = DateTime.Now;

        // Initialize Playwright
        _playwright = await Playwright.CreateAsync();
        Browser = await _playwright.Chromium.LaunchAsync(new()
        {
            Headless = Settings.Browser.Headless,
            SlowMo = Settings.Browser.SlowMo
        });

        // Create test results directory structure
        var projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
        var testResultsRoot = Path.Combine(projectRoot, "TestResults");
        var reportsDir = Path.Combine(testResultsRoot, "Reports");
        var videosDir = Path.Combine(reportsDir, "Videos");
        var logsDir = Path.Combine(reportsDir, "Logs");
        var tracesDir = Path.Combine(reportsDir, "Traces");

        Directory.CreateDirectory(videosDir);
        Directory.CreateDirectory(logsDir);
        Directory.CreateDirectory(tracesDir);

        // Initialize Authentication and get storage state
        // Assuming you have an AuthHelper class. Replace with your actual authentication logic
        var authHelper = new AuthHelper(Logger, ConfigurationLoader.Configuration);
        var storageState = await authHelper.GenerateAuthStateAsync("your_username", "your_password"); // Provide credentials or generate them

        // Create BrowserContext with storage state
        Context = await Browser.NewContextAsync(new()
        {
            ViewportSize = new ViewportSize
            {
                Width = Settings.Browser.Viewport.Width,
                Height = Settings.Browser.Viewport.Height
            },
            RecordVideoDir = videosDir,
            RecordVideoSize = new RecordVideoSize
            {
                Width = Settings.Browser.Viewport.Width,
                Height = Settings.Browser.Viewport.Height
            },
            StorageState = storageState // Set the storage state here
        });

        // Initialize APIRequestContext
        ApiContext = await _playwright.APIRequest.NewContextAsync(new()
        {
            BaseURL = Settings.Environment.ApiBaseUrl,
            ExtraHTTPHeaders = new Dictionary<string, string>
            {
                { "Accept", "application/json" },
                { "Authorization", $"Bearer {Environment.GetEnvironmentVariable("API_TOKEN")}" }
            }
        });

        // Create a new page for the test
        Page = await Context.NewPageAsync();
    }

    [TearDown]
    public virtual async Task BaseTestCleanup()
    {
        var testFailed = TestContext.CurrentContext.Result.Outcome.Status == NUnit.Framework.Interfaces.TestStatus.Failed;
        var duration = DateTime.Now - _testStartTime;

        try
        {
            LogInfo($"Test completed with status: {(testFailed ? "Failed" : "Passed")}");

            // Create test results directory structure
            var projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
            var testResultsRoot = Path.Combine(projectRoot, "TestResults");
            var reportsDir = Path.Combine(testResultsRoot, "Reports");
            var logsDir = Path.Combine(reportsDir, "Logs");
            Directory.CreateDirectory(logsDir);

            // Handle video recording
            if (Page != null)
            {
                var video = Page.Video;
                if (video != null)
                {
                    var videoPath = await video.PathAsync();
                    if (!string.IsNullOrEmpty(videoPath))
                    {
                        var fileName = $"{TestContext.CurrentContext.Test.Name}_{DateTime.Now:yyyyMMdd_HHmmss}.webm";
                        var destinationPath = Path.Combine(Path.GetDirectoryName(videoPath)!, fileName);
                        await video.SaveAsAsync(destinationPath);
                    }
                }
            }

            // Save and attach test logs
            var testOutput = GetTestLogContent();
            var logFileName = $"{TestContext.CurrentContext.Test.Name}_{DateTime.Now:yyyyMMdd_HHmmss}.log";
            var logPath = Path.Combine(logsDir, logFileName);
            await File.WriteAllTextAsync(logPath, testOutput);

            if (testFailed)
            {
                LogError("Test failed");
            }
            else
            {
                LogInfo("Test passed");
            }
        }
        catch (Exception ex)
        {
            LogError("Error during test cleanup", ex);
        }
        finally
        {
            try
            {
                // Cleanup resources in the correct order
                if (Page != null)
                {
                    await Page.CloseAsync();
                    Page = null!;
                }

                if (Context != null)
                {
                    await Context.CloseAsync();
                    await Context.DisposeAsync();
                    Context = null!;
                }

                if (Browser != null)
                {
                    await Browser.CloseAsync();
                    await Browser.DisposeAsync();
                    Browser = null!;
                }

                if (ApiContext != null)
                {
                    await ApiContext.DisposeAsync();
                    ApiContext = null!;
                }

                if (_playwright != null)
                {
                    _playwright.Dispose();
                    _playwright = null!;
                }
            }
            catch (Exception ex)
            {
                LogError("Error during resource cleanup", ex);
            }
        }
    }

    private string GetTestLogContent()
    {
        var output = new StringBuilder();
        output.AppendLine($"Test Name: {TestContext.CurrentContext.Test.Name}");
        output.AppendLine($"Test Status: {TestContext.CurrentContext.Result.Outcome.Status}");
        output.AppendLine($"Test Start Time: {_testStartTime:yyyy-MM-dd HH:mm:ss}");
        output.AppendLine($"Test Duration: {(DateTime.Now - _testStartTime).TotalSeconds:F2} seconds");
        output.AppendLine($"Test Class: {TestContext.CurrentContext.Test.ClassName}");
        output.AppendLine("\nTest Log Messages:");

        // Get all log messages from TestContext
        var testOutput = TestContext.CurrentContext.Result.Outcome.Status == NUnit.Framework.Interfaces.TestStatus.Failed
            ? TestContext.CurrentContext.Test.ClassName + "." + TestContext.CurrentContext.Test.Name + " Failed"
            : TestContext.CurrentContext.Test.ClassName + "." + TestContext.CurrentContext.Test.Name + " Passed";
        output.AppendLine(testOutput);

        if (TestContext.CurrentContext.Result.Outcome.Status == NUnit.Framework.Interfaces.TestStatus.Failed)
        {
            var errorMessage = TestContext.CurrentContext.Result.Message;
            var stackTrace = TestContext.CurrentContext.Result.StackTrace;

            output.AppendLine("\nError Details:");
            output.AppendLine($"Message: {errorMessage}");
            output.AppendLine($"Stack Trace:\n{stackTrace}");
        }

        return output.ToString();
    }

    protected void LogInfo(string message)
    {
        Logger.LogInformation(message);
        TestContext.WriteLine($"[INFO] {message}");
    }

    protected void LogWarning(string message)
    {
        Logger.LogWarning(message);
        TestContext.WriteLine($"[WARNING] {message}");
    }

    protected void LogError(string message, Exception? ex = null)
    {
        Logger.LogError(ex, message);
        TestContext.WriteLine($"[ERROR] {message}");
        if (ex != null)
        {
            TestContext.WriteLine($"[ERROR] Exception: {ex.Message}");
            TestContext.WriteLine($"[ERROR] Stack Trace: {ex.StackTrace}");
        }
    }
}

C#

Phase 3: API and UI Interaction Abstraction

Step 11: Create ApiTestHelper

Cursor Action: Create a new class in the Infrastructure/API folder named ApiTestHelper.cs.

Action: Implement methods for common API operations, including error handling and assertions:

using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using ParkPlaceSample.Infrastructure.Config;
using ParkPlaceSample.Infrastructure.TestData;
using ParkPlaceSample.Infrastructure.TestData.Models;
using NUnit.Framework;
using System.Net.Http.Headers;
using Microsoft.Playwright;

namespace ParkPlaceSample.Infrastructure.API;

public class ApiTestHelper
{
    private readonly ILogger _logger;
    private readonly TestSettings _settings;
    private readonly TestDataGenerator _dataGenerator;
    private readonly List<string> _createdResources;
    private readonly IAPIRequestContext _apiContext;

    public ApiTestHelper(ILogger logger, TestSettings settings, IAPIRequestContext apiContext)
    {
        _logger = logger;
        _settings = settings;
        _apiContext = apiContext;
        _dataGenerator = new TestDataGenerator(logger, settings);
        _createdResources = new List<string>();
    }

    public async Task<UserData> CreateTestUserAsync()
    {
        var userData = _dataGenerator.GenerateUserData();
        _logger.LogInformation("Creating test user: {Username}", userData.Username);

        var response = await _apiContext.PostAsync("/api/users", new() { DataObject = userData });
        
        // Assertion to verify the API response
        Assert.IsTrue(response.Ok, $"User creation failed with status code: {response.Status}");

        // Deserialize the response and return UserData
        var createdUser = await response.JsonAsync<UserData>(); 
        _createdResources.Add($"users/{createdUser!.Username}");
        return createdUser;
    }

    public async Task<ProductData> CreateTestProductAsync()
    {
        var productData = _dataGenerator.GenerateProductData();
        _logger.LogInformation("Creating test product: {Name}", productData.Name);

        var response = await _apiContext.PostAsync("/api/products", new() { DataObject = productData });
        
        // Assertion to verify the API response
        Assert.IsTrue(response.Ok, $"Product creation failed with status code: {response.Status}");

        // Deserialize the response and return ProductData
        var createdProduct = await response.JsonAsync<ProductData>(); 
        _createdResources.Add($"products/{createdProduct!.SKU}");
        return createdProduct;
    }

    public async Task<OrderData> CreateTestOrderAsync(int itemCount = 3)
    {
        var orderData = _dataGenerator.GenerateOrderData(itemCount);
        _logger.LogInformation("Creating test order with {ItemCount} items", itemCount);

        var response = await _apiContext.PostAsync("/api/orders", new() { DataObject = orderData });
        
        // Assertion to verify the API response
        Assert.IsTrue(response.Ok, $"Order creation failed with status code: {response.Status}");

        // Deserialize the response and return OrderData
        var createdOrder = await response.JsonAsync<OrderData>();
        _createdResources.Add($"orders/{createdOrder!.OrderNumber}");
        return createdOrder;
    }

    public async Task<CompanyData> CreateTestCompanyAsync()
    {
        var companyData = _dataGenerator.GenerateCompanyData();
        _logger.LogInformation("Creating test company: {Name}", companyData.Name);

        var response = await _apiContext.PostAsync("/api/companies", new() { DataObject = companyData });

        // Assertion to verify the API response
        Assert.IsTrue(response.Ok, $"Company creation failed with status code: {response.Status}");

        // Deserialize the response and return CompanyData
        var createdCompany = await response.JsonAsync<CompanyData>();
        _createdResources.Add($"companies/{createdCompany!.Name}");
        return createdCompany;
    }

    /// <summary>
    /// Cleans up all resources created during the test.
    /// </summary>
    public async Task CleanupTestResourcesAsync()
    {
        _logger.LogInformation("Cleaning up {Count} test resources", _createdResources.Count);

        foreach (var resource in _createdResources)
        {
            try
            {
                var response = await _apiContext.DeleteAsync($"/api/{resource}");
                if (!response.Ok)
                {
                    _logger.LogWarning("Failed to delete test resource: {Resource} with status code: {StatusCode}", resource, response.Status);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete test resource: {Resource}", resource);
            }
        }

        _createdResources.Clear();
    }

    /// <summary>
    /// Gets the list of resources created during the test.
    /// </summary>
    /// <returns>A list of resource identifiers.</returns>
    public IReadOnlyList<string> GetCreatedResources() => _createdResources.AsReadOnly();
}

C#

Step 12: Create AuthHelper

Cursor Action: Create a new class in the Infrastructure/API folder named AuthHelper.cs.

Action: Implement the GenerateAuthStateAsync method to handle authentication and return the storage state:

using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using ParkPlaceSample.Infrastructure.Config;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace ParkPlaceSample.Infrastructure.API;

public class AuthHelper
{
    private readonly ILogger _logger;
    private readonly IConfiguration _configuration;

    public AuthHelper(ILogger logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<string> GenerateAuthStateAsync(string username, string password)
    {
        var playwright = await Playwright.CreateAsync();

        // Use APIRequestContext to perform the initial authentication
        var requestContext = await playwright.APIRequest.NewContextAsync(new()
        {
            BaseURL = _configuration["Environment:ApiBaseUrl"],
            ExtraHTTPHeaders = new Dictionary<string, string>
            {
                { "Accept", "application/json" }
            }
        });

        // Perform the login API call
        var authData = new
        {
            username = username,
            password = password
        };
        var authResponse = await requestContext.PostAsync("/api/login", new() { DataObject = authData }); // Replace with your actual login endpoint

        if (!authResponse.Ok)
        {
            _logger.LogError("Authentication failed with status code: {StatusCode}", authResponse.Status);
            throw new Exception($"Authentication failed with status code: {authResponse.Status}");
        }

        _logger.LogInformation("Authentication successful. Saving storage state.");

        // Save the storage state
        var state = await requestContext.StorageStateAsync();

        // Dispose the request context
        await requestContext.DisposeAsync();
        playwright.Dispose();

        return state;
    }
}

C#

Step 13: Create BasePage and BaseComponent

Cursor Action: Create BasePage.cs in Pages and BaseComponent.cs in Pages/Components.

Action: Implement base classes for UI interactions:

// Pages/BasePage.cs
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using NUnit.Framework;
using ParkPlaceSample.Infrastructure.Config;
using Microsoft.Extensions.Configuration;

namespace ParkPlaceSample.Pages;

public abstract class BasePage
{
    protected readonly IPage Page;
    protected readonly ILogger Logger;
    protected readonly IConfiguration Configuration;

    protected BasePage(IPage page, ILogger logger, IConfiguration configuration)
    {
        Page = page;
        Logger = logger;
        Configuration = configuration;
    }

    protected string BuildUrl(string path)
    {
        var baseUrl = Configuration["Environment:BaseUrl"]?.TrimEnd('/');
        path = path.TrimStart('/');
        return $"{baseUrl}/{path}";
    }
}

// Pages/Components/BaseComponent.cs
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using ParkPlaceSample.Infrastructure.Config;
using Microsoft.Extensions.Configuration;

namespace ParkPlaceSample.Pages.Components;

public abstract class BaseComponent
{
    protected readonly IPage Page;
    protected readonly ILogger Logger;
    protected readonly IConfiguration Configuration;

    protected BaseComponent(IPage page, ILogger logger, IConfiguration configuration)
    {
        Page = page;
        Logger = logger;
        Configuration = configuration;
    }
}

C#

Phase 4: Test Implementation

Step 14: Convert SampleTest

Cursor Action: Open Tests/SampleTest.cs.

Actions:

Convert to NUnit attributes.

Use the ApiTestHelper and create specific Page Objects (e.g., HomePage, LoginPage, etc.) as needed.

Add assertions and logging.

Example (Illustrative - adapt to your specific test cases):

using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using NUnit.Framework;
using ParkPlaceSample.Infrastructure.Base;
using ParkPlaceSample.Infrastructure.API;
using ParkPlaceSample.Pages;

namespace ParkPlaceSample.Tests;

[TestFixture]
public class SampleTest : TestBase
{
    private ApiTestHelper _apiTestHelper;

    [SetUp]
    public override async Task BaseTestInitialize()
    {
        await base.BaseTestInitialize();
        _apiTestHelper = new ApiTestHelper(Logger, Settings, ApiContext);
    }

    [Test]
    public async Task SampleEndToEndTest()
    {
        // Arrange
        var user = await _apiTestHelper.CreateTestUserAsync();
        var homePage = new HomePage(Page, Logger, Configuration); // Assuming you have a HomePage Page Object

        // Act
        await homePage.GotoAsync();
        await homePage.LoginAsync(user.Username, user.Password); // Assuming you have a login method on HomePage

        // Assert
        // ... assertions to verify successful login and user state
    }
}

C#

Phase 5: Reporting and Cleanup

Step 15: Update Reporting Components

Cursor Actions:

Open Infrastructure/Reporting/AttachmentHelper.cs.

Open Infrastructure/Reporting/TestReportManager.cs.

Open Infrastructure/Reporting/TestMetrics.cs.

Open Infrastructure/Reporting/HtmlReporterConfig.cs.

Actions:

Review each file and make sure file paths, especially in TestReportManager, are correct and that the logic is compatible with NUnit.

You might need to adjust how test results are collected and passed to the reporting methods, as NUnit uses a different TestContext.

Make sure you have [OneTimeSetUp] and [OneTimeTearDown] in a fixture (or a base class like TestBase) to initialize and finalize the ExtentReports instance, similar to what you had with [AssemblyInitialize] and [AssemblyCleanup].

Step 16: Implement Cleanup in ApiTestHelper

Cursor Action: Open Infrastructure/API/ApiTestHelper.cs.

Action: Implement the CleanupTestResourcesAsync method to delete created resources during teardown:

public async Task CleanupTestResourcesAsync()
{
    _logger.LogInformation("Cleaning up {Count} test resources", _createdResources.Count);

    foreach (var resource in _createdResources)
    {
        try
        {
            var response = await _apiContext.DeleteAsync($"/api/{resource}");
            if (!response.Ok)
            {
                _logger.LogWarning("Failed to delete test resource: {Resource} with status code: {StatusCode}", resource, response.Status);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete test resource: {Resource}", resource);
        }
    }

    _createdResources.Clear();
}

C#

Step 17: Call Cleanup in TestBase

Cursor Action: Open Infrastructure/Base/TestBase.cs.

Action: In the [TearDown] method (BaseTestCleanup), call the CleanupTestResourcesAsync method of your ApiTestHelper instance:

[TearDown]
public virtual async Task BaseTestCleanup()
{
    // ... other cleanup code

    // Cleanup API resources if the helper is available
    if (_apiTestHelper != null)
    {
        await _apiTestHelper.CleanupTestResourcesAsync();
    }

    // ... rest of the cleanup code
}


Phase 6: Run, Debug, and Iterate

Step 18: Run Tests and Debug

Cursor Action: Use the integrated test runner in Cursor to run your tests.

Actions:

Observe the test results.

Debug any failures using Cursor's debugging tools.

Iterate on the code, making adjustments as needed.

Step 19: Review and Refine

Actions:

Review the refactored code.

Ensure that all tests are passing and that the framework is functioning as expected.

Address any TODOs or areas for improvement that you identified during the refactoring process.

Additional Tips for Using Cursor IDE:

AI Assistance: Use Cursor's AI features to help you generate code snippets, refactor code, and find solutions to problems.

Code Completion: Leverage Cursor's intelligent code completion to write code faster and with fewer errors.

Integrated Terminal: Use the integrated terminal to run commands without leaving the IDE.

Debugging Tools: Take advantage