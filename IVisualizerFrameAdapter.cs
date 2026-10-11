namespace TheSingularityWorkshop.Visualize;

/// <summary>Maps a source renderer's frame model into Visualizer's backend-neutral frame contract.</summary>
/// <typeparam name="TSourceFrame">The source frame type, which may belong to a separate package.</typeparam>
/// <remarks>
/// This adapter keeps Visualizer independent of a specific Renderer package version while making the
/// integration boundary explicit and testable. Implementations must preserve source command ordering,
/// transforms, logical resource identity, index ranges, and the upstream representation decision.
/// </remarks>
public interface IVisualizerFrameAdapter<in TSourceFrame>
{
    /// <summary>Converts one source frame into a Visualizer frame snapshot.</summary>
    VisualizerFrame Adapt(TSourceFrame sourceFrame);
}
