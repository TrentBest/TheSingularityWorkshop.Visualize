using System.Numerics;
using TheSingularityWorkshop.Visualize;
using Xunit;

namespace TheSingularityWorkshop.Visualize.Tests;

public sealed class SvgVisualizerBackendTests
{
    [Theory]
    [InlineData("")]
    [InlineData("red")]
    [InlineData("#12345")]
    public void Constructor_RejectsInvalidBackgroundColors(string background)
    {
        Assert.Throws<ArgumentException>(() => new SvgVisualizerBackend(_ => { }, background: background));
    }

    [Fact]
    public void BuildSvg_EmitsTriangleAndStableSemanticMetadata()
    {
        var resources = CreateResources();
        var frame = CreateFrame(indexCount: 3);
        var backend = new SvgVisualizerBackend(_ => { }, width: 320, height: 200);

        var first = backend.BuildSvg(frame, resources);
        var second = backend.BuildSvg(frame, resources);

        Assert.Equal(first, second);
        Assert.Contains("<svg", first, StringComparison.Ordinal);
        Assert.Contains("width=\"320\"", first, StringComparison.Ordinal);
        Assert.Contains("height=\"200\"", first, StringComparison.Ordinal);
        Assert.Contains("fill=\"#FF0000\"", first, StringComparison.Ordinal);
        Assert.Contains("data-representation=\"near\"", first, StringComparison.Ordinal);
        Assert.Contains("data-entity-id=\"42\"", first, StringComparison.Ordinal);
        Assert.Equal(new VisualizerCapabilities("Visualize static SVG artifact backend", "SVG/CPU", false, false, false, false), backend.Capabilities);
    }

