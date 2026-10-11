using System.Numerics;
using TheSingularityWorkshop.Visualize;
using Xunit;

namespace TheSingularityWorkshop.Visualize.Tests;

public sealed class VisualizerContractTests
{
    [Fact]
    public void ResourceCatalog_ResolvesRegisteredGeometryAndMaterial()
    {
        var catalog = new VisualizerResourceCatalog();
        var geometryId = new VisualizerResourceId(1);
        var materialId = new RenderResourceId(2);
        var geometry = new VisualizerGeometry(
            1,
            [new VisualizerVertex(Vector3.Zero, Vector3.UnitZ, Vector2.Zero, Vector4.One)],
            [0]);
        var material = new VisualizerMaterial(1, Vector4.One);

        catalog.SetGeometry(geometryId, geometry);
        catalog.SetMaterial(materialId, material);

        Assert.True(catalog.TryGetGeometry(geometryId, out var resolvedGeometry));
        Assert.Same(geometry, resolvedGeometry);
        Assert.True(catalog.TryGetMaterial(materialId, out var resolvedMaterial));
        Assert.Same(material, resolvedMaterial);
    }

    [Fact]
    public void ResourceCatalog_RejectsIndicesOutsideVertexArray()
    {
        Assert.Throws<ArgumentException>(() => new VisualizerGeometry(
            1,
            [new VisualizerVertex(Vector3.Zero, Vector3.UnitZ, Vector2.Zero, Vector4.One)],
            [1]));
    }

    [Fact]
    public async Task Visualizer_RejectsMissingResourcesBeforeBackendSubmission()
    {
        var backend = new RecordingBackend();
        var visualizer = new Visualizer(backend, new VisualizerResourceCatalog());
        var view = new VisualizerView(Vector3.Zero, Matrix4x4.Identity, Matrix4x4.Identity);
        var frame = new VisualizerFrame(view,
        [
            new VisualizerDrawCommand(new RenderResourceId(10), new RenderResourceId(20), Matrix4x4.Identity, 3, 0, 0, default)
        ]);

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await visualizer.RenderAsync(frame);
        });
        Assert.Equal(0, backend.SubmissionCount);
        await visualizer.DisposeAsync();
    }

    [Fact]
    public async Task Visualizer_DisposesBackendOnce()
    {
        var backend = new RecordingBackend();
        var visualizer = new Visualizer(backend, new VisualizerResourceCatalog());

        await visualizer.DisposeAsync();
        await visualizer.DisposeAsync();

        Assert.Equal(1, backend.DisposeCount);
    }
 
    [Fact]
    public async Task Visualizer_RejectsDrawRangeBeyondGeometry()
    {
        var catalog = new VisualizerResourceCatalog();
        catalog.SetGeometry(new RenderResourceId(10), new VisualizerGeometry(
            1,
            [
                new VisualizerVertex(Vector3.Zero, Vector3.UnitZ, Vector2.Zero, Vector4.One),
                new VisualizerVertex(Vector3.UnitX, Vector3.UnitZ, Vector2.UnitX, Vector4.One),
                new VisualizerVertex(Vector3.UnitY, Vector3.UnitZ, Vector2.UnitY, Vector4.One)
            ],
            [0, 1, 2]));
        catalog.SetMaterial(new RenderResourceId(20), new VisualizerMaterial(1, Vector4.One));

        var backend = new RecordingBackend();
        var visualizer = new Visualizer(backend, catalog);
        var frame = new RenderFrame(
            new RenderView(Vector3.Zero, Matrix4x4.Identity, Matrix4x4.Identity),
            [
                new RenderDrawCommand(new RenderResourceId(10), new RenderResourceId(20), Matrix4x4.Identity, 3, 1, 0, default)
            ]);

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await visualizer.RenderAsync(frame);
        });
        Assert.Equal(0, backend.SubmissionCount);
        await visualizer.DisposeAsync();
    }

    private sealed class RecordingBackend : IVisualizerBackend
    {
        public int SubmissionCount { get; private set; }
        public int DisposeCount { get; private set; }

        public VisualizerCapabilities Capabilities { get; } =
            new("Recording", "Test", false, false, false, false);

        public ValueTask<VisualizerFrameResult> RenderAsync(
            RenderFrame frame,
            IVisualizerResourceProvider resources,
            CancellationToken cancellationToken = default)
        {
            SubmissionCount++;
            return ValueTask.FromResult(new VisualizerFrameResult(0, frame.Commands.Count));
        }

        public ValueTask DisposeAsync()
        {
            DisposeCount++;
            return ValueTask.CompletedTask;
        }
    }
}
