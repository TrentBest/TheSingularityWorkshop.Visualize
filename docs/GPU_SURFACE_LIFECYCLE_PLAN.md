# GPU Surface Lifecycle Plan

**Status:** Implementation plan; no API or binding selected  
**Owner:** Visualize GPU + native host adapter  
**Branch:** `work/gpu-foundation`  
**Purpose:** Define a surface lifecycle that fits the existing desktop host and preserves the browser path as a separate implementation.

## Boundary

- **Visualize core** owns logical frames, resource IDs/revisions, draw semantics, and backend-neutral capability reporting.
- **Visualize.GPU** owns GPU resource preparation, buffers, pipelines, submission, synchronization, and backend disposal.
- **Native host adapter** owns the connection between the desktop window and the graphics surface. It may use WPF/Windows types, but those types must not enter Visualize core or the logical Renderer contracts.
- **Host application** owns the window, event loop, visibility, DPI, and shutdown. A renderer must not create a second window or independent event loop just to get a surface.
- **Browser host** has its own canvas and WebGPU lifecycle. Do not force browser canvas/device-loss semantics into the WPF adapter.

The backend should accept a host-supplied surface bridge or narrow host-owned handle contract. Keep API-native objects private to the implementation assembly. Do not expose Vulkan, WebGPU, Direct3D, WPF, or HWND types in the public core API.

## Verified current desktop-host constraints

The current AnyApp development branch targets `net8.0-windows`, enables WPF, and declares `win-x64`. Its `MainWindow.xaml` currently exposes a WPF `Grid` named `RootHost`. The existing `RendererDesktopProofView` is a WPF `FrameworkElement` that calls `DrawingContext.DrawRectangle`, `DrawEllipse`, `DrawLine`, and `DrawText` to render a diagnostic projection. Its source explicitly states that it is not a GPU proof. It adapts Renderer commands to a `VisualizerFrame`, but does not submit a Visualize GPU workload.

This gives us a concrete integration target, not yet a GPU surface contract:

- Preserve the existing WPF window and dispatcher; the first native proof should be an opt-in diagnostic view hosted in `RootHost`, not a second window or loop.
- Keep the current `RendererDesktopProofView` as the CPU/WPF comparison path. Do not relabel its output as GPU-rendered.
- Inspect how the WPF visual/handle is created and destroyed before choosing the surface bridge. The root `Grid` alone does not establish that a Vulkan or wgpu surface can be safely created from it.
- The current AnyApp project is `net8.0-windows` and `win-x64`; do not claim x86, ARM64, or cross-platform WPF support from this target.
- The actual native proof must run on a Windows machine with the intended runtime assets and a real graphics device (or explicitly identified software adapter), and must capture a visible output or verified readback.

