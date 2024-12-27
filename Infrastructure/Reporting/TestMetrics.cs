using System.Text;

namespace ParkPlaceSample.Infrastructure.Reporting;

public class TestExecutionMetric
{
    public string TestName { get; set; } = "";
    public TimeSpan Duration { get; set; }
    public bool Passed { get; set; }
    public string FailureStep { get; set; } = "";
    public string Category { get; set; } = "";
}

public static class TestMetricsManager
{
    private static readonly Dictionary<string, List<TestExecutionMetric>> _testMetrics = new();

    public static void InitializeTest(string testName)
    {
        if (_testMetrics.ContainsKey(testName))
        {
            _testMetrics[testName].Clear();
        }
        else
        {
            _testMetrics[testName] = new List<TestExecutionMetric>();
        }
    }

    public static void RecordTestResult(string testName, TimeSpan duration, bool passed, string failureStep, string category)
    {
        var metric = new TestExecutionMetric
        {
            TestName = testName,
            Duration = duration,
            Passed = passed,
            FailureStep = failureStep,
            Category = category
        };

        if (!_testMetrics.ContainsKey(testName))
        {
            _testMetrics[testName] = new List<TestExecutionMetric>();
        }
        _testMetrics[testName].Add(metric);
    }

    public static string GenerateMetricsReport()
    {
        if (!_testMetrics.Any()) return string.Empty;

        var dashboardHtml = new StringBuilder();
        dashboardHtml.AppendLine(@"<div class='test-analysis'>");

        // Add timing statistics
        var allMetrics = _testMetrics.SelectMany(x => x.Value)
                                   .GroupBy(x => x.TestName)
                                   .Select(g => g.Last()) // Take only the last execution of each test
                                   .ToList();

        var avgDuration = allMetrics.Average(m => m.Duration.TotalSeconds);
        var slowestTest = allMetrics.OrderByDescending(m => m.Duration).First();
        var fastestTest = allMetrics.OrderBy(m => m.Duration).First();

        dashboardHtml.AppendLine(@"<div class='timing-stats'>");
        dashboardHtml.AppendLine("<h4>📊 Test Execution Analysis</h4>");
        dashboardHtml.AppendLine($"<div class='timing-item'>Average Duration: {avgDuration:F2} seconds</div>");
        dashboardHtml.AppendLine($"<div class='timing-item'>Slowest Test: {slowestTest.TestName} ({slowestTest.Duration.TotalSeconds:F2}s)</div>");
        dashboardHtml.AppendLine($"<div class='timing-item'>Fastest Test: {fastestTest.TestName} ({fastestTest.Duration.TotalSeconds:F2}s)</div>");
        dashboardHtml.AppendLine("</div>");

        // Add failure patterns if any tests failed
        var failedTests = allMetrics.Where(m => !m.Passed).ToList();
        if (failedTests.Any())
        {
            dashboardHtml.AppendLine(@"<div class='failure-patterns'>");
            dashboardHtml.AppendLine("<h4>❌ Failure Analysis</h4>");
            foreach (var failedTest in failedTests)
            {
                dashboardHtml.AppendLine($@"<div class='failure-pattern'>
                    <strong>{failedTest.TestName}</strong><br/>
                    Failed at step: {failedTest.FailureStep}<br/>
                    Category: {failedTest.Category}
                </div>");
            }
            dashboardHtml.AppendLine("</div>");
        }

        dashboardHtml.AppendLine("</div>");
        return dashboardHtml.ToString();
    }
}