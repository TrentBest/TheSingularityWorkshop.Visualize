# GPU Backend Candidate Comparison — 2026-10-20

**Status: research snapshot; no backend selected.** This document narrows the next experiment without turning package popularity or a sample triangle into an architecture decision.

## Evidence reviewed

- [Silk.NET.WebGPU 2.23.0 on NuGet](https://www.nuget.org/packages/Silk.NET.WebGPU/): MIT-licensed binding package; NuGet lists version 2.23.0, last updated 2026-01-23, at about 394 KB for the package archive.
- [Silk.NET.Vulkan 2.23.0 on NuGet](https://www.nuget.org/packages/Silk.NET.Vulkan/): MIT-licensed binding package; NuGet lists version 2.23.0, last updated 2026-01-23, at about 5.36 MB for the package archive.
- [Silk.NET project package feed](https://github.com/orgs/dotnet/packages/nuget/package/Silk.NET): prerelease 3.0 builds are active. The stable 2.23.0 line and prerelease 3.x line therefore need to be evaluated separately; do not silently choose a moving prerelease for the prototype.
- [Silk.NET WebGPU triangle experiment](https://github.com/dotnet/Silk.NET/blob/main/src/Lab/Experiments/WebGPUTriangle/Program.cs): demonstrates adapter/device, WGSL pipeline and surface presentation in a windowed sample. It is a starting reference, not an offscreen pixel-readback proof and not evidence of compatibility with our WPF host.
- [Vulkan documentation](https://docs.vulkan.org/): authoritative API documentation; Vulkan provides native graphics/compute but requires explicit host surface, device, synchronization, resource and pipeline management.
- [wgpu project](https://github.com/gfx-rs/wgpu): implementation family relevant to WebGPU; a .NET integration must still establish exact native runtime packaging, supported platforms, license/redistribution, and host integration.

Package archive size is not deployed runtime footprint. These package metadata snapshots do not establish transitive dependency size, native runtime size, maintenance response time, GPU availability, or actual performance.

## Comparison against Visualize requirements

| Candidate | Native WPF path | Browser path | Array/index/instance work | Compute-to-render | Primary unresolved risks |
| --- | --- | --- | --- | --- | --- |
| **Vulkan + Silk.NET** | Possible through a native surface integration, but WPF/Win32 interop and lifecycle glue must be proved. An offscreen render target can isolate device/pixel correctness first, but does not satisfy the WPF surface gate. | Not a browser API; would be a separate native backend. | Suitable API control for explicit buffers and indexed/instanced drawing; must prove layout, revisions and update costs. | Supported in the API; must be exercised in a real test. | Integration complexity, synchronization/lifetime correctness, shader tooling, driver/device coverage, package and deployment footprint. |
| **WebGPU + Silk.NET / wgpu-native** | A native implementation is possible in principle, but the exact native runtime, surface handles and WPF host path must be proved; a browser sample is not that proof. | Best conceptual fit for the browser host, where browser WebGPU is available. Browser support/capabilities remain runtime-dependent. | WebGPU has buffer and render-pipeline concepts, but the actual native binding must prove the required data paths and performance. | WebGPU exposes compute and render passes; the proof must demonstrate a compute result consumed by rendering and read back. | Native runtime distribution, API/binding version drift, asynchronous device setup, feature differences between browser/native, and WPF surface integration. |
| **Direct3D 12 / Vortice** | The existing WPF sample proves only compile-time viability and has a clear/present loop in source; no indexed geometry, readback evidence, hardware capture, or performance proof. | Not a browser backend. | Possible, but the current sample does not implement these gates. | Possible, but not implemented by the current sample. | Windows lock-in, native deployment, and the existing sample's serial wait-for-GPU behavior. It is explicitly not the default next step. |
| **Higher-level cross-backend renderer** | Depends on its host adapters and control over WPF lifecycle. | Potentially shared renderer model. | Must be checked for batching, resource revisions, direct array paths and escape hatches. | Must be checked for compute capability and access to produced resources. | An abstraction may constrain capabilities, obscure synchronization, add dependencies, or prevent the control Visualize needs. No concrete candidate has yet passed this audit. |

## Decision for the next experiment

**Do not add a graphics dependency to Visualize core and do not create or publish the `Visualize.GPU` package yet.**

Run a small, disposable, opt-in comparison harness before choosing the long-lived backend:

1. Pin the candidate package version and capture the resolved dependency graph and license notices.
2. First prove adapter/device identity and render a deterministic indexed triangle into a GPU-owned offscreen texture; copy the texture to a readback buffer and assert known pixels. This isolates GPU execution and readback from WPF surface complexity.
3. Label the adapter as hardware or software where the API/runtime exposes enough information; retain all raw device/runtime metadata.
4. Only after the offscreen proof passes, add the AnyApp/WPF surface adapter and test resize, presentation, shutdown and repeated create/dispose.
5. Extend the same backend to revision-aware vertex/index/instance uploads, then indexed + instanced drawing, then compute-to-render. Keep browser WebGPU as a separate host validation.
6. Compare implementation effort, reproducibility, device availability, deployment footprint and correctness before choosing a backend. Performance comparison starts only when equivalent workloads run on named hardware.

The first candidate to implement in the harness should be **Vulkan with a stable Silk.NET package**, because it directly tests the required native graphics/compute control without committing Visualize to Direct3D. This is a choice of *first experiment only*, not a permanent backend selection. WebGPU remains the leading browser candidate and should be evaluated independently for WebPage/browser hosting. If the Vulkan offscreen and WPF host costs are disproportionate, record that evidence and evaluate WebGPU/wgpu-native next rather than forcing the initial candidate into the architecture.

## Explicit non-claims

- No GPU output or pixel readback has been produced by this research.
- No physical GPU, adapter, driver or native runtime was tested by this comparison.
- No benchmark, speedup, zero-copy upload or performance claim is supported.
- Passing a CI build or running on a software adapter would establish only the specific correctness gate it actually exercises.

## Sources and freshness

Package version, archive size and license details above were observed on NuGet pages on 2026-10-20. Recheck versions and license/deployment details at implementation time; package metadata is not a substitute for auditing the resolved runtime graph.
