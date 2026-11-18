using System.Numerics;

namespace TheSingularityWorkshop.Visualize;

/// <summary>Describes CPU-side geometry data from which a backend can create device resources.</summary>
/// <remarks>The first version intentionally supports one simple vertex layout; richer layouts require evidence from the first backend.</remarks>
public sealed class VisualizerGeometry
{
    private readonly VisualizerVertex[] _vertices;
    private readonly uint[] _indices;

    /// <summary>Creates geometry from vertex and index snapshots.</summary>
    public VisualizerGeometry(
        ulong revision,
        IReadOnlyList<VisualizerVertex> vertices,
        IReadOnlyList<uint> indices)
    {
        ArgumentNullException.ThrowIfNull(vertices);
        ArgumentNullException.ThrowIfNull(indices);

        // Validate the snapshots, not the caller-owned collections, so subsequent caller mutation
        // cannot change which data was checked or introduce invalid data after construction.
        _vertices = vertices.ToArray();
        _indices = indices.ToArray();

        if (_vertices.Length == 0)
            throw new ArgumentException("At least one vertex is required.", nameof(vertices));
        if (_indices.Length == 0)
            throw new ArgumentException("At least one index is required.", nameof(indices));

        for (var index = 0; index < _vertices.Length; index++)
        {
            var vertex = _vertices[index];
            if (!IsFinite(vertex.Position))
                throw new ArgumentOutOfRangeException(nameof(vertices), $"Vertex {index} position components must be finite.");
            if (!IsFinite(vertex.Normal))
                throw new ArgumentOutOfRangeException(nameof(vertices), $"Vertex {index} normal components must be finite.");
            if (!IsFinite(vertex.TextureCoordinate))
                throw new ArgumentOutOfRangeException(nameof(vertices), $"Vertex {index} texture-coordinate components must be finite.");
            if (!IsFinite(vertex.Color))
                throw new ArgumentOutOfRangeException(nameof(vertices), $"Vertex {index} color components must be finite.");
        }

        if (_indices.Any(index => index >= _vertices.Length))
            throw new ArgumentException("Every index must refer to an existing vertex.", nameof(indices));

        Revision = revision;
        Vertices = Array.AsReadOnly(_vertices);
        Indices = Array.AsReadOnly(_indices);
    }

    /// <summary>Gets the caller-managed content revision used for backend cache invalidation.</summary>
    public ulong Revision { get; }

    /// <summary>Gets an immutable snapshot of the vertex data.</summary>
    public IReadOnlyList<VisualizerVertex> Vertices { get; }

    /// <summary>Gets an immutable snapshot of the 32-bit index data.</summary>
    public IReadOnlyList<uint> Indices { get; }

    /// <summary>Gets contiguous read-only vertex memory for bulk backend uploads.</summary>
    /// <remarks>The memory is backed by the same private snapshot exposed through <see cref="Vertices"/>.</remarks>
    public ReadOnlyMemory<VisualizerVertex> VertexMemory => _vertices;

    /// <summary>Gets contiguous read-only index memory for bulk backend uploads.</summary>
    /// <remarks>The memory is backed by the same private snapshot exposed through <see cref="Indices"/>.</remarks>
    public ReadOnlyMemory<uint> IndexMemory => _indices;

    private static bool IsFinite(Vector2 value) =>
        float.IsFinite(value.X) && float.IsFinite(value.Y);

    private static bool IsFinite(Vector3 value) =>
        float.IsFinite(value.X) && float.IsFinite(value.Y) && float.IsFinite(value.Z);

    private static bool IsFinite(Vector4 value) =>
        float.IsFinite(value.X) && float.IsFinite(value.Y) &&
        float.IsFinite(value.Z) && float.IsFinite(value.W);
}

/// <summary>Initial backend-neutral vertex format for a first indexed-draw prototype.</summary>
public readonly record struct VisualizerVertex(Vector3 Position, Vector3 Normal, Vector2 TextureCoordinate, Vector4 Color);
