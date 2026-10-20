using TheSingularityWorkshop.Visualize;

namespace TheSingularityWorkshop.Visualize.GPU;

/// <summary>Resolves and validates Visualizer draw commands before native GPU submission.</summary>
/// <remarks>
/// This component does not allocate GPU resources or cache native handles. It resolves the frame
/// against the host-owned resource provider and rejects missing resources and invalid ranges with
/// command-indexed diagnostics.
/// </remarks>
public static class VisualizerFrameResolver
{
    /// <summary>Resolves all commands in frame order and validates their resource ranges.</summary>
    /// <param name="frame">Immutable frame snapshot to resolve.</param>
    /// <param name="resources">Host-owned provider of geometry, material, and instance snapshots.</param>
    /// <returns>Resolved commands in the same order as the source frame.</returns>
    /// <exception cref="ArgumentNullException">The frame or resource provider is null.</exception>
    /// <exception cref="InvalidOperationException">A resource is missing or a requested range is invalid.</exception>
    public static IReadOnlyList<ResolvedVisualizerDrawCommand> Resolve(
        VisualizerFrame frame,
        IVisualizerResourceProvider resources)
    {
        ArgumentNullException.ThrowIfNull(frame);
        ArgumentNullException.ThrowIfNull(resources);

        var resolved = new ResolvedVisualizerDrawCommand[frame.Commands.Count];
        for (var commandIndex = 0; commandIndex < frame.Commands.Count; commandIndex++)
        {
            var command = frame.Commands[commandIndex];

            if (!resources.TryGetGeometry(command.Geometry, out var geometry) || geometry is null)
                throw MissingResource(commandIndex, "geometry", command.Geometry.Value);

            if (!resources.TryGetMaterial(command.Material, out var material) || material is null)
                throw MissingResource(commandIndex, "material", command.Material.Value);

            ValidateIndexRange(commandIndex, command, geometry);

            VisualizerInstanceBuffer? instances = null;
            if (command.Instances.IsValid)
            {
                if (!resources.TryGetInstances(command.Instances, out instances) || instances is null)
                    throw MissingResource(commandIndex, "instance data", command.Instances.Value);

                if (command.FirstInstance > instances.Instances.Count ||
                    command.InstanceCount > instances.Instances.Count - command.FirstInstance)
                {
                    throw new InvalidOperationException(
                        $"Draw command {commandIndex} instance range [{command.FirstInstance}, " +
                        $"{(long)command.FirstInstance + command.InstanceCount}) exceeds instance resource " +
                        $"{command.Instances.Value} count {instances.Instances.Count}.");
                }
            }

            resolved[commandIndex] = new ResolvedVisualizerDrawCommand(command, geometry, material, instances);
        }

        return Array.AsReadOnly(resolved);
    }

    private static void ValidateIndexRange(
        int commandIndex,
        VisualizerDrawCommand command,
        VisualizerGeometry geometry)
    {
        if (command.FirstIndex > geometry.Indices.Count ||
            command.IndexCount > geometry.Indices.Count - command.FirstIndex)
        {
            throw new InvalidOperationException(
                $"Draw command {commandIndex} index range [{command.FirstIndex}, " +
                $"{(long)command.FirstIndex + command.IndexCount}) exceeds geometry resource " +
                $"{command.Geometry.Value} index count {geometry.Indices.Count}.");
        }

        for (var indexOffset = command.FirstIndex; indexOffset < command.FirstIndex + command.IndexCount; indexOffset++)
        {
            var effectiveVertex = (long)geometry.Indices[indexOffset] + command.BaseVertex;
            if (effectiveVertex < 0 || effectiveVertex >= geometry.Vertices.Count)
            {
                throw new InvalidOperationException(
                    $"Draw command {commandIndex} index at offset {indexOffset} with base vertex " +
                    $"{command.BaseVertex} resolves to vertex {effectiveVertex}, outside geometry resource " +
                    $"{command.Geometry.Value} vertex count {geometry.Vertices.Count}.");
            }
        }
    }

    private static InvalidOperationException MissingResource(int commandIndex, string role, ulong resourceId) =>
        new($"Draw command {commandIndex} references missing {role} resource {resourceId}.");
}
