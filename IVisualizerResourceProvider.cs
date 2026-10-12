namespace TheSingularityWorkshop.Visualize;

/// <summary>Resolves logical resource identifiers into backend-neutral resource descriptions.</summary>
/// <remarks>Native GPU handles are intentionally not exposed through this contract.</remarks>
public interface IVisualizerResourceProvider
{
    /// <summary>Attempts to resolve a geometry resource.</summary>
    bool TryGetGeometry(VisualizerResourceId id, out VisualizerGeometry? geometry);

    /// <summary>Attempts to resolve a material resource.</summary>
    bool TryGetMaterial(VisualizerResourceId id, out VisualizerMaterial? material);

    /// <summary>Attempts to resolve a contiguous instance-data resource.</summary>
    bool TryGetInstances(VisualizerResourceId id, out VisualizerInstanceBuffer? instances);
}
