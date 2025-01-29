using System.Text;
using System.Web;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using System.Text.Json;
using System.Collections.Concurrent;

namespace PlaywrightDemo.Infrastructure.Reporting;

public class TestExecutionMetric
{
    public string TestName { get; set; } = "";
    public TimeSpan Duration { get; set; }
    public bool Passed { get; set; }
    public string FailureStep { get; set; } = "";
    public string Category { get; set; } = "";
    public string Description { get; set; } = "";
    public TestStatus Status { get; set; }
    public string ErrorMessage { get; set; } = "";
    public string StackTrace { get; set; } = "";
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}

// Custom thread-safe HashSet implementation
public class ConcurrentHashSet<T> : ConcurrentDictionary<T, byte>
{
    public bool Add(T item) => TryAdd(item, 0);
    public bool Remove(T item) => TryRemove(item, out _);
    public bool Contains(T item) => ContainsKey(item);
}

public static class TestMetricsManager
{
    private static readonly ConcurrentDictionary<string, List<TestExecutionMetric>> _testMetrics = new();
    private static readonly ConcurrentDictionary<string, DateTime> _testStartTimes = new();
    private static readonly ConcurrentHashSet<string> _executedTestsInCurrentRun = new();
    private const string METRICS_FILE = "TestResults/metrics_history.json";
    private const string METRICS_LOCK_FILE = "TestResults/metrics.lock";
    private static readonly SemaphoreSlim _metricsSemaphore = new(1, 1);
    private static readonly TimeSpan LOCK_TIMEOUT = TimeSpan.FromMinutes(5);

    static TestMetricsManager()
    {
        InitializeMetrics();
    }

    private static void InitializeMetrics()
    {
        try
        {
            // Ensure directories exist
            Directory.CreateDirectory(Path.GetDirectoryName(METRICS_FILE)!);

            // Initial load with retry logic
            var retryCount = 3;
            var delay = TimeSpan.FromSeconds(1);

            while (retryCount > 0)
            {
                try
                {
                    LoadMetricsHistory();
                    break;
                }
                catch (Exception ex) when (retryCount > 1)
                {
                    TestContext.WriteLine($"Retry {4 - retryCount}/3: Failed to load metrics: {ex.Message}");
                    Thread.Sleep(delay);
                    delay = TimeSpan.FromSeconds(delay.TotalSeconds * 2); // Exponential backoff
                    retryCount--;
                }
            }
        }
        catch (Exception ex)
        {
            TestContext.WriteLine($"Failed to initialize metrics after retries: {ex.Message}");
            // Continue with empty metrics rather than failing the test run
            _testMetrics.Clear();
        }
    }

    private static async Task<bool> AcquireFileLockAsync()
    {
        try
        {
            if (!await _metricsSemaphore.WaitAsync(LOCK_TIMEOUT))
            {
                TestContext.WriteLine("Warning: Timeout waiting for metrics lock");
                return false;
            }

            var lockPath = METRICS_LOCK_FILE;
            var lockInfo = new
            {
                MachineName = Environment.MachineName,
                ProcessId = Environment.ProcessId,
                Timestamp = DateTime.UtcNow,
                BuildId = Environment.GetEnvironmentVariable("BUILD_BUILDID") ?? "local"
            };

            File.WriteAllText(lockPath, JsonSerializer.Serialize(lockInfo));
            return true;
        }
        catch (Exception ex)
        {
            TestContext.WriteLine($"Warning: Failed to acquire file lock: {ex.Message}");
            return false;
        }
    }

    private static void ReleaseFileLock()
    {
        try
        {
            if (File.Exists(METRICS_LOCK_FILE))
            {
                File.Delete(METRICS_LOCK_FILE);
            }
            _metricsSemaphore.Release();
        }
        catch (Exception ex)
        {
            TestContext.WriteLine($"Warning: Failed to release file lock: {ex.Message}");
        }
    }

