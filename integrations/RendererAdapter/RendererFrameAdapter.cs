using TheSingularityWorkshop.Renderer;
using TheSingularityWorkshop.Visualize;

namespace TheSingularityWorkshop.Renderer.VisualizeAdapter;

/// <summary>Maps the Workshop Renderer frame contract to Visualize without changing upstream decisions.</summary>
/// <remarks>
/// This adapter belongs at the integration boundary rather than in either core package:
/// Renderer does not depend on Visualize, and Visualize does not depend on a particular Renderer version.
/// Resource identifiers retain their numeric identity; command order, transforms, ranges, and representation
/// choices are preserved.
/// </remarks>
public sealed class RendererFrameAdapter : IVisualizerFrameAdapter<RenderFrame>
{
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
                new VisualizerResourceId(source.Geometry.Value),
                new VisualizerResourceId(source.Material.Value),
                source.WorldTransform,
                source.IndexCount,
                source.FirstIndex,
                source.BaseVertex,
                MapRepresentation(source.Representation),
                source.Instances.IsValid
                    ? new VisualizerResourceId(source.Instances.Value)
                    : default,
                source.InstanceCount,
                source.FirstInstance);
        }

        return new VisualizerFrame(
            new VisualizerView(view.ObserverPosition, view.View, view.Projection),
            commands);
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