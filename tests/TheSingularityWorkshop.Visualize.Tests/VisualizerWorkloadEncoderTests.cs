using System.Buffers.Binary;
using System.Numerics;
using TheSingularityWorkshop.Visualize;
using TheSingularityWorkshop.Visualize.GPU;
using Xunit;

namespace TheSingularityWorkshop.Visualize.Tests;

public sealed class VisualizerWorkloadEncoderTests
{
    [Fact]
    public void Encode_UsesPositionOnlyVerticesUint32IndicesAndEightyByteInstances()
    {
        var draw = Resolve(instanced: true);

        var bytes = VisualizerWorkloadEncoder.Encode(draw);

        Assert.Equal(3, bytes.VertexCount);
        Assert.Equal(3 * 12, bytes.Vertices.Length);
        Assert.Equal(3, bytes.IndexCount);
        Assert.Equal(3 * 4, bytes.Indices.Length);
        Assert.Equal(1, bytes.InstanceCount);
        Assert.Equal(80, bytes.Instances.Length);

        Assert.Equal(0f, ReadSingle(bytes.Vertices, 0));
        Assert.Equal(0.25f, ReadSingle(bytes.Vertices, 4));
        Assert.Equal(-0.5f, ReadSingle(bytes.Vertices, 8));
        Assert.Equal(2u, BinaryPrimitives.ReadUInt32LittleEndian(bytes.Indices.AsSpan(8, 4)));

        // The selected second instance has translation (7, 8, 9) and tint (0.1, 0.2, 0.3, 0.4).
        Assert.Equal(7f, ReadSingle(bytes.Instances, 48));
        Assert.Equal(8f, ReadSingle(bytes.Instances, 52));
        Assert.Equal(9f, ReadSingle(bytes.Instances, 56));
        Assert.Equal(0.1f, ReadSingle(bytes.Instances, 64));
        Assert.Equal(0.4f, ReadSingle(bytes.Instances, 76));
    }

    [Fact]
    public void Encode_NonInstancedCommandUsesWorldTransformAndMaterialColor()
    {
        var draw = Resolve(instanced: false);

        var bytes = VisualizerWorkloadEncoder.Encode(draw);

        Assert.Equal(1, bytes.InstanceCount);
        Assert.Equal(11f, ReadSingle(bytes.Instances, 48));
        Assert.Equal(0.25f, ReadSingle(bytes.Instances, 64));
        Assert.Equal(0.5f, ReadSingle(bytes.Instances, 68));
        Assert.Equal(0.75f, ReadSingle(bytes.Instances, 72));
        Assert.Equal(1f, ReadSingle(bytes.Instances, 76));
    }

    [Fact]
    public void Encode_RejectsNonFinitePositionRatherThanEmittingInvalidBytes()
    {
        var catalog = CreateCatalog();
        catalog.SetGeometry(new VisualizerResourceId(10), new VisualizerGeometry(2,
        [
            new VisualizerVertex(new Vector3(float.NaN, 0, 0), Vector3.UnitZ, Vector2.Zero, Vector4.One),
            new VisualizerVertex(Vector3.UnitX, Vector3.UnitZ, Vector2.UnitX, Vector4.One),
            new VisualizerVertex(Vector3.UnitY, Vector3.UnitZ, Vector2.UnitY, Vector4.One)
        ], [0, 1, 2]));
        var command = new VisualizerDrawCommand(
            new VisualizerResourceId(10), new VisualizerResourceId(20), Matrix4x4.Identity,
            3, 0, 0, VisualizerRepresentation.Near);
        var resolved = VisualizerFrameResolver.Resolve(new VisualizerFrame(
            new VisualizerView(Vector3.Zero, Matrix4x4.Identity, Matrix4x4.Identity), [command]), catalog);

        var error = Assert.Throws<InvalidOperationException>(() => VisualizerWorkloadEncoder.Encode(resolved[0]));

        Assert.Contains("non-finite", error.Message);
        Assert.Contains("vertex 0 position", error.Message);
    }

    private static ResolvedVisualizerDrawCommand Resolve(bool instanced)
    {
        var catalog = CreateCatalog();
        var command = new VisualizerDrawCommand(
            new VisualizerResourceId(10),
            new VisualizerResourceId(20),
            Matrix4x4.CreateTranslation(11, 12, 13),
            3,
            0,
            0,
            VisualizerRepresentation.Near,
            instanced ? new VisualizerResourceId(30) : default,
            instanced ? 1 : 1,
            instanced ? 1 : 0);
        var frame = new VisualizerFrame(
            new VisualizerView(Vector3.Zero, Matrix4x4.Identity, Matrix4x4.Identity), [command]);
        return VisualizerFrameResolver.Resolve(frame, catalog)[0];
    }

    private static VisualizerResourceCatalog CreateCatalog()
    {
        var catalog = new VisualizerResourceCatalog();
        catalog.SetGeometry(new VisualizerResourceId(10), new VisualizerGeometry(1,
        [
            new VisualizerVertex(new Vector3(0, 0.25f, -0.5f), Vector3.UnitZ, Vector2.Zero, Vector4.One),
            new VisualizerVertex(Vector3.UnitX, Vector3.UnitZ, Vector2.UnitX, Vector4.One),
            new VisualizerVertex(Vector3.UnitY, Vector3.UnitZ, Vector2.UnitY, Vector4.One)
        ], [0, 1, 2]));
        catalog.SetMaterial(new VisualizerResourceId(20), new VisualizerMaterial(1, new Vector4(0.25f, 0.5f, 0.75f, 1f)));
        catalog.SetInstances(new VisualizerResourceId(30), new VisualizerInstanceBuffer(1,
        [
            new VisualizerInstance(Matrix4x4.CreateTranslation(1, 2, 3), Vector4.One),
            new VisualizerInstance(Matrix4x4.CreateTranslation(7, 8, 9), new Vector4(0.1f, 0.2f, 0.3f, 0.4f))
        ]));
        return catalog;
    }

    private static float ReadSingle(byte[] bytes, int offset) =>
        BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(offset, sizeof(float))));
}
