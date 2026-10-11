using System.Numerics;

namespace TheSingularityWorkshop.Visualize;

/// <summary>Describes CPU-side geometry data from which a backend can create device resources.</summary>
/// <remarks>The first version intentionally supports one simple vertex layout; richer layouts require evidence from the first backend.</remarks>
public sealed class VisualizerGeometry
{
    /// <summary>Creates geometry from vertex and index snapshots.</summary>
    public VisualizerGeometry(
        ulong revision,
        IReadOnlyList<VisualizerVertex> vertices,
        IReadOnlyList<uint> indices)
    {
        ArgumentNullException.ThrowIfNull(vertices);
        ArgumentNullException.ThrowIfNull(indices);
        if (vertices.Count == 0)
            throw new ArgumentException("At least one vertex is required.", nameof(vertices));
        if (indices.Count == 0)
            throw new ArgumentException("At least one index is required.", nameof(indices));
        if (indices.Any(index => index >= vertices.Count))
            throw new ArgumentException("Every index must refer to an existing vertex.", nameof(indices));

        Revision = revision;
        Vertices = Array.AsReadOnly(vertices.ToArray());
        Indices = Array.AsReadOnly(indices.ToArray());
    }

    /// <summary>Gets the caller-managed content revision used for backend cache invalidation.</summary>
    public ulong Revision { get; }
    /// <summary>Gets an immutable snapshot of the vertex data.</summary>
    public IReadOnlyList<VisualizerVertex> Vertices { get; }
    /// <summary>Gets an immutable snapshot of the 32-bit index data.</summary>
    public IReadOnlyList<uint> Indices { get; }
}

/// <summary>Initial backend-neutral vertex format for a first indexed-draw prototype.</summary>
public readonly record struct VisualizerVertex(Vector3 Position, Vector3 Normal, Vector2 TextureCoordinate, Vector4 Color);
