using Microsoft.Extensions.Logging;
using ParkPlaceSample.Config;

namespace ParkPlaceSample.Infrastructure.Utilities;

public static class RetryUtility
{
    private static readonly RetrySettings DefaultSettings = new();

    public static async Task RetryAsync(Func<Task> action, int maxAttempts = 3, TimeSpan? retryInterval = null)
    {
        await RetryAsync(action, DefaultSettings, null);
    }

    public static async Task RetryAsync(Func<Task> action, RetrySettings settings, ILogger? logger = null)
    {
        var attempts = 0;
        var interval = TimeSpan.FromSeconds(settings.RetryIntervalSeconds);

        while (true)
        {
            try
            {
                attempts++;
                await action();
                return;
            }
            catch (Exception ex)
            {
                if (attempts >= settings.MaxAttempts)
                {
                    logger?.LogError(ex, "Failed after {Attempts} attempts", attempts);
                    throw;
                }

                logger?.LogWarning(ex, "Attempt {Attempt} of {MaxAttempts} failed. Retrying in {Interval} seconds",
                    attempts, settings.MaxAttempts, settings.RetryIntervalSeconds);

                await Task.Delay(interval);
            }
        }
    }

    public static async Task<T> RetryAsync<T>(Func<Task<T>> action, int maxAttempts = 3, TimeSpan? retryInterval = null)
    {
        return await RetryAsync(action, DefaultSettings, null);
    }

    public static async Task<T> RetryAsync<T>(Func<Task<T>> action, RetrySettings settings, ILogger? logger = null)
    {
        var attempts = 0;
        var interval = TimeSpan.FromSeconds(settings.RetryIntervalSeconds);

        while (true)
        {
            try
            {
                attempts++;
                return await action();
            }
            catch (Exception ex)
            {
                if (attempts >= settings.MaxAttempts)
                {
                    logger?.LogError(ex, "Failed after {Attempts} attempts", attempts);
                    throw;
                }

                logger?.LogWarning(ex, "Attempt {Attempt} of {MaxAttempts} failed. Retrying in {Interval} seconds",
                    attempts, settings.MaxAttempts, settings.RetryIntervalSeconds);

                await Task.Delay(interval);
            }
        }
    }
}