    private static async Task SaveMetricsHistoryAsync()
    {
        if (!await AcquireFileLockAsync())
        {
            TestContext.WriteLine("Warning: Skipping metrics save due to lock acquisition failure");
            return;
        }

        try
        {
            var updatedMetrics = new ConcurrentDictionary<string, List<TestExecutionMetric>>();

            // Process executed tests first
            foreach (var testName in _executedTestsInCurrentRun.Keys)
            {
                if (_testMetrics.TryGetValue(testName, out var metrics))
                {
                    updatedMetrics[testName] = metrics
                        .OrderByDescending(m => m.EndTime)
                        .Take(10)
                        .ToList();
                }
            }

            // Add non-executed tests
            foreach (var kvp in _testMetrics)
            {
                if (!_executedTestsInCurrentRun.ContainsKey(kvp.Key))
                {
                    updatedMetrics[kvp.Key] = kvp.Value;
                }
            }

            // Create backup before saving
            if (File.Exists(METRICS_FILE))
            {
                File.Copy(METRICS_FILE, $"{METRICS_FILE}.bak", true);
            }

            var json = JsonSerializer.Serialize(updatedMetrics, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            await File.WriteAllTextAsync(METRICS_FILE, json);

            // Clean up backup after successful save
            if (File.Exists($"{METRICS_FILE}.bak"))
            {
                File.Delete($"{METRICS_FILE}.bak");
            }
        }
        catch (Exception ex)
        {
            TestContext.WriteLine($"Warning: Failed to save metrics: {ex.Message}");

            // Attempt to restore from backup
            if (File.Exists($"{METRICS_FILE}.bak"))
            {
                try
                {
                    File.Copy($"{METRICS_FILE}.bak", METRICS_FILE, true);
                    TestContext.WriteLine("Restored metrics from backup");
                }
                catch
                {
                    TestContext.WriteLine("Failed to restore metrics from backup");
                }
            }
        }
        finally
        {
            ReleaseFileLock();
        }
    }

    public static async Task RecordTestResultAsync(string testName, TimeSpan duration, bool passed, string failureStep, string category)
    {
        var metric = CreateTestMetric(testName, duration, passed, failureStep, category);

        _testMetrics.AddOrUpdate(
            testName,
            new List<TestExecutionMetric> { metric },
            (_, existing) =>
            {
                var updated = new List<TestExecutionMetric>(existing) { metric };
                return updated.OrderByDescending(m => m.EndTime).Take(10).ToList();
            }
        );

        _testStartTimes.TryRemove(testName, out _);
        await SaveMetricsHistoryAsync();
    }

    private static TestExecutionMetric CreateTestMetric(string testName, TimeSpan duration, bool passed, string failureStep, string category)
    {
        var currentTest = TestContext.CurrentContext.Test;
        var result = TestContext.CurrentContext.Result;

        return new TestExecutionMetric
        {
            TestName = testName,
            Duration = duration,
            Passed = passed,
            FailureStep = failureStep,
            Category = category,
            Description = currentTest.Properties["Description"]?.Cast<string>().FirstOrDefault() ?? "",
            Status = result.Outcome.Status,
            ErrorMessage = result.Message ?? "",
            StackTrace = result.StackTrace ?? "",
            StartTime = _testStartTimes.GetValueOrDefault(testName, DateTime.Now.AddSeconds(-duration.TotalSeconds)),
            EndTime = DateTime.Now
        };
    }

    public static void InitializeTest(string testName)
    {
        _executedTestsInCurrentRun.Add(testName);
        _testStartTimes[testName] = DateTime.Now;
    }

    public static string GenerateMetricsReport()
    {
        if (!_testMetrics.Any()) return string.Empty;

        var dashboardHtml = new StringBuilder();
        dashboardHtml.AppendLine(@"<div class='test-analysis'>");

        // Add summary statistics
        var allMetrics = _testMetrics.SelectMany(x => x.Value)
                                   .GroupBy(x => x.TestName)
                                   .Select(g => g.Last()) // Take only the last execution of each test
                                   .ToList();

        var totalTests = allMetrics.Count;
        var passedTests = allMetrics.Count(m => m.Passed);
        var failedTests = allMetrics.Count(m => !m.Passed);
        var avgDuration = allMetrics.Average(m => m.Duration.TotalSeconds);
        var slowestTest = allMetrics.OrderByDescending(m => m.Duration).First();
        var fastestTest = allMetrics.OrderBy(m => m.Duration).First();

        dashboardHtml.AppendLine(@"<div class='test-summary'>");
        dashboardHtml.AppendLine("<h3>📊 Test Execution Summary</h3>");
        dashboardHtml.AppendLine("<div class='summary-grid'>");
        dashboardHtml.AppendLine($@"
            <div class='summary-item'>
                <span class='label'>Total Tests</span>
                <span class='value'>{totalTests}</span>
            </div>
            <div class='summary-item {(failedTests == 0 ? "success" : "failure")}'>
                <span class='label'>Pass Rate</span>
                <span class='value'>{(double)passedTests / totalTests:P0}</span>
            </div>
            <div class='summary-item'>
                <span class='label'>Average Duration</span>
                <span class='value'>{avgDuration:F2}s</span>
            </div>");
        dashboardHtml.AppendLine("</div></div>");

        // Add timing statistics
        dashboardHtml.AppendLine(@"<div class='timing-stats'>");
        dashboardHtml.AppendLine("<h3>⏱️ Performance Analysis</h3>");
        dashboardHtml.AppendLine($@"
            <div class='timing-details'>
                <div class='timing-item'>
                    <span class='label'>Slowest Test</span>
                    <span class='value'>{slowestTest.TestName}</span>
                    <span class='duration'>{slowestTest.Duration.TotalSeconds:F2}s</span>
                </div>
                <div class='timing-item'>
                    <span class='label'>Fastest Test</span>
                    <span class='value'>{fastestTest.TestName}</span>
                    <span class='duration'>{fastestTest.Duration.TotalSeconds:F2}s</span>
                </div>
            </div>");
        dashboardHtml.AppendLine("</div>");

        // Add category analysis
        var categoryMetrics = allMetrics.GroupBy(m => m.Category)
                                      .Select(g => new
                                      {
                                          Category = g.Key,
                                          Total = g.Count(),
                                          Passed = g.Count(m => m.Passed),
                                          AvgDuration = g.Average(m => m.Duration.TotalSeconds)
                                      })
                                      .OrderByDescending(c => c.Total);

        dashboardHtml.AppendLine(@"<div class='category-analysis'>");
        dashboardHtml.AppendLine("<h3>📑 Category Analysis</h3>");
        dashboardHtml.AppendLine("<div class='category-grid'>");
        foreach (var category in categoryMetrics)
        {
            dashboardHtml.AppendLine($@"
                <div class='category-item'>
                    <div class='category-name'>{category.Category}</div>
                    <div class='category-stats'>
                        <span class='stat'>Total: {category.Total}</span>
                        <span class='stat'>Pass Rate: {(double)category.Passed / category.Total:P0}</span>
                        <span class='stat'>Avg Duration: {category.AvgDuration:F2}s</span>
                    </div>
                </div>");
        }
        dashboardHtml.AppendLine("</div></div>");

        // Add failure analysis if any tests failed
        var failedTestsList = allMetrics.Where(m => !m.Passed).ToList();
        if (failedTestsList.Any())
        {
            dashboardHtml.AppendLine(@"<div class='failure-analysis'>");
            dashboardHtml.AppendLine("<h3>❌ Failure Analysis</h3>");
            foreach (var failedTest in failedTestsList)
            {
                dashboardHtml.AppendLine($@"
                    <div class='failure-item'>
                        <div class='failure-header'>
                            <span class='test-name'>{failedTest.TestName}</span>
                            <span class='category-badge'>{failedTest.Category}</span>
                        </div>
                        <div class='failure-details'>
                            <div class='error-message'>{HttpUtility.HtmlEncode(failedTest.ErrorMessage)}</div>
                            <div class='failure-step'>Failed at: {failedTest.FailureStep}</div>
                            <div class='timing'>Duration: {failedTest.Duration.TotalSeconds:F2}s</div>
                            <pre class='stack-trace'>{HttpUtility.HtmlEncode(failedTest.StackTrace)}</pre>
                        </div>
                    </div>");
            }
            dashboardHtml.AppendLine("</div>");
        }

        dashboardHtml.AppendLine("</div>");
        return dashboardHtml.ToString();
    }

    public static IReadOnlyList<TestExecutionMetric> GetTestMetrics(string testName)
    {
        return _testMetrics.TryGetValue(testName, out var metrics)
            ? metrics.AsReadOnly()
            : new List<TestExecutionMetric>().AsReadOnly();
    }

    public static IReadOnlyDictionary<string, List<TestExecutionMetric>> GetAllTestMetrics()
    {
        return _testMetrics.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.ToList()
        );
    }

    public static IReadOnlyDictionary<string, List<TestExecutionMetric>> GetExecutedTestMetrics()
    {
        return _testMetrics
            .Where(kvp => _executedTestsInCurrentRun.Contains(kvp.Key))
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.ToList()
            );
    }

    public static void ClearMetrics()
    {
        _metricsSemaphore.Wait();
        try
        {
            _testMetrics.Clear();
            _testStartTimes.Clear();
            _executedTestsInCurrentRun.Clear();
            if (File.Exists(METRICS_FILE))
            {
                File.Delete(METRICS_FILE);
            }
            if (File.Exists(METRICS_LOCK_FILE))
            {
                File.Delete(METRICS_LOCK_FILE);
            }
        }
        finally
        {
            _metricsSemaphore.Release();
        }
    }

    public static void PruneMetricsHistory(int daysToKeep = 30)
    {
        var cutoffDate = DateTime.Now.AddDays(-daysToKeep);
        var modified = false;

        foreach (var metrics in _testMetrics.Values)
        {
            var oldCount = metrics.Count;
            metrics.RemoveAll(m => m.EndTime < cutoffDate);
            if (metrics.Count != oldCount)
            {
                modified = true;
            }
        }

        if (modified)
        {
            SaveMetricsHistoryAsync().GetAwaiter().GetResult();
        }
    }

    private static void LoadMetricsHistory()
    {
        if (!File.Exists(METRICS_FILE)) return;

        var json = File.ReadAllText(METRICS_FILE);
        var metrics = JsonSerializer.Deserialize<Dictionary<string, List<TestExecutionMetric>>>(json);
        if (metrics == null) return;

        foreach (var kvp in metrics)
        {
            _testMetrics[kvp.Key] = kvp.Value;
        }
    }

    public static string GetMetricsFilePath() => METRICS_FILE;

    public static async Task PruneMetricsHistoryAsync(int daysToKeep = 30)
    {
        var cutoffDate = DateTime.Now.AddDays(-daysToKeep);
        var modified = false;

        foreach (var metrics in _testMetrics.Values)
        {
            var oldCount = metrics.Count;
            metrics.RemoveAll(m => m.EndTime < cutoffDate);
            if (metrics.Count != oldCount)
            {
                modified = true;
            }
        }

        if (modified)
        {
            await SaveMetricsHistoryAsync();
        }
    }

    public static async Task<IReadOnlyDictionary<string, List<TestExecutionMetric>>> GetAllTestMetricsAsync()
    {
        return await Task.FromResult(_testMetrics.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.ToList()
        ));
    }

    public static async Task SaveMetricsAsync(IDictionary<string, List<TestExecutionMetric>> metrics)
    {
        if (!await AcquireFileLockAsync())
        {
            TestContext.WriteLine("Warning: Skipping metrics save due to lock acquisition failure");
            return;
        }

        try
        {
            foreach (var kvp in metrics)
            {
                _testMetrics.AddOrUpdate(
                    kvp.Key,
                    kvp.Value,
                    (_, _) => kvp.Value
                );
            }

            await SaveMetricsHistoryAsync();
        }
        finally
        {
            ReleaseFileLock();
        }
    }
}

public class TestResult
{
    public bool IsSuccess { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
}

public class TestMetrics<T> where T : notnull
{
    private readonly ConcurrentDictionary<T, TestResult> _results = new();

    public void AddResult(T key, TestResult result)
    {
        _results.TryAdd(key, result);
    }

    public TestResult? GetResult(T key)
    {
        return _results.TryGetValue(key, out var result) ? result : null;
    }
}