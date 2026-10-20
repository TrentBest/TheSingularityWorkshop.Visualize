using System.Numerics;

namespace TheSingularityWorkshop.Visualize;

/// <summary>One per-instance transform and tint for an indexed instanced draw.</summary>
/// <remarks>Instance data is presentation state, not authoritative world state.</remarks>
public readonly record struct VisualizerInstance(Matrix4x4 WorldTransform, Vector4 Color);

/// <summary>An immutable snapshot of a contiguous instance-data array.</summary>
/// <remarks>The revision lets a backend avoid re-uploading unchanged instance data.</remarks>
public sealed class VisualizerInstanceBuffer
{
    private readonly VisualizerInstance[] _instances;

    /// <summary>Creates an instance-buffer snapshot.</summary>
    public VisualizerInstanceBuffer(ulong revision, IReadOnlyList<VisualizerInstance> instances)
    {
        ArgumentNullException.ThrowIfNull(instances);
        if (instances.Count == 0)
            throw new ArgumentException("At least one instance is required.", nameof(instances));

        Revision = revision;
        _instances = instances.ToArray();
        Instances = Array.AsReadOnly(_instances);
    }

    /// <summary>Gets the caller-managed content revision used for backend cache invalidation.</summary>
    public ulong Revision { get; }

    /// <summary>Gets the immutable contiguous instance snapshot.</summary>
    public IReadOnlyList<VisualizerInstance> Instances { get; }

    /// <summary>Gets contiguous read-only instance memory for bulk backend uploads.</summary>
    /// <remarks>The memory is backed by the same private snapshot exposed through <see cref="Instances"/>.</remarks>
    public ReadOnlyMemory<VisualizerInstance> InstanceMemory => _instances;
}
