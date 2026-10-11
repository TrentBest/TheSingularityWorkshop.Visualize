using TheSingularityWorkshop.Renderer;

namespace TheSingularityWorkshop.Visualize;

/// <summary>Executes a platform-neutral Renderer frame through a concrete presentation backend.</summary>
/// <remarks>
/// Implementations own device resources and synchronization. They must not make GPU output authoritative
/// world state. A backend is not considered production-ready until it presents a real GPU-backed frame.
/// </remarks>
public interface IVisualizerBackend : IAsyncDisposable
{
    /// <summary>Gets the backend's current capability snapshot.</summary>
    VisualizerCapabilities Capabilities { get; }

    /// <summary>Submits a Renderer frame for presentation.</summary>
    /// <param name="frame">The observer-relative frame selected by Renderer.</param>
    /// <param name="resources">Provides logical geometry and material descriptions for the frame.</param>
    /// <param name="cancellationToken">Cancels work that has not yet been submitted to the device.</param>
    ValueTask<VisualizerFrameResult> RenderAsync(
        RenderFrame frame,
        IVisualizerResourceProvider resources,
        CancellationToken cancellationToken = default);
}
