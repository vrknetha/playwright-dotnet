# Cursor Rules for .NET Playwright Testing Framework

## Code Organization

1. Keep test files organized by feature or functionality
2. Use meaningful test names that describe the test scenario
3. Follow a consistent file structure:
   - `Infrastructure/` for framework components
   - `Tests/` for test files
   - `Pages/` for page objects
   - `TestData/` for test data and fixtures

## Test Structure

1. Each test should follow the Arrange-Act-Assert pattern
2. Use descriptive logging messages at each step
3. Keep tests focused and independent
4. Use proper test categories for better organization

## Test Reporting

1. HTML Report Structure:

   - Use semantic HTML with proper nesting
   - Include CSS in a separate style section
   - Ensure proper encoding of all dynamic content
   - Use consistent class names for styling

2. Report Components:

   - Test header with title, status, and metadata
   - Organized test artifacts (videos, traces, logs)
   - Clear error messages and stack traces
   - Downloadable attachments with proper icons

3. Video Recording:

   - Save videos with meaningful names including timestamp
   - Provide proper video controls and download options
   - Use appropriate video container styling

4. Trace Files:

   - Include clear instructions for viewing traces
   - Provide direct link to Playwright Trace Viewer
   - Add proper download options

5. Log Files:
   - Use consistent log formatting
   - Include timestamp and log level
   - Provide proper log file download options

## Configuration Management

1. Use strongly-typed configuration classes
2. Keep environment-specific settings in appropriate config files
3. Use proper configuration hierarchy:
   - Base settings in `appsettings.json`
   - Environment overrides in `appsettings.{env}.json`

## Error Handling

1. Use proper exception handling in cleanup methods
2. Include detailed error information in reports
3. Ensure all resources are properly disposed

## Common Mistakes to Avoid

1. Not encoding HTML content properly
2. Missing file paths or incorrect path construction
3. Not handling null references properly
4. Improper resource cleanup
5. Inconsistent logging formats

## Best Practices

1. Test Reporting:

   - Use consistent styling across all reports
   - Ensure all attachments are properly linked
   - Provide clear instructions for trace viewing
   - Include meaningful test metadata

2. Resource Management:

   - Properly dispose of Playwright resources
   - Clean up temporary files
   - Handle video and trace files correctly

3. Configuration:

   - Use strongly-typed settings
   - Validate configuration values
   - Handle missing configuration gracefully

4. Logging:
   - Use appropriate log levels
   - Include contextual information
   - Format logs for readability

## Recent Learnings

1. HTML Report Generation:

   - Use proper HTML structure for better organization
   - Include CSS styles for consistent formatting
   - Handle file paths and URLs correctly
   - Ensure proper encoding of all content

2. Test Artifacts:

   - Organize artifacts by type
   - Provide clear download instructions
   - Use appropriate icons and styling
   - Include proper file metadata

3. Configuration:

   - Use environment-specific settings
   - Handle base URLs properly
   - Validate configuration at startup

4. Resource Management:
   - Properly handle video recordings
   - Manage trace files effectively
   - Clean up temporary resources
