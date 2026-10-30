# Visualize GPU Vulkan Adapter

This optional adapter package implements `IGpuAdapterProvider` using Silk.NET.Vulkan 2.23.0.

## Current scope

- Creates a minimal Vulkan 1.0 instance through the installed Vulkan loader.
- Enumerates every physical device exposed by the loader.
- Reports Vulkan device name/type, raw vendor and device IDs, raw driver-version bits, API version, memory-heap totals, and graphics/compute/transfer queue availability.
- Disposes the temporary Vulkan instance after enumeration.
- Does not create a logical device, choose a device, create a WPF/browser surface, submit commands, or claim that a Visualize workload renders.

Vulkan's `driverVersion` is vendor-defined. It is preserved as a hexadecimal raw value rather than presented as a universally meaningful semantic version. Memory values are heap totals, not a promise of currently available memory or a memory budget.

## Use

Register `VulkanAdapterProvider` with `GpuSystemDetector` from the application composition root. Keep this package optional; the API-neutral `Visualize.GPU` assembly does not reference Vulkan or Silk.NET.

## Evidence boundary

A returned adapter is **discovered**, not initialized or workload-verified. A software Vulkan implementation may appear in the inventory and must not be counted as physical-GPU performance evidence. Run on target hardware and record the API/device/driver before claiming hardware recognition.
