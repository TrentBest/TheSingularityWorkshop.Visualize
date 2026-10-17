namespace TheSingularityWorkshop.Visualize.GPU;

/// <summary>Owns a selected GPU device and creates resources for explicit submission.</summary>
/// <remarks>
/// Implementations are native-API adapters. They must validate backend constraints, report useful
/// diagnostics, and defer reclamation of in-flight resources until synchronization makes it safe.
/// This contract intentionally does not expose API-specific handles or dictate a windowing toolkit.
/// </remarks>
public interface IGpuDevice : IAsyncDisposable
{
    /// <summary>Gets stable information about the selected device.</summary>
    GpuDeviceInfo Info { get; }

    /// <summary>Creates a buffer using an explicit size and usage declaration.</summary>
    ValueTask<IGpuBuffer> CreateBufferAsync(
        GpuBufferDescriptor descriptor,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Uploads one contiguous memory region into a GPU buffer.
    /// Implementations must reject disposed buffers and out-of-range writes.
    /// </summary>
    ValueTask UploadAsync(
        IGpuBuffer destination,
        ReadOnlyMemory<byte> source,
        ulong destinationOffset = 0,
        CancellationToken cancellationToken = default);
}
