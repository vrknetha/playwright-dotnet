using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using ParkPlaceSample.Infrastructure.Config;

namespace ParkPlaceSample.Infrastructure.Reporting;

public class AzureDevOpsTestReporter
{
    private readonly ILogger _logger;
    private readonly AzureDevOpsSettings _settings;
    private readonly HttpClient _httpClient;
    private string _testRunId;

    public AzureDevOpsTestReporter(ILogger logger, AzureDevOpsSettings settings)
    {
        _logger = logger;
        _settings = settings;
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Basic",
            Convert.ToBase64String(Encoding.ASCII.GetBytes($":{_settings.PersonalAccessToken}"))
        );
    }

    public async Task StartTestRunAsync(string runName)
    {
        if (!_settings.Enabled) return;

        try
        {
            var requestBody = JsonSerializer.Serialize(new
            {
                name = runName,
                automated = true,
                plan = new { id = _settings.TestPlanId },
                testSuite = new { id = _settings.TestSuiteId }
            });

            var url = $"{_settings.OrganizationUrl}/{_settings.ProjectName}/_apis/test/runs?api-version=7.0";
            var response = await _httpClient.PostAsync(url, new StringContent(requestBody, Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(content);
            _testRunId = result.GetProperty("id").GetString();

            _logger.LogInformation("Test run created with ID: {TestRunId}", _testRunId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create test run");
            throw;
        }
    }

    public async Task PublishTestResultAsync(TestContext testContext, TimeSpan duration)
    {
        if (!_settings.Enabled || string.IsNullOrEmpty(_testRunId)) return;

        try
        {
            var outcome = testContext.CurrentTestOutcome switch
            {
                UnitTestOutcome.Passed => "Passed",
                UnitTestOutcome.Failed => "Failed",
                UnitTestOutcome.Inconclusive => "Inconclusive",
                UnitTestOutcome.Timeout => "Timeout",
                _ => "NotExecuted"
            };

            var requestBody = JsonSerializer.Serialize(new[]
            {
                new
                {
                    testCaseTitle = testContext.TestName,
                    automatedTestName = testContext.FullyQualifiedTestClassName,
                    outcome = outcome,
                    durationInMs = duration.TotalMilliseconds,
                    errorMessage = testContext.CurrentTestOutcome == UnitTestOutcome.Failed ? testContext.FullyQualifiedTestClassName : null,
                    state = "Completed"
                }
            });

            var url = $"{_settings.OrganizationUrl}/{_settings.ProjectName}/_apis/test/runs/{_testRunId}/results?api-version=7.0";
            var response = await _httpClient.PostAsync(url, new StringContent(requestBody, Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();

            _logger.LogInformation("Test result published for {TestName}", testContext.TestName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish test result for {TestName}", testContext.TestName);
            throw;
        }
    }

    public async Task AttachFileToTestResultAsync(string filePath, TestContext testContext)
    {
        if (!_settings.Enabled || string.IsNullOrEmpty(_testRunId)) return;

        try
        {
            var fileName = System.IO.Path.GetFileName(filePath);
            var fileContent = await System.IO.File.ReadAllBytesAsync(filePath);
            var base64Content = Convert.ToBase64String(fileContent);

            var requestBody = JsonSerializer.Serialize(new
            {
                stream = base64Content,
                fileName = fileName,
                comment = $"Attachment for test: {testContext.TestName}",
                attachmentType = "GeneralAttachment"
            });

            var url = $"{_settings.OrganizationUrl}/{_settings.ProjectName}/_apis/test/runs/{_testRunId}/attachments?api-version=7.0";
            var response = await _httpClient.PostAsync(url, new StringContent(requestBody, Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();

            _logger.LogInformation("File {FileName} attached to test run", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to attach file {FilePath} to test run", filePath);
            throw;
        }
    }

    public async Task CompleteTestRunAsync()
    {
        if (!_settings.Enabled || string.IsNullOrEmpty(_testRunId)) return;

        try
        {
            var requestBody = JsonSerializer.Serialize(new
            {
                state = "Completed"
            });

            var url = $"{_settings.OrganizationUrl}/{_settings.ProjectName}/_apis/test/runs/{_testRunId}?api-version=7.0";
            var response = await _httpClient.PatchAsync(url, new StringContent(requestBody, Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();

            _logger.LogInformation("Test run {TestRunId} completed", _testRunId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to complete test run {TestRunId}", _testRunId);
            throw;
        }
    }
}