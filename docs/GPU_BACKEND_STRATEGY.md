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
