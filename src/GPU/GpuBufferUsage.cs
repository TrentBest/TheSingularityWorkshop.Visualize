namespace TheSingularityWorkshop.Visualize.GPU;

/// <summary>Declares the operations a GPU buffer is intended to support.</summary>
[Flags]
public enum GpuBufferUsage
{
    /// <summary>No declared usage.</summary>
    None = 0,

    /// <summary>The buffer can be used as a vertex source.</summary>
    Vertex = 1 << 0,

    /// <summary>The buffer can be used as an index source.</summary>
    Index = 1 << 1,

    /// <summary>The buffer can be read by shader stages.</summary>
    ShaderRead = 1 << 2,

    /// <summary>The buffer can be written by shader stages.</summary>
    ShaderWrite = 1 << 3,

    /// <summary>The buffer can receive data from the CPU.</summary>
    TransferDestination = 1 << 4,

    /// <summary>The buffer can provide data to a transfer operation.</summary>
    TransferSource = 1 << 5,

    /// <summary>The buffer can be used as an indirect command source.</summary>
    Indirect = 1 << 6
}
