namespace PlaywrightDemo.Infrastructure.Config.Models;

public class BrowserSettings
{
    public string Type { get; set; } = "chromium";
    public bool Headless { get; set; } = false;
    public int SlowMo { get; set; } = 0;
    public ViewportSettings Viewport { get; set; } = new();
}

public class ViewportSettings
{
    public int Width { get; set; } = 1920;
    public int Height { get; set; } = 1080;
}