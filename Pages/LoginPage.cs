using System;
using System.Threading.Tasks;
using Microsoft.Playwright; // Ensure this line is present
using System.Text.Json;
using System.IO;
using PlaywrightDemo.Pages;

public class LoginPage : BasePage
{
    private readonly IPage _page;

    public LoginPage(IPage page) : base(page)
    {
        _page = page;
    }

    public async Task<bool> LoginAsync(User user)
    {
        // Load session storage if it exists

        await _page.GotoAsync("https://github.com/login");
        await _page.GetByLabel("Username or email address").FillAsync(user.Username);
        await Expect(_page.GetByLabel("Password")).ToBeVisibleAsync();
        await _page.GetByLabel("Password").FillAsync(user.Password);

        // Click the "Sign in" button
        await _page.GetByRole(AriaRole.Button, new() { Name = "Sign in" }).First.ClickAsync();

        // Check if login was successful
        await AssertLoginSuccessfulAsync();
        await SaveSessionStorageAsync(user.Username);
        return true;
    }

    public async Task NavitageToDashBoard()
    {
        await _page.GotoAsync("https://github.com");
    }

    public async Task AssertLoginSuccessfulAsync()
    {
        await Expect(_page.GetByText("Dashboard").First).ToBeAttachedAsync();
    }

    private async Task SaveSessionStorageAsync(string username)
    {
        // Define the path for the session storage file
        var filePath = Path.Combine("C:\\Users\\caw_qa\\Documents\\Projects\\lastest-modification\\playwright-dotnet\\playwright\\.auth", $"{username}_state.json");

        // Save the current storage state to the specified file
        await _page.Context.StorageStateAsync(new BrowserContextStorageStateOptions
        {
            Path = filePath // Save the storage state to the specified file
        });
    }


}

