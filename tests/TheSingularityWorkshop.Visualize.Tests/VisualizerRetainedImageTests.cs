using System.Numerics;
using TheSingularityWorkshop.Visualize;
using Xunit;

namespace TheSingularityWorkshop.Visualize.Tests;

public sealed class VisualizerRetainedImageTests
{
    [Fact]
    public void Catalog_ReusesUnchangedFarImageAcrossFrames()
    {
        var catalog = new VisualizerRetainedImageCatalog();
        var far = Descriptor(1, contentRevision: 4, sourceRevision: 8, observerRevision: 2);
        catalog.Set(far);

        for (var frame = 0; frame < 120; frame++)
        {
            Assert.True(catalog.TryGetReusable(far, out var retained));
            Assert.Same(far, retained);
        }
    }

    [Fact]
    public void Catalog_PreservesFarImageWhenOnlyIndependentNearSourceChanges()
    {
        var catalog = new VisualizerRetainedImageCatalog();
        var far = Descriptor(1, contentRevision: 4, sourceRevision: 8, observerRevision: 2);
        var near = Descriptor(2, contentRevision: 1, sourceRevision: 3, observerRevision: 2);
        catalog.Set(far);
        catalog.Set(near);

        catalog.Set(Descriptor(2, contentRevision: 2, sourceRevision: 4, observerRevision: 2));

        Assert.True(catalog.TryGetReusable(far, out _));
        Assert.False(catalog.TryGetReusable(near, out _));
    }

    [Theory]
    [InlineData(5, 8, 2)]
    [InlineData(4, 9, 2)]
    [InlineData(4, 8, 3)]
    public void Catalog_RejectsChangedContentSourceOrObserverRevision(
        ulong contentRevision, ulong sourceRevision, ulong observerRevision)
    {
        var catalog = new VisualizerRetainedImageCatalog();
        catalog.Set(Descriptor(1, 4, 8, 2));

        Assert.False(catalog.TryGetReusable(
            Descriptor(1, contentRevision, sourceRevision, observerRevision), out _));
    }

    [Fact]
    public void Frame_PreservesOrderedImageCompositionOperations()
    {
        var compositions = new[]
        {
            new VisualizerImageComposition(new VisualizerImageId(1), 10),
            new VisualizerImageComposition(new VisualizerImageId(2), 20)
        };

        var frame = new VisualizerFrame(
            new VisualizerView(Vector3.Zero, Matrix4x4.Identity, Matrix4x4.Identity),
            [],
            compositions);

        Assert.Equal(compositions, frame.ImageCompositions);
    }

    [Fact]
    public void Frame_RejectsCompositionOutsideBackToFrontLayerOrder()
    {
        Assert.Throws<ArgumentException>(() => new VisualizerFrame(
            new VisualizerView(Vector3.Zero, Matrix4x4.Identity, Matrix4x4.Identity),
            [],
            [
                new VisualizerImageComposition(new VisualizerImageId(2), 20),
                new VisualizerImageComposition(new VisualizerImageId(1), 10)
            ]));
    }

    [Fact]
    public void Frame_RejectsCompositionWithInvalidImageIdentity()
    {
        Assert.Throws<ArgumentException>(() => new VisualizerFrame(
            new VisualizerView(Vector3.Zero, Matrix4x4.Identity, Matrix4x4.Identity),
            [],
            [new VisualizerImageComposition(default, 0)]));
    }

    private static VisualizerRetainedImageDescriptor Descriptor(
        ulong image, ulong contentRevision, ulong sourceRevision, ulong observerRevision) =>
        new(new VisualizerImageId(image), sourceBucket: 99, contentRevision, sourceRevision,
            observerRevision, width: 1024, height: 512);
}
