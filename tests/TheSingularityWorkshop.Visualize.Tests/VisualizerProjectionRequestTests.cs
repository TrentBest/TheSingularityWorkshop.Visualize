using System.Numerics;
using Xunit;

namespace TheSingularityWorkshop.Visualize.Tests;

public sealed class VisualizerProjectionRequestTests
{
    [Fact]
    public void Request_KeepsSourceRectangleIndependentFromDestinationViewport()
    {
        var source = new VisualizerSourceRectangle(10, 20, 320, 180, "logical-pixels");
        var transform = Matrix4x4.CreateScale(0.5f, 0.5f, 1f);
        var target = new VisualizerProjectionTarget("panel:preview", VisualizerProjectionSpace.Screen, 1920, 1080, transform);
        var request = new VisualizerProjectionRequest(source, target);

        Assert.Equal(10, request.Source.X);
        Assert.Equal(20, request.Source.Y);
        Assert.Equal(320, request.Source.Width);
        Assert.Equal(180, request.Source.Height);
        Assert.Equal("logical-pixels", request.Source.CoordinateSystem);
        Assert.Equal("panel:preview", request.Target.TargetId);
        Assert.Equal(1920, request.Target.ViewportWidth);
        Assert.Equal(1080, request.Target.ViewportHeight);
        Assert.Equal(transform, request.Target.SourceToDestination);
    }

    [Theory]
    [InlineData(float.NaN, 0, 10, 10)]
    [InlineData(0, float.PositiveInfinity, 10, 10)]
    [InlineData(0, 0, 0, 10)]
    [InlineData(0, 0, 10, -1)]
    public void SourceRectangle_RejectsInvalidCoordinatesAndDimensions(float x, float y, float width, float height)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new VisualizerSourceRectangle(x, y, width, height, "logical-pixels"));
    }

    [Theory]
    [InlineData(float.MaxValue, 0, float.MaxValue, 1)]
    [InlineData(0, float.MaxValue, 1, float.MaxValue)]
    public void SourceRectangle_RejectsOverflowingRightOrBottomEdge(float x, float y, float width, float height)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new VisualizerSourceRectangle(x, y, width, height, "logical-pixels"));
    }

    [Fact]
    public void SourceRectangle_RequiresCoordinateSystem()
    {
        Assert.Throws<ArgumentException>(() => new VisualizerSourceRectangle(0, 0, 10, 10, " "));
    }

    [Fact]
    public void ProjectionTarget_RejectsInvalidViewportAndNonFiniteTransform()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new VisualizerProjectionTarget("preview", VisualizerProjectionSpace.Screen, 0, 100, Matrix4x4.Identity));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new VisualizerProjectionTarget("preview", VisualizerProjectionSpace.World, 100, 100,
                new Matrix4x4(float.NaN, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1)));
    }

    [Fact]
    public void ProjectionTarget_RequiresStableTargetIdentity()
    {
        Assert.Throws<ArgumentException>(() =>
            new VisualizerProjectionTarget(" ", VisualizerProjectionSpace.Screen, 100, 100, Matrix4x4.Identity));
    }

    [Fact]
    public void Request_RejectsDefaultUninitializedSourceRectangle()
    {
        Assert.Throws<ArgumentException>(() =>
            new VisualizerProjectionRequest(default, new VisualizerProjectionTarget(
                "preview", VisualizerProjectionSpace.Screen, 100, 100, Matrix4x4.Identity)));
    }
}
