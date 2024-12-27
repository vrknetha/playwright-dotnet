
# Plan of Action: Create a Test Automation Framework in .NET Using MSTest

## **Phase 1: Setup and Environment Preparation**

### **1. Create a .NET MSTest Project**
- Create a new MSTest project:
  ```bash
  dotnet new mstest -n TestAutomationFramework
  cd TestAutomationFramework
  ```
- Add required NuGet packages:
  ```bash
  dotnet add package Microsoft.Playwright
  dotnet add package Microsoft.Extensions.Configuration.Json
  dotnet add package FluentAssertions
  ```

### **2. Add `appsettings.json` for Configuration**
- Add a file `appsettings.json` to the project root.
- Example content:
  ```json
  {
    "TestSettings": {
      "Browser": "Chromium",
      "Headless": true,
      "Timeout": 30000,
      "Viewport": {
        "Width": 1280,
        "Height": 720
      },
      "BaseUrl": "https://example.com",
      "Retries": 2
    }
  }
  ```

### **3. Create a Strongly-Typed Configuration Class**
- Add a class `TestSettings.cs` in a `Config` folder.
  ```csharp
  public class TestSettings
  {
      public string Browser { get; set; }
      public bool Headless { get; set; }
      public int Timeout { get; set; }
      public Viewport Viewport { get; set; }
      public string BaseUrl { get; set; }
      public int Retries { get; set; }
  }

  public class Viewport
  {
      public int Width { get; set; }
      public int Height { get; set; }
  }
  ```

### **4. Load Configuration in Framework**
- Add a static loader `ConfigLoader.cs` in a `Utilities` folder.
  ```csharp
  using Microsoft.Extensions.Configuration;

  public static class ConfigLoader
  {
      public static TestSettings LoadSettings(string fileName = "appsettings.json")
      {
          var config = new ConfigurationBuilder()
              .SetBasePath(Directory.GetCurrentDirectory())
              .AddJsonFile(fileName, optional: false, reloadOnChange: true)
              .Build();

          return config.GetSection("TestSettings").Get<TestSettings>();
      }
  }
  ```

---

## **Phase 2: Core Framework Development**

### **1. Implement Base Test Class**
- Create `BaseTest.cs` for common setup and teardown logic.
  ```csharp
  using Microsoft.Playwright;
  using Microsoft.VisualStudio.TestTools.UnitTesting;
  using System.Threading.Tasks;

  public class BaseTest
  {
      protected IPlaywright Playwright;
      protected IBrowser Browser;
      protected TestSettings Config;

      [TestInitialize]
      public async Task Setup()
      {
          Config = ConfigLoader.LoadSettings();
          Playwright = await Playwright.CreateAsync();

          Browser = Config.Browser switch
          {
              "Chromium" => await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = Config.Headless }),
              "Firefox" => await Playwright.Firefox.LaunchAsync(new BrowserTypeLaunchOptions { Headless = Config.Headless }),
              "Webkit" => await Playwright.Webkit.LaunchAsync(new BrowserTypeLaunchOptions { Headless = Config.Headless }),
              _ => throw new ArgumentException($"Unsupported browser: {Config.Browser}")
          };
      }

      [TestCleanup]
      public async Task Teardown()
      {
          await Browser.CloseAsync();
          Playwright?.Dispose();
      }
  }
  ```

### **2. Use Configuration in Tests**
- Example test using configuration:
  ```csharp
  [TestClass]
  public class ExampleTest : BaseTest
  {
      [TestMethod]
      public async Task OpenHomePage()
      {
          var context = await Browser.NewContextAsync(new BrowserNewContextOptions
          {
              ViewportSize = Config.Viewport,
              BaseURL = Config.BaseUrl
          });

          var page = await context.NewPageAsync();
          await page.GotoAsync("/");

          Assert.AreEqual("Example Domain", await page.TitleAsync());
      }
  }
  ```

---

## **Phase 3: Advanced Features**

### **1. Parallel Execution**
- Enable parallel execution by adding the following to `AssemblyInfo.cs`:
  ```csharp
  [assembly: Parallelize(Workers = 4)]
  ```

### **2. Custom Assertions with FluentAssertions**
- Replace MSTest assertions with FluentAssertions for readability:
  ```csharp
  actual.Should().Be(expected);
  ```

### **3. Add Retry Logic**
- Integrate retry logic using the `Retries` configuration.
  ```csharp
  [TestMethod]
  [TestCategory("Retry")]
  public async Task RetryableTest()
  {
      int attempt = 0;
      bool success = false;

      while (attempt < Config.Retries && !success)
      {
          try
          {
              // Test logic
              success = true;
          }
          catch
          {
              attempt++;
              if (attempt >= Config.Retries) throw;
          }
      }
  }
  ```

---

## **Phase 4: Reporting**

### **1. HTML Reporting with ExtentReports**
- Add `ExtentReports` NuGet package:
  ```bash
  dotnet add package ExtentReports
  ```
- Create `ExtentReporter.cs`:
  ```csharp
  using AventStack.ExtentReports;
  using AventStack.ExtentReports.Reporter;

  public class ExtentReporter
  {
      private static ExtentReports _extent;
      private static ExtentTest _test;

      public static void InitializeReport(string reportPath)
      {
          var htmlReporter = new ExtentHtmlReporter(reportPath);
          _extent = new ExtentReports();
          _extent.AttachReporter(htmlReporter);
      }

      public static void CreateTest(string testName)
      {
          _test = _extent.CreateTest(testName);
      }

      public static void LogInfo(string message)
      {
          _test.Log(Status.Info, message);
      }

      public static void Flush()
      {
          _extent.Flush();
      }
  }
  ```
- Call `ExtentReporter` methods in test setup and cleanup.

---

## **Phase 5: CI/CD Integration**

### **1. Create a GitHub Actions Workflow**
- Add a `.github/workflows/test.yml` file:
  ```yaml
  name: .NET MSTest CI

  on:
    push:
      branches:
        - main

  jobs:
    test:
      runs-on: ubuntu-latest

      steps:
        - uses: actions/checkout@v3
        - name: Setup .NET
          uses: actions/setup-dotnet@v3
          with:
            dotnet-version: '7.0.x'
        - name: Install Dependencies
          run: dotnet restore
        - name: Run Tests
          run: dotnet test --logger "trx"
  ```

---

## **Outcome**
- A fully functional test automation framework using .NET and MSTest with configurable options, Playwright integration, custom reporting, and CI/CD pipeline integration.
