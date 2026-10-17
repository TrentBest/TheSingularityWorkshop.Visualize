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

    /// <summary>Adapts a source renderer frame and submits the resulting snapshot.</summary>
    public ValueTask<VisualizerFrameResult> RenderAsync<TSourceFrame>(
        TSourceFrame sourceFrame,
        IVisualizerFrameAdapter<TSourceFrame> adapter,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(adapter);
        return RenderAsync(adapter.Adapt(sourceFrame), cancellationToken);
    }

    /// <summary>Validates referenced resources and submits a frame to the backend.</summary>
    public ValueTask<VisualizerFrameResult> RenderAsync(
        VisualizerFrame frame,
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(Volatile.Read(ref _disposed) != 0, this);
        ArgumentNullException.ThrowIfNull(frame);
        cancellationToken.ThrowIfCancellationRequested();

        for (var i = 0; i < frame.Commands.Count; i++)
        {
            var command = frame.Commands[i];
            if (!_resources.TryGetGeometry(command.Geometry, out var geometry) || geometry is null)
                throw new InvalidOperationException($"Frame command {i} references missing geometry resource {command.Geometry.Value}.");
            if ((long)command.FirstIndex + command.IndexCount > geometry.Indices.Count)
                throw new InvalidOperationException($"Frame command {i} requests indices outside geometry resource {command.Geometry.Value}.");

            // BaseVertex is a vertex offset, not an offset into the index buffer.
            // Validate the effective vertex indices before handing the frame to any backend.
            for (var index = command.FirstIndex; index < command.FirstIndex + command.IndexCount; index++)
            {
                var vertexIndex = (long)geometry.Indices[index] + command.BaseVertex;
                if (vertexIndex < 0 || vertexIndex >= geometry.Vertices.Count)
                    throw new InvalidOperationException(
                        $"Frame command {i} resolves index {index} to vertex {vertexIndex}, outside geometry resource {command.Geometry.Value}.");
            }

            if (!_resources.TryGetMaterial(command.Material, out var material) || material is null)
                throw new InvalidOperationException($"Frame command {i} references missing material resource {command.Material.Value}.");

            if (command.Instances.IsValid)
            {
                if (!_resources.TryGetInstances(command.Instances, out var instances) || instances is null)
                    throw new InvalidOperationException($"Frame command {i} references missing instance resource {command.Instances.Value}.");
                if ((long)command.FirstInstance + command.InstanceCount > instances.Instances.Count)
                    throw new InvalidOperationException($"Frame command {i} requests instances outside instance resource {command.Instances.Value}.");
            }
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
