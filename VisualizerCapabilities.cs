namespace TheSingularityWorkshop.Visualize;

/// <summary>Describes features explicitly supported by a visualizer backend.</summary>
/// <remarks>Capabilities are runtime facts, not promises that every backend implements every feature.</remarks>
public sealed record VisualizerCapabilities
{
    /// <summary>Initializes a capability snapshot.</summary>
    public VisualizerCapabilities(
        string backendName,
        string apiName,
        bool supportsCompute,
        bool supportsInstancing,
        bool supportsIndirectDrawing,
        bool supportsPresentationSurface,
        int maximumTextureDimension = 0,
        int maximumBufferBytes = 0)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(backendName);
        ArgumentException.ThrowIfNullOrWhiteSpace(apiName);
        if (maximumTextureDimension < 0)
            throw new ArgumentOutOfRangeException(nameof(maximumTextureDimension));
        if (maximumBufferBytes < 0)
            throw new ArgumentOutOfRangeException(nameof(maximumBufferBytes));

        BackendName = backendName;
        ApiName = apiName;
        SupportsCompute = supportsCompute;
        SupportsInstancing = supportsInstancing;
        SupportsIndirectDrawing = supportsIndirectDrawing;
        SupportsPresentationSurface = supportsPresentationSurface;
        MaximumTextureDimension = maximumTextureDimension;
        MaximumBufferBytes = maximumBufferBytes;
    }

    /// <summary>Gets the backend implementation name.</summary>
    public string BackendName { get; }
    /// <summary>Gets the underlying graphics API or execution mechanism.</summary>
    public string ApiName { get; }
    /// <summary>Gets whether general-purpose compute work is supported.</summary>
    public bool SupportsCompute { get; }
    /// <summary>Gets whether instanced drawing is supported.</summary>
    public bool SupportsInstancing { get; }
    /// <summary>Gets whether indirect drawing is supported.</summary>
    public bool SupportsIndirectDrawing { get; }
    /// <summary>Gets whether the backend can present to a host surface.</summary>
    public bool SupportsPresentationSurface { get; }
    /// <summary>Gets the maximum texture dimension when known; zero means not reported.</summary>
    public int MaximumTextureDimension { get; }
    /// <summary>Gets the maximum single buffer size in bytes when known; zero means not reported.</summary>
    public int MaximumBufferBytes { get; }
}
