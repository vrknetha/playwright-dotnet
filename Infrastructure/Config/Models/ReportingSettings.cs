using PlaywrightDemo.Infrastructure.Config.Models;

namespace PlaywrightDemo.Infrastructure.Config.Models;

public class ReportingSettings
{
    public string BaseDirectory { get; set; } = "TestResults";
    public TraceSettings Trace { get; set; } = new();
    public VideoSettings Video { get; set; } = new();
    public ScreenshotSettings Screenshots { get; set; } = new();
    public HtmlReportSettings HtmlReport { get; set; } = new();
}

public class VideoSettings
{
    public bool Enabled { get; set; } = true;
    public string Directory { get; set; } = "Videos";
    public bool RetainOnFailure { get; set; } = true;
    public VideoSize Size { get; set; } = new();
}

public class VideoSize
{
    public int Width { get; set; } = 1280;
    public int Height { get; set; } = 720;
}

public class ScreenshotSettings
{
    public bool Enabled { get; set; } = true;
    public string Directory { get; set; } = "Screenshots";
    public bool TakeOnFailure { get; set; } = true;
    public string Format { get; set; } = "png";
    public bool FullPage { get; set; } = true;
}

public class HtmlReportSettings
{
    public bool Enabled { get; set; } = true;
    public string Directory { get; set; } = "Report";
    public string Title { get; set; } = "Test Execution Report";
    public string Theme { get; set; } = "Standard";
    public bool EmbedScreenshots { get; set; } = true;
    public bool EmbedVideos { get; set; } = true;
    public bool EmbedTraces { get; set; } = true;
}