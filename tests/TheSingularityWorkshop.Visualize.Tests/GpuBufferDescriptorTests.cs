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

    [Fact]
    public void GpuDeviceInfo_PreservesAvailableDeviceDiagnostics()
    {
        var info = new GpuDeviceInfo(
            "Workshop Test GPU",
            "TestAdapter",
            VendorId: 0x1234,
            DeviceId: 0x5678,
            DedicatedMemoryBytes: 1024UL * 1024 * 1024,
            IsSoftware: false);

        Assert.Equal("Workshop Test GPU", info.Name);
        Assert.Equal("TestAdapter", info.Backend);
        Assert.Equal(0x1234u, info.VendorId);
        Assert.Equal(0x5678u, info.DeviceId);
        Assert.Equal(1024UL * 1024 * 1024, info.DedicatedMemoryBytes);
        Assert.False(info.IsSoftware);
    }

    [Fact]
    public void GpuDeviceInfo_AllowsUnknownOptionalHardwareDetails()
    {
        var info = new GpuDeviceInfo("Unknown device", "DiagnosticAdapter");

        Assert.Null(info.VendorId);
        Assert.Null(info.DeviceId);
        Assert.Null(info.DedicatedMemoryBytes);
        Assert.False(info.IsSoftware);
    }
}
