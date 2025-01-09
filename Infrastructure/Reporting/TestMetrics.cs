using System.Text;
using System.Web;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace ParkPlaceSample.Infrastructure.Reporting;

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

public static class TestMetricsManager
{
    private static readonly Dictionary<string, List<TestExecutionMetric>> _testMetrics = new();
    private static readonly Dictionary<string, DateTime> _testStartTimes = new();

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

        _testStartTimes[testName] = DateTime.Now;
    }

    public static void RecordTestResult(string testName, TimeSpan duration, bool passed, string failureStep, string category)
    {
        var currentTest = TestContext.CurrentContext.Test;
        var result = TestContext.CurrentContext.Result;

        var metric = new TestExecutionMetric
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

        if (!_testMetrics.ContainsKey(testName))
        {
            _testMetrics[testName] = new List<TestExecutionMetric>();
        }
        _testMetrics[testName].Add(metric);

        // Clean up start time
        _testStartTimes.Remove(testName);
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

    public static void ClearMetrics()
    {
        _testMetrics.Clear();
        _testStartTimes.Clear();
    }
}