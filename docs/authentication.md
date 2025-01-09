# Authentication Implementation Guide

This document describes the authentication implementation in the Playwright test framework, including storage state management, test organization, and usage examples.

## Overview

The framework uses Playwright's storage state feature to manage authentication across tests. This allows tests to:
- Run with pre-generated authentication states
- Generate new auth states as needed
- Run without authentication
- Share authentication states between tests

## Directory Structure

Authentication-related files are organized as follows:

```
├── Infrastructure/
│   ├── Auth/
│   │   └── AuthHelper.cs       # Core authentication functionality
│   └── Base/
│       └── TestBase.cs         # Base test class with auth support
├── Tests/
│   └── Auth/
│       ├── AuthenticatedUserTests.cs  # Regular user tests
│       ├── AdminUserTests.cs          # Admin user tests
│       └── UnauthenticatedTests.cs    # Public access tests
└── TestResults/
    └── Reports/
        └── .auth/              # Storage state files
```

## Auth State Files

### Naming Conventions

- Regular user: `user1.json`, `user2.json`, etc.
- Admin user: `admin.json`
- Custom states: `authstate-{guid}.json`

### File Locations

Auth state files are stored in `TestResults/Reports/.auth/` directory. This location is managed by the `AuthHelper` class.

## Usage Examples

### Running Tests with Authentication

```csharp
[TestFixture]
[Category("Auth")]
public class AuthenticatedUserTests : TestBase
{
    private const string AUTH_STATE_FILE = "user1.json";

    [OneTimeSetUp]
    public async Task AuthSetup()
    {
        AuthStateToUse = AUTH_STATE_FILE;
    }

    [Test]
    public async Task UserCanAccessProtectedPage()
    {
        await Page.GotoAsync("/profile");
        Assert.That(Page.Url, Does.Not.Contain("/login"));
    }
}
```

### Running Tests without Authentication

```csharp
[TestFixture]
[Category("Auth")]
public class UnauthenticatedTests : TestBase
{
    [Test]
    public async Task RedirectsToLoginForProtectedPages()
    {
        await Page.GotoAsync("/profile");
        Assert.That(Page.Url, Does.Contain("/login"));
    }
}
```

### Generating New Auth States

```csharp
// Generate and save auth state
var authStatePath = await AuthHelper.GenerateAuthStateAsync(
    username: "user@example.com",
    password: "password123",
    filename: "custom-user.json"
);

// Use the generated auth state
AuthStateToUse = "custom-user.json";
```

## Best Practices

1. **Auth State Management**
   - Use meaningful names for auth state files
   - Clean up old auth states regularly
   - Verify auth states before use

2. **Test Organization**
   - Group tests by authentication requirement
   - Use appropriate test categories
   - Keep auth setup in `OneTimeSetUp`

3. **Error Handling**
   - Always verify auth state validity
   - Handle expired or invalid states
   - Clean up resources properly

## Common Issues and Solutions

1. **Invalid Auth State**
   ```csharp
   // Verify and regenerate if needed
   if (!await AuthHelper.VerifyAuthStateAsync(authStatePath))
   {
       await AuthHelper.GenerateAuthStateAsync(username, password, filename);
   }
   ```

2. **Resource Cleanup**
   ```csharp
   // Clean up old auth states
   AuthHelper.DeleteAuthState("old-state.json");
   ```

3. **Auth State Sharing**
   ```csharp
   // Share auth state between test classes
   protected const string SHARED_AUTH_STATE = "shared-user.json";
   ```

## Performance Considerations

1. **State Caching**
   - Auth states are cached for reuse
   - Validation results are tracked
   - Browser instance is reused

2. **Resource Management**
   - Auth states are generated only when needed
   - Invalid states are automatically regenerated
   - Resources are properly disposed

## Security Considerations

1. **Credentials**
   - Never commit auth state files
   - Use environment variables for credentials
   - Clean up auth states after test runs

2. **File Management**
   - Store auth states in secure location
   - Clean up sensitive data
   - Use proper file permissions 