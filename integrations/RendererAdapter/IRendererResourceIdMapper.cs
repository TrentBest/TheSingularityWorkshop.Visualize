using TheSingularityWorkshop.Renderer;
using TheSingularityWorkshop.Visualize;

namespace TheSingularityWorkshop.Renderer.VisualizeAdapter;

/// <summary>
/// Maps resource identities at the Renderer-to-Visualize boundary.
/// </summary>
/// <remarks>
/// Resource IDs are logical names, not content or native handles. Implementations may translate
/// between independently allocated namespaces; the adapter validates that each mapped ID is valid.
/// </remarks>
public interface IRendererResourceIdMapper
{
    /// <summary>Maps a Renderer resource ID to its corresponding Visualize resource ID.</summary>
    /// <param name="sourceId">The valid Renderer resource ID to map.</param>
    /// <returns>A valid Visualize resource ID.</returns>
    VisualizerResourceId Map(RenderResourceId sourceId);
}

/// <summary>
/// Explicit identity mapping for hosts that intentionally share resource-ID allocation across
/// Renderer and Visualize.
/// </summary>
/// <remarks>
/// This implementation preserves the numeric value only; it does not prove that a resource exists
/// or that both systems associate the ID with equivalent content. Hosts with separate catalogs
/// should supply their own <see cref="IRendererResourceIdMapper"/>.
/// </remarks>
public sealed class IdentityRendererResourceIdMapper : IRendererResourceIdMapper
{
    /// <inheritdoc />
    public VisualizerResourceId Map(RenderResourceId sourceId)
    {
        if (!sourceId.IsValid)
        {
            throw new ArgumentOutOfRangeException(
                nameof(sourceId), sourceId, "Renderer resource ID must be valid.");
        }

        return new VisualizerResourceId(sourceId.Value);
    }
}
