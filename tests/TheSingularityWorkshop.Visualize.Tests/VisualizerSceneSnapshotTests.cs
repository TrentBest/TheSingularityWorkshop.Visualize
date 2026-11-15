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

        Assert.Equal([3UL, 8UL, 9UL], snapshot.Objects.Select(item => item.Entity.Value));
        Assert.Equal([3UL, 8UL, 9UL],
            snapshot.CreateFrame(View()).Commands.Select(command => command.Entity.Value));
    }

    [Fact]
    public void FrameFiltersHiddenObjectsAndUnselectedLayers()
    {
        var snapshot = new VisualizerSceneSnapshot("scene", 1,
        [
            Object(1, layer: 0, visible: true, Draw(1)),
            Object(2, layer: 1, visible: false, Draw(2)),
            Object(3, layer: 2, visible: true, Draw(3))
        ]);

        var allVisibleLayers = snapshot.CreateFrame(View());
        var selectedLayer = snapshot.CreateFrame(View(), new HashSet<int> { 2 });

        Assert.Equal([1UL, 3UL], allVisibleLayers.Commands.Select(command => command.Entity.Value));
        Assert.Equal([3UL], selectedLayer.Commands.Select(command => command.Entity.Value));
    }

    [Fact]
    public void ViewChangesDoNotMutateSnapshotOrWorldTransforms()
    {
        var transform = Matrix4x4.CreateTranslation(4, 5, 0);
        var snapshot = new VisualizerSceneSnapshot("scene", 12,
            [Object(5, commands: [Draw(5)])]);
        var first = snapshot.CreateFrame(View());
        var second = snapshot.CreateFrame(View(Matrix4x4.CreateTranslation(-10, 0, 0)));

        Assert.Equal(12UL, snapshot.Revision);
        Assert.Equal(first.Commands[0].WorldTransform, second.Commands[0].WorldTransform);
        Assert.Equal(first.Commands[0].WorldTransform, Matrix4x4.Identity);
        Assert.NotEqual(first.View.View, second.View.View);
        Assert.Equal(transform.M41, 4); // World-space transforms remain independent of camera state.
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
