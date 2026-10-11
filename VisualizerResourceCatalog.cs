namespace TheSingularityWorkshop.Visualize;

/// <summary>A thread-safe catalog of backend-neutral geometry and material descriptions.</summary>
/// <remarks>This initial catalog is a bootstrap utility, not yet the final streaming or residency system.</remarks>
public sealed class VisualizerResourceCatalog : IVisualizerResourceProvider
{
    private readonly object _gate = new();
    private readonly Dictionary<VisualizerResourceId, VisualizerGeometry> _geometry = [];
    private readonly Dictionary<RenderResourceId, VisualizerMaterial> _materials = [];

    /// <summary>Registers or replaces geometry for a logical resource identifier.</summary>
    public void SetGeometry(RenderResourceId id, VisualizerGeometry geometry)
    {
        if (!id.IsValid)
            throw new ArgumentException("A valid resource identifier is required.", nameof(id));
        ArgumentNullException.ThrowIfNull(geometry);
        lock (_gate)
            _geometry[id] = geometry;
    }

    /// <summary>Registers or replaces a material for a logical resource identifier.</summary>
    public void SetMaterial(RenderResourceId id, VisualizerMaterial material)
    {
        if (!id.IsValid)
            throw new ArgumentException("A valid resource identifier is required.", nameof(id));
        ArgumentNullException.ThrowIfNull(material);
        lock (_gate)
            _materials[id] = material;
    }

    /// <inheritdoc />
    public bool TryGetGeometry(RenderResourceId id, out VisualizerGeometry? geometry)
    {
        lock (_gate)
            return _geometry.TryGetValue(id, out geometry);
    }

    /// <inheritdoc />
    public bool TryGetMaterial(RenderResourceId id, out VisualizerMaterial? material)
    {
        lock (_gate)
            return _materials.TryGetValue(id, out material);
    }

    /// <summary>Removes geometry for an identifier, if present.</summary>
    public bool RemoveGeometry(RenderResourceId id)
    {
        lock (_gate)
            return _geometry.Remove(id);
    }

    /// <summary>Removes a material for an identifier, if present.</summary>
    public bool RemoveMaterial(RenderResourceId id)
    {
        lock (_gate)
            return _materials.Remove(id);
    }
}
