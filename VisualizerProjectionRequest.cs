using System.Numerics;

namespace TheSingularityWorkshop.Visualize;

/// <summary>
/// Identifies a rectangular region of source content to project to a destination.
/// </summary>
/// <remarks>
/// The coordinate-system label is supplied by the producer (for example, logical-pixels or normalized-units).
/// Source coordinates are intentionally independent of destination viewport dimensions.
/// </remarks>
public readonly record struct VisualizerSourceRectangle
{
    /// <summary>Creates a source rectangle with an explicit coordinate-system label.</summary>
    public VisualizerSourceRectangle(float x, float y, float width, float height, string coordinateSystem)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(coordinateSystem);
        if (!float.IsFinite(x)) throw new ArgumentOutOfRangeException(nameof(x));
        if (!float.IsFinite(y)) throw new ArgumentOutOfRangeException(nameof(y));
        if (!float.IsFinite(width) || width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
        if (!float.IsFinite(height) || height <= 0) throw new ArgumentOutOfRangeException(nameof(height));

        X = x;
        Y = y;
        Width = width;
        Height = height;
        CoordinateSystem = coordinateSystem;
    }

    /// <summary>Gets the source rectangle's horizontal origin in its declared coordinate system.</summary>
    public float X { get; }
    /// <summary>Gets the source rectangle's vertical origin in its declared coordinate system.</summary>
    public float Y { get; }
    /// <summary>Gets the positive source rectangle width.</summary>
    public float Width { get; }
    /// <summary>Gets the positive source rectangle height.</summary>
    public float Height { get; }
    /// <summary>Gets the producer-defined coordinate-system label for the source rectangle.</summary>
    public string CoordinateSystem { get; }
}

/// <summary>Distinguishes a screen-space destination from a world-space projection target.</summary>
public enum VisualizerProjectionSpace
{
    /// <summary>A host-managed screen or window surface.</summary>
    Screen = 0,
    /// <summary>A host-managed surface mapped into a world coordinate system.</summary>
    World = 1
}

/// <summary>
/// Describes where a source region should be projected, independently of GUI layout or native surface ownership.
/// </summary>
/// <remarks>
/// The host owns destination lifetime and layout. The adapter owns interpretation of the mapping matrix and
/// must document its matrix convention. This contract does not itself perform projection or imply GPU support.
/// </remarks>
public sealed record VisualizerProjectionTarget
{
    /// <summary>Creates a projection target description.</summary>
    public VisualizerProjectionTarget(
        string targetId,
        VisualizerProjectionSpace space,
        int viewportWidth,
        int viewportHeight,
        Matrix4x4 sourceToDestination)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(targetId);
        if (viewportWidth <= 0) throw new ArgumentOutOfRangeException(nameof(viewportWidth));
        if (viewportHeight <= 0) throw new ArgumentOutOfRangeException(nameof(viewportHeight));
        if (!Enum.IsDefined(space)) throw new ArgumentOutOfRangeException(nameof(space));
        if (!VisualizerMatrixValidation.IsFinite(sourceToDestination))
            throw new ArgumentOutOfRangeException(nameof(sourceToDestination), "Projection matrix components must be finite.");

        TargetId = targetId;
        Space = space;
        ViewportWidth = viewportWidth;
        ViewportHeight = viewportHeight;
        SourceToDestination = sourceToDestination;
    }

    /// <summary>Gets the stable host-defined identity of the destination target.</summary>
    public string TargetId { get; }
    /// <summary>Gets whether the destination is screen-space or world-space.</summary>
    public VisualizerProjectionSpace Space { get; }
    /// <summary>Gets the destination viewport width in pixels.</summary>
    public int ViewportWidth { get; }
    /// <summary>Gets the destination viewport height in pixels.</summary>
    public int ViewportHeight { get; }
    /// <summary>Gets the host/adapter-defined source-to-destination mapping matrix.</summary>
    public Matrix4x4 SourceToDestination { get; }
}

/// <summary>
/// Binds an explicit source rectangle to one destination target for a projection handoff.
/// </summary>
/// <remarks>
/// Multiple requests may reference the same immutable source content with different target identities,
/// source rectangles, viewport sizes, or transforms. Routing, ordering, and surface lifecycle remain host policy.
/// </remarks>
public sealed record VisualizerProjectionRequest
{
    /// <summary>Creates a projection request with separate source-region and destination descriptions.</summary>
    public VisualizerProjectionRequest(VisualizerSourceRectangle source, VisualizerProjectionTarget target)
    {
        if (string.IsNullOrWhiteSpace(source.CoordinateSystem) || source.Width <= 0 || source.Height <= 0)
            throw new ArgumentException("A valid explicit source rectangle is required.", nameof(source));
        ArgumentNullException.ThrowIfNull(target);
        Source = source;
        Target = target;
    }

    /// <summary>Gets the source content rectangle, independent of destination size.</summary>
    public VisualizerSourceRectangle Source { get; }
    /// <summary>Gets the destination target and projection mapping.</summary>
    public VisualizerProjectionTarget Target { get; }
}
