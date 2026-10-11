namespace TheSingularityWorkshop.Visualize;

/// <summary>Identifies a logical Visualizer resource without exposing a native GPU handle.</summary>
public readonly record struct VisualizerResourceId(ulong Value)
{
    /// <summary>Gets whether this identifier is valid.</summary>
    public bool IsValid => Value != 0;
}
