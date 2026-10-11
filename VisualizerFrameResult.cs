namespace TheSingularityWorkshop.Visualize;

/// <summary>Reports the outcome and timing observations for one submitted frame.</summary>
/// <remarks>Timing fields are optional backend-reported observations, not cross-backend performance guarantees.</remarks>
public sealed record VisualizerFrameResult
{
    /// <summary>Initializes a frame result.</summary>
    public VisualizerFrameResult(
        long frameNumber,
        int submittedDrawCount,
        TimeSpan? cpuSubmissionTime = null,
        TimeSpan? gpuExecutionTime = null,
        bool wasPresented = false)
    {
        if (frameNumber < 0)
            throw new ArgumentOutOfRangeException(nameof(frameNumber));
        if (submittedDrawCount < 0)
            throw new ArgumentOutOfRangeException(nameof(submittedDrawCount));
        if (cpuSubmissionTime is { } cpu && cpu < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(cpuSubmissionTime));
        if (gpuExecutionTime is { } gpu && gpu < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(gpuExecutionTime));

        FrameNumber = frameNumber;
        SubmittedDrawCount = submittedDrawCount;
        CpuSubmissionTime = cpuSubmissionTime;
        GpuExecutionTime = gpuExecutionTime;
        WasPresented = wasPresented;
    }

    /// <summary>Gets the backend frame sequence number.</summary>
    public long FrameNumber { get; }
    /// <summary>Gets the number of draw commands accepted for submission.</summary>
    public int SubmittedDrawCount { get; }
    /// <summary>Gets the CPU duration used to prepare and submit the frame when measured.</summary>
    public TimeSpan? CpuSubmissionTime { get; }
    /// <summary>Gets the GPU execution duration when the backend can measure it.</summary>
    public TimeSpan? GpuExecutionTime { get; }
    /// <summary>Gets whether the backend reports that this frame was presented.</summary>
    public bool WasPresented { get; }
}
