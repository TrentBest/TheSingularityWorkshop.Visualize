using System.Runtime.InteropServices;

namespace TheSingularityWorkshop.Visualize.GPU;

/// <summary>Describes the host process and operating system that constrain GPU backend availability.</summary>
public sealed record GpuHostInfo(
    string OperatingSystem,
    string OperatingSystemVersion,
    Architecture ProcessArchitecture,
    Architecture OperatingSystemArchitecture,
    string RuntimeVersion,
    bool Is64BitProcess)
{
    /// <summary>Captures host facts from the current .NET process without probing or loading a graphics API.</summary>
    public static GpuHostInfo CaptureCurrent() => new(
        RuntimeInformation.OSDescription,
        Environment.OSVersion.VersionString,
        RuntimeInformation.ProcessArchitecture,
        RuntimeInformation.OSArchitecture,
        RuntimeInformation.FrameworkDescription,
        Environment.Is64BitProcess);
}
