namespace ParkPlaceSample.Infrastructure.Config.Models;

public class EnvironmentSettings
{
    public string Name { get; set; } = "Development";
    public string BaseUrl { get; set; } = string.Empty;
    public string ApiBaseUrl { get; set; } = string.Empty;
}