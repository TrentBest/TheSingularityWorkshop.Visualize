using System.Numerics;
using Xunit;

namespace TheSingularityWorkshop.Visualize.Tests;

public sealed class VisualizerProjectionTransformTests
{
    [Fact]
    public void TryTransformPoint_IdentityPreservesSourceCoordinates()
    {
        var success = VisualizerProjectionTransform.TryTransformPoint(
            new Vector2(12.5f, -4f), Matrix4x4.Identity, out var destination);

        Assert.True(success);
        Assert.Equal(new Vector3(12.5f, -4f, 0f), destination);
    }

    [Fact]
    public void TryTransformPoint_UsesSystemNumericsRowVectorTranslationConvention()
    {
        var matrix = Matrix4x4.CreateTranslation(7f, 11f, 3f);

        var success = VisualizerProjectionTransform.TryTransformPoint(
            new Vector2(2f, 5f), matrix, out var destination);

        Assert.True(success);
        Assert.Equal(new Vector3(9f, 16f, 3f), destination);
    }

    [Fact]
    public void TryTransformPoint_PerspectiveDividesByHomogeneousW()
    {
        var matrix = Matrix4x4.Identity;
        matrix.M44 = 2f;

        var success = VisualizerProjectionTransform.TryTransformPoint(
            new Vector2(8f, 6f), matrix, out var destination);

        Assert.True(success);
        Assert.Equal(new Vector3(4f, 3f, 0f), destination);
    }

    [Theory]
    [InlineData(float.NaN, 0f)]
    [InlineData(float.PositiveInfinity, 0f)]
    [InlineData(0f, float.NegativeInfinity)]
    public void TryTransformPoint_RejectsNonFiniteSourcePoint(float x, float y)
    {
        var success = VisualizerProjectionTransform.TryTransformPoint(
            new Vector2(x, y), Matrix4x4.Identity, out var destination);

        Assert.False(success);
        Assert.Equal(default, destination);
    }

    [Fact]
    public void TryTransformPoint_RejectsNonFiniteMatrix()
    {
        var matrix = Matrix4x4.Identity;
        matrix.M12 = float.NaN;

        var success = VisualizerProjectionTransform.TryTransformPoint(
            Vector2.One, matrix, out var destination);

        Assert.False(success);
        Assert.Equal(default, destination);
    }

    [Fact]
    public void TryTransformPoint_RejectsNearZeroHomogeneousW()
    {
        var matrix = Matrix4x4.Identity;
        matrix.M44 = 1e-8f;

        var success = VisualizerProjectionTransform.TryTransformPoint(
            Vector2.One, matrix, out var destination);

        Assert.False(success);
        Assert.Equal(default, destination);
    }

    [Fact]
    public void TryTransformPoint_RejectsNonFinitePerspectiveDividedResult()
    {
        var matrix = Matrix4x4.Identity;
        matrix.M11 = float.MaxValue;
        matrix.M44 = 0.5f;

        var success = VisualizerProjectionTransform.TryTransformPoint(
            new Vector2(2f, 0f), matrix, out var destination);

        Assert.False(success);
        Assert.Equal(default, destination);
    }
}
