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

    [Fact]
    public void Coverage_ReprojectsAnUnchangedViewWithoutLosingSamples()
    {
        var coverage = new VisualizerRetainedImageCoverage(2, 1,
        [
            new(true, new Vector3(-0.5f, 0f, 0.5f)),
            new(true, new Vector3(0.5f, 0f, 0.5f))
        ]);

        var result = coverage.Reproject(Matrix4x4.Identity, Matrix4x4.Identity, 2, 1);

        Assert.True(result.IsComplete);
        Assert.Equal(2, result.CoveredPixelCount);
        Assert.True(result.TryGetSourcePixelIndex(0, 0, out var left));
        Assert.True(result.TryGetSourcePixelIndex(1, 0, out var right));
        Assert.Equal(0, left);
        Assert.Equal(1, right);
    }

    [Fact]
    public void Coverage_ReportsDisocclusionInsteadOfClaimingCompleteReprojection()
    {
        var coverage = new VisualizerRetainedImageCoverage(2, 1,
        [
            new(true, new Vector3(-0.5f, 0f, 0.5f)),
            new(true, new Vector3(0.5f, 0f, 0.5f))
        ]);

        var movedView = Matrix4x4.CreateTranslation(-1f, 0f, 0f);
        var result = coverage.Reproject(movedView, Matrix4x4.Identity, 2, 1);

        Assert.False(result.IsComplete);
        Assert.Equal(1, result.CoveredPixelCount);
        Assert.False(result.TryGetSourcePixelIndex(1, 0, out _));
    }

    [Fact]
    public void Coverage_ResolvesOverlappingSamplesToNearestDepth()
    {
        var coverage = new VisualizerRetainedImageCoverage(2, 1,
        [
            new(true, new Vector3(0f, 0f, 0.8f)),
            new(true, new Vector3(0f, 0f, 0.2f))
        ]);

        var result = coverage.Reproject(Matrix4x4.Identity, Matrix4x4.Identity, 1, 1);

        Assert.True(result.TryGetSourcePixelIndex(0, 0, out var sourceIndex));
        Assert.Equal(1, sourceIndex);
        Assert.True(result.TryGetDepth(0, 0, out var depth));
        Assert.Equal(0.2f, depth, precision: 5);
    }

    [Fact]
    public void Coverage_RejectsIncorrectSampleCountAndNonFiniteCoveredPositions()
    {
        Assert.Throws<ArgumentException>(() => new VisualizerRetainedImageCoverage(
            2, 1, [new(true, Vector3.Zero)]));

        Assert.Throws<ArgumentException>(() => new VisualizerRetainedImageCoverage(
            1, 1, [new(true, new Vector3(float.NaN, 0f, 0f))]));
    }

    [Fact]
    public void Coverage_RejectsNonFiniteTargetTransforms()
    {
        var coverage = new VisualizerRetainedImageCoverage(1, 1, [new(true, Vector3.Zero)]);
        var invalidView = Matrix4x4.Identity;
        invalidView.M11 = float.NaN;

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            coverage.Reproject(invalidView, Matrix4x4.Identity, 1, 1));
    }
}
