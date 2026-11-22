using System.Numerics;
using WorkshopFsmApi = FSM_API.FsmApi;
using TheSingularityWorkshop.Visualize;
using Xunit;

namespace TheSingularityWorkshop.Visualize.Tests;

public sealed class VisualizerExecutionTests
{
    [Fact]
    public async Task ProcessingGroupUpdatesAdvanceAnOwnedFrameContext()
    {
        var group = $"Visualize.Tests.{Guid.NewGuid():N}";
        var backend = new DeferredBackend();
        var visualizer = new Visualizer(backend, new VisualizerResourceCatalog());
        using var execution = new VisualizerExecution(visualizer, group);
        using var submission = execution.Submit(new VisualizerFrame(
            new VisualizerView(Vector3.Zero, Matrix4x4.Identity, Matrix4x4.Identity),
            []));

        Assert.Equal("Queued", submission.State);

        WorkshopFsmApi.Interact.Update(group);
        Assert.Equal("Preparing", submission.State);

        WorkshopFsmApi.Interact.Update(group);
        Assert.Equal("Submitting", submission.State);
        Assert.False(submission.IsTerminal);
        Assert.Equal(1, backend.SubmissionCount);

        backend.Completion.SetResult(new VisualizerFrameResult(1, 0));
        WorkshopFsmApi.Interact.Update(group);

        Assert.Equal("Completed", submission.State);
        Assert.True(submission.IsTerminal);
        Assert.Equal(1L, submission.Result?.FrameNumber);
        Assert.Null(submission.Error);

        await visualizer.DisposeAsync();
    }

    private sealed class DeferredBackend : IVisualizerBackend
    {
        public TaskCompletionSource<VisualizerFrameResult> Completion { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public int SubmissionCount { get; private set; }

        public VisualizerCapabilities Capabilities { get; } =
            new("Deferred", "FSM execution test", false, false, false, false);

        public ValueTask<VisualizerFrameResult> RenderAsync(
            VisualizerFrame frame,
            IVisualizerResourceProvider resources,
            CancellationToken cancellationToken = default)
        {
            SubmissionCount++;
            return new ValueTask<VisualizerFrameResult>(Completion.Task);
        }

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
