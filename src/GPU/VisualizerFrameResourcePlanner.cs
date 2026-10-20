using TheSingularityWorkshop.Visualize;

namespace TheSingularityWorkshop.Visualize.GPU;

/// <summary>A frame-level plan that resolves each logical resource once for ordered submission.</summary>
/// <remarks>
/// The plan is a CPU-side snapshot only. A native backend may use resource identity plus revision
/// to reuse device allocations; this type does not allocate, upload, or own GPU handles.
/// </remarks>
public sealed class VisualizerFrameResourcePlan
{
    internal VisualizerFrameResourcePlan(
        IReadOnlyList<VisualizerPlannedGeometry> geometries,
        IReadOnlyList<VisualizerPlannedMaterial> materials,
        IReadOnlyList<VisualizerPlannedInstances> instances,
        IReadOnlyList<VisualizerPlannedDraw> draws)
    {
        Geometries = geometries;
        Materials = materials;
        Instances = instances;
        Draws = draws;
    }

    /// <summary>Gets unique geometry snapshots in first-use order.</summary>
    public IReadOnlyList<VisualizerPlannedGeometry> Geometries { get; }

    /// <summary>Gets unique material snapshots in first-use order.</summary>
    public IReadOnlyList<VisualizerPlannedMaterial> Materials { get; }

    /// <summary>Gets unique instance-buffer snapshots in first-use order.</summary>
    public IReadOnlyList<VisualizerPlannedInstances> Instances { get; }

    /// <summary>Gets draw references in the exact order supplied by the frame.</summary>
    public IReadOnlyList<VisualizerPlannedDraw> Draws { get; }
}

/// <summary>A unique geometry resource selected for this frame.</summary>
public sealed record VisualizerPlannedGeometry(
    VisualizerResourceId Id,
    ulong Revision,
    VisualizerGeometry Snapshot);

/// <summary>A unique material resource selected for this frame.</summary>
public sealed record VisualizerPlannedMaterial(
    VisualizerResourceId Id,
    ulong Revision,
    VisualizerMaterial Snapshot);

/// <summary>A unique instance-data resource selected for this frame.</summary>
public sealed record VisualizerPlannedInstances(
    VisualizerResourceId Id,
    ulong Revision,
    VisualizerInstanceBuffer Snapshot);

/// <summary>A draw and its indices into the frame's unique resource lists.</summary>
/// <param name="CommandIndex">Original command index in the frame.</param>
/// <param name="Resolved">Command plus its resolved immutable resource snapshots.</param>
/// <param name="GeometryResourceIndex">Index into <see cref="VisualizerFrameResourcePlan.Geometries"/>.</param>
/// <param name="MaterialResourceIndex">Index into <see cref="VisualizerFrameResourcePlan.Materials"/>.</param>
/// <param name="InstanceResourceIndex">Index into <see cref="VisualizerFrameResourcePlan.Instances"/>, or null for a non-instanced draw.</param>
public sealed record VisualizerPlannedDraw(
    int CommandIndex,
    ResolvedVisualizerDrawCommand Resolved,
    int GeometryResourceIndex,
    int MaterialResourceIndex,
    int? InstanceResourceIndex);

