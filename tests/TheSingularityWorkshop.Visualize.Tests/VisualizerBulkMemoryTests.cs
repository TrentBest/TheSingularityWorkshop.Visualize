using System.Numerics;
using TheSingularityWorkshop.Visualize;
using Xunit;

namespace TheSingularityWorkshop.Visualize.Tests;

public sealed class VisualizerBulkMemoryTests
{
    [Fact]
    public void Geometry_ExposesContiguousMemoryForTheSameSnapshot()
    {
        var geometry = new VisualizerGeometry(
            12,
            [
                new VisualizerVertex(Vector3.Zero, Vector3.UnitZ, Vector2.Zero, Vector4.One),
                new VisualizerVertex(Vector3.One, Vector3.UnitZ, Vector2.One, Vector4.One),
                new VisualizerVertex(Vector3.UnitX, Vector3.UnitZ, Vector2.Zero, Vector4.One)
            ],
            [0, 1, 2]);

        Assert.Equal(3, geometry.VertexMemory.Length);
        Assert.Equal(3, geometry.IndexMemory.Length);
        Assert.Equal(geometry.Vertices[1], geometry.VertexMemory.Span[1]);
        Assert.Equal(geometry.Indices[2], geometry.IndexMemory.Span[2]);
        Assert.Equal(12UL, geometry.Revision);
    }

    [Fact]
    public void InstanceBuffer_ExposesContiguousMemoryForTheSameSnapshot()
    {
        var instances = new VisualizerInstanceBuffer(
            9,
            [
                new VisualizerInstance(Matrix4x4.Identity, Vector4.One),
                new VisualizerInstance(Matrix4x4.CreateTranslation(1, 2, 3), new Vector4(0, 1, 0, 1))
            ]);

        Assert.Equal(2, instances.InstanceMemory.Length);
        Assert.Equal(instances.Instances[1], instances.InstanceMemory.Span[1]);
        Assert.Equal(9UL, instances.Revision);
    }

    [Fact]
    public void BulkMemoryDoesNotExposeTheCallerOwnedMutableArray()
    {
        var source = new[]
        {
            new VisualizerVertex(Vector3.Zero, Vector3.UnitZ, Vector2.Zero, Vector4.One)
        };
        var geometry = new VisualizerGeometry(1, source, [0]);

        source[0] = new VisualizerVertex(Vector3.One, Vector3.UnitX, Vector2.One, Vector4.Zero);

        Assert.Equal(Vector3.Zero, geometry.VertexMemory.Span[0].Position);
    }
}
