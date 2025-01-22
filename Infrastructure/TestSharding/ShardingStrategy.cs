using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PlaywrightDemo.Infrastructure.Reporting;

namespace PlaywrightDemo.Infrastructure.TestSharding;

public class ShardingStrategy
{
    private readonly Dictionary<string, double> _testWeights = new();
    private const string SHARD_INDEX_ENV = "TEST_SHARD_INDEX";
    private const string TOTAL_SHARDS_ENV = "TEST_TOTAL_SHARDS";

    public static bool ShouldRunTest()
    {
        var shardIndex = GetShardIndex();
        var totalShards = GetTotalShards();

        if (shardIndex == -1 || totalShards == -1)
            return true; // If not running in sharded mode, run all tests

        var testName = TestContext.CurrentContext.Test.FullName;
        return new ShardingStrategy().GetShardForTest(testName) == shardIndex;
    }

    private int GetShardForTest(string testName)
    {
        LoadTestWeights();

        // Get all tests and their weights
        var testWeights = _testWeights.ToDictionary(
            kv => kv.Key,
            kv => kv.Value > 0 ? kv.Value : 1.0 // Use 1.0 as default weight if no history
        );

        // Calculate target weight per shard
        var totalWeight = testWeights.Values.Sum();
        var targetWeightPerShard = totalWeight / GetTotalShards();

        // Distribute tests to shards trying to keep weights balanced
        var shardWeights = new double[GetTotalShards()];
        var shardAssignments = new Dictionary<string, int>();

        // Sort tests by weight descending to place heaviest tests first
        var sortedTests = testWeights.OrderByDescending(kv => kv.Value);

        foreach (var test in sortedTests)
        {
            // Find the shard with minimum current weight
            var minWeightShard = Array.IndexOf(shardWeights, shardWeights.Min());
            shardAssignments[test.Key] = minWeightShard;
            shardWeights[minWeightShard] += test.Value;
        }

        return shardAssignments.TryGetValue(testName, out var shard) ? shard : 0;
    }

    private void LoadTestWeights()
    {
        var metrics = TestMetricsManager.GetAllTestMetrics();
        foreach (var metric in metrics)
        {
            // Use average duration if test was run multiple times
            var avgDuration = metric.Value
                .Where(m => m.Passed) // Only consider successful runs
                .Select(m => m.Duration.TotalSeconds)
                .DefaultIfEmpty(1.0) // Default to 1 second if no successful runs
                .Average();

            _testWeights[metric.Key] = avgDuration;
        }
    }

    private static int GetShardIndex() =>
        int.TryParse(Environment.GetEnvironmentVariable(SHARD_INDEX_ENV), out var index) ? index : -1;

    private static int GetTotalShards() =>
        int.TryParse(Environment.GetEnvironmentVariable(TOTAL_SHARDS_ENV), out var total) ? total : -1;
}