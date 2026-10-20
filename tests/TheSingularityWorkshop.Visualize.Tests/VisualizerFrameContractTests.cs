using System.Numerics;
using TheSingularityWorkshop.Visualize;
using Xunit;

namespace TheSingularityWorkshop.Visualize.Tests;

public sealed class VisualizerFrameContractTests
{
    [Fact]
    public void FrameAcceptsFiniteViewAndEmptyCommands()
    {
        var frame = new VisualizerFrame(
            new VisualizerView(Vector3.One, Matrix4x4.Identity, Matrix4x4.Identity),
            []);

        Assert.Equal(Vector3.One, frame.View.ObserverPosition);
        Assert.Empty(frame.Commands);
    }

    [Fact]
    public void FrameRejectsNonFiniteObserverPosition()
    {
        var view = new VisualizerView(
            new Vector3(0, float.NaN, 0), Matrix4x4.Identity, Matrix4x4.Identity);

        var error = Assert.Throws<ArgumentOutOfRangeException>(
            () => new VisualizerFrame(view, []));

        Assert.Equal("view", error.ParamName);
        Assert.Contains("Observer position", error.Message);
    }

    [Fact]
    public void FrameRejectsNonFiniteViewMatrix()
    {
        var matrix = Matrix4x4.Identity;
        matrix.M12 = float.PositiveInfinity;
        var view = new VisualizerView(Vector3.Zero, matrix, Matrix4x4.Identity);

        var error = Assert.Throws<ArgumentOutOfRangeException>(
            () => new VisualizerFrame(view, []));

        Assert.Equal("view", error.ParamName);
        Assert.Contains("View matrix", error.Message);
    }

    [Fact]
    public void FrameRejectsNonFiniteProjectionMatrix()
    {
        var matrix = Matrix4x4.Identity;
        matrix.M43 = float.NaN;
        var view = new VisualizerView(Vector3.Zero, Matrix4x4.Identity, matrix);

        var error = Assert.Throws<ArgumentOutOfRangeException>(
            () => new VisualizerFrame(view, []));

        Assert.Equal("view", error.ParamName);
        Assert.Contains("Projection matrix", error.Message);
    }

    [Fact]
    public void DrawCommandRejectsNonFiniteWorldTransform()
    {
        var transform = Matrix4x4.Identity;
        transform.M33 = float.NegativeInfinity;

        var error = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new VisualizerDrawCommand(
                new VisualizerResourceId(1),
                new VisualizerResourceId(2),
                transform,
                3,
                0,
                0,
                VisualizerRepresentation.Near));

        Assert.Equal("worldTransform", error.ParamName);
        Assert.Contains("World transform", error.Message);
    }
}
