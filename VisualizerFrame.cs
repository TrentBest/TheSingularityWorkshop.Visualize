using System.Numerics;

namespace TheSingularityWorkshop.Visualize;

/// <summary>Represents one observer-relative presentation snapshot supplied to Visualizer.</summary>
/// <remarks>A thin adapter maps the Workshop Renderer frame to this backend-neutral contract.</remarks>
public sealed class VisualizerFrame
{
    /// <summary>Creates a frame snapshot from a view and ordered draw commands.</summary>
    public VisualizerFrame(VisualizerView view, IEnumerable<VisualizerDrawCommand> commands)
    {
        ArgumentNullException.ThrowIfNull(commands);
        View = view;
        Commands = Array.AsReadOnly(commands.ToArray());
    }

    /// <summary>Gets the view state for this frame.</summary>
    public VisualizerView View { get; }

    /// <summary>Gets the immutable ordered draw-command snapshot.</summary>
    public IReadOnlyList<VisualizerDrawCommand> Commands { get; }
}

/// <summary>Describes observer/view transforms for a presentation frame.</summary>
public readonly record struct VisualizerView(Vector3 ObserverPosition, Matrix4x4 View, Matrix4x4 Projection);

/// <summary>Identifies the representation chosen upstream by Renderer.</summary>
public enum VisualizerRepresentation
{
    /// <summary>Only semantic existence is required.</summary>
    Semantic = 0,
    /// <summary>An aggregate environmental representation is sufficient.</summary>
    Aggregate = 1,
    /// <summary>A distant visual representation is sufficient.</summary>
    Distant = 2,
    /// <summary>A near-field representation is required.</summary>
    Near = 3,
    /// <summary>Interactive detail is required.</summary>
    Interactive = 4
}

/// <summary>Describes one backend-neutral indexed draw operation.</summary>
public readonly record struct VisualizerDrawCommand
{
    /// <summary>Initializes an indexed draw command.</summary>
    public VisualizerDrawCommand(
        VisualizerResourceId geometry,
        VisualizerResourceId material,
        Matrix4x4 worldTransform,
        int indexCount,
        int firstIndex,
        int baseVertex,
        VisualizerRepresentation representation)
    {
        if (!geometry.IsValid)
            throw new ArgumentException("A valid geometry resource is required.", nameof(geometry));
        if (!material.IsValid)
            throw new ArgumentException("A valid material resource is required.", nameof(material));
        if (indexCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(indexCount));
        if (firstIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(firstIndex));

        Geometry = geometry;
        Material = material;
        WorldTransform = worldTransform;
        IndexCount = indexCount;
        FirstIndex = firstIndex;
        BaseVertex = baseVertex;
        Representation = representation;
    }

    /// <summary>Gets the geometry resource identifier.</summary>
    public VisualizerResourceId Geometry { get; }
    /// <summary>Gets the material resource identifier.</summary>
    public VisualizerResourceId Material { get; }
    /// <summary>Gets the world transform.</summary>
    public Matrix4x4 WorldTransform { get; }
    /// <summary>Gets the number of indices to draw.</summary>
    public int IndexCount { get; }
    /// <summary>Gets the first index in the geometry resource.</summary>
    public int FirstIndex { get; }
    /// <summary>Gets the base vertex offset.</summary>
    public int BaseVertex { get; }
    /// <summary>Gets the representation selected by the upstream Renderer.</summary>
    public VisualizerRepresentation Representation { get; }
}
