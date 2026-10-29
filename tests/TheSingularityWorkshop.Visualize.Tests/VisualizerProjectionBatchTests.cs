using System.Numerics;
using Xunit;

namespace TheSingularityWorkshop.Visualize.Tests;

public sealed class VisualizerProjectionBatchTests
{
    [Fact]
    public void Batch_PreservesCallerOrderAcrossIndependentTargets()
    {
        var source = new VisualizerSourceRectangle(0, 0, 320, 180, "logical-pixels");
        var first = new VisualizerProjectionRequest(source, new VisualizerProjectionTarget(
            "panel:left", VisualizerProjectionSpace.Screen, 640, 360, Matrix4x4.Identity));
        var second = new VisualizerProjectionRequest(source, new VisualizerProjectionTarget(
            "panel:world-preview", VisualizerProjectionSpace.World, 1024, 768, Matrix4x4.CreateScale(2)));

        var batch = new VisualizerProjectionBatch(new[] { first, second });

        Assert.Equal(2, batch.Requests.Count);
        Assert.Same(first, batch.Requests[0]);
        Assert.Same(second, batch.Requests[1]);
        Assert.NotEqual(batch.Requests[0].Target.ViewportWidth, batch.Requests[1].Target.ViewportWidth);
        Assert.NotEqual(batch.Requests[0].Target.Space, batch.Requests[1].Target.Space);
    }

    [Fact]
    public void Batch_RejectsEmptyRequestSet()
    {
        Assert.Throws<ArgumentException>(() => new VisualizerProjectionBatch(Array.Empty<VisualizerProjectionRequest>()));
    }

    [Fact]
    public void Batch_RejectsDuplicateTargetIdsUsingOrdinalIdentity()
    {
        var source = new VisualizerSourceRectangle(0, 0, 10, 10, "logical-pixels");
        var first = new VisualizerProjectionRequest(source, new VisualizerProjectionTarget(
            "panel:preview", VisualizerProjectionSpace.Screen, 100, 100, Matrix4x4.Identity));
        var duplicate = new VisualizerProjectionRequest(source, new VisualizerProjectionTarget(
            "panel:preview", VisualizerProjectionSpace.World, 200, 200, Matrix4x4.Identity));

        Assert.Throws<ArgumentException>(() => new VisualizerProjectionBatch(new[] { first, duplicate }));
    }

    [Fact]
    public void Batch_ExposesReadOnlyRequestCollection()
    {
        var source = new VisualizerSourceRectangle(0, 0, 10, 10, "logical-pixels");
        var request = new VisualizerProjectionRequest(source, new VisualizerProjectionTarget(
            "panel:preview", VisualizerProjectionSpace.Screen, 100, 100, Matrix4x4.Identity));
        var batch = new VisualizerProjectionBatch(new[] { request });

        Assert.IsAssignableFrom<IReadOnlyList<VisualizerProjectionRequest>>(batch.Requests);
        Assert.IsNotAssignableFrom<VisualizerProjectionRequest[]>(batch.Requests);
    }

    [Fact]
    public void Batch_SnapshotsCallerCollection()
    {
        var source = new VisualizerSourceRectangle(0, 0, 10, 10, "logical-pixels");
        var request = new VisualizerProjectionRequest(source, new VisualizerProjectionTarget(
            "panel:preview", VisualizerProjectionSpace.Screen, 100, 100, Matrix4x4.Identity));
        var callerOwned = new List<VisualizerProjectionRequest> { request };

        var batch = new VisualizerProjectionBatch(callerOwned);
        callerOwned.Clear();

        Assert.Single(batch.Requests);
        Assert.Same(request, batch.Requests[0]);
    }

    [Fact]
    public void Batch_RejectsNullRequestEntry()
    {
        var requests = new VisualizerProjectionRequest[] { null! };

        Assert.Throws<ArgumentException>(() => new VisualizerProjectionBatch(requests));
    }
}
