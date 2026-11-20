using System.Numerics;
using TheSingularityWorkshop.Visualize;
using Xunit;

namespace TheSingularityWorkshop.Visualize.Tests;

public sealed class MyCityBlockPipelineTests
{
    [Fact]
    public async Task EmptySceneIsAValidEmptyArtifactWhileAbsentFrameIsRejected()
    {
        var snapshot = new VisualizerSceneSnapshot("workshop://mycity/empty", 1, []);
        var frame = snapshot.CreateFrame(new VisualizerView(Vector3.Zero, Matrix4x4.Identity, Matrix4x4.Identity));
        var backend = new SvgVisualizerBackend(_ => { });
        var svg = backend.BuildSvg(frame, new VisualizerResourceCatalog());

        Assert.Empty(frame.Commands);
        Assert.Contains("<svg", svg, StringComparison.Ordinal);
        Assert.DoesNotContain("<polygon", svg, StringComparison.Ordinal);

        await using var visualizer = new Visualizer(backend, new VisualizerResourceCatalog());
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await visualizer.RenderAsync(null!));
    }

    [Fact]
    public async Task MissingGeometryResourceIsRejectedRatherThanRenderedAsEmpty()
    {
        var frame = Frame();
        await using var visualizer = new Visualizer(
            new SvgVisualizerBackend(_ => { }),
            new VisualizerResourceCatalog());

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await visualizer.RenderAsync(frame));

        Assert.Contains("missing geometry", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task InvalidGeometryIndexIsRejectedBeforeBackendSubmission()
    {
        var resources = new VisualizerResourceCatalog();
        resources.SetGeometry(new VisualizerResourceId(1), new VisualizerGeometry(
            1,
            [
                Vertex(-0.5f, -0.5f),
                Vertex(0.5f, -0.5f),
                Vertex(0, 0.5f)
            ],
            [0, 1, 99]));
        resources.SetMaterial(new VisualizerResourceId(2), new VisualizerMaterial(1, Vector4.One));
        await using var visualizer = new Visualizer(new SvgVisualizerBackend(_ => { }), resources);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await visualizer.RenderAsync(Frame()));

        Assert.Contains("outside geometry", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void RepresentationChangePreservesCityEntityIdentity()
    {
        var resources = Resources();
        var first = Snapshot(1, VisualizerRepresentation.Near);
        var second = Snapshot(2, VisualizerRepresentation.Distant);
        var backend = new SvgVisualizerBackend(_ => { });
        var view = new VisualizerView(Vector3.Zero, Matrix4x4.Identity, Matrix4x4.Identity);

        var nearSvg = backend.BuildSvg(first.CreateFrame(view), resources);
        var distantSvg = backend.BuildSvg(second.CreateFrame(view), resources);

        Assert.Contains("data-entity-id=\"301\"", nearSvg, StringComparison.Ordinal);
        Assert.Contains("data-entity-id=\"301\"", distantSvg, StringComparison.Ordinal);
        Assert.Contains("data-representation=\"near\"", nearSvg, StringComparison.Ordinal);
        Assert.Contains("data-representation=\"distant\"", distantSvg, StringComparison.Ordinal);
        Assert.Equal(first.SceneIdentity, second.SceneIdentity);
        Assert.Equal(1UL, first.Revision);
        Assert.Equal(2UL, second.Revision);
    }

    private static VisualizerFrame Frame() =>
        new(
            new VisualizerView(Vector3.Zero, Matrix4x4.Identity, Matrix4x4.Identity),
            [new VisualizerDrawCommand(
                new VisualizerResourceId(1),
                new VisualizerResourceId(2),
                Matrix4x4.Identity,
                3,
                0,
                0,
                VisualizerRepresentation.Near,
                entity: new VisualizerEntityId(301))]);

    private static VisualizerResourceCatalog Resources()
    {
        var resources = new VisualizerResourceCatalog();
        resources.SetGeometry(new VisualizerResourceId(1), new VisualizerGeometry(
            1,
            [Vertex(-0.5f, -0.5f), Vertex(0.5f, -0.5f), Vertex(0, 0.5f)],
            [0, 1, 2]));
        resources.SetMaterial(new VisualizerResourceId(2), new VisualizerMaterial(1, Vector4.One));
        return resources;
    }

    private static VisualizerVertex Vertex(float x, float y) =>
        new(new Vector3(x, y, 0), Vector3.UnitZ, Vector2.Zero, Vector4.One);

    private static VisualizerSceneSnapshot Snapshot(ulong revision, VisualizerRepresentation representation)
    {
        var command = new VisualizerDrawCommand(
            new VisualizerResourceId(1),
            new VisualizerResourceId(2),
            Matrix4x4.Identity,
            3,
            0,
            0,
            representation,
            entity: new VisualizerEntityId(301));
        return new VisualizerSceneSnapshot(
            "workshop://mycity/block/test",
            revision,
            [new VisualizerSceneObject(new VisualizerEntityId(301), 0, true, [command])]);
    }
}
