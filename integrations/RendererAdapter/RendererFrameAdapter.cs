using TheSingularityWorkshop.Renderer;
using TheSingularityWorkshop.Visualize;

namespace TheSingularityWorkshop.Renderer.VisualizeAdapter;

/// <summary>Maps the Workshop Renderer frame contract to Visualize without changing upstream decisions.</summary>
/// <remarks>
/// This adapter belongs at the integration boundary rather than in either core package:
/// Renderer does not depend on Visualize, and Visualize does not depend on a particular Renderer version.
/// Resource IDs pass through an explicit mapper. The default mapper preserves numeric identity only for
/// hosts that intentionally share ID allocation; it does not imply that resource content is shared.
/// Command order, transforms, ranges, entity IDs, and representation choices are preserved.
/// </remarks>
public sealed class RendererFrameAdapter : IVisualizerFrameAdapter<RenderFrame>
{
    private readonly IRendererResourceIdMapper _resourceIdMapper;

    /// <summary>Creates an adapter using the explicit numeric-identity mapping convention.</summary>
    public RendererFrameAdapter()
        : this(new IdentityRendererResourceIdMapper())
    {
    }

    /// <summary>Creates an adapter using a host-provided resource namespace mapping.</summary>
    /// <param name="resourceIdMapper">Maps Renderer resource IDs to Visualize resource IDs.</param>
    public RendererFrameAdapter(IRendererResourceIdMapper resourceIdMapper)
    {
        _resourceIdMapper = resourceIdMapper
            ?? throw new ArgumentNullException(nameof(resourceIdMapper));
    }

    /// <inheritdoc />
    public VisualizerFrame Adapt(RenderFrame sourceFrame)
    {
        ArgumentNullException.ThrowIfNull(sourceFrame);

        var view = sourceFrame.View;
        var commands = new VisualizerDrawCommand[sourceFrame.Commands.Count];

        for (var i = 0; i < sourceFrame.Commands.Count; i++)
        {
            var source = sourceFrame.Commands[i];
            commands[i] = new VisualizerDrawCommand(
                MapResource(source.Geometry, nameof(source.Geometry)),
                MapResource(source.Material, nameof(source.Material)),
                source.WorldTransform,
                source.IndexCount,
                source.FirstIndex,
                source.BaseVertex,
                MapRepresentation(source.Representation),
                source.Instances.IsValid
                    ? MapResource(source.Instances, nameof(source.Instances))
                    : default,
                source.InstanceCount,
                source.FirstInstance,
                new VisualizerEntityId(source.Entity.Value));
        }

        return new VisualizerFrame(
            new VisualizerView(view.ObserverPosition, view.View, view.Projection),
            commands);
    }

    private VisualizerResourceId MapResource(RenderResourceId sourceId, string resourceRole)
    {
        var mapped = _resourceIdMapper.Map(sourceId);
        if (!mapped.IsValid)
        {
            throw new InvalidOperationException(
                $"The Renderer resource ID mapper returned an invalid Visualize ID for {resourceRole} (source ID {sourceId.Value}).");
        }

        return mapped;
    }

    private static VisualizerRepresentation MapRepresentation(RendererRepresentation representation) =>
        representation switch
        {
            RendererRepresentation.Semantic => VisualizerRepresentation.Semantic,
            RendererRepresentation.Aggregate => VisualizerRepresentation.Aggregate,
            RendererRepresentation.Distant => VisualizerRepresentation.Distant,
            RendererRepresentation.Near => VisualizerRepresentation.Near,
            RendererRepresentation.Interactive => VisualizerRepresentation.Interactive,
            _ => throw new ArgumentOutOfRangeException(
                nameof(representation),
                representation,
                "The Renderer representation has no Visualize mapping.")
        };
}
