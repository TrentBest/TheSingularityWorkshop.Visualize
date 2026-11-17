using System.Numerics;
using TheSingularityWorkshop.Visualize;
using Xunit;

namespace TheSingularityWorkshop.Visualize.Tests;

public sealed class VisualizerInstanceBufferValidationTests
{
    [Fact]
    public void InstanceBufferRejectsNonFiniteWorldTransform()
    {
        var instance = new VisualizerInstance(
            Matrix4x4.CreateScale(float.NaN),
            Vector4.One);

        var error = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new VisualizerInstanceBuffer(1, [instance]));

        Assert.Contains("Instance 0 world transform components must be finite", error.Message);
    }

    [Fact]
    public void InstanceBufferRejectsNonFiniteTint()
    {
        var instance = new VisualizerInstance(
            Matrix4x4.Identity,
            new Vector4(1, float.PositiveInfinity, 1, 1));

        var error = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new VisualizerInstanceBuffer(1, [instance]));

        Assert.Contains("Instance 0 color components must be finite", error.Message);
    }

    [Fact]
    public void InstanceBufferAcceptsFiniteTransformAndTint()
    {
        var instance = new VisualizerInstance(
            Matrix4x4.CreateTranslation(1, 2, 3),
            new Vector4(0.5f, 0.75f, 1, 1));

        var buffer = new VisualizerInstanceBuffer(7, [instance]);

        Assert.Equal(7UL, buffer.Revision);
        Assert.Single(buffer.Instances);
        Assert.Equal(instance, buffer.Instances[0]);
    }
}
