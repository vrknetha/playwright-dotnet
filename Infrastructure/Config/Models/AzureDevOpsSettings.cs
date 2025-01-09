namespace ParkPlaceSample.Infrastructure.Config.Models;

public class AzureDevOpsSettings
{
    public bool Enabled { get; set; } = false;
    public string OrganizationUrl { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public string PersonalAccessToken { get; set; } = string.Empty;
    public string TestPlanId { get; set; } = string.Empty;
    public string TestSuiteId { get; set; } = string.Empty;
}