using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using PlaywrightDemo.Infrastructure.Config.Models;
using PlaywrightDemo.Infrastructure.TestData.Models;
using PlaywrightDemo.Pages.UI;

namespace AuthSetup.Services;

public class AuthStateService
{
    private readonly ILogger _logger;
    private readonly TestSettings _settings;
    private readonly string _rootPath;
    private readonly LoginPage _loginPage;
    private readonly IBrowserContext _browserContext;

    public AuthStateService(ILogger logger, TestSettings settings, string rootPath, LoginPage loginPage, IBrowserContext browserContext)
    {
        _logger = logger;
        _settings = settings;
        _rootPath = rootPath;
        _loginPage = loginPage;
        _browserContext = browserContext;
    }

    public async Task GenerateUsersJsonAsync(string commonPassword)
    {
        var users = new List<User>
        {
            new User
            {
                Username = "AniketSelokar-CawTech",
                Email = "aniket.selokar@caw.tech",
                Password = commonPassword
            }
        };

        // Validate user data
        foreach (var user in users)
        {
            if (string.IsNullOrEmpty(user.Username))
                throw new InvalidOperationException("Username cannot be empty");
            if (string.IsNullOrEmpty(user.Email))
                throw new InvalidOperationException("Email cannot be empty");
            if (string.IsNullOrEmpty(user.Password))
                throw new InvalidOperationException("Password cannot be empty");
        }

        var usersPath = Path.Combine(_rootPath, "users.json");
        var usersJson = JsonSerializer.Serialize(users, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await File.WriteAllTextAsync(usersPath, usersJson);
        _logger.LogInformation("📝 Created users.json with {Count} users at {Path}", users.Count, usersPath);

        await GenerateAuthStatesAsync(users);
    }

    private async Task GenerateAuthStatesAsync(List<User> users)
    {
        var authDir = Path.Combine(_rootPath, ".auth");
        Directory.CreateDirectory(authDir);

        foreach (var user in users)
        {
            _logger.LogInformation("Generating auth state for user: {Username}", user.Username);
            await _loginPage.LoginAsync(user);
            await _loginPage.ExpectLoginSuccessfulAsync();

            var authStatePath = Path.Combine(authDir, $"{user.Username}_state.json");
            _logger.LogInformation("Saving auth state to: {Path}", authStatePath);
            await _browserContext.StorageStateAsync(new BrowserContextStorageStateOptions
            {
                Path = authStatePath
            });
            _logger.LogInformation("✅ Auth state generated successfully for user: {Username}", user.Username);
        }
    }
}