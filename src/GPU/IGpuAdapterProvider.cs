namespace TheSingularityWorkshop.Visualize.GPU;

/// <summary>Enumerates adapters exposed by one graphics or compute API.</summary>
/// <remarks>Implementations belong in backend adapter packages; this contract has no vendor or API binding dependency.</remarks>
public interface IGpuAdapterProvider
{
    /// <summary>Stable, human-readable backend identifier (for example, Vulkan or Direct3D 12).</summary>
    string Backend { get; }

    /// <summary>Enumerates all adapters exposed by this backend, not merely the primary display adapter.</summary>
    ValueTask<IReadOnlyList<GpuAdapterInfo>> EnumerateAdaptersAsync(
        CancellationToken cancellationToken = default);
}
