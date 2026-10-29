using System.Numerics;

namespace TheSingularityWorkshop.Visualize;

/// <summary>
/// Applies Visualize's documented homogeneous-coordinate convention to a source-space point.
/// </summary>
/// <remarks>
/// The input point is treated as (x, y, 0, 1) and transformed using
/// <see cref="Vector4.Transform(Vector4, Matrix4x4)"/>. This follows System.Numerics' row-vector
/// convention, including translation in the fourth matrix row. The result is perspective-divided
/// by W. This helper does not clip geometry or decide whether a point is inside a viewport.
/// </remarks>
public static class VisualizerProjectionTransform
{
    private const float MinimumAbsoluteW = 1e-6f;

    /// <summary>
    /// Transforms a two-dimensional source point into destination coordinates.
    /// </summary>
    /// <param name="sourcePoint">Point in the source coordinate system declared by the request.</param>
    /// <param name="sourceToDestination">Mapping matrix using System.Numerics row-vector convention.</param>
    /// <param name="destinationPoint">Transformed three-dimensional point when successful; otherwise default.</param>
    /// <returns>
    /// True when the input, matrix, homogeneous result, and perspective-divided point are finite
    /// and the homogeneous W component is sufficiently far from zero; otherwise false.
    /// </returns>
    public static bool TryTransformPoint(
        Vector2 sourcePoint,
        Matrix4x4 sourceToDestination,
        out Vector3 destinationPoint)
    {
        destinationPoint = default;

        if (!float.IsFinite(sourcePoint.X) || !float.IsFinite(sourcePoint.Y) ||
            !VisualizerMatrixValidation.IsFinite(sourceToDestination))
            return false;

        var transformed = Vector4.Transform(
            new Vector4(sourcePoint.X, sourcePoint.Y, 0f, 1f),
            sourceToDestination);

        if (!float.IsFinite(transformed.X) ||
            !float.IsFinite(transformed.Y) ||
            !float.IsFinite(transformed.Z) ||
            !float.IsFinite(transformed.W) ||
            MathF.Abs(transformed.W) <= MinimumAbsoluteW)
            return false;

        var inverseW = 1f / transformed.W;
        var result = new Vector3(
            transformed.X * inverseW,
            transformed.Y * inverseW,
            transformed.Z * inverseW);

        if (!float.IsFinite(result.X) || !float.IsFinite(result.Y) || !float.IsFinite(result.Z))
            return false;

        destinationPoint = result;
        return true;
    }
}
