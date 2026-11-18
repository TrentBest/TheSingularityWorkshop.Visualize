using System.Numerics;
using TheSingularityWorkshop.Visualize;
using Xunit;

namespace TheSingularityWorkshop.Visualize.Tests;

public sealed class VisualizerGeometryValidationTests
{
    [Fact]
    public void GeometryRejectsNonFinitePosition()
    {
        var error = Assert.Throws<ArgumentOutOfRangeException>(() =>
            CreateGeometry(new VisualizerVertex(
                new Vector3(float.NaN, 0, 0), Vector3.UnitZ, Vector2.Zero, Vector4.One)));

        Assert.Contains("Vertex 0 position components must be finite", error.Message);
    }

    [Fact]
    public void GeometryRejectsNonFiniteNormal()
    {
        var error = Assert.Throws<ArgumentOutOfRangeException>(() =>
            CreateGeometry(new VisualizerVertex(
                Vector3.Zero, new Vector3(0, float.PositiveInfinity, 1), Vector2.Zero, Vector4.One)));

        Assert.Contains("Vertex 0 normal components must be finite", error.Message);
    }

    [Fact]
    public void GeometryRejectsNonFiniteTextureCoordinate()
    {
        var error = Assert.Throws<ArgumentOutOfRangeException>(() =>
            CreateGeometry(new VisualizerVertex(
                Vector3.Zero, Vector3.UnitZ, new Vector2(0, float.NegativeInfinity), Vector4.One)));

        Assert.Contains("Vertex 0 texture-coordinate components must be finite", error.Message);
    }

    [Fact]
    public void GeometryRejectsNonFiniteColor()
    {
        var error = Assert.Throws<ArgumentOutOfRangeException>(() =>
            CreateGeometry(new VisualizerVertex(
                Vector3.Zero, Vector3.UnitZ, Vector2.Zero, new Vector4(1, 1, float.NaN, 1))));

        Assert.Contains("Vertex 0 color components must be finite", error.Message);
    }

    [Fact]
    public void GeometryAcceptsFiniteVertexDataAndPreservesSnapshot()
    {
        var source = new List<VisualizerVertex>
        {
            new(Vector3.Zero, Vector3.UnitZ, Vector2.Zero, Vector4.One),
            new(Vector3.UnitX, Vector3.UnitZ, Vector2.UnitX, new Vector4(0.5f, 0.75f, 1, 1)),
            new(Vector3.UnitY, Vector3.UnitZ, Vector2.UnitY, Vector4.One)
        };
        var geometry = new VisualizerGeometry(9, source, [0, 1, 2]);

        source[0] = new VisualizerVertex(
            new Vector3(float.NaN, 0, 0), Vector3.UnitZ, Vector2.Zero, Vector4.One);

        Assert.Equal(9UL, geometry.Revision);
        Assert.Equal(Vector3.Zero, geometry.Vertices[0].Position);
        Assert.Equal(new uint[] { 0, 1, 2 }, geometry.Indices);
        Assert.Equal(3, geometry.VertexMemory.Length);
        Assert.Equal(3, geometry.IndexMemory.Length);
    }

    private static VisualizerGeometry CreateGeometry(VisualizerVertex firstVertex) =>
        new(1,
        [
            firstVertex,
            new VisualizerVertex(Vector3.UnitX, Vector3.UnitZ, Vector2.UnitX, Vector4.One),
            new VisualizerVertex(Vector3.UnitY, Vector3.UnitZ, Vector2.UnitY, Vector4.One)
        ],
        [0, 1, 2]);
}
