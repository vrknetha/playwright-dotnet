namespace ParkPlaceSample.Infrastructure.Config.Models;

public enum TraceMode
{
    All,
    RetainOnFailure,
    OnFailure
}

public class TraceSettings
{
    public bool Enabled { get; set; } = true;
    public string Directory { get; set; } = "Traces";
    public TraceMode Mode { get; set; } = TraceMode.OnFailure;
    public TraceOptions Options { get; set; } = new();
}

public class TraceOptions
{
    public bool Screenshots { get; set; } = true;
    public bool Snapshots { get; set; } = true;
    public bool Sources { get; set; } = true;
    public bool Network { get; set; } = true;
    public int SnapshotInterval { get; set; } = 100; // Milliseconds between snapshots
}