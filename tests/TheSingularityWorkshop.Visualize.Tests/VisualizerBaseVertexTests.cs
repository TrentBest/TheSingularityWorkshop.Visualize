using System.Numerics;
using TheSingularityWorkshop.Visualize;
using Xunit;

namespace TheSingularityWorkshop.Visualize.Tests;

public sealed class VisualizerBaseVertexTests
{
    [Fact]
    public async Task Base_vertex_is_applied_to_vertex_indices_not_index_buffer_offsets()
    {
        var resources = CreateResources(vertexCount: 6);
        var backend = new RecordingBackend();
        await using var visualizer = new Visualizer(backend, resources);
        var command = new VisualizerDrawCommand(
            new VisualizerResourceId(1), new VisualizerResourceId(2), Matrix4x4.Identity,
            indexCount: 3, firstIndex: 0, baseVertex: 3, VisualizerRepresentation.Near);
        var frame = new VisualizerFrame(
            new VisualizerView(Vector3.Zero, Matrix4x4.Identity, Matrix4x4.Identity), [command]);

        var result = await visualizer.RenderAsync(frame);

        Assert.True(backend.WasCalled);
        Assert.Equal(1, result.SubmittedDrawCount);
    }

    [Fact]
    public async Task Rejects_base_vertex_that_resolves_outside_geometry()
    {
        var resources = CreateResources(vertexCount: 3);
        var backend = new RecordingBackend();
        await using var visualizer = new Visualizer(backend, resources);
        var command = new VisualizerDrawCommand(
            new VisualizerResourceId(1), new VisualizerResourceId(2), Matrix4x4.Identity,
            indexCount: 3, firstIndex: 0, baseVertex: 1, VisualizerRepresentation.Near);
        var frame = new VisualizerFrame(
            new VisualizerView(Vector3.Zero, Matrix4x4.Identity, Matrix4x4.Identity), [command]);

        var error = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await visualizer.RenderAsync(frame));

        Assert.Contains("outside geometry resource", error.Message);
        Assert.False(backend.WasCalled);
    }

    private static VisualizerResourceCatalog CreateResources(int vertexCount)
    {
        var catalog = new VisualizerResourceCatalog();
        var vertices = Enumerable.Range(0, vertexCount)
            .Select(index => new VisualizerVertex(
                new Vector3(index, 0, 0), Vector3.UnitZ, Vector2.Zero, Vector4.One))
            .ToArray();
        catalog.SetGeometry(new VisualizerResourceId(1), new VisualizerGeometry(1, vertices, [0, 1, 2]));
        catalog.SetMaterial(new VisualizerResourceId(2), new VisualizerMaterial(1, Vector4.One));
        return catalog;
    }

    private sealed class RecordingBackend : IVisualizerBackend
    {
        public bool WasCalled { get; private set; }
        public VisualizerCapabilities Capabilities { get; } = new("Test backend", "Test", false, false, false, false);

        public ValueTask<VisualizerFrameResult> RenderAsync(
            VisualizerFrame frame, IVisualizerResourceProvider resources, CancellationToken cancellationToken = default)
        {
            WasCalled = true;
            return ValueTask.FromResult(new VisualizerFrameResult(1, frame.Commands.Count));
        }

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
