using System.Collections.ObjectModel;

namespace TheSingularityWorkshop.Visualize;

/// <summary>
/// An ordered, immutable set of projection requests for one source presentation submission.
/// </summary>
/// <remarks>
/// This is a target-description batch, not a renderer or a routing engine. It preserves caller-supplied
/// order and requires each destination identity to appear at most once. The host still owns target lifetime,
/// frame freshness, submission scheduling, and isolation when a target fails.
/// </remarks>
public sealed class VisualizerProjectionBatch
{
    private readonly ReadOnlyCollection<VisualizerProjectionRequest> _requests;

    /// <summary>Creates a batch containing one or more requests in explicit submission order.</summary>
    public VisualizerProjectionBatch(IEnumerable<VisualizerProjectionRequest> requests)
    {
        ArgumentNullException.ThrowIfNull(requests);
        var items = requests.ToArray();
        if (items.Length == 0)
            throw new ArgumentException("A projection batch must contain at least one target request.", nameof(requests));

        var targetIds = new HashSet<string>(StringComparer.Ordinal);
        for (var i = 0; i < items.Length; i++)
        {
            if (items[i] is null)
                throw new ArgumentException($"Projection request at index {i} is null.", nameof(requests));
            if (!targetIds.Add(items[i].Target.TargetId))
                throw new ArgumentException($"Target ID '{items[i].Target.TargetId}' occurs more than once in the batch.", nameof(requests));
        }

        _requests = Array.AsReadOnly(items);
    }

    /// <summary>Gets the target requests in caller-specified order.</summary>
    public IReadOnlyList<VisualizerProjectionRequest> Requests => _requests;
}
