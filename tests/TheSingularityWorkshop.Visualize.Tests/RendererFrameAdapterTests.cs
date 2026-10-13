using System.Numerics;
using TheSingularityWorkshop.Renderer;
using TheSingularityWorkshop.Renderer.VisualizeAdapter;
using TheSingularityWorkshop.Visualize;
using Xunit;

namespace TheSingularityWorkshop.Visualize.Tests;

public sealed class RendererFrameAdapterTests
{
    [Fact]
    public void Adapt_PreservesViewCommandOrderResourcesAndRepresentation()
    {
        var viewMatrix = Matrix4x4.CreateTranslation(1, 2, 3);
        var projectionMatrix = Matrix4x4.CreateScale(4, 5, 6);
        var worldMatrix = Matrix4x4.CreateTranslation(7, 8, 9);
        var source = new RenderFrame(
            new RenderView(new Vector3(10, 11, 12), viewMatrix, projectionMatrix),
            [
                new RenderDrawCommand(
                    new RenderResourceId(101),
                    new RenderResourceId(202),
                    worldMatrix,
                    indexCount: 36,
                    firstIndex: 6,
                    baseVertex: 3,
                    RendererRepresentation.Interactive,
                    new RenderResourceId(303),
                    instanceCount: 8,
                    firstInstance: 2),
                new RenderDrawCommand(
                    new RenderResourceId(104),
                    new RenderResourceId(205),
                    Matrix4x4.Identity,
                    indexCount: 3,
                    firstIndex: 0,
                    baseVertex: 0,
                    RendererRepresentation.Distant)
            ]);

        var actual = new RendererFrameAdapter().Adapt(source);

        Assert.Equal(source.View.ObserverPosition, actual.View.ObserverPosition);
        Assert.Equal(viewMatrix, actual.View.View);
        Assert.Equal(projectionMatrix, actual.View.Projection);
        Assert.Equal(2, actual.Commands.Count);

        var first = actual.Commands[0];
        Assert.Equal(101UL, first.Geometry.Value);
        Assert.Equal(202UL, first.Material.Value);
        Assert.Equal(worldMatrix, first.WorldTransform);
        Assert.Equal(36, first.IndexCount);
        Assert.Equal(6, first.FirstIndex);
        Assert.Equal(3, first.BaseVertex);
        Assert.Equal(VisualizerRepresentation.Interactive, first.Representation);
        Assert.Equal(303UL, first.Instances.Value);
        Assert.Equal(8, first.InstanceCount);
        Assert.Equal(2, first.FirstInstance);

        var second = actual.Commands[1];
        Assert.Equal(104UL, second.Geometry.Value);
        Assert.Equal(205UL, second.Material.Value);
        Assert.Equal(VisualizerRepresentation.Distant, second.Representation);
        Assert.False(second.Instances.IsValid);
        Assert.Equal(1, second.InstanceCount);
        Assert.Equal(0, second.FirstInstance);
    }

    [Fact]
    public void Adapt_RejectsUndefinedRendererRepresentation()
    {
        var source = new RenderFrame(
            new RenderView(Vector3.Zero, Matrix4x4.Identity, Matrix4x4.Identity),
            [
                new RenderDrawCommand(
                    new RenderResourceId(1),
                    new RenderResourceId(2),
                    Matrix4x4.Identity,
                    3,
                    0,
                    0,
                    (RendererRepresentation)99)
            ]);

        Assert.Throws<ArgumentOutOfRangeException>(() => new RendererFrameAdapter().Adapt(source));
    }
}