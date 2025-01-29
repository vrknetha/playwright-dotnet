using System;
using System.Threading.Tasks;
using AuthSetup.Infrastructure.Base;
using AuthSetup.Infrastructure.Helpers;
using AuthSetup.Services;
using PlaywrightDemo.Infrastructure.Config;
using PlaywrightDemo.Pages.UI;

namespace AuthSetup;

public class Program : PlaywrightConsoleBase
{
    private LoginPage _loginPage = null!;
    private AuthStateService _authStateService = null!;

    public static async Task<int> Main(string[] args)
    {
        var program = new Program();
        return await program.Run();
    }

    private async Task<int> Run()
    {
        Console.WriteLine("🚀 Starting setup...");
        Console.WriteLine($"Using environment: {Settings.Environment.Name}");

        // Get password from configuration
        var commonPassword = ConfigurationLoader.Configuration.GetSection("TestSettings:Auth:CommonPassword").Value
            ?? throw new InvalidOperationException("CommonPassword not found in settings");

        // Get solution root path
        var rootPath = PathHelper.GetSolutionRootPath();
        Console.WriteLine($"Using root path: {rootPath}");

        // Initialize browser
        await InitializeAsync();
        _loginPage = new LoginPage(Page);

        // Initialize auth state service
        _authStateService = new AuthStateService(Logger, Settings, rootPath, _loginPage, Context);

        // Generate users.json and auth states
        await _authStateService.GenerateUsersJsonAsync(commonPassword);

        await CleanupAsync();
        Console.WriteLine("✅ Setup completed successfully");
        return 0;
    }
}