using TheSingularityWorkshop.Visualize;

namespace TheSingularityWorkshop.Visualize.GPU;

/// <summary>A draw command resolved to immutable CPU-side resource snapshots.</summary>
/// <remarks>
/// This is a validation boundary, not a GPU upload cache. Native adapters can use each resource's
/// ID and revision to manage derived device allocations.
/// </remarks>
public sealed record ResolvedVisualizerDrawCommand(
    VisualizerDrawCommand Command,
    VisualizerGeometry Geometry,
    VisualizerMaterial Material,
    VisualizerInstanceBuffer? Instances);