/// <summary>Builds a coherent, de-duplicated resource plan from a Visualizer frame.</summary>
public static class VisualizerFrameResourcePlanner
{
    /// <summary>
    /// Resolves and de-duplicates frame resources while preserving the original draw order.
    /// A logical ID must resolve to one revision within a frame; observing two revisions for the
    /// same ID indicates that the provider changed during resolution and the frame is not coherent.
    /// </summary>
    public static VisualizerFrameResourcePlan Plan(
        VisualizerFrame frame,
        IVisualizerResourceProvider resources)
    {
        var resolved = VisualizerFrameResolver.Resolve(frame, resources);

        var geometries = new List<VisualizerPlannedGeometry>();
        var materials = new List<VisualizerPlannedMaterial>();
        var instances = new List<VisualizerPlannedInstances>();
        var geometryIndices = new Dictionary<VisualizerResourceId, int>();
        var materialIndices = new Dictionary<VisualizerResourceId, int>();
        var instanceIndices = new Dictionary<VisualizerResourceId, int>();
        var geometryRevisions = new Dictionary<VisualizerResourceId, ulong>();
        var materialRevisions = new Dictionary<VisualizerResourceId, ulong>();
        var instanceRevisions = new Dictionary<VisualizerResourceId, ulong>();
        var draws = new VisualizerPlannedDraw[resolved.Count];

        for (var commandIndex = 0; commandIndex < resolved.Count; commandIndex++)
        {
            var draw = resolved[commandIndex];
            var geometryIndex = AddGeometry(draw, geometries, geometryIndices, geometryRevisions, commandIndex);
            var materialIndex = AddMaterial(draw, materials, materialIndices, materialRevisions, commandIndex);
            int? instanceIndex = draw.Instances is null
                ? null
                : AddInstances(draw, instances, instanceIndices, instanceRevisions, commandIndex);

            draws[commandIndex] = new VisualizerPlannedDraw(
                commandIndex, draw, geometryIndex, materialIndex, instanceIndex);
        }

        return new VisualizerFrameResourcePlan(
            Array.AsReadOnly(geometries.ToArray()),
            Array.AsReadOnly(materials.ToArray()),
            Array.AsReadOnly(instances.ToArray()),
            Array.AsReadOnly(draws));
    }

    private static int AddGeometry(
        ResolvedVisualizerDrawCommand draw,
        List<VisualizerPlannedGeometry> resources,
        Dictionary<VisualizerResourceId, int> indices,
        Dictionary<VisualizerResourceId, ulong> revisions,
        int commandIndex)
    {
        var id = draw.Command.Geometry;
        var revision = draw.Geometry.Revision;
        EnsureConsistentRevision("geometry", id, revision, revisions, commandIndex);
        if (indices.TryGetValue(id, out var index))
            return index;

        index = resources.Count;
        indices.Add(id, index);
        resources.Add(new VisualizerPlannedGeometry(id, revision, draw.Geometry));
        return index;
    }

    private static int AddMaterial(
        ResolvedVisualizerDrawCommand draw,
        List<VisualizerPlannedMaterial> resources,
        Dictionary<VisualizerResourceId, int> indices,
        Dictionary<VisualizerResourceId, ulong> revisions,
        int commandIndex)
    {
        var id = draw.Command.Material;
        var revision = draw.Material.Revision;
        EnsureConsistentRevision("material", id, revision, revisions, commandIndex);
        if (indices.TryGetValue(id, out var index))
            return index;

        index = resources.Count;
        indices.Add(id, index);
        resources.Add(new VisualizerPlannedMaterial(id, revision, draw.Material));
        return index;
    }

    private static int AddInstances(
        ResolvedVisualizerDrawCommand draw,
        List<VisualizerPlannedInstances> resources,
        Dictionary<VisualizerResourceId, int> indices,
        Dictionary<VisualizerResourceId, ulong> revisions,
        int commandIndex)
    {
        var snapshot = draw.Instances!;
        var id = draw.Command.Instances;
        EnsureConsistentRevision("instance data", id, snapshot.Revision, revisions, commandIndex);
        if (indices.TryGetValue(id, out var index))
            return index;

        index = resources.Count;
        indices.Add(id, index);
        resources.Add(new VisualizerPlannedInstances(id, snapshot.Revision, snapshot));
        return index;
    }

    private static void EnsureConsistentRevision(
        string role,
        VisualizerResourceId id,
        ulong revision,
        Dictionary<VisualizerResourceId, ulong> revisions,
        int commandIndex)
    {
        if (revisions.TryGetValue(id, out var existingRevision))
        {
            if (existingRevision != revision)
            {
                throw new InvalidOperationException(
                    $"Draw command {commandIndex} resolved {role} resource {id.Value} at revision {revision}, " +
                    $"but an earlier command resolved the same resource at revision {existingRevision}. " +
                    "The resource provider changed during frame resolution; retry with a coherent snapshot.");
            }

            return;
        }

        revisions.Add(id, revision);
    }
}
