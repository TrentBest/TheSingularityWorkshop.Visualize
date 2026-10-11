using TheSingularityWorkshop.Renderer;

namespace TheSingularityWorkshop.Visualize;

/// <summary>Resolves Renderer resource identifiers into backend-neutral resource descriptions.</summary>
/// <remarks>Native GPU handles are intentionally not exposed through this contract.</remarks>
public interface IVisualizerResourceProvider
{
    /// <summary>Attempts to resolve a geometry resource.</summary>
    bool TryGetGeometry(RenderResourceId id, out VisualizerGeometry? geometry);

    /// <summary>Attempts to resolve a material resource.</summary>
    bool TryGetMaterial(RenderResourceId id, out VisualizerMaterial? material);
}
