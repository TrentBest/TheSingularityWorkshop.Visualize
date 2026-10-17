# GPU Backend — First Vertical Slice

**Owner:** Visualize  
**Branch:** `development`  
**Purpose:** Turn the existing frame/resource contracts into a verifiable, interactive GPU presentation path without confusing CPU/SVG proofs with GPU rendering.

## Current baseline

Visualize already defines geometry, materials, resource IDs, draw commands, views, frames, a public `Visualizer.RenderAsync(frame)` entry point, and a GUI.SVG integration. The repository also generates a perspective-projected 3D mesh proof from C#.

These are valuable contract and projection proofs. They are not a GPU backend: the current SVG path is CPU-side, the perspective sample uses painter ordering, and neither establishes depth-buffer correctness, general clipping, GPU resource lifetime, or interactive presentation.

## Graphics API strategy: own the renderer, not a hardware driver

**Decision:** Visualize owns the rendering model and backend contract. DirectX is not a required dependency of Visualize core and is not allowed to leak into Renderer resource, scene, or observer contracts. We will write and own the Workshop's GPU execution backend; we will not attempt to replace vendor kernel drivers or write a hardware driver from scratch.

These are different levels of the stack:

- **Ours:** frame interpretation, resource revision tracking, upload policy, batch construction, shader/pipeline definitions, render passes, diagnostics, lifetime rules, and the Renderer-to-Visualize handoff.
- **Platform adapter:** the narrow API binding that creates a device/surface and submits commands. It stays behind the backend boundary and can be replaced.
- **Hardware driver:** supplied by the device/platform vendor. Reimplementing this is not a sensible first milestone for an independent renderer.

For a native backend that does not require DirectX, evaluate **Vulkan** first. For the browser host, evaluate **WebGPU** separately. They are not the same API and should not be forced into a lowest-common-denominator abstraction that hides capabilities. Instead, Visualize defines the work and resource semantics; each backend reports capabilities and implements them explicitly. A browser may use an implementation whose internal platform layer selects a native graphics API, but that does not make DirectX a Visualize dependency.

The first proof must answer these questions with a tiny running sample before we commit to a large API surface:

1. Can the native backend create a real surface on our supported desktop host without requiring a DirectX-specific Visualize contract?
2. Can it consume the existing `VisualizerFrame` and `VisualizerResourceCatalog`, including indexed geometry, materials, transforms, and `BaseVertex` semantics?
3. Can it draw to a depth-tested surface, report its backend/device, and release every owned resource on disposal?
4. Can the browser implementation consume the same frame/resource concepts without adding browser APIs to the core assembly?
5. Can CI test resource preparation, batching, transforms, and lifetime without pretending that a headless test produced GPU pixels?

Do not choose an API because it is familiar, and do not add a second backend merely to make a diagram look cross-platform. Build the smallest real native proof, measure the integration and maintenance cost, and keep the browser path as a separate host adapter. If Vulkan surface integration proves disproportionately expensive for the intended host, record the measured obstacle and compare alternatives before changing this decision.

## Native binding spike: Vulkan on the existing WPF host

**First binding candidate:** Silk.NET.Vulkan, pinned to the latest stable release verified at implementation time (the NuGet listing currently shows 2.23.0, published 2026-01-23). Silk.NET supplies low-level Vulkan bindings; it is not itself our renderer and does not replace the host or device driver. Keep the package reference inside the GPU implementation project.

Do not start by embedding a second windowing/event loop inside AnyApp. The first spike should test the direct native-surface path:

1. Host the render target through a small WPF `HwndHost` adapter so WPF owns the child HWND and its lifetime.
2. Create a Vulkan instance with the required platform surface extensions, create the Win32 surface from the hosted HWND/HINSTANCE, and verify presentation support for the selected queue family.
3. Render and present one indexed triangle with a minimal shader pipeline; record API version, physical-device name, surface format, and present result.
4. Resize and close the host repeatedly; prove swapchain recreation and deterministic disposal before integrating with the full Visualizer frame loop.

This is a **candidate, not a binding commitment**. Do not add Silk.NET.Windowing merely to obtain a Vulkan surface if it creates a separate top-level window or event loop. If direct Win32 surface integration is too costly, record the exact blocker and compare alternatives in a short decision record before changing the API choice. Keep Vulkan-specific types and handles out of Visualize core and the Renderer adapter. Browser WebGPU remains an independent path.

## Package boundary: Visualize.GPU

**Working package name:** `TheSingularityWorkshop.Visualize.GPU` (repository/project identity: `Visualize.GPU`).

Keep the backend-neutral `TheSingularityWorkshop.Visualize` package small and graphics-API independent. Put concrete GPU execution in a separate `Visualize.GPU` implementation package that references Visualize core, never the reverse. This lets SVG, tests, and future backends continue using the same frame/resource contracts without inheriting native graphics dependencies.

Initial responsibilities of `Visualize.GPU`:

