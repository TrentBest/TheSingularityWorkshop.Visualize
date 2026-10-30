namespace TheSingularityWorkshop.Visualize.GPU;

/// <summary>Describes a backend enumeration failure without hiding failures behind an empty device list.</summary>
public sealed record GpuBackendDiscoveryFailure(string Backend, string ExceptionType, string Message);

/// <summary>Snapshot of host facts, discovered adapters, and backend enumeration failures.</summary>
public sealed record GpuSystemDetectionResult(
    GpuHostInfo Host,
    DateTimeOffset DetectedAt,
    IReadOnlyList<GpuAdapterInfo> Adapters,
    IReadOnlyList<GpuBackendDiscoveryFailure> Failures)
{
    /// <summary>True when at least one backend returned an adapter.</summary>
    public bool HasAdapters => Adapters.Count > 0;

    /// <summary>True when every configured backend completed enumeration successfully.</summary>
    public bool IsComplete => Failures.Count == 0;
}
