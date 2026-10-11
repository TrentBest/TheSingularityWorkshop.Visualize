using TheSingularityWorkshop.Renderer;

namespace TheSingularityWorkshop.Visualize;

/// <summary>Coordinates Renderer frames, logical resources, and a concrete presentation backend.</summary>
/// <remarks>This class is orchestration only; pixels are produced by an actual backend implementation.</remarks>
public sealed class Visualizer : IAsyncDisposable
{
    private readonly IVisualizerBackend _backend;
    private readonly IVisualizerResourceProvider _resources;
    private int _disposed;

    /// <summary>Creates a Visualizer around a backend and resource provider.</summary>
    public Visualizer(IVisualizerBackend backend, IVisualizerResourceProvider resources)
    {
        ArgumentNullException.ThrowIfNull(backend);
        ArgumentNullException.ThrowIfNull(resources);
        _backend = backend;
        _resources = resources;
    }

    /// <summary>Gets the active backend's capability report.</summary>
    public VisualizerCapabilities Capabilities => _backend.Capabilities;

    /// <summary>Validates referenced resources and submits a frame to the backend.</summary>
    public ValueTask<VisualizerFrameResult> RenderAsync(
        RenderFrame frame,
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(Volatile.Read(ref _disposed) != 0, this);
        ArgumentNullException.ThrowIfNull(frame);
        cancellationToken.ThrowIfCancellationRequested();

        for (var i = 0; i < frame.Commands.Count; i++)
        {
            var command = frame.Commands[i];
            if (!_resources.TryGetGeometry(command.Geometry, out _))
                throw new InvalidOperationException($"Frame command {i} references missing geometry resource {command.Geometry.Value}.");
            if (!_resources.TryGetMaterial(command.Material, out _))
                throw new InvalidOperationException($"Frame command {i} references missing material resource {command.Material.Value}.");
        }

        return _backend.RenderAsync(frame, _resources, cancellationToken);
    }

    /// <summary>Disposes the backend and its device resources.</summary>
    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
            return;

        await _backend.DisposeAsync().ConfigureAwait(false);
    }
}
