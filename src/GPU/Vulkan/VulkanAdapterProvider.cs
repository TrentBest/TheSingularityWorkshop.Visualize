using System.Runtime.InteropServices;
using Silk.NET.Vulkan;

namespace TheSingularityWorkshop.Visualize.GPU.Vulkan;

/// <summary>Enumerates every physical device exposed by the installed Vulkan loader and ICDs.</summary>
/// <remarks>
/// This provider performs Vulkan instance creation and physical-device inventory only. It does not create
/// logical devices, select a rendering device, create a surface, or prove workload execution.
/// </remarks>
public sealed unsafe class VulkanAdapterProvider : IGpuAdapterProvider
{
    /// <inheritdoc />
    public string Backend => "Vulkan";

    /// <inheritdoc />
    public ValueTask<IReadOnlyList<GpuAdapterInfo>> EnumerateAdaptersAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        Vk? vk = null;
        Instance instance = default;
        var instanceCreated = false;

        try
        {
            vk = Vk.GetApi();
            var applicationInfo = new ApplicationInfo
            {
                SType = StructureType.ApplicationInfo,
                ApiVersion = (1u << 22)
            };
            var createInfo = new InstanceCreateInfo
            {
                SType = StructureType.InstanceCreateInfo,
                PApplicationInfo = &applicationInfo
            };

            var createResult = vk.CreateInstance(&createInfo, null, out instance);
            if (createResult != Result.Success)
                throw new InvalidOperationException($"Vulkan instance creation failed: {createResult}.");
            instanceCreated = true;

            cancellationToken.ThrowIfCancellationRequested();

            uint deviceCount = 0;
            var countResult = vk.EnumeratePhysicalDevices(instance, &deviceCount, null);
            if (countResult != Result.Success)
                throw new InvalidOperationException($"Vulkan physical-device count query failed: {countResult}.");
            if (deviceCount == 0)
                return ValueTask.FromResult<IReadOnlyList<GpuAdapterInfo>>(Array.Empty<GpuAdapterInfo>());

            var devices = stackalloc PhysicalDevice[checked((int)deviceCount)];
            var enumerateResult = vk.EnumeratePhysicalDevices(instance, &deviceCount, devices);
            if (enumerateResult != Result.Success && enumerateResult != Result.Incomplete)
                throw new InvalidOperationException($"Vulkan physical-device enumeration failed: {enumerateResult}.");

            var adapters = new List<GpuAdapterInfo>(checked((int)deviceCount));
            for (var index = 0; index < deviceCount; index++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var device = devices[index];
                vk.GetPhysicalDeviceProperties(device, out var properties);
                vk.GetPhysicalDeviceMemoryProperties(device, out var memory);
                var features = ReadQueueFeatures(vk, device);

                var name = ReadDeviceName(&properties);
                var (deviceLocalBytes, hostVisibleBytes) = ReadHeapSizes(&memory);

                adapters.Add(new GpuAdapterInfo(
                    AdapterId: $"vulkan:{properties.VendorID:x4}:{properties.DeviceID:x4}:{index}",
                    Backend: Backend,
                    Name: string.IsNullOrWhiteSpace(name) ? $"Vulkan adapter {index}" : name,
                    Type: MapDeviceType(properties.DeviceType),
                    VendorId: properties.VendorID,
                    DeviceId: properties.DeviceID,
                    DriverVersion: $"0x{properties.DriverVersion:x8}",
                    ApiVersion: FormatVulkanVersion(properties.ApiVersion),
                    DedicatedMemoryBytes: deviceLocalBytes,
                    SharedMemoryBytes: hostVisibleBytes,
                    SupportedFeatures: features));
            }

            return ValueTask.FromResult<IReadOnlyList<GpuAdapterInfo>>(adapters.AsReadOnly());
        }
        finally
        {
            if (instanceCreated && vk is not null)
                vk.DestroyInstance(instance, null);
            vk?.Dispose();
        }
    }

    private static string ReadDeviceName(PhysicalDeviceProperties* properties)
    {
        byte* name = properties->DeviceName;
        return Marshal.PtrToStringAnsi((nint)name) ?? string.Empty;
    }

    private static (ulong DeviceLocal, ulong HostVisible) ReadHeapSizes(
        PhysicalDeviceMemoryProperties* memory)
    {
        ulong deviceLocal = 0;
        ulong hostVisible = 0;

        for (var index = 0; index < memory->MemoryHeapCount; index++)
        {
            var heap = memory->MemoryHeaps[(int)index];
            if ((heap.Flags & MemoryHeapFlags.DeviceLocalBit) != 0)
                deviceLocal = checked(deviceLocal + heap.Size);
            else
                hostVisible = checked(hostVisible + heap.Size);
        }

        return (deviceLocal, hostVisible);
    }

    private static IReadOnlyList<string> ReadQueueFeatures(Vk vk, PhysicalDevice device)
    {
        uint count = 0;
        vk.GetPhysicalDeviceQueueFamilyProperties(device, &count, null);
        if (count == 0)
            return Array.Empty<string>();

        var queues = stackalloc QueueFamilyProperties[checked((int)count)];
        vk.GetPhysicalDeviceQueueFamilyProperties(device, &count, queues);

        var graphics = false;
        var compute = false;
        var transfer = false;
        for (var index = 0; index < count; index++)
        {
            var flags = queues[index].QueueFlags;
            graphics |= (flags & QueueFlags.GraphicsBit) != 0;
            compute |= (flags & QueueFlags.ComputeBit) != 0;
            transfer |= (flags & QueueFlags.TransferBit) != 0;
        }

        var result = new List<string>(3);
        if (graphics) result.Add("queue.graphics");
        if (compute) result.Add("queue.compute");
        if (transfer) result.Add("queue.transfer");
        return result.AsReadOnly();
    }

    private static GpuAdapterType MapDeviceType(PhysicalDeviceType type) => type switch
    {
        PhysicalDeviceType.DiscreteGpu => GpuAdapterType.Discrete,
        PhysicalDeviceType.IntegratedGpu => GpuAdapterType.Integrated,
        PhysicalDeviceType.VirtualGpu => GpuAdapterType.Virtual,
        PhysicalDeviceType.Cpu => GpuAdapterType.Software,
        _ => GpuAdapterType.Unknown
    };

    private static string FormatVulkanVersion(uint version)
    {
        var major = version >> 22;
        var minor = (version >> 12) & 0x3ff;
        var patch = version & 0xfff;
        return $"{major}.{minor}.{patch}";
    }
}
