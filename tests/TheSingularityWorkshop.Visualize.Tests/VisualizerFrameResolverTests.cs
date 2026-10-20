using System.Numerics;
using TheSingularityWorkshop.Visualize;
using TheSingularityWorkshop.Visualize.GPU;
using Xunit;

namespace TheSingularityWorkshop.Visualize.Tests;

public sealed class VisualizerFrameResolverTests
{
    [Fact]
    public void Resolve_ReturnsResourceSnapshotsAndPreservesCommandOrder()
    {
        var catalog = CreateCatalog();
        var frame = new VisualizerFrame(View(), [Command(), Command(geometry: 11)]);

        var resolved = VisualizerFrameResolver.Resolve(frame, catalog);

        Assert.Equal(2, resolved.Count);
        Assert.Equal(10UL, resolved[0].Command.Geometry.Value);
        Assert.Equal(11UL, resolved[1].Command.Geometry.Value);
        Assert.Equal(3, resolved[0].Geometry.Indices.Count);
        Assert.Equal(20UL, resolved[0].Material.Revision);
        Assert.Equal(30UL, resolved[0].Instances!.Revision);
    }

    [Theory]
    [InlineData("geometry")]
    [InlineData("material")]
    [InlineData("instance data")]
    public void Resolve_RejectsMissingResources(string role)
    {
        var catalog = CreateCatalog();
        var command = role switch
        {
            "geometry" => Command(geometry: 999),
            "material" => Command(material: 999),
            _ => Command(instances: 999)
        };

        var error = Assert.Throws<InvalidOperationException>(
            () => VisualizerFrameResolver.Resolve(new VisualizerFrame(View(), [command]), catalog));

        Assert.Contains("Draw command 0", error.Message);
        Assert.Contains(role, error.Message);
        Assert.Contains("999", error.Message);
    }

    [Fact]
    public void Resolve_RejectsIndexRangePastGeometry()
    {
        var catalog = CreateCatalog();
        var command = Command(indexCount: 3, firstIndex: 1);

        var error = Assert.Throws<InvalidOperationException>(
            () => VisualizerFrameResolver.Resolve(new VisualizerFrame(View(), [command]), catalog));

        Assert.Contains("index range", error.Message);
        Assert.Contains("index count 3", error.Message);
    }

    [Fact]
    public void Resolve_RejectsBaseVertexThatResolvesOutsideGeometry()
    {
        var catalog = CreateCatalog();
        var command = Command(baseVertex: 1);

        var error = Assert.Throws<InvalidOperationException>(
            () => VisualizerFrameResolver.Resolve(new VisualizerFrame(View(), [command]), catalog));

        Assert.Contains("base vertex 1", error.Message);
        Assert.Contains("vertex 3", error.Message);
    }

    [Fact]
    public void Resolve_RejectsInstanceRangePastResource()
    {
        var catalog = CreateCatalog();
        var command = Command(instanceCount: 2, firstInstance: 1);

        var error = Assert.Throws<InvalidOperationException>(
            () => VisualizerFrameResolver.Resolve(new VisualizerFrame(View(), [command]), catalog));

        Assert.Contains("instance range", error.Message);
        Assert.Contains("count 2", error.Message);
    }

    [Fact]
    public void Resolve_RejectsNullArguments()
    {
        Assert.Throws<ArgumentNullException>(() =>
            VisualizerFrameResolver.Resolve(null!, CreateCatalog()));
        Assert.Throws<ArgumentNullException>(() =>
            VisualizerFrameResolver.Resolve(new VisualizerFrame(View(), []), null!));
    }

    private static VisualizerResourceCatalog CreateCatalog()
    {
        var catalog = new VisualizerResourceCatalog();
        var vertices = new[]
        {
            new VisualizerVertex(Vector3.Zero, Vector3.UnitZ, Vector2.Zero, Vector4.One),
            new VisualizerVertex(Vector3.UnitX, Vector3.UnitZ, Vector2.UnitX, Vector4.One),
            new VisualizerVertex(Vector3.UnitY, Vector3.UnitZ, Vector2.UnitY, Vector4.One)
        };
        catalog.SetGeometry(new VisualizerResourceId(10), new VisualizerGeometry(1, vertices, [0, 1, 2]));
        catalog.SetGeometry(new VisualizerResourceId(11), new VisualizerGeometry(2, vertices, [0, 1, 2]));
        catalog.SetMaterial(new VisualizerResourceId(20), new VisualizerMaterial(20, Vector4.One));
        catalog.SetInstances(new VisualizerResourceId(30), new VisualizerInstanceBuffer(30,
        [
            new VisualizerInstance(Matrix4x4.Identity, Vector4.One),
            new VisualizerInstance(Matrix4x4.CreateTranslation(1, 0, 0), Vector4.One)
        ]));
        return catalog;
    }

    private static VisualizerView View() => new(Vector3.Zero, Matrix4x4.Identity, Matrix4x4.Identity);

    private static VisualizerDrawCommand Command(
        ulong geometry = 10,
        ulong material = 20,
        ulong instances = 30,
        int indexCount = 3,
        int firstIndex = 0,
        int baseVertex = 0,
        int instanceCount = 1,
        int firstInstance = 0) =>
        new(
            new VisualizerResourceId(geometry),
            new VisualizerResourceId(material),
            Matrix4x4.Identity,
            indexCount,
            firstIndex,
            baseVertex,
            VisualizerRepresentation.Near,
            new VisualizerResourceId(instances),
            instanceCount,
            firstInstance);
}
