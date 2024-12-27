# Cursor Rules for .NET Playwright Testing

## Code Organization

### Namespaces

- ✅ Use `ParkPlaceSample.Infrastructure.Config` for configuration-related code
- ✅ Use `ParkPlaceSample.Infrastructure.Base` for base test classes
- ✅ Use `ParkPlaceSample.Infrastructure.Reporting` for reporting-related code
- ✅ Use `ParkPlaceSample.Infrastructure.Tracing` for tracing-related code
- ✅ Use `ParkPlaceSample.Tests` for test classes

### File Structure

- ✅ Keep test files in the `Tests` directory
- ✅ Keep infrastructure code in the `Infrastructure` directory
- ✅ Keep configuration files in the project root
- ✅ Use appropriate subdirectories for specialized components (API, UI, etc.)

## Coding Standards

### Naming Conventions

- ✅ Use PascalCase for class names and public members
- ✅ Use camelCase for private fields with underscore prefix
- ✅ Use descriptive names for test methods that indicate their purpose
- ✅ Suffix test classes with "Test" (e.g., `SampleTest`)

### Test Methods

- ✅ Use async/await consistently
- ✅ Include proper test attributes (`[TestClass]`, `[TestMethod]`)
- ✅ Implement proper test initialization and cleanup
- ✅ Use meaningful assertions with descriptive messages

### Error Handling

- ✅ Use proper exception handling in infrastructure code
- ✅ Log exceptions with appropriate context
- ✅ Clean up resources in finally blocks
- ✅ Handle async operations properly

## Testing Best Practices

### Test Structure

- ✅ Follow Arrange-Act-Assert pattern
- ✅ Use proper logging throughout test execution
- ✅ Include appropriate waiting strategies
- ✅ Capture screenshots and traces for failures

### Configuration

- ✅ Use appsettings.json for test configuration
- ✅ Support environment-specific settings
- ✅ Use strongly-typed configuration classes
- ✅ Initialize configuration before test execution

### Logging

- ✅ Use structured logging with appropriate log levels
- ✅ Include context information in log messages
- ✅ Log test steps and important actions
- ✅ Format log messages for readability

## Common Mistakes to Avoid

### Namespace Issues

- ❌ Don't use incorrect namespace references
- ❌ Don't mix infrastructure and test code in the same namespace
- ❌ Don't forget to update namespaces when moving files
- ✅ Use global using directives for common namespaces

### Resource Management

- ❌ Don't leave resources uncleaned
- ❌ Don't ignore Dispose patterns
- ❌ Don't forget to handle async disposal
- ✅ Implement proper cleanup in test base classes

### Test Design

- ❌ Don't create redundant test files
- ❌ Don't mix test infrastructure verification with actual tests
- ❌ Don't hardcode test data
- ✅ Use test data generators for dynamic data

## Recent Learnings

1. Configuration Initialization:

   - ✅ Always initialize configuration before using it
   - ✅ Log configuration values for debugging
   - ✅ Handle missing configuration gracefully

2. Test Infrastructure:

   - ✅ Verify infrastructure components with intentional failures
   - ✅ Implement proper test reporting
   - ✅ Enable test parallelization carefully

3. Code Organization:
   - ✅ Remove redundant test files
   - ✅ Keep test examples consolidated
   - ✅ Use proper project structure

## Recommendations

1. Always run tests after making infrastructure changes
2. Keep test code clean and well-documented
3. Use proper logging for debugging
4. Implement proper waiting strategies
5. Handle browser automation carefully
6. Clean up resources properly
7. Use strongly-typed configurations
8. Follow async/await best practices
9. Implement proper test isolation
10. Use appropriate test categories
