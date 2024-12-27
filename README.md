# Playwright .NET Test Automation Framework

A robust and scalable test automation framework built with Playwright for .NET, featuring comprehensive reporting, logging, and configuration management.

## Features

- **Cross-browser Testing**: Support for Chromium, Firefox, and WebKit
- **Parallel Test Execution**: MSTest-based parallel test execution
- **Rich Reporting**: HTML reports with videos, traces, and logs
- **Configuration Management**: Environment-based configuration
- **Logging**: Comprehensive logging with different levels
- **Page Object Model**: Structured and maintainable test architecture
- **Tracing**: Built-in Playwright trace viewer support
- **Video Recording**: Automatic video recording of test executions

## Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download)
- [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) or [Visual Studio Code](https://code.visualstudio.com/)
- [PowerShell](https://docs.microsoft.com/en-us/powershell/) (for running Playwright installation scripts)

## Getting Started

1. Clone the repository:

   ```bash
   git clone https://github.com/vrknetha/playwright-dotnet.git
   cd playwright-dotnet
   ```

2. Install dependencies:

   ```bash
   dotnet restore
   ```

3. Install Playwright browsers:

   ```bash
   pwsh bin/Debug/net9.0/playwright.ps1 install
   ```

4. Build the solution:

   ```bash
   dotnet build
   ```

5. Run the tests:
   ```bash
   dotnet test
   ```

## Project Structure

```
├── Infrastructure/
│   ├── Base/               # Base classes for tests and page objects
│   ├── Config/             # Configuration management
│   ├── Reporting/          # Test reporting infrastructure
│   ├── Tracing/           # Playwright tracing setup
│   └── UI/                # UI interaction helpers
├── Pages/                 # Page Object Models
├── Tests/                 # Test classes
├── TestResults/          # Test execution results
│   └── Reports/          # HTML reports with artifacts
└── appsettings.json      # Application configuration
```

## Configuration

The framework uses `appsettings.json` for configuration:

```json
{
  "TestSettings": {
    "Browser": {
      "Name": "chromium",
      "Headless": false,
      "SlowMo": 0,
      "Viewport": {
        "Width": 1920,
        "Height": 1080
      }
    },
    "Trace": {
      "Enabled": true,
      "Mode": "Always",
      "Screenshots": true,
      "Snapshots": true,
      "Sources": true
    }
  }
}
```

## Writing Tests

Example test using the framework:

```csharp
[TestClass]
public class SampleTest : TestBase
{
    [TestMethod]
    public async Task NavigationTest()
    {
        // Arrange
        await Page.GotoAsync("https://playwright.dev");

        // Act
        await Page.ClickAsync("text=Get Started");

        // Assert
        await Expect(Page).ToHaveURLAsync("**/docs/intro");
    }
}
```

## Test Reports

After test execution, you can find the following artifacts in the `TestResults/Reports` directory:

- HTML report (`index.html`)
- Test videos (`.webm` files)
- Playwright traces (`.zip` files)
- Test logs (`.log` files)

## Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
