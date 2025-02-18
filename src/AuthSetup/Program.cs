using System;
using System.Threading.Tasks;
using System.IO;
using AuthSetup.Infrastructure.Base;
using PlaywrightDemo.Infrastructure.Auth;
using PlaywrightDemo.Infrastructure.Config;

namespace AuthSetup;

public class Program : PlaywrightConsoleBase
{
    private AuthHelper _authHelper = null!;

    public static async Task<int> Main(string[] args)
    {
        // Set the current directory to the root project directory where appsettings.json is located
        var rootDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
        Directory.SetCurrentDirectory(rootDir);

        var program = new Program();
        return await program.Run();
    }

    private async Task<int> Run()
    {
        Console.WriteLine("🚀 Starting auth state generation...");
        Console.WriteLine($"Using environment: {Settings.Environment.Name}");

        try
        {
            // Initialize AuthHelper which handles all the auth logic
            _authHelper = new AuthHelper();

            // Generate auth states for all users
            await _authHelper.GenerateAllAuthStatesAsync();

            Console.WriteLine("✅ Auth state generation completed successfully");
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error during auth state generation: {ex.Message}");
            return 1;
        }
        finally
        {
            if (_authHelper != null)
            {
                await _authHelper.DisposeAsync().ConfigureAwait(false);
            }
        }
    }
}