namespace TheSingularityWorkshop.Visualize.GPU;

/// <summary>Describes a GPU buffer without exposing an API-specific handle.</summary>
/// <param name="SizeInBytes">Requested buffer size in bytes; must be positive.</param>
/// <param name="Usage">Intended buffer usages.</param>
/// <param name="Label">Optional diagnostic label.</param>
public readonly record struct GpuBufferDescriptor(
    ulong SizeInBytes,
    GpuBufferUsage Usage,
    string? Label = null)
{
    /// <summary>Validates this descriptor before native allocation.</summary>
    public void Validate()
    {
        if (SizeInBytes == 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(SizeInBytes), SizeInBytes, "GPU buffer size must be greater than zero.");
        }

        if (Usage == GpuBufferUsage.None)
        {
            throw new ArgumentOutOfRangeException(
                nameof(Usage), Usage, "At least one GPU buffer usage must be specified.");
        }

        const GpuBufferUsage allUsages =
            GpuBufferUsage.Vertex |
            GpuBufferUsage.Index |
            GpuBufferUsage.ShaderRead |
            GpuBufferUsage.ShaderWrite |
            GpuBufferUsage.TransferDestination |
            GpuBufferUsage.TransferSource |
            GpuBufferUsage.Indirect;

        if ((Usage & ~allUsages) != 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(Usage), Usage, "GPU buffer usage contains unsupported flag bits.");
        }
    }
}
