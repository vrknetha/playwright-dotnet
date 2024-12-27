namespace ParkPlaceSample.Config;

/// <summary>
/// Represents retry configuration settings.
/// </summary>
public class RetrySettings
{
    /// <summary>
    /// Gets or sets the maximum number of retry attempts.
    /// </summary>
    public int MaxAttempts { get; set; } = 3;

    /// <summary>
    /// Gets or sets the interval between retry attempts in seconds.
    /// </summary>
    public int RetryIntervalSeconds { get; set; } = 1;
}