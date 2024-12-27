namespace ParkPlaceSample.Infrastructure.Tracing;

public class TraceSettings
{
    public bool Enabled { get; set; }
    public string Directory { get; set; } = "TestResults/Traces";
    public TracingMode Mode { get; set; }
    public bool Screenshots { get; set; }
    public bool Snapshots { get; set; }
    public bool Sources { get; set; }
}

public enum TracingMode
{
    Always,
    OnFailure,
    Never
}