namespace TheSingularityWorkshop.Visualize.GPU;

/// <summary>Stable, API-neutral information about a selected GPU device.</summary>
/// <param name="Name">Human-readable device name.</param>
/// <param name="Backend">Name of the native adapter implementing this device.</param>
/// <param name="VendorId">Optional PCI/vendor identifier when the backend exposes it.</param>
/// <param name="DeviceId">Optional device identifier when the backend exposes it.</param>
/// <param name="DedicatedMemoryBytes">Dedicated device memory in bytes, or null when unknown.</param>
/// <param name="IsSoftware">Whether the backend identifies this as a software device.</param>
public sealed record GpuDeviceInfo(
    string Name,
    string Backend,
    uint? VendorId = null,
    uint? DeviceId = null,
    ulong? DedicatedMemoryBytes = null,
    bool IsSoftware = false);
