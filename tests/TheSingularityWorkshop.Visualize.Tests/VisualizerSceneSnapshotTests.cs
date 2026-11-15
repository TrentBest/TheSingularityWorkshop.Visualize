using System.Numerics;
using TheSingularityWorkshop.Visualize;
using Xunit;

namespace TheSingularityWorkshop.Visualize.Tests;

public sealed class VisualizerSceneSnapshotTests
{
    private static VisualizerDrawCommand Draw(ulong entity, float x = 0) =>
        new(
            new VisualizerResourceId(1),
            new VisualizerResourceId(2),
            Matrix4x4.CreateTranslation(x, 0, 0),
            3,
            0,
            0,
            VisualizerRepresentation.Near,
            entity: new VisualizerEntityId(entity));

    private static VisualizerSceneObject Object(
        ulong entity,
        int layer = 0,
        bool visible = true,
        params VisualizerDrawCommand[] commands) =>
        new(new VisualizerEntityId(entity), layer, visible, commands);

    private static VisualizerView View(Matrix4x4? view = null) =>
        new(Vector3.Zero, view ?? Matrix4x4.Identity, Matrix4x4.Identity);


    [Fact]
    public void CompleteSceneSnapshotProducesDeterministicMultiObjectSvgArtifact()
    {
        var resources = new VisualizerResourceCatalog();
        resources.SetGeometry(new VisualizerResourceId(1), new VisualizerGeometry(
            1,
            [
                new VisualizerVertex(new Vector3(-0.2f, -0.2f, 0), Vector3.UnitZ, Vector2.Zero, Vector4.One),
                new VisualizerVertex(new Vector3(0.2f, -0.2f, 0), Vector3.UnitZ, Vector2.UnitX, Vector4.One),
                new VisualizerVertex(new Vector3(0, 0.2f, 0), Vector3.UnitZ, Vector2.UnitY, Vector4.One)
            ],
            [0, 1, 2]));
        resources.SetMaterial(new VisualizerResourceId(2), new VisualizerMaterial(1, Vector4.One));

        var snapshot = new VisualizerSceneSnapshot("scene:svg-proof", 1,
        [
            Object(101, layer: 0, commands: [Draw(101, -0.5f)]),
            Object(202, layer: 1, commands: [Draw(202, 0.5f)])
        ]);
        var backend = new SvgVisualizerBackend(_ => { }, width: 320, height: 200);
        var frame = snapshot.CreateFrame(View());

        var first = backend.BuildSvg(frame, resources);
        var second = backend.BuildSvg(frame, resources);

        Assert.Equal(first, second);
        Assert.Equal(2, first.Split("<polygon", StringSplitOptions.None).Length - 1);
        Assert.Contains("data-entity-id=" + (char)34 + "101" + (char)34, first, StringComparison.Ordinal);
        Assert.Contains("data-entity-id=" + (char)34 + "202" + (char)34, first, StringComparison.Ordinal);
    }

    [Fact]
    public void SnapshotKeepsSceneIdentityAcrossIncreasingCompleteRevisions()
    {
        var first = new VisualizerSceneSnapshot("workshop://scene/test", 1,
            [Object(7, commands: [Draw(7)])]);
        var second = new VisualizerSceneSnapshot("workshop://scene/test", 2,
            [Object(7, commands: [Draw(7, 1), Draw(7, 2)])]);

        Assert.Equal(first.SceneIdentity, second.SceneIdentity);
        Assert.Equal(1UL, first.Revision);
        Assert.Equal(2UL, second.Revision);
        Assert.Single(first.Objects);
        Assert.Equal(2, second.CreateFrame(View()).Commands.Count);
        Assert.All(second.CreateFrame(View()).Commands, command => Assert.Equal(new VisualizerEntityId(7), command.Entity));
    }

    [Fact]
    public void SnapshotOrdersObjectsByLayerThenStableEntityIdentity()
    {
        var snapshot = new VisualizerSceneSnapshot("scene", 1,
        [
            Object(9, layer: 2, commands: [Draw(9)]),
            Object(8, layer: 1, commands: [Draw(8)]),
            Object(3, layer: 1, commands: [Draw(3)])
        ]);

        Assert.Equal(new ulong[] { 3, 8, 9 }, snapshot.Objects.Select(item => item.Entity.Value));
        Assert.Equal(new ulong[] { 3, 8, 9 },
            snapshot.CreateFrame(View()).Commands.Select(command => command.Entity.Value));
    }

    [Fact]
    public void FrameFiltersHiddenObjectsAndUnselectedLayers()
    {
        var snapshot = new VisualizerSceneSnapshot("scene", 1,
        [
            Object(1, layer: 0, visible: true, commands: [Draw(1)]),
            Object(2, layer: 1, visible: false, commands: [Draw(2)]),
            Object(3, layer: 2, visible: true, commands: [Draw(3)])
        ]);

        var allVisibleLayers = snapshot.CreateFrame(View());
        var selectedLayer = snapshot.CreateFrame(View(), new HashSet<int> { 2 });

        Assert.Equal(new ulong[] { 1, 3 }, allVisibleLayers.Commands.Select(command => command.Entity.Value));
        Assert.Equal(new ulong[] { 3 }, selectedLayer.Commands.Select(command => command.Entity.Value));
    }

    [Fact]
    public void ViewChangesDoNotMutateSnapshotOrWorldTransforms()
    {
        var snapshot = new VisualizerSceneSnapshot("scene", 12,
            [Object(5, commands: [Draw(5, 4)])]);
        var first = snapshot.CreateFrame(View());
        var second = snapshot.CreateFrame(View(Matrix4x4.CreateTranslation(-10, 0, 0)));

        Assert.Equal(12UL, snapshot.Revision);
        Assert.Equal(first.Commands[0].WorldTransform, second.Commands[0].WorldTransform);
        Assert.Equal(Matrix4x4.CreateTranslation(4, 0, 0), first.Commands[0].WorldTransform);
        Assert.NotEqual(first.View.View, second.View.View);
    }

    [Fact]
    public void SnapshotRejectsInvalidOrDuplicateObjectIdentity()
    {
        Assert.Throws<ArgumentException>(() => new VisualizerSceneObject(
            default, 0, true, [Draw(1)]));

        var duplicate = Object(4, commands: [Draw(4)]);
        Assert.Throws<ArgumentException>(() => new VisualizerSceneSnapshot(
            "scene", 1, [duplicate, duplicate]));
    }

    [Fact]
    public void ObjectRejectsCommandsOwnedByAnotherEntity()
    {
        Assert.Throws<ArgumentException>(() => Object(1, commands: [Draw(2)]));
    }

    [Fact]
    public void SnapshotRequiresStableIdentityAndPositiveRevision()
    {
        Assert.Throws<ArgumentException>(() => new VisualizerSceneSnapshot(" ", 1, []));
        Assert.Throws<ArgumentOutOfRangeException>(() => new VisualizerSceneSnapshot("scene", 0, []));
    }
}
