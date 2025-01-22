using System.Text.Json;
using NUnit.Framework;
using System.Collections.Concurrent;
using System.Threading;
using System.Reflection;

namespace PlaywrightDemo.Infrastructure.Reporting;

public class TestMetricsCleanup
{
    private const int MAX_METRICS_FILE_SIZE_MB = 50;
    private const int DEFAULT_RETENTION_DAYS = 30;
    private const int RETRY_COUNT = 3;
    private static readonly TimeSpan INITIAL_DELAY = TimeSpan.FromSeconds(1);

    public static async Task PerformCleanup()
    {
        await Task.WhenAll(
            CleanupOldMetrics(),
            CleanupRenamedTests(),
            EnforceFileSizeLimit()
        );
    }

    private static async Task CleanupOldMetrics()
    {
        await TestMetricsManager.PruneMetricsHistoryAsync(DEFAULT_RETENTION_DAYS);
    }

    private static async Task CleanupRenamedTests()
    {
        var currentTests = GetCurrentTestNames();
        var metrics = await TestMetricsManager.GetAllTestMetricsAsync();
        var renamedTests = new Dictionary<string, string>();

        // Find potential renamed tests based on similarity
        foreach (var oldTest in metrics.Keys)
        {
            if (!currentTests.Contains(oldTest))
            {
                var mostSimilar = FindMostSimilarTest(oldTest, currentTests);
                if (mostSimilar != null)
                {
                    renamedTests[oldTest] = mostSimilar;
                }
            }
        }

        // Log potential renames for review
        if (renamedTests.Any())
        {
            TestContext.WriteLine("Potential renamed tests detected:");
            foreach (var kvp in renamedTests)
            {
                TestContext.WriteLine($"  Old: {kvp.Key}");
                TestContext.WriteLine($"  New: {kvp.Value}");
                TestContext.WriteLine();
            }
        }
    }

    private static async Task EnforceFileSizeLimit()
    {
        var metricsFile = TestMetricsManager.GetMetricsFilePath();
        if (!File.Exists(metricsFile)) return;

        var fileInfo = new FileInfo(metricsFile);
        if (fileInfo.Length > MAX_METRICS_FILE_SIZE_MB * 1024 * 1024)
        {
            TestContext.WriteLine($"Metrics file exceeds {MAX_METRICS_FILE_SIZE_MB}MB limit. Performing cleanup...");

            var metrics = await TestMetricsManager.GetAllTestMetricsAsync();
            var mutableMetrics = metrics.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value
            );

            // Keep only last 5 runs for tests not executed in last 7 days
            var cutoffDate = DateTime.Now.AddDays(-7);
            foreach (var testMetrics in mutableMetrics.Values)
            {
                var lastRun = testMetrics.Max(m => m.EndTime);
                if (lastRun < cutoffDate)
                {
                    var trimmed = testMetrics
                        .OrderByDescending(m => m.EndTime)
                        .Take(5)
                        .ToList();
                    testMetrics.Clear();
                    testMetrics.AddRange(trimmed);
                }
            }

            await TestMetricsManager.SaveMetricsAsync(mutableMetrics);
        }
    }

    private static HashSet<string> GetCurrentTestNames()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var testNames = new HashSet<string>();

        foreach (var type in assembly.GetTypes())
        {
            if (type.GetCustomAttributes(typeof(TestFixtureAttribute), true).Any())
            {
                foreach (var method in type.GetMethods())
                {
                    if (method.GetCustomAttributes(typeof(TestAttribute), true).Any())
                    {
                        testNames.Add($"{type.FullName}.{method.Name}");
                    }
                }
            }
        }

        return testNames;
    }

    private static string? FindMostSimilarTest(string oldTest, HashSet<string> currentTests)
    {
        const double SIMILARITY_THRESHOLD = 0.8;
        var maxSimilarity = 0.0;
        string? mostSimilar = null;

        foreach (var currentTest in currentTests)
        {
            var similarity = CalculateSimilarity(oldTest, currentTest);
            if (similarity > maxSimilarity && similarity >= SIMILARITY_THRESHOLD)
            {
                maxSimilarity = similarity;
                mostSimilar = currentTest;
            }
        }

        return mostSimilar;
    }

    private static double CalculateSimilarity(string s1, string s2)
    {
        if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2)) return 0;

        // Simple Levenshtein distance implementation
        var distance = new int[s1.Length + 1, s2.Length + 1];

        for (int i = 0; i <= s1.Length; i++)
            distance[i, 0] = i;
        for (int j = 0; j <= s2.Length; j++)
            distance[0, j] = j;

        for (int i = 1; i <= s1.Length; i++)
        {
            for (int j = 1; j <= s2.Length; j++)
            {
                var cost = (s1[i - 1] == s2[j - 1]) ? 0 : 1;
                distance[i, j] = Math.Min(
                    Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1),
                    distance[i - 1, j - 1] + cost
                );
            }
        }

        var maxLength = Math.Max(s1.Length, s2.Length);
        return 1 - ((double)distance[s1.Length, s2.Length] / maxLength);
    }
}