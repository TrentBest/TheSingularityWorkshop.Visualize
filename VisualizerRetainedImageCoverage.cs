using System.Numerics;

namespace TheSingularityWorkshop.Visualize;

/// <summary>A source-image pixel and the world-space point represented by that pixel.</summary>
/// <remarks>An uncovered sample carries no world-space position and is ignored during reprojection.</remarks>
public readonly record struct VisualizerWorldCoverageSample(bool IsCovered, Vector3 WorldPosition);

/// <summary>
/// CPU reference coverage for a retained image. It records world-space sample positions, not color or GPU data.
/// </summary>
public sealed class VisualizerRetainedImageCoverage
{
    private readonly VisualizerWorldCoverageSample[] _samples;

    /// <summary>Creates a row-major coverage grid with exactly one sample per source pixel.</summary>
    public VisualizerRetainedImageCoverage(
        int width,
        int height,
        IEnumerable<VisualizerWorldCoverageSample> samples)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);
        ArgumentNullException.ThrowIfNull(samples);

        var expectedCount = checked(width * height);
        _samples = samples.ToArray();
        if (_samples.Length != expectedCount)
            throw new ArgumentException("Coverage sample count must equal width multiplied by height.", nameof(samples));

        for (var index = 0; index < _samples.Length; index++)
        {
            var sample = _samples[index];
            if (sample.IsCovered &&
                (!float.IsFinite(sample.WorldPosition.X) ||
                 !float.IsFinite(sample.WorldPosition.Y) ||
                 !float.IsFinite(sample.WorldPosition.Z)))
                throw new ArgumentException("Covered world-space samples must contain finite coordinates.", nameof(samples));
        }

        Width = width;
        Height = height;
    }

    /// <summary>Gets the source image width.</summary>
    public int Width { get; }

    /// <summary>Gets the source image height.</summary>
    public int Height { get; }

    /// <summary>
    /// Reprojects world-space coverage into a target view. The returned grid explicitly reports holes;
    /// this method never claims that uncovered target pixels have valid retained-image content.
    /// </summary>
    public VisualizerReprojectedCoverage Reproject(
        Matrix4x4 targetView,
        Matrix4x4 targetProjection,
        int targetWidth,
        int targetHeight)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(targetWidth);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(targetHeight);
        if (!VisualizerMatrixValidation.IsFinite(targetView))
            throw new ArgumentOutOfRangeException(nameof(targetView), "View matrix components must be finite.");
        if (!VisualizerMatrixValidation.IsFinite(targetProjection))
            throw new ArgumentOutOfRangeException(nameof(targetProjection), "Projection matrix components must be finite.");

        var targetCount = checked(targetWidth * targetHeight);
        var sourceIndices = new int[targetCount];
        Array.Fill(sourceIndices, -1);
        var depths = new float[targetCount];
        Array.Fill(depths, float.PositiveInfinity);

        // System.Numerics uses row-vector transforms: world * view * projection.
        var viewProjection = Matrix4x4.Multiply(targetView, targetProjection);
        for (var sourceIndex = 0; sourceIndex < _samples.Length; sourceIndex++)
        {
            var sample = _samples[sourceIndex];
            if (!sample.IsCovered)
                continue;

            var clip = Vector4.Transform(
                new Vector4(sample.WorldPosition, 1f),
                viewProjection);

            if (!float.IsFinite(clip.X) || !float.IsFinite(clip.Y) ||
                !float.IsFinite(clip.Z) || !float.IsFinite(clip.W) ||
                clip.W <= 0f)
                continue;

            var inverseW = 1f / clip.W;
            var ndcX = clip.X * inverseW;
            var ndcY = clip.Y * inverseW;
            var depth = clip.Z * inverseW;
            if (!float.IsFinite(ndcX) || !float.IsFinite(ndcY) || !float.IsFinite(depth) ||
                ndcX < -1f || ndcX > 1f || ndcY < -1f || ndcY > 1f ||
                depth < 0f || depth > 1f)
                continue;

            var pixelX = Math.Min((int)MathF.Floor((ndcX * 0.5f + 0.5f) * targetWidth), targetWidth - 1);
            var pixelY = Math.Min((int)MathF.Floor((0.5f - ndcY * 0.5f) * targetHeight), targetHeight - 1);
            var targetIndex = pixelY * targetWidth + pixelX;

            // Smaller normalized device depth is nearer in the documented 0..1 depth convention.
            if (depth < depths[targetIndex])
            {
                depths[targetIndex] = depth;
                sourceIndices[targetIndex] = sourceIndex;
            }
        }

        return new VisualizerReprojectedCoverage(targetWidth, targetHeight, sourceIndices, depths);
    }
}

/// <summary>Result of projecting source world-space coverage into a target image grid.</summary>
public sealed class VisualizerReprojectedCoverage
{
    private readonly int[] _sourceIndices;
    private readonly float[] _depths;

    internal VisualizerReprojectedCoverage(int width, int height, int[] sourceIndices, float[] depths)
    {
        Width = width;
        Height = height;
        _sourceIndices = sourceIndices;
        _depths = depths;
        CoveredPixelCount = sourceIndices.Count(index => index >= 0);
    }

    /// <summary>Gets the target image width.</summary>
    public int Width { get; }

    /// <summary>Gets the target image height.</summary>
    public int Height { get; }

    /// <summary>Gets the number of target pixels backed by a projected source sample.</summary>
    public int CoveredPixelCount { get; }

    /// <summary>Gets whether every target pixel has valid projected source coverage.</summary>
    public bool IsComplete => CoveredPixelCount == _sourceIndices.Length;

    /// <summary>Gets the source row-major pixel index selected for a target pixel, if covered.</summary>
    public bool TryGetSourcePixelIndex(int x, int y, out int sourcePixelIndex)
    {
        var index = GetIndex(x, y);
        sourcePixelIndex = _sourceIndices[index];
        return sourcePixelIndex >= 0;
    }

    /// <summary>Gets the nearest projected normalized depth for a covered target pixel.</summary>
    public bool TryGetDepth(int x, int y, out float depth)
    {
        var index = GetIndex(x, y);
        depth = _depths[index];
        return float.IsFinite(depth);
    }

    private int GetIndex(int x, int y)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(x);
        ArgumentOutOfRangeException.ThrowIfNegative(y);
        if (x >= Width) throw new ArgumentOutOfRangeException(nameof(x));
        if (y >= Height) throw new ArgumentOutOfRangeException(nameof(y));
        return checked(y * Width + x);
    }
}
