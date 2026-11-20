using System.Numerics;
using TheSingularityWorkshop.Visualize;
using Xunit;

namespace TheSingularityWorkshop.Visualize.Tests;

public sealed class VisualizerDrawValidationTests
{
    [Fact]
    public void DrawCommandRejectsUndefinedRepresentation()
    {
        var error = Assert.Throws<ArgumentOutOfRangeException>(() =>
            CreateCommand(representation: (VisualizerRepresentation)int.MaxValue));

        Assert.Equal("representation", error.ParamName);
    }

    [Fact]
    public async Task DrawRangeOutsideIndexBuffer_IsRejectedBeforeBackendSubmission()
    {
        var (visualizer, backend) = CreateVisualizer();
        await using (visualizer)
        {
            var error = await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await visualizer.RenderAsync(CreateFrame(firstIndex: 1, indexCount: 3)));

            Assert.Contains("requests indices outside geometry resource 1", error.Message);
            Assert.Equal(0, backend.SubmissionCount);
        }
    }

    [Fact]
    public async Task BaseVertexOutsideGeometry_IsRejectedBeforeBackendSubmission()
    {
        var (visualizer, backend) = CreateVisualizer();
        await using (visualizer)
        {
            var error = await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await visualizer.RenderAsync(CreateFrame(baseVertex: 1)));

            Assert.Contains("outside geometry resource 1", error.Message);
            Assert.Equal(0, backend.SubmissionCount);
        }
    }

    private static VisualizerDrawCommand CreateCommand(
        int firstIndex = 0,
        int indexCount = 3,
        int baseVertex = 0,
        VisualizerRepresentation representation = VisualizerRepresentation.Near) =>
        new(new VisualizerResourceId(1), new VisualizerResourceId(2), Matrix4x4.Identity,
            indexCount, firstIndex, baseVertex, representation);

    private static VisualizerFrame CreateFrame(int firstIndex = 0, int indexCount = 3, int baseVertex = 0) =>
        new(new VisualizerView(Vector3.Zero, Matrix4x4.Identity, Matrix4x4.Identity),
            [CreateCommand(firstIndex, indexCount, baseVertex)]);

    private static (Visualizer Visualizer, RecordingBackend Backend) CreateVisualizer()
    {
        var resources = new VisualizerResourceCatalog();
        resources.SetGeometry(new VisualizerResourceId(1), new VisualizerGeometry(
            1,
            [
                new VisualizerVertex(Vector3.Zero, Vector3.UnitZ, Vector2.Zero, Vector4.One),
                new VisualizerVertex(Vector3.UnitX, Vector3.UnitZ, Vector2.UnitX, Vector4.One),
                new VisualizerVertex(Vector3.UnitY, Vector3.UnitZ, Vector2.UnitY, Vector4.One)
            ],
            [0, 1, 2]));
        resources.SetMaterial(new VisualizerResourceId(2), new VisualizerMaterial(1, Vector4.One));

        var backend = new RecordingBackend();
        return (new Visualizer(backend, resources), backend);
    }

    private sealed class RecordingBackend : IVisualizerBackend
    {
        public int SubmissionCount { get; private set; }

        public VisualizerCapabilities Capabilities { get; } =
            new("Draw validation test backend", "Test", false, false, false, false);

        public ValueTask<VisualizerFrameResult> RenderAsync(
            VisualizerFrame frame,
            IVisualizerResourceProvider resources,
            CancellationToken cancellationToken = default)
        {
            SubmissionCount++;
            return ValueTask.FromResult(new VisualizerFrameResult(1, frame.Commands.Count));
        }

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
