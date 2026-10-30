namespace TheSingularityWorkshop.Visualize.GPU;

/// <summary>Best-known classification of a physical or emulated adapter.</summary>
public enum GpuAdapterType
{
    /// <summary>The adapter type is not reported by its backend.</summary>
    Unknown,
    /// <summary>A discrete GPU.</summary>
    Discrete,
    /// <summary>An integrated GPU.</summary>
    Integrated,
    /// <summary>A virtual GPU or virtualized adapter.</summary>
    Virtual,
    /// <summary>A software rasterizer or software compute device.</summary>
    Software
}

/// <summary>A backend's raw, API-neutral summary of one enumerated adapter.</summary>
/// <remarks>Unknown values remain unknown; consumers must not infer capabilities or hardware identity from names or vendor IDs.</remarks>
public sealed record GpuAdapterInfo(
    string AdapterId,
    string Backend,
    string Name,
    GpuAdapterType Type = GpuAdapterType.Unknown,
    uint? VendorId = null,
    uint? DeviceId = null,
    string? DriverVersion = null,
    string? ApiVersion = null,
    ulong? DedicatedMemoryBytes = null,
    ulong? SharedMemoryBytes = null,
    IReadOnlyList<string>? SupportedFeatures = null,
    ulong? DeviceLocalMemoryBytes = null,
    ulong? NonDeviceLocalMemoryBytes = null);
