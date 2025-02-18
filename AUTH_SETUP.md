# Authentication State Management

## Overview
This project implements a robust authentication state management system using Playwright for .NET. The system generates and manages authentication states for multiple users, allowing tests to reuse these states instead of performing repeated logins.

## Project Structure
```
PlaywrightDemo/
├── users.json                    # User credentials
├── .auth/                        # Generated auth states
│   └── {user.email}.json        # Auth state per user
├── Pages/
│   └── UI/
│       └── LoginPage.cs         # Login page interactions
└── Infrastructure/
    └── Auth/
        └── AuthHelper.cs        # Auth state management
```

## Key Components

### 1. AuthSetup Project
Located in `src/AuthSetup`, this is a separate project that handles authentication state generation. It runs independently from test execution to avoid interference with parallel test runs.

```csharp
public class Program : PlaywrightConsoleBase
{
    private AuthHelper _authHelper = null!;

    private async Task<int> Run()
    {
        try
        {
            _authHelper = new AuthHelper();
            await _authHelper.GenerateAllAuthStatesAsync();
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
            return 1;
        }
    }
}
```

### 2. LoginPage Component
Located in `Pages/UI/LoginPage.cs`, this component handles all UI interactions related to the login process.

Key responsibilities:
- Navigation to login page
- Form interactions
- Login verification
- Page state management

```csharp
public class LoginPage : BasePage
{
    public async Task LoginWithCredentialsAsync(string username, string password)
    {
        await GoToLoginPage();
        await UsernameInput.FillAsync(username);
        await PasswordInput.FillAsync(password);
        await SignInButton.ClickAsync();
        await Page.WaitForURLAsync("**/dashboard");
    }

    public async Task ExpectLoginSuccessfulAsync()
    {
        await Expect(_page.Locator("#copilot-dashboard-entrypoint-textarea").First)
            .ToBeVisibleAsync(new() { Timeout = 30000 });
    }
}
```

### 3. AuthHelper Component
Located in `Infrastructure/Auth/AuthHelper.cs`, this component manages authentication states and coordinates the authentication process.

Key responsibilities:
- User configuration management
- Browser context management
- Auth state generation and storage
- State validation and caching

## Authentication Flow

1. **Initialization**
   - Load user configuration from users.json
   - Initialize Playwright browser
   - Create auth directory if not exists

2. **For Each User**
   - Create new browser context
   - Initialize LoginPage
   - Perform login
   - Verify successful login
   - Generate and save auth state

3. **Auth State Storage**
   - States stored in `.auth/{user.email}.json`
   - Cached for quick access
   - Validated before reuse

## Usage in Tests

1. **Configuration**
   ```json
   // users.json
   [
     {
       "username": "testuser",
       "email": "test@example.com",
       "password": "password123"
     }
   ]
   ```

2. **Generating Auth States**
   ```bash
   # Run the AuthSetup project
   dotnet run --project src/AuthSetup
   ```

3. **Using Auth States in Tests**
   ```csharp
   // In your test
   var authHelper = new AuthHelper();
   var authState = await authHelper.GetAuthStateAsync("user@email.com");
   ```

## Error Handling

The system implements comprehensive error handling:
- Per-user error isolation
- Detailed logging
- Resource cleanup
- State validation

## Best Practices

1. **Auth State Generation**
   - Generate states before test runs
   - Validate states periodically
   - Clean up old states

2. **Test Implementation**
   - Use auth states instead of live login
   - Handle auth state failures gracefully
   - Implement proper cleanup

3. **Maintenance**
   - Regular validation of stored states
   - Update states when user credentials change
   - Monitor auth directory size

## Troubleshooting

Common issues and solutions:

1. **Auth State Generation Fails**
   - Check user credentials in users.json
   - Verify network connectivity
   - Check browser configuration

2. **Auth State Not Found**
   - Verify auth directory exists
   - Check file permissions
   - Regenerate auth states

3. **Login Verification Fails**
   - Check login selectors
   - Verify expected dashboard elements
   - Update timeout settings if needed

## Benefits

1. **Performance**
   - Reduced test execution time
   - Avoid repeated logins
   - Efficient resource usage

2. **Reliability**
   - Consistent test execution
   - Isolated auth state generation
   - Better parallel test execution

3. **Maintainability**
   - Clear separation of concerns
   - Centralized auth management
   - Easy to update and extend

## Contributing

When contributing to the auth system:
1. Follow existing patterns
2. Add proper error handling
3. Update documentation
4. Add relevant tests

## Future Improvements

Planned enhancements:
1. Auth state expiration management
2. Automatic state regeneration
3. Enhanced state validation
4. Performance optimizations 