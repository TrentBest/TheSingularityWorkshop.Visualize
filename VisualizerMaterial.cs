using System.Numerics;

namespace TheSingularityWorkshop.Visualize;

/// <summary>Describes a minimal material input for the first backend prototype.</summary>
/// <remarks>This is not yet a complete physically based material model.</remarks>
public sealed record VisualizerMaterial
{
    /// <summary>Creates a basic material description.</summary>
    public VisualizerMaterial(ulong revision, Vector4 baseColor, bool isDoubleSided = false)
    {
        if (!float.IsFinite(baseColor.X) || !float.IsFinite(baseColor.Y) ||
            !float.IsFinite(baseColor.Z) || !float.IsFinite(baseColor.W))
            throw new ArgumentOutOfRangeException(nameof(baseColor), "Material color components must be finite.");

        Revision = revision;
        BaseColor = baseColor;
        IsDoubleSided = isDoubleSided;
    }

    /// <summary>Gets the caller-managed content revision.</summary>
    public ulong Revision { get; }
    /// <summary>Gets the base color in linear color space.</summary>
    public Vector4 BaseColor { get; }
    /// <summary>Gets whether both triangle faces should be rendered.</summary>
    public bool IsDoubleSided { get; }
}