Sources inspected on the AnyApp `development` branch: [AnyApp.csproj](https://github.com/TrentBest/AnyApp/blob/development/AnyApp.csproj), [MainWindow.xaml](https://github.com/TrentBest/AnyApp/blob/development/MainWindow.xaml), and [RendererDesktopProofView.cs](https://github.com/TrentBest/AnyApp/blob/development/RendererDesktopProofView.cs).

## Native host lifecycle

1. **Host created:** the existing AnyApp/WPF window owns its normal dispatcher and lifetime. The GPU adapter attaches only after the target visual/handle is initialized.
2. **Surface ready:** create the API instance/device, choose a compatible surface format and present mode, then create the swap chain/surface configuration and size-dependent attachments. Record adapter/device/driver/API version and selected formats.
3. **First frame:** upload the canonical workload's vertex/index/instance arrays, create the smallest shader/pipeline set, and draw one indexed triangle. Capture the actual presented surface or a verified readback; a successful command submission alone is insufficient.
4. **Frame submission:** acquire a surface image, record commands, submit, present, and associate completion with the resources used by that submission. Avoid a global device-idle wait per frame except as an explicitly measured diagnostic mode.
5. **Resize / DPI change:** coalesce host resize notifications; suspend rendering for zero-sized surfaces; rebuild only size-dependent surface/depth resources after the host reports a stable nonzero physical-pixel size. Track logical WPF size separately from device-pixel extent. Do not recreate immutable geometry buffers for a window resize.
6. **Occlusion / minimization:** stop or throttle frame acquisition when the host is hidden/minimized or the surface is not drawable. Resume through the host's existing lifecycle notifications.
7. **Resource revision:** upload only when the logical resource's revision or content identity changes. Keep upload-byte and allocation counters. Defer releasing a replaced resource until all submissions that reference it have completed.
8. **Device/surface loss:** return a structured backend failure and stop submitting to invalid resources. Recreate only if recovery is intentionally implemented and tested; otherwise fail clearly without corrupting the host dispatcher.
9. **Shutdown:** stop new submissions; wait for or retire in-flight work; release size-dependent attachments, pipelines, buffers, surface, device, and API instance in dependency order; detach host callbacks. Repeated create/render/dispose cycles must not retain windows, callbacks, or GPU allocations.

## Threading and ownership rules

- WPF window/surface operations that require dispatcher affinity remain on the host dispatcher. GPU work may use a dedicated submission thread only if the chosen API and host integration require it; it must not start another UI/event loop.
- One owner is responsible for disposal. The host owns the window; the adapter owns its surface bridge; the backend owns API objects and GPU allocations. Avoid dual ownership and hidden static caches.
- Resize, disposal, and submission must be serialized or guarded by a documented state machine. Never destroy a swap chain/surface while an in-flight submission may still reference it.
- Async render calls must define cancellation and shutdown behavior. A cancelled caller must not imply that already-submitted GPU work was cancelled.
- Report backend state explicitly: uninitialized, ready, suspended, lost/faulted, disposing, disposed. Reject submissions in states where they are unsafe.

## Candidate-specific proof checklist

Before choosing a dependency, verify each item against the exact candidate version and intended target framework. This is an investigation checklist, not a claim that any candidate already passes.

| Question | Required evidence |
| --- | --- |
| WPF surface | A real surface created from the existing host without a second window/event loop; ownership and resize behavior documented |
| Runtime support | Exact package versions, target frameworks, OS/architecture matrix, and native runtime assets inspected |
| Licensing | License texts and notices for direct and transitive packages plus native binaries recorded |
| Shaders | Source format, compilation path, runtime compiler or precompiled artifact plan, diagnostics, and distribution footprint |
| Device discovery | Adapter selection, software/fallback detection, device/API/driver reporting |
| Presentation | Actual pixels on a real surface; format, color space, vsync/present behavior recorded |
| Workload | Canonical manifest/hash verified; indexed and instanced draw demonstrated; no duplicate generator |
| Compute | Compute output is consumed by a render pass, or a documented capability blocker and next test |
| Lifetime | Resize, zero-size suspend/resume, repeated submission, revision replacement, device-loss behavior, disposal |
| Measurement | CPU encode/allocation, upload, CPU submission, GPU timestamps where supported, synchronization, present interval, and frame percentiles separated |
| CI split | Headless contract tests distinguished from Windows surface builds and real-hardware evidence |

## Benchmark integrity

The canonical array-first workload and its manifest live in the separate workload PR. The backend must consume those files and verify the declared hashes and byte counts before adapting them. Backend-specific conversion is allowed, but conversion time and bytes must be reported separately. Never rewrite the canonical source data to suit a candidate.

Use GPU timestamp queries where the API and device support them. If unavailable, mark GPU execution timing unavailable; do not substitute queue-submit duration. Keep CPU submission duration, upload duration, GPU execution, synchronization wait, presentation interval, and end-to-end frame time as separate measurements. Report sample count and p50/p95/p99 rather than only a best frame.

## Evidence levels

- **Level A — contract/CI:** deterministic tests for descriptor validation, byte layout, resource revisions, lifecycle state transitions, and disposal policy. Does not claim pixels.
- **Level B — host build:** Windows CI compiles the host adapter and executable proof. Does not claim the workflow ran on a physical GPU.
- **Level C — device proof:** sample ran on a named API/device/driver/runtime; actual pixels and workload hash recorded; resize and disposal exercised.
- **Level D — performance evidence:** Level C plus repeatable workload runs, measurement definitions, sample counts, percentiles, and hardware/software environment.

A green headless build is Level A. A successful Windows compile is Level B. Do not label either a GPU-rendering or performance pass.

## Next concrete task

Audit the exact current stable releases, target frameworks, runtime assets, dependency trees, license notices, shader tooling, and WPF surface integration for Vulkan + Silk.NET, native WebGPU + Silk.NET, and wgpu-native/WGPU.NET. Then implement a disposable one-triangle proof in an isolated adapter/sample project for the strongest native candidate. No package dependency should be added to Visualize core, and no backend should be selected until the real host-surface proof has evidence.

## Related decisions

- [GPU backend implementation plan](GPU_BACKEND_IMPLEMENTATION.md)
- [ADR-0003: GPU backend selection](decisions/ADR-0003-gpu-backend-selection.md)
- [ADR-0004: Native GPU prototype](decisions/ADR-0004-native-gpu-prototype.md)
- [ADR-0005: GPU candidate shortlist](decisions/ADR-0005-gpu-candidate-shortlist.md)