- Implement `IVisualizerBackend` using the existing `VisualizerFrame`, draw commands, and resource catalog.
- Own device selection, shader/pipeline setup, GPU buffer allocation and revision-based uploads, command submission, synchronization, and deterministic GPU-resource disposal.
- Report the actual API/device and supported capabilities; fail clearly when a required capability is unavailable.
- Keep native handles and graphics-API types private to the implementation boundary wherever practical.
- Provide a minimal executable proof that renders an indexed triangle to a real surface and records verifiable evidence.

Keep host/window integration distinct from rendering contracts. AnyApp/WPF is the first native host to validate, but WPF surface handles and window lifetime must not enter Visualize core. If the native implementation requires host-specific glue, put that glue in a narrow host adapter rather than turning the core or the GPU package into a WPF-only library. WebPage/browser rendering is a separate WebGPU host/backend path, not a reason to add browser APIs to the .NET core.

This is a package boundary decision, not a claim that the GPU backend already exists. Do not create a placeholder package or publish it until the first real surface proof, build/test lane, and lifecycle evidence are in place.

## First vertical slice: one triangle, one real surface

Implement the smallest end-to-end GPU path before expanding the scene vocabulary.

1. **Choose the first host deliberately.** The repository-level integration target is **AnyApp on WPF**: its `development` project references Visualize and the Renderer adapter from source, and its `RendererDesktopProofView` is already an explicit diagnostic entry point (`--renderer-proof`). This confirms a real host and source-integration route, but not that the current Visualize path produces GPU pixels or that the desktop app was run in this work session. The existing proof view explicitly describes itself as a WPF diagnostic projection, not a GPU proof. Next, replace or complement that projection with a Visualize-backed GPU surface without moving WPF dependencies into Visualize core. Keep browser/WebGPU as a separate host implementation.
2. **Define a narrow backend boundary.** The core accepts a frame and logical resource descriptions; the backend owns device creation, buffers, pipelines, submission, presentation, and device-resource disposal. Keep native handles private to the backend.
3. **Resolve logical resources explicitly.** Geometry/material IDs must resolve through the Visualize resource catalog. Track resource revisions so unchanged geometry is not uploaded every frame. Never treat a logical ID as a native GPU pointer.
4. **Render one indexed triangle.** Use a real GPU surface and shader pipeline. Demonstrate vertex/index upload, transform handling, color/material input, and a successful frame submission.
5. **Add depth and clipping next.** Extend the proof to overlapping triangles at different depths and a triangle crossing the near plane. Assert the nearer surface wins regardless of submission order; verify clipped geometry does not explode or disappear incorrectly.
6. **Prove lifetime.** Dispose the backend and its GPU allocations deterministically. Test repeated create/render/dispose cycles and ensure resources are not retained by a static cache. If device loss/recreation is supported, make it an explicit follow-up test.
7. **Keep SVG as a diagnostic backend.** It remains useful for inspectable generated documentation, deterministic regression fixtures, and environments without a GPU. It is not the acceptance test for GPU execution.

## Required evidence

- A CI build/test job compiles the backend and runs all host-independent tests.
- A generated artifact or screenshot shows pixels produced by the GPU path, with backend/device information recorded.
- Tests cover indexed geometry, world/view/projection transforms, depth ordering, near-plane clipping, resource revision/reuse, and disposal.
- Diagnostics report submitted primitives, buffer uploads/bytes, pipeline or draw batches, frame duration, and backend identity. CPU and GPU work must both be measured; the architecture should exploit both where appropriate.
- The sample can be run from a documented command, with no Unity dependency and no Unity types in Visualize core.
- Any unavailable hardware-dependent test is reported as skipped/not-run, never described as passed.

## Second vertical slice: a world, not a triangle

Once the triangle proof is real and repeatable:

1. Render a lit, indexed 3D mesh with a camera that can orbit and move.
2. Add a small deterministic city block with stable entity IDs and multiple building representations.
3. Drive representation selection through Renderer observer/Event Horizon policy; do not duplicate world state for plan, aerial, and first-person views.
4. Show plan, aerial, and street/FPS observation of the same world, with explicit camera/view state.
5. Demonstrate that distant detail reduction changes submitted work while preserving stable identity and that moving closer promotes the same entity to richer geometry.
6. Use the same scene model in the AnyApp desktop host and the WebPage browser host; each host owns its surface and input integration.

## Resource and composition ownership

FSM_COS may select a rendering capability, but it is not the renderer or the graphics loop. The host/adapter owns the backend instance and its cleanup. MicroBundleDomain's current contract does not itself guarantee an unload/dispose callback, so a selected rendering bundle must not silently imply GPU-resource teardown. The composition proof and the manifestation proof are separate acceptance criteria.

## Scope discipline

- Work on `development`; do not publish packages or change `master` as part of this milestone.
- Do not introduce Unity types or a graphics API into Visualize core.
- Do not claim GPU output from SVG, software projection, or a passing compile.
- Do not optimize around hypothetical city-scale performance before a real surface and measurements exist.
- Keep Renderer responsible for observer-relative work selection, Visualize responsible for presentation execution, and the host responsible for window/surface lifecycle.
