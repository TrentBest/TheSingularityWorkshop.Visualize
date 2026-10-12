using System.Numerics;

namespace TheSingularityWorkshop.Visualize;

/// <summary>One per-instance transform and tint for an indexed instanced draw.</summary>
/// <remarks>Instance data is presentation state, not authoritative world state.</remarks>
public readonly record struct VisualizerInstance(Matrix4x4 WorldTransform, Vector4 Color);

/// <summary>An immutable snapshot of a contiguous instance-data array.</summary>
/// <remarks>The revision lets a backend avoid re-uploading unchanged instance data.</remarks>
public sealed class VisualizerInstanceBuffer
{
    /// <summary>Creates an instance-buffer snapshot.</summary>
    public VisualizerInstanceBuffer(ulong revision, IReadOnlyList<VisualizerInstance> instances)
    {
        ArgumentNullException.ThrowIfNull(instances);
        if (instances.Count == 0)
            throw new ArgumentException("At least one instance is required.", nameof(instances));

        Revision = revision;
        Instances = Array.AsReadOnly(instances.ToArray());
    }

    /// <summary>Gets the caller-managed content revision used for cache invalidation.</summary>
    public ulong Revision { get; }

    /// <summary>Gets the immutable contiguous instance snapshot.</summary>
    public IReadOnlyList<VisualizerInstance> Instances { get; }
}
