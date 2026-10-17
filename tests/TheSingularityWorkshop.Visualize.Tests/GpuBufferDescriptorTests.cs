using TheSingularityWorkshop.Visualize.GPU;
using Xunit;

namespace TheSingularityWorkshop.Visualize.Tests;

public sealed class GpuBufferDescriptorTests
{
    [Fact]
    public void Validate_AcceptsPositiveSizeAndKnownUsage()
    {
        var descriptor = new GpuBufferDescriptor(256, GpuBufferUsage.Vertex | GpuBufferUsage.TransferDestination);

        descriptor.Validate();
    }

    [Theory]
    [InlineData(0)]
    public void Validate_RejectsZeroSize(ulong sizeInBytes)
    {
        var descriptor = new GpuBufferDescriptor(sizeInBytes, GpuBufferUsage.Vertex);

        Assert.Throws<ArgumentOutOfRangeException>(descriptor.Validate);
    }

    [Fact]
    public void Validate_RejectsNoUsage()
    {
        var descriptor = new GpuBufferDescriptor(16, GpuBufferUsage.None);

        Assert.Throws<ArgumentOutOfRangeException>(descriptor.Validate);
    }

    [Fact]
    public void Validate_RejectsUnknownUsageBits()
    {
        var descriptor = new GpuBufferDescriptor(16, (GpuBufferUsage)int.MinValue);

        Assert.Throws<ArgumentOutOfRangeException>(descriptor.Validate);
    }
}
