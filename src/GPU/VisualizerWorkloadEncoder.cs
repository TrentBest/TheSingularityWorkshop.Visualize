using System.Buffers.Binary;
using System.Numerics;

namespace TheSingularityWorkshop.Visualize.GPU;

/// <summary>Contiguous little-endian arrays encoded from one resolved Visualize draw command.</summary>
/// <remarks>
/// Vertex layout v1 stores position only (XYZ float32, 12 bytes). Indices are uint32.
/// Instance layout v1 stores 16 row-major float32 matrix components followed by RGBA float32
/// (80 bytes). Normal, texture-coordinate, and per-vertex color fields are intentionally not
/// encoded by this first array-first proof; material and instance colors remain available.
/// </remarks>
public sealed record VisualizerWorkloadBytes(byte[] Vertices, byte[] Indices, byte[] Instances)
{
    /// <summary>Gets the vertex count in the position-only v1 layout.</summary>
    public int VertexCount => Vertices.Length / VisualizerWorkloadEncoder.VertexStrideBytes;

    /// <summary>Gets the uint32 index count.</summary>
    public int IndexCount => Indices.Length / sizeof(uint);

    /// <summary>Gets the instance count in the 80-byte v1 layout.</summary>
    public int InstanceCount => Instances.Length / VisualizerWorkloadEncoder.InstanceStrideBytes;
}

/// <summary>Encodes resolved Visualize CPU snapshots into explicit array-first workload bytes.</summary>
public static class VisualizerWorkloadEncoder
{
    /// <summary>Position-only XYZ float32 vertex stride for layout v1.</summary>
    public const int VertexStrideBytes = 12;

    /// <summary>UInt32 index stride.</summary>
    public const int IndexStrideBytes = sizeof(uint);

    /// <summary>16 row-major float32 matrix values plus RGBA float32 color.</summary>
    public const int InstanceStrideBytes = 80;

    /// <summary>Encodes all geometry indices and vertices, plus the command's effective instance data.</summary>
    /// <param name="draw">Resolved draw command and immutable CPU resource snapshots.</param>
    /// <returns>Fresh arrays ready for hashing and upload; the caller owns their lifetime.</returns>
    /// <exception cref="ArgumentNullException">The resolved draw is null.</exception>
    /// <exception cref="InvalidOperationException">A resolved resource contains non-finite encoded values.</exception>
    public static VisualizerWorkloadBytes Encode(ResolvedVisualizerDrawCommand draw)
    {
        ArgumentNullException.ThrowIfNull(draw);

        var vertices = new byte[checked(draw.Geometry.Vertices.Count * VertexStrideBytes)];
        for (var i = 0; i < draw.Geometry.Vertices.Count; i++)
        {
            var position = draw.Geometry.Vertices[i].Position;
            WriteVector3(vertices, i * VertexStrideBytes, position, $"vertex {i} position");
        }

        var indices = new byte[checked(draw.Geometry.Indices.Count * IndexStrideBytes)];
        for (var i = 0; i < draw.Geometry.Indices.Count; i++)
            BinaryPrimitives.WriteUInt32LittleEndian(indices.AsSpan(i * IndexStrideBytes, IndexStrideBytes), draw.Geometry.Indices[i]);

        var instanceCount = draw.Instances is null ? 1 : draw.Command.InstanceCount;
        var instances = new byte[checked(instanceCount * InstanceStrideBytes)];
        if (draw.Instances is null)
        {
            WriteInstance(instances, 0, draw.Command.WorldTransform, draw.Material.BaseColor, "draw instance");
        }
        else
        {
            for (var i = 0; i < instanceCount; i++)
            {
                var source = draw.Instances.Instances[draw.Command.FirstInstance + i];
                WriteInstance(instances, i * InstanceStrideBytes, source.WorldTransform, source.Color, $"instance {draw.Command.FirstInstance + i}");
            }
        }

        return new VisualizerWorkloadBytes(vertices, indices, instances);
    }

    private static void WriteVector3(byte[] target, int offset, Vector3 value, string description)
    {
        WriteSingle(target, offset, value.X, description);
        WriteSingle(target, offset + 4, value.Y, description);
        WriteSingle(target, offset + 8, value.Z, description);
    }

    private static void WriteInstance(byte[] target, int offset, Matrix4x4 matrix, Vector4 color, string description)
    {
        Span<float> components = stackalloc float[20]
        {
            matrix.M11, matrix.M12, matrix.M13, matrix.M14,
            matrix.M21, matrix.M22, matrix.M23, matrix.M24,
            matrix.M31, matrix.M32, matrix.M33, matrix.M34,
            matrix.M41, matrix.M42, matrix.M43, matrix.M44,
            color.X, color.Y, color.Z, color.W
        };

        for (var i = 0; i < components.Length; i++)
            WriteSingle(target, offset + i * sizeof(float), components[i], description);
    }

    private static void WriteSingle(byte[] target, int offset, float value, string description)
    {
        if (!float.IsFinite(value))
            throw new InvalidOperationException($"Cannot encode non-finite value in {description}.");
        BinaryPrimitives.WriteInt32LittleEndian(target.AsSpan(offset, sizeof(float)), BitConverter.SingleToInt32Bits(value));
    }
}
