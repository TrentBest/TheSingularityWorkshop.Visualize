namespace TheSingularityWorkshop.Visualize.GPU;

/// <summary>Aggregates host facts and adapter reports from independently supplied backend providers.</summary>
/// <remarks>
/// Detection is deliberately separate from device initialization, workload verification, selection, and benchmarking.
/// A returned adapter is discovered hardware or software, not proof that it can render a Visualize workload.
/// </remarks>
public sealed class GpuSystemDetector
{
    private readonly IReadOnlyList<IGpuAdapterProvider> _providers;

    /// <summary>Creates a detector using the supplied backend providers.</summary>
    public GpuSystemDetector(IEnumerable<IGpuAdapterProvider> providers)
    {
        ArgumentNullException.ThrowIfNull(providers);
        _providers = providers.ToArray();
        if (_providers.Any(provider => provider is null))
            throw new ArgumentException("Adapter providers cannot contain null entries.", nameof(providers));
        if (_providers.Any(provider => string.IsNullOrWhiteSpace(provider.Backend)))
            throw new ArgumentException("Every adapter provider must declare a backend identifier.", nameof(providers));
    }

    /// <summary>Captures the current host and enumerates every configured backend.</summary>
    public ValueTask<GpuSystemDetectionResult> DetectAsync(CancellationToken cancellationToken = default) =>
        DetectAsync(GpuHostInfo.CaptureCurrent(), cancellationToken);

    /// <summary>Enumerates every configured backend using an explicit host profile (useful for deterministic tests).</summary>
    public async ValueTask<GpuSystemDetectionResult> DetectAsync(
        GpuHostInfo host,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(host);

        var adapters = new List<GpuAdapterInfo>();
        var failures = new List<GpuBackendDiscoveryFailure>();

        foreach (var provider in _providers)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                var discovered = await provider.EnumerateAdaptersAsync(cancellationToken).ConfigureAwait(false);
                ArgumentNullException.ThrowIfNull(discovered);

                foreach (var adapter in discovered)
                {
                    if (adapter is null)
                        throw new InvalidOperationException($"Backend '{provider.Backend}' returned a null adapter.");
                    if (!string.Equals(adapter.Backend, provider.Backend, StringComparison.Ordinal))
                        throw new InvalidOperationException(
                            $"Backend '{provider.Backend}' returned adapter '{adapter.AdapterId}' labelled '{adapter.Backend}'.");
                    if (string.IsNullOrWhiteSpace(adapter.AdapterId) || string.IsNullOrWhiteSpace(adapter.Name))
                        throw new InvalidOperationException($"Backend '{provider.Backend}' returned an adapter without an ID or name.");

                    adapters.Add(adapter);
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                failures.Add(new GpuBackendDiscoveryFailure(
                    provider.Backend,
                    exception.GetType().FullName ?? exception.GetType().Name,
                    exception.Message));
            }
        }

        return new GpuSystemDetectionResult(
            host,
            DateTimeOffset.UtcNow,
            adapters.AsReadOnly(),
            failures.AsReadOnly());
    }
}
