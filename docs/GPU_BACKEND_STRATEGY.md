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

### 2.1 Event-Horizon-first execution

The Renderer’s Event Horizons are the first optimization boundary—not a graphics API's LOD feature. Perception determines which representation is required, how much work is justified, and how frequently that representation needs evaluation. Visualize should consume those decisions rather than independently reconstructing world semantics.

The intended frame path is:

1. **Observe:** determine the observer/camera relationship and current perception budgets.
2. **Classify and invalidate:** identify which horizon representations are visible, stale, newly required, or still valid.
3. **Schedule with FSM_API:** use FSM state and processing groups to coordinate horizon work, dependencies, cadence, and transitions. Far horizons may be evaluated less frequently than near ones.
4. **Reuse before recomputing:** preserve valid distant representations. A horizon may be a cached texture, impostor, mesh/instance buffer, or another representation appropriate to its content; a horizon is not required to be a separate full-screen image.
5. **Execute the due work:** produce or update compact arrays, upload only changed data where practical, dispatch suitable parallel GPU work, and batch presentation.
6. **Compose the result:** reuse valid far-field output and combine it with updated nearer representations in correct depth/order and with appropriate transitions.

An unchanged horizon must not be regenerated simply because a new display frame is presented. Its cache needs explicit validity/invalidation rules—for example, changes to the world representation, observer movement or orientation, lighting, resources, or horizon-specific freshness budgets. Those rules must be tested for correctness, not inferred from distance alone.

**FSM_API and the GPU have complementary roles.** FSM_API is the stateful coordination and scheduling layer: it determines which work is valid, due, dependent, or deferred. It should not be mistaken for the pixel rasterizer or assumed to replace GPU-parallel arithmetic. The GPU executes data-parallel transforms, culling, procedural evaluation, and graphics/compute workloads where measurements show they belong. The backend should accept batched arrays and explicit work descriptions rather than requiring one managed object and one submission per world item.

The benchmark must therefore measure the whole perception-to-presentation pipeline, including horizon invalidation, cache reuse, CPU scheduling/submission, data movement, GPU execution, and final composition. A raw triangle benchmark cannot validate this architecture.

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

The shared [Array-First GPU Workload Contract](../samples/ArrayFirstWorkload/README.md) defines initial scale, near-field update, far-field cadence, invalidation, compute-to-render, and resize profiles. Its adjacent `workload.json` captures the logical instance layout, seed, population sizes, cadence settings, and target measurement window. It is deliberately marked definition-only until a runner consumes it.

The evaluation must cover:

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

## 2.2 Cached horizons are presentation inputs, not merely draw lists

The frame-oriented VisualizerFrame contract is a useful first boundary, but by itself it does not define a persistent Event Horizon cache or guarantee that unchanged distant content avoids reconstruction. The backend strategy must evolve beyond assuming that every display frame is a wholly new list of draws.

Visualize should ultimately be able to distinguish the following kinds of work, without taking ownership of Renderer’s semantic decisions:

- **reuse** an already rendered horizon result;
- **reproject or shift** cached output when the representation and available depth/coverage make that correct;
- **update source arrays/resources** when their revisions change;
- **render geometry/visibility** for a newly required or invalidated representation;
- **refresh lighting, shadows, or material shading** when their own dependencies change;
- **compose** cached distant output with newly rendered nearer layers for the current view.

This is a capability target, not a claim that the current backend-neutral contracts already implement these operations. Cache lifetime, validity, dependencies, and device-resource recovery need explicit contracts and tests before the first backend is treated as production-ready.

### A layered cadence model

The display can update every frame while the work underneath it runs at independent rates. A hypothetical scene might update immediate/near geometry at display cadence, local context at a lower cadence, and distant environment content at a much lower cadence—for example, testing a one-minute refresh for content at and beyond 1 km. These intervals are workload settings to measure, never hard-coded universal thresholds.

Geometry and lighting also need not share one cadence. A useful experiment is to establish geometry/visibility and a relevant-light list first, then schedule costly shadowing and shading only when their inputs change or their freshness budget expires. This does not prescribe forward rendering, deferred rendering, ray tracing, or any one algorithm; the backend comparison must determine which staged approach best fits the actual workloads and hardware.

### Background reuse and observer movement

The visible world must extend beyond the Renderer’s immediate focus. The focus determines where detailed computation is justified; it does not mean everything outside the focus disappears. Distant environment representations remain part of the final composition.

When the observer moves, the backend may shift or reproject a cached background instead of rebuilding it, but only when the representation supports that operation:

- orientation changes require the background to follow the new view;
- translation can reveal parallax and disocclusion, so a single flat image is not generally sufficient;
- depth-aware reprojection or world-space cached representations can preserve more correct motion;
- uncovered regions need newly rendered content or an explicit fallback;
- dynamic objects and light changes should invalidate dependent regions/results without forcing unrelated valid horizons to rebuild.

The implementation should prefer dependency-aware, partial invalidation over a single all-or-nothing scene cache. It must also avoid the opposite mistake: retaining stale pixels after the world has changed merely because the scheduled refresh interval has not elapsed. Correctness-critical events can override the normal cadence.

### Required cache/cadence proof

Before selecting a backend on performance grounds, the benchmark must demonstrate that:

1. a valid far horizon is reused across many display frames without re-running its expensive rendering work;
2. near-field geometry can change while the far cache remains valid;
3. observer rotation and translation cause only the invalidation/reprojection required by the selected cache representation;
4. disoccluded or newly visible regions are filled correctly rather than exposing stale or empty pixels;
5. a light-source change can refresh affected lighting/shadow results without automatically rebuilding unchanged geometry;
6. resource/world revisions and explicit freshness deadlines invalidate the correct results;
7. frame-time percentiles, CPU work, GPU work, transfer volume, allocations, and cache memory are measured together.

The intended result is not “draw the background once and forget it.” It is **retain and reuse every result whose dependencies are still valid, and pay again only for the work that became necessary**.
