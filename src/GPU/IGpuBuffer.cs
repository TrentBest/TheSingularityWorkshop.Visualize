namespace TheSingularityWorkshop.Visualize.GPU;

/// <summary>A GPU allocation with an explicit size, usage, and asynchronous lifetime.</summary>
/// <remarks>
/// The native handle is intentionally not exposed. Adapters own API-specific handles and must
/// ensure that disposal respects in-flight submissions according to the backend's synchronization model.
/// </remarks>
public interface IGpuBuffer : IAsyncDisposable
{
    /// <summary>Gets the descriptor used to create this allocation.</summary>
    GpuBufferDescriptor Descriptor { get; }

    /// <summary>Gets whether this allocation has been disposed.</summary>
    bool IsDisposed { get; }
}
