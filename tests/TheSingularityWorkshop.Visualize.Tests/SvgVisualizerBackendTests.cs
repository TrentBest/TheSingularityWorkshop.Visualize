using System.Numerics;
using TheSingularityWorkshop.Visualize;
using Xunit;

namespace TheSingularityWorkshop.Visualize.Tests;

public sealed class SvgVisualizerBackendTests
{
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

    private static VisualizerResourceCatalog CreateResources()
    {
        var resources = new VisualizerResourceCatalog();
        resources.SetGeometry(new VisualizerResourceId(1), new VisualizerGeometry(
            revision: 1,
            vertices:
            [
                new VisualizerVertex(new Vector3(-0.5f, -0.5f, 0), Vector3.UnitZ, Vector2.Zero, Vector4.One),
                new VisualizerVertex(new Vector3(0.5f, -0.5f, 0), Vector3.UnitZ, Vector2.UnitX, Vector4.One),
                new VisualizerVertex(new Vector3(0, 0.5f, 0), Vector3.UnitZ, Vector2.UnitY, Vector4.One)
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
