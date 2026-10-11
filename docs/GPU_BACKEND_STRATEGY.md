# GPU Backend Strategy

**Status:** Research framework. No graphics API has been selected.

## 1. The objective

We want direct control of GPU resource use, parallel compute, graphics pipelines, synchronization, and presentation—not a dependency on a large game engine that dictates the world's architecture.

That does not mean writing every low-level primitive ourselves or assuming the lowest-level API will win every benchmark. Drivers, API implementation, shader compiler, memory traffic, synchronization, CPU submission, and host integration all contribute to actual performance.

## 2. CUDA is not the whole rendering stack

CUDA is NVIDIA's general-purpose GPU compute platform. It offers a strong programming and profiling model for parallel compute, but it is not a universal cross-vendor graphics and display API. A CUDA kernel does not itself create a WPF surface, rasterize a scene, or present a frame.

We should learn from CUDA's explicit compute model while keeping the graphics path portable. An NVIDIA-specific compute path may be explored only when a measured workload justifies the added backend and interoperability complexity.

## 3. Candidates to prototype

| Candidate | Why investigate it | Risks / limits |
| --- | --- | --- |
| **Direct3D 12** | Low-level Windows graphics/compute control; natural native candidate for AnyApp/WPF | Windows-centric; explicit synchronization, resource lifetime, pipeline and device-loss responsibility |
| **Vulkan** | Explicit cross-platform native graphics/compute | Complex API and synchronization; not directly available as a native API inside browser content |
| **WebGPU** | Graphics and compute API designed for browser content; natural web path | Browser/device capability variation; cannot assume native and browser feature parity |
| **Cross-backend library** | May allow shared code while preserving access to modern GPU operations | Abstraction may hide features or constrain access; must audit license, maintenance, and performance |
| **CUDA** | Strong NVIDIA-specific compute path | Vendor-specific; not a replacement for general graphics/presentation |

Direct3D 12 is not equivalent to “DirectX is deprecated.” Do not assume a DirectX 13 API exists or plan around an unverified successor. The backend boundary exists so that future options can be assessed without rewriting Gesture, Renderer, or Experience semantics.

## 4. First decision: a working native surface

For the first AnyApp milestone, evaluate:
- direct native Windows graphics API through a maintained .NET interop library;
- a credible cross-backend library that can expose the needed low-level control.

Do not pick from feature lists alone. Build the same tiny scene through the candidate paths:
1. create a device and a real WPF-hosted presentation surface;
2. clear and present;
3. upload vertex/index buffers;
4. draw one indexed object;
5. draw an instanced population;
6. dispatch a compute pass whose output is consumed by a graphics pass;
7. resize, close, and recreate resources;
8. measure CPU submission, GPU pass time, frame-time distribution, memory, transfer volume, and synchronization stalls.

The WPF surface integration is part of the benchmark. A fast device API that integrates poorly with the host is not a successful Visualizer.

## 5. Shader and data strategy

No shader language or GPU data layout is selected. Candidate shader toolchains must be evaluated for:
- supported native and browser targets;
- compilation and reflection;
- pipeline-layout validation;
- debugging/profiling;
- reproducible build output;
- asset packaging and versioning;
- licensing and redistribution;
- hot reload for development without runtime shipping surprises.

The compact four-channel 8-bit state-texture concept from raWWar is a hypothesis. Compare structured/storage buffers, textures, packed integer layouts, and hybrid designs against real Gesture and instance access patterns. Do not pack unrelated values into one format solely because it looks compact.

## 6. Backend-neutral does not mean lowest-common-denominator

The public contract should expose capabilities and limits explicitly. It may offer common operations for the supported path and a controlled backend-specific extension mechanism for advanced operations.

Do not silently emulate an unsupported feature in a way that changes correctness or performance. Do not force every backend to use identical shader binaries.

## 7. References

- [NVIDIA CUDA Programming Guide](https://docs.nvidia.com/cuda/cuda-programming-guide/index.html)
- [WebGPU specification](https://www.w3.org/TR/webgpu/)
- [Vulkan](https://www.vulkan.org/)
- [Direct3D 12 documentation](https://learn.microsoft.com/windows/win32/direct3d12/direct3d-12-graphics)
- [wgpu project](https://github.com/gfx-rs/wgpu)

These are research starting points, not endorsements. The exact .NET bindings, supported API versions, package versions, license compatibility, and current maintenance status must be checked before adding a dependency.


## 8. .NET binding investigation (2026-10-11)

The first pass of current dependency research narrows the native prototype candidates:

- [Silk.NET.Direct3D12 2.23.0](https://www.nuget.org/packages/Silk.NET.Direct3D12/2.23.0) is a stable published release with .NET Standard 2.0 compatibility and active ecosystem maintenance. It is a plausible .NET 8 binding for a Direct3D 12 prototype. The package download is comparatively large, so its actual transitive/runtime footprint must be measured rather than ignored.
- [Vortice.Direct3D12 3.8.3](https://www.nuget.org/packages/Vortice.Direct3D12/3.8.3) is listed as compatible with .NET 8 as well as newer targets, despite the upstream repository's current development branch focusing on newer .NET versions. It is the binding used by the isolated WPF clear-and-present proof. Its dependency graph includes prerelease SharpGen runtime packages, so the exact graph, footprint, license, and redistribution contents still require audit.
- Microsoft's [WPF/Win32 interoperation guide](https://github.com/dotnet/docs-desktop/blob/main/dotnet-desktop-guide/wpf/advanced/wpf-and-win32-interoperation.md) describes HwndHost as the supported mechanism for hosting a native child HWND inside a WPF visual tree. That is a plausible route for a swap-chain surface without making WPF controls the scene renderer.
- Silk.NET's [Direct3D 11 example](https://github.com/dotnet/Silk.NET/blob/main/examples/CSharp/Direct3D11%20Tutorials/Tutorial%201.3%20-%20Textures/Program.cs) demonstrates the general DXGI swap-chain pattern, but it is not a substitute for a D3D12 prototype. We should implement the selected native path directly and keep the WPF surface integration separate.

### Provisional prototype plan

1. Prototype a native Direct3D 12 device/swap-chain surface hosted by a WPF HwndHost in AnyApp.
2. Prefer a maintained .NET binding for the first proof, but record dependency footprint and isolate it behind the Visualizer backend project.
3. Prove clear-and-present, resize, indexed draw, and cleanup before connecting the full Renderer command model.
4. In parallel or immediately afterward, prove a browser WebGPU surface with the same conceptual frame and resource contract.
5. Compare measured behavior and implementation cost. This is a candidate investigation, not a permanent API selection.


## 9. First native surface proof (2026-10-11)

The isolated [WPF Direct3D 12 proof](../samples/WpfD3D12Proof/README.md) now creates a native child HWND, Direct3D 12 device and direct queue, DXGI swap chain, RTV heap, back buffers, command allocator/list, and fence. It clears and presents a real GPU back buffer and recreates swap-chain buffers on resize. A Windows CI workflow builds the proof separately from the backend-neutral contract project.

This is deliberately a correctness-first proof. It waits for GPU completion before allocator reuse, draws no indexed geometry, and is not yet connected to VisualizerFrame or AnyApp. It is not performance evidence and must not be described as a finished renderer. The next gate is a known indexed triangle and non-blocking frame-resource scheduling, followed by an adapter into the core contract.
