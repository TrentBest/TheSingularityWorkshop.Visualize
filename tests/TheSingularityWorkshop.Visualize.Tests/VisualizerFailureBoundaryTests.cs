using System.Numerics;
using TheSingularityWorkshop.Visualize;
using Xunit;

namespace TheSingularityWorkshop.Visualize.Tests;

/// <summary>Verifies that resource and backend failures remain explicit at the submission boundary.</summary>
public sealed class VisualizerFailureBoundaryTests
{
    [Fact]
    public async Task MissingMaterial_IsRejectedBeforeBackendSubmission()
    {
        var catalog = CreateCatalog(includeMaterial: false);
        var backend = new RecordingBackend();
        await using var visualizer = new Visualizer(backend, catalog);

        var error = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await visualizer.RenderAsync(CreateFrame()));

        Assert.Contains("missing material resource 2", error.Message);
        Assert.Equal(0, backend.SubmissionCount);
    }

    [Fact]
    public async Task MissingGeometry_IsRejectedBeforeBackendSubmission()
    {
        var catalog = CreateCatalog(includeMaterial: true, includeGeometry: false);
        var backend = new RecordingBackend();
        await using var visualizer = new Visualizer(backend, catalog);

        var error = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await visualizer.RenderAsync(CreateFrame()));

        Assert.Contains("missing geometry resource 1", error.Message);
        Assert.Equal(0, backend.SubmissionCount);
    }

    [Fact]
    public async Task MissingInstanceResource_IsRejectedBeforeBackendSubmission()
    {
        var catalog = CreateCatalog(includeMaterial: true);
        var backend = new RecordingBackend();
        await using var visualizer = new Visualizer(backend, catalog);
        var frame = CreateFrame(instanced: true);

        var error = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await visualizer.RenderAsync(frame));

        Assert.Contains("missing instance resource 3", error.Message);
        Assert.Equal(0, backend.SubmissionCount);
    }

    [Fact]
    public async Task BackendFailure_IsPropagatedAfterExactlyOneSubmission()
    {
        var catalog = CreateCatalog(includeMaterial: true);
        var expected = new IOException("simulated backend write failure");
        var backend = new RecordingBackend(expected);
        await using var visualizer = new Visualizer(backend, catalog);

        var actual = await Assert.ThrowsAsync<IOException>(
            async () => await visualizer.RenderAsync(CreateFrame()));

        Assert.Same(expected, actual);
        Assert.Equal(1, backend.SubmissionCount);
    }

    private static VisualizerResourceCatalog CreateCatalog(bool includeMaterial, bool includeGeometry = true)
    {
        var catalog = new VisualizerResourceCatalog();
        if (includeGeometry)
            catalog.SetGeometry(new VisualizerResourceId(1), new VisualizerGeometry(
                1,
                [
                    new VisualizerVertex(Vector3.Zero, Vector3.UnitZ, Vector2.Zero, Vector4.One),
                    new VisualizerVertex(Vector3.UnitX, Vector3.UnitZ, Vector2.UnitX, Vector4.One),
                    new VisualizerVertex(Vector3.UnitY, Vector3.UnitZ, Vector2.UnitY, Vector4.One)
                ],
                [0, 1, 2]));
        if (includeMaterial)
            catalog.SetMaterial(new VisualizerResourceId(2), new VisualizerMaterial(1, Vector4.One));
        return catalog;
    }

    private static VisualizerFrame CreateFrame(bool instanced = false) => new(
        new VisualizerView(Vector3.Zero, Matrix4x4.Identity, Matrix4x4.Identity),
        [
            new VisualizerDrawCommand(
                new VisualizerResourceId(1),
                new VisualizerResourceId(2),
                Matrix4x4.Identity,
                3,
                0,
                0,
                VisualizerRepresentation.Near,
                instances: instanced ? new VisualizerResourceId(3) : default)
        ]);

    private sealed class RecordingBackend(Exception? failure = null) : IVisualizerBackend
    {
        public int SubmissionCount { get; private set; }

        public VisualizerCapabilities Capabilities { get; } =
            new("Failure-boundary test backend", "Test", false, false, false, false);

        public ValueTask<VisualizerFrameResult> RenderAsync(
            VisualizerFrame frame,
            IVisualizerResourceProvider resources,
            CancellationToken cancellationToken = default)
        {
            SubmissionCount++;
            if (failure is not null)
                return ValueTask.FromException<VisualizerFrameResult>(failure);
            return ValueTask.FromResult(new VisualizerFrameResult(1, frame.Commands.Count));
        }

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
