namespace TheSingularityWorkshop.Visualize;

/// <summary>A thread-safe catalog of backend-neutral geometry, material, and instance descriptions.</summary>
/// <remarks>This initial catalog is a bootstrap utility, not yet the final streaming or residency system.</remarks>
public sealed class VisualizerResourceCatalog : IVisualizerResourceProvider
{
    private readonly object _gate = new();
    private readonly Dictionary<VisualizerResourceId, VisualizerGeometry> _geometry = [];
    private readonly Dictionary<VisualizerResourceId, VisualizerMaterial> _materials = [];
    private readonly Dictionary<VisualizerResourceId, VisualizerInstanceBuffer> _instances = [];

    /// <summary>Registers or replaces geometry for a logical resource identifier.</summary>
    public void SetGeometry(VisualizerResourceId id, VisualizerGeometry geometry)
    {
        ValidateId(id);
        ArgumentNullException.ThrowIfNull(geometry);
        lock (_gate)
            _geometry[id] = geometry;
    }

    /// <summary>Registers or replaces a material for a logical resource identifier.</summary>
    public void SetMaterial(VisualizerResourceId id, VisualizerMaterial material)
    {
        ValidateId(id);
        ArgumentNullException.ThrowIfNull(material);
        lock (_gate)
            _materials[id] = material;
    }

    /// <summary>Registers or replaces an instance-data array for a logical resource identifier.</summary>
    public void SetInstances(VisualizerResourceId id, VisualizerInstanceBuffer instances)
    {
        ValidateId(id);
        ArgumentNullException.ThrowIfNull(instances);
        lock (_gate)
            _instances[id] = instances;
    }

    /// <inheritdoc />
    public bool TryGetGeometry(VisualizerResourceId id, out VisualizerGeometry? geometry)
    {
        lock (_gate)
            return _geometry.TryGetValue(id, out geometry);
    }

    /// <inheritdoc />
    public bool TryGetMaterial(VisualizerResourceId id, out VisualizerMaterial? material)
    {
        lock (_gate)
            return _materials.TryGetValue(id, out material);
    }

    /// <inheritdoc />
    public bool TryGetInstances(VisualizerResourceId id, out VisualizerInstanceBuffer? instances)
    {
        lock (_gate)
            return _instances.TryGetValue(id, out instances);
    }

    /// <summary>Removes geometry for an identifier, if present.</summary>
    public bool RemoveGeometry(VisualizerResourceId id)
    {
        lock (_gate)
            return _geometry.Remove(id);
    }

    /// <summary>Removes a material for an identifier, if present.</summary>
    public bool RemoveMaterial(VisualizerResourceId id)
    {
        lock (_gate)
            return _materials.Remove(id);
    }

    /// <summary>Removes instance data for an identifier, if present.</summary>
    public bool RemoveInstances(VisualizerResourceId id)
    {
        lock (_gate)
            return _instances.Remove(id);
    }

    private static void ValidateId(VisualizerResourceId id)
    {
        if (!id.IsValid)
            throw new ArgumentException("A valid resource identifier is required.", nameof(id));
    }
}