    [Theory]
    [InlineData(-0.1f)]
    [InlineData(1.1f)]
    [InlineData(float.NaN)]
    [InlineData(float.PositiveInfinity)]
    public void BuildSvg_RejectsInvalidComputedAlpha(float alpha)
    {
        var resources = CreateResources(vertexAlpha: alpha);
        var backend = new SvgVisualizerBackend(_ => { });

        var exception = Assert.Throws<InvalidOperationException>(() => backend.BuildSvg(CreateFrame(indexCount: 3), resources));
        Assert.Contains("alpha", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void BuildSvg_PreservesTransparentAndOpaqueAlphaEndpoints()
    {
        var transparent = new SvgVisualizerBackend(_ => { }).BuildSvg(CreateFrame(3), CreateResources(vertexAlpha: 0f));
        var opaque = new SvgVisualizerBackend(_ => { }).BuildSvg(CreateFrame(3), CreateResources(vertexAlpha: 1f));

        Assert.Contains("fill-opacity=\"0\"", transparent, StringComparison.Ordinal);
        Assert.DoesNotContain("fill-opacity=", opaque, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RenderAsync_EmitsArtifactAndDoesNotClaimPresentation()
    {
        string? artifact = null;
        var backend = new SvgVisualizerBackend(svg => artifact = svg);
        var result = await backend.RenderAsync(CreateFrame(indexCount: 3), CreateResources());

        Assert.NotNull(artifact);
        Assert.Contains("<polygon", artifact, StringComparison.Ordinal);
        Assert.Equal(0, result.FrameNumber);
        Assert.Equal(1, result.SubmittedDrawCount);
        Assert.False(result.WasPresented);
        Assert.Null(result.GpuExecutionTime);
    }

    [Fact]
    public void BuildSvg_RejectsNonTriangleIndexCounts()
    {
        var backend = new SvgVisualizerBackend(_ => { });
        var exception = Assert.Throws<NotSupportedException>(() =>
            backend.BuildSvg(CreateFrame(indexCount: 2), CreateResources()));

        Assert.Contains("triangle-list", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void BuildSvg_ObservesCancellationBeforeWork()
    {
        var backend = new SvgVisualizerBackend(_ => { });
        using var source = new CancellationTokenSource();
        source.Cancel();

        Assert.Throws<OperationCanceledException>(() =>
            backend.BuildSvg(CreateFrame(indexCount: 3), CreateResources(), source.Token));
    }

    [Fact]
    public async Task RenderAsync_SinkFailurePropagatesWithoutConsumingFrameNumber()
    {
        var fail = true;
        var backend = new SvgVisualizerBackend(_ =>
        {
            if (fail)
                throw new InvalidOperationException("sink failed");
        });

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await backend.RenderAsync(CreateFrame(indexCount: 3), CreateResources()));
        Assert.Equal("sink failed", exception.Message);

        fail = false;
        var result = await backend.RenderAsync(CreateFrame(indexCount: 3), CreateResources());
        Assert.Equal(0, result.FrameNumber);
    }

    [Fact]
    public async Task RenderAsync_ObservesCancellationBeforeCallingSink()
    {
        var sinkCalls = 0;
        var backend = new SvgVisualizerBackend(_ => sinkCalls++);
        using var source = new CancellationTokenSource();
        source.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            await backend.RenderAsync(CreateFrame(indexCount: 3), CreateResources(), source.Token));

        Assert.Equal(0, sinkCalls);
    }

    [Fact]
    public async Task DisposedBackend_RejectsBuildAndSubmission()
    {
        var backend = new SvgVisualizerBackend(_ => { });
        await backend.DisposeAsync();

        Assert.Throws<ObjectDisposedException>(() =>
            backend.BuildSvg(CreateFrame(indexCount: 3), CreateResources()));
        await Assert.ThrowsAsync<ObjectDisposedException>(async () =>
            await backend.RenderAsync(CreateFrame(indexCount: 3), CreateResources()));
    }


    [Fact]
    public async Task RenderAsync_SerializesConcurrentSinkCallsAndFrameNumbers()
    {
        using var firstSinkEntered = new ManualResetEventSlim();
        using var releaseFirstSink = new ManualResetEventSlim();
        using var secondSinkEntered = new ManualResetEventSlim();
        using var secondRenderStarted = new ManualResetEventSlim();
        var sinkCalls = 0;
        var activeSinkCalls = 0;
        var maxActiveSinkCalls = 0;
        var backend = new SvgVisualizerBackend(_ =>
        {
            var active = Interlocked.Increment(ref activeSinkCalls);
            UpdateMaximum(ref maxActiveSinkCalls, active);
            var call = Interlocked.Increment(ref sinkCalls);
            try
            {
                if (call == 1)
                {
                    firstSinkEntered.Set();
                    if (!releaseFirstSink.Wait(TimeSpan.FromSeconds(5)))
                        throw new TimeoutException("Test did not release the first sink call.");
                }
                else if (call == 2)
                {
                    secondSinkEntered.Set();
                }
            }
            finally
            {
                Interlocked.Decrement(ref activeSinkCalls);
            }
        });

        var first = Task.Run(async () => await backend.RenderAsync(CreateFrame(3), CreateResources()));
        Assert.True(firstSinkEntered.Wait(TimeSpan.FromSeconds(5)), "The first sink call did not start.");

        var second = Task.Run(async () =>
        {
            secondRenderStarted.Set();
            return await backend.RenderAsync(CreateFrame(3), CreateResources());
        });
        Assert.True(secondRenderStarted.Wait(TimeSpan.FromSeconds(5)), "The second render task did not start.");
        Assert.False(secondSinkEntered.Wait(TimeSpan.FromMilliseconds(100)),
            "A second sink call entered while the first call was still active.");

        releaseFirstSink.Set();
        var results = await Task.WhenAll(first, second);

        Assert.Equal(1, maxActiveSinkCalls);
        Assert.Equal(new long[] { 0, 1 }, results.Select(result => result.FrameNumber).Order().ToArray());
        Assert.Equal(2, sinkCalls);
    }

    [Fact]
    public async Task DisposeAsync_WaitsForActiveSinkCallBeforeCompleting()
    {
        using var sinkEntered = new ManualResetEventSlim();
        using var releaseSink = new ManualResetEventSlim();
        var backend = new SvgVisualizerBackend(_ =>
        {
            sinkEntered.Set();
            if (!releaseSink.Wait(TimeSpan.FromSeconds(5)))
                throw new TimeoutException("Test did not release the sink call.");
        });

        var render = Task.Run(async () => await backend.RenderAsync(CreateFrame(3), CreateResources()));
        Assert.True(sinkEntered.Wait(TimeSpan.FromSeconds(5)), "The sink call did not start.");

        using var disposalStarted = new ManualResetEventSlim();
        var dispose = Task.Run(async () =>
        {
            disposalStarted.Set();
            await backend.DisposeAsync();
        });
        Assert.True(disposalStarted.Wait(TimeSpan.FromSeconds(5)), "Disposal did not start.");
        Assert.False(dispose.IsCompleted, "Disposal completed while the sink call was still active.");

        releaseSink.Set();
        await render;
        await dispose;

        await Assert.ThrowsAsync<ObjectDisposedException>(async () =>
            await backend.RenderAsync(CreateFrame(3), CreateResources()));
    }

    private static void UpdateMaximum(ref int maximum, int candidate)
    {
        var observed = Volatile.Read(ref maximum);
        while (candidate > observed)
        {
            var previous = Interlocked.CompareExchange(ref maximum, candidate, observed);
            if (previous == observed)
                return;
            observed = previous;
        }
    }

    private static VisualizerResourceCatalog CreateResources(float vertexAlpha = 1f)
    {
        var resources = new VisualizerResourceCatalog();
        resources.SetGeometry(new VisualizerResourceId(1), new VisualizerGeometry(
            revision: 1,
            vertices:
            [
                new VisualizerVertex(new Vector3(-0.5f, -0.5f, 0), Vector3.UnitZ, Vector2.Zero, new Vector4(1, 1, 1, vertexAlpha)),
                new VisualizerVertex(new Vector3(0.5f, -0.5f, 0), Vector3.UnitZ, Vector2.UnitX, new Vector4(1, 1, 1, vertexAlpha)),
                new VisualizerVertex(new Vector3(0, 0.5f, 0), Vector3.UnitZ, Vector2.UnitY, new Vector4(1, 1, 1, vertexAlpha))
            ],
            indices: [0, 1, 2]));
        resources.SetMaterial(new VisualizerResourceId(2), new VisualizerMaterial(1, new Vector4(1, 0, 0, 1)));
        return resources;
    }

    private static VisualizerFrame CreateFrame(int indexCount) =>
        new(
            new VisualizerView(Vector3.Zero, Matrix4x4.Identity, Matrix4x4.Identity),
            [
                new VisualizerDrawCommand(
                    new VisualizerResourceId(1),
                    new VisualizerResourceId(2),
                    Matrix4x4.Identity,
                    indexCount,
                    firstIndex: 0,
                    baseVertex: 0,
                    VisualizerRepresentation.Near,
                    entity: new VisualizerEntityId(42))
            ]);
}
