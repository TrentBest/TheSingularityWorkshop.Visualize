using System.Runtime.InteropServices;
using TheSingularityWorkshop.Visualize.GPU;
using Xunit;

namespace TheSingularityWorkshop.Visualize.Tests;

public sealed class GpuSystemDetectorTests
{
    private static readonly GpuHostInfo TestHost = new(
        "Test OS", "1.0", Architecture.X64, Architecture.X64, ".NET Test", true);

    [Fact]
    public async Task DetectAsync_ReportsHostAndAllAdaptersAcrossProviders()
    {
        var nvidia = Adapter("vulkan", "gpu-0", "Discrete GPU", GpuAdapterType.Discrete, 0x10DE, 0x2684);
        var integrated = Adapter("direct3d12", "gpu-1", "Integrated GPU", GpuAdapterType.Integrated, 0x8086, 0x1234);
        var detector = new GpuSystemDetector(
        [
            new FakeProvider("vulkan", nvidia),
            new FakeProvider("direct3d12", integrated)
        ]);

        var result = await detector.DetectAsync(TestHost);

        Assert.Equal(TestHost, result.Host);
        Assert.Equal(2, result.Adapters.Count);
        Assert.Equal("gpu-0", result.Adapters[0].AdapterId);
        Assert.Equal("gpu-1", result.Adapters[1].AdapterId);
        Assert.True(result.HasAdapters);
        Assert.True(result.IsComplete);
        Assert.Empty(result.Failures);
    }

    [Fact]
    public async Task DetectAsync_PreservesRawIdsAndUnknownHardwareDetails()
    {
        var known = Adapter("backend", "known", "Device", GpuAdapterType.Unknown, 0x1234, 0xABCD);
        var unknown = new GpuAdapterInfo("unknown", "backend", "Unclassified adapter");
        var detector = new GpuSystemDetector([new FakeProvider("backend", known, unknown)]);

        var result = await detector.DetectAsync(TestHost);

        Assert.Equal(0x1234u, result.Adapters[0].VendorId);
        Assert.Equal(0xABCDu, result.Adapters[0].DeviceId);
        Assert.Equal(GpuAdapterType.Unknown, result.Adapters[0].Type);
        Assert.Null(result.Adapters[1].VendorId);
        Assert.Null(result.Adapters[1].DriverVersion);
        Assert.Null(result.Adapters[1].DedicatedMemoryBytes);
    }

    [Fact]
    public async Task DetectAsync_ReportsSoftwareAdaptersWithoutDiscardingThem()
    {
        var software = Adapter("backend", "swiftshader", "Software Renderer", GpuAdapterType.Software);
        var result = await new GpuSystemDetector([new FakeProvider("backend", software)]).DetectAsync(TestHost);

        Assert.Single(result.Adapters);
        Assert.Equal(GpuAdapterType.Software, result.Adapters[0].Type);
    }

    [Fact]
    public async Task DetectAsync_RecordsOneBackendFailureAndContinuesOtherBackends()
    {
        var working = Adapter("working", "gpu", "Working GPU");
        var detector = new GpuSystemDetector(
        [
            new FakeProvider("broken", exception: new InvalidOperationException("Runtime unavailable.")),
            new FakeProvider("working", working)
        ]);

        var result = await detector.DetectAsync(TestHost);

        Assert.Single(result.Adapters);
        Assert.Equal("working", result.Adapters[0].Backend);
        Assert.Single(result.Failures);
        Assert.Equal("broken", result.Failures[0].Backend);
        Assert.Contains("Runtime unavailable", result.Failures[0].Message);
        Assert.False(result.IsComplete);
    }

    [Fact]
    public async Task DetectAsync_RejectsAdapterMislabelledByProvider()
    {
        var detector = new GpuSystemDetector(
            [new FakeProvider("expected", Adapter("other", "gpu", "Mislabeled GPU"))]);

        var result = await detector.DetectAsync(TestHost);

        Assert.Empty(result.Adapters);
        Assert.Single(result.Failures);
        Assert.Contains("labelled 'other'", result.Failures[0].Message);
    }

    [Fact]
    public async Task DetectAsync_PropagatesRequestedCancellation()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var detector = new GpuSystemDetector([new FakeProvider("backend")]);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            async () => await detector.DetectAsync(TestHost, cancellation.Token));
    }

    [Fact]
    public void CaptureCurrent_ReportsRuntimeAndProcessArchitecture()
    {
        var host = GpuHostInfo.CaptureCurrent();

        Assert.False(string.IsNullOrWhiteSpace(host.OperatingSystem));
        Assert.False(string.IsNullOrWhiteSpace(host.RuntimeVersion));
        Assert.Equal(RuntimeInformation.ProcessArchitecture, host.ProcessArchitecture);
        Assert.Equal(Environment.Is64BitProcess, host.Is64BitProcess);
    }

    private static GpuAdapterInfo Adapter(
        string backend,
        string id,
        string name,
        GpuAdapterType type = GpuAdapterType.Unknown,
        uint? vendorId = null,
        uint? deviceId = null) =>
        new(id, backend, name, type, vendorId, deviceId);

    private sealed class FakeProvider(
        string backend,
        params GpuAdapterInfo[] adapters) : IGpuAdapterProvider
    {
        private readonly Exception? _exception = null;

        public FakeProvider(string backend, Exception exception) : this(backend) =>
            _exception = exception;

        public string Backend { get; } = backend;

        public ValueTask<IReadOnlyList<GpuAdapterInfo>> EnumerateAdaptersAsync(
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (_exception is not null)
                throw _exception;
            return ValueTask.FromResult<IReadOnlyList<GpuAdapterInfo>>(adapters);
        }
    }
}
