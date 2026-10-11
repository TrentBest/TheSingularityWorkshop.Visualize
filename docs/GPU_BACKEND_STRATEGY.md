# GPU Backend Strategy

**Status:** Investigation framework. No graphics API or binding has been selected.

## 1. The objective

Visualize should exploit the Workshop's data-oriented design instead of inheriting the architecture of a graphics API, windowing toolkit, or game engine. The aim is direct, efficient control over GPU resources, parallel compute, graphics pipelines, synchronization, and presentation—while keeping world meaning and observer-relative detail decisions outside the graphics backend.

The optimization target is the whole path: select only the detail an observer needs, express the remaining work in compact arrays, transfer data efficiently, batch submission, execute parallel GPU work, and present with minimal stalls. Low-level APIs can expose control, but control alone is not proof of speed.

## 2. Array-first is a design constraint

The core workload should prefer contiguous, explicitly laid-out data over a forest of per-object runtime wrappers. Candidate arrays/streams include:

- vertex and index data;
- instance transforms and per-instance parameters;
- pose/joint samples and animation inputs;
- visibility, LOD, material, and resource-index lists;
- compact draw/dispatch descriptions.

The backend should support bulk uploads and stable resource revisions. Avoid per-object conversion, transient allocation, redundant copies, and CPU loops that merely repackage data the GPU can consume in batches.

This does **not** mean blindly uploading every CPU array every frame. Compare persistent GPU residency, incremental updates, mapped/upload buffers, storage buffers, packed formats, textures, and hybrid approaches against actual update patterns. The GPU copy is disposable derived state; authoritative state remains outside the backend.

## 3. CUDA is not the whole rendering stack

CUDA is NVIDIA's general-purpose GPU compute platform. It can be valuable for measured NVIDIA-specific workloads, but it is not a universal cross-vendor graphics and display API. A CUDA kernel does not by itself create a WPF surface, rasterize a scene, or present a frame. Consider it only when measured benefit justifies vendor-specific code and interoperability complexity.

## 4. Backend candidates

| Candidate | Why investigate it | Risks / limits |
| --- | --- | --- |
| **Vulkan** | Explicit native graphics and compute across platforms | Significant resource, synchronization, shader, and device-management complexity; not a browser API |
| **WebGPU** | Graphics and compute designed for browser content; relevant to the web path | Browser/device capability variation; native and browser feature parity cannot be assumed |
| **Cross-backend implementation** | May share a GPU model across native and browser targets | Abstraction may hide needed control, add overhead, constrain features, or complicate diagnostics |
| **Direct3D 12** | Explicit Windows graphics/compute control and a possible WPF-native path | Windows-centric; a current isolated experiment exists, but that is not evidence it is the right architectural choice |
| **CUDA** | Strong NVIDIA-specific compute tooling | Vendor-specific; not a replacement for general graphics/presentation |

The existence of code for a candidate is not a selection criterion. **Do not continue the Direct3D 12 sample by default.** See [ADR-0004](decisions/ADR-0004-native-gpu-prototype.md): it records an isolated experiment, not the chosen direction.

## 5. Compare candidates using the same workload

Build a repeatable evaluation that covers:

1. Create a real host surface and GPU device.
2. Clear and present, including resize and clean shutdown.
3. Bulk-upload known vertex/index arrays.
4. Draw indexed and instanced populations.
5. Dispatch compute whose output is consumed by a render pass.
6. Exercise changing data and persistent resources, not only static geometry.
7. Record device/driver, CPU submission time, GPU pass time, frame-time percentiles, allocations, transfer volume, synchronization stalls, memory use, package graph, and deployed footprint.

The first benchmark is a correctness gate, not a performance victory. Compare the same scene, visible detail, resolution, hardware, and interaction workload. Include the host integration in the comparison: a fast GPU path with costly or fragile presentation integration is not a successful Visualizer.

## 6. Shader and data strategy

No shader language or GPU data layout is selected. Evaluate toolchains for native and browser targets, compilation/reflection, debugging/profiling, reproducible build output, shader asset packaging, hot reload, licensing, and redistribution.

The compact four-channel 8-bit state-texture concept from raWWar is a hypothesis—not a universal format. Compare structured/storage buffers, textures, packed integer layouts, and hybrid designs against real Gesture and instance access patterns. Avoid packing unrelated values solely because a format looks compact.

Backend-neutral does not mean lowest-common-denominator. Expose capabilities and limits explicitly, permit backend-specific shaders and advanced operations, and do not silently emulate unsupported features in ways that change correctness or performance.

## 7. Host boundaries

- Visualize core contracts describe work and resources without WPF controls, HWNDs, or native graphics handles.
- AnyApp/WPF owns the desktop host lifecycle.
- WebPage/browser owns browser lifecycle and uses browser-supported GPU APIs.
- Backend implementations own device resources, uploads, command submission, synchronization, and presentation.
- Renderer decides observer-relative representation and detail; Visualize should not redo those semantic decisions.

## 8. Existing Direct3D 12 experiment

The isolated [WPF Direct3D 12 proof](../samples/WpfD3D12Proof/README.md) creates a native child HWND, Direct3D 12 device/queue, DXGI swap chain, and clear/present loop. It is compile-verified in Windows CI, but a real hardware/runtime capture has not been recorded. It draws no geometry and deliberately waits for GPU completion each frame.

Keep this sample isolated. Do not add indexed drawing, connect it to VisualizerFrame, or integrate it with Renderer/AnyApp until the backend strategy is deliberately revisited. Its current value is as a reference experiment and a reminder to demand real runtime evidence.

## 9. Research references

- [Vulkan](https://www.vulkan.org/)
- [WebGPU specification](https://www.w3.org/TR/webgpu/)
- [wgpu project](https://github.com/gfx-rs/wgpu)
- [NVIDIA CUDA Programming Guide](https://docs.nvidia.com/cuda/cuda-programming-guide/index.html)
- [Direct3D 12 documentation](https://learn.microsoft.com/windows/win32/direct3d12/direct3d-12-graphics)
- [WPF and Win32 interoperation](https://github.com/dotnet/docs-desktop/blob/main/dotnet-desktop-guide/wpf/advanced/wpf-and-win32-interoperation.md)

These are research starting points, not endorsements. Verify current binding support, package versions, license compatibility, maintenance status, runtime footprint, and actual performance before selecting a dependency.
