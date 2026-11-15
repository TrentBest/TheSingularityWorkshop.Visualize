namespace TheSingularityWorkshop.Visualize;

/// <summary>Stable identity for a retained presentation image, separate from geometry/material resources.</summary>
public readonly record struct VisualizerImageId(ulong Value)
{
    /// <summary>Gets whether this identity is assigned.</summary>
    public bool IsValid => Value != 0;
}

/// <summary>Describes the source and observer provenance of a retained image.</summary>
/// <remarks>
/// Observer revision is an exact reuse key, not a reprojection claim. Reuse after observer movement
/// requires a future coverage/depth-aware contract and must not be inferred from this descriptor.
/// </remarks>
public sealed record VisualizerRetainedImageDescriptor
{
    /// <summary>Creates a retained-image descriptor.</summary>
    public VisualizerRetainedImageDescriptor(
        VisualizerImageId image,
        ulong sourceBucket,
        ulong contentRevision,
        ulong sourceRevision,
        ulong observerRevision,
        int width,
        int height)
    {
        if (!image.IsValid) throw new ArgumentException("A valid image identity is required.", nameof(image));
        if (sourceBucket == 0) throw new ArgumentOutOfRangeException(nameof(sourceBucket));
        if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
        if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));

        Image = image;
        SourceBucket = sourceBucket;
        ContentRevision = contentRevision;
        SourceRevision = sourceRevision;
        ObserverRevision = observerRevision;
        Width = width;
        Height = height;
    }

    /// <summary>Gets the stable image identity.</summary>
    public VisualizerImageId Image { get; }
    /// <summary>Gets the upstream Renderer bucket identity, carried as opaque provenance.</summary>
    public ulong SourceBucket { get; }
    /// <summary>Gets the image content revision.</summary>
    public ulong ContentRevision { get; }
    /// <summary>Gets the source-data revision used to produce the image.</summary>
    public ulong SourceRevision { get; }
    /// <summary>Gets the observer revision for which the image was produced.</summary>
    public ulong ObserverRevision { get; }
    /// <summary>Gets the image width in pixels.</summary>
    public int Width { get; }
    /// <summary>Gets the image height in pixels.</summary>
    public int Height { get; }
}

/// <summary>References a retained image for ordered frame composition.</summary>
/// <param name="Image">Stable identity of the retained image.</param>
/// <param name="Layer">Back-to-front composition layer; lower values are composed first.</param>
public readonly record struct VisualizerImageComposition(VisualizerImageId Image, int Layer);

/// <summary>
/// CPU-side metadata registry for retained images. It owns no pixel storage, GPU handles, or synchronization.
/// </summary>
public sealed class VisualizerRetainedImageCatalog
{
    private readonly Dictionary<VisualizerImageId, VisualizerRetainedImageDescriptor> _images = [];

    /// <summary>Registers or replaces the current descriptor for an image identity.</summary>
    public void Set(VisualizerRetainedImageDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        _images[descriptor.Image] = descriptor;
    }

    /// <summary>Removes an image identity and its descriptor.</summary>
    public bool Remove(VisualizerImageId image) => _images.Remove(image);

    /// <summary>
    /// Gets an image only when every provenance and extent field matches the requested descriptor.
    /// This deliberately conservative rule prevents stale content or observer-specific imagery from being reused.
    /// </summary>
    public bool TryGetReusable(
        VisualizerRetainedImageDescriptor requested,
        out VisualizerRetainedImageDescriptor? retained)
    {
        ArgumentNullException.ThrowIfNull(requested);
        if (_images.TryGetValue(requested.Image, out var current) && current == requested)
        {
            retained = current;
            return true;
        }

        retained = null;
        return false;
    }
}
