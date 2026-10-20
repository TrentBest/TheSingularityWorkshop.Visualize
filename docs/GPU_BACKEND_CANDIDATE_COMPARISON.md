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

**Do not add a graphics dependency to Visualize core and do not publish `Visualize.GPU` yet.** There are already two relevant open draft workstreams: [Visualize.GPU foundation PR #15](https://github.com/TrentBest/TheSingularityWorkshop.Visualize/pull/15) and [shared array-first workload PR #6](https://github.com/TrentBest/TheSingularityWorkshop.Visualize/pull/6). Treat them as coordinated parts of one GPU program, not competing architectures.

The next experiment should reuse the existing deterministic array-first workload rather than start with an unrelated triangle demo:

1. Revalidate the shared workload generator, byte layout, manifest counts/hashes, index bounds, and the browser WebGPU runner in PR #6. Capture the exact source revision and runtime/browser/device metadata for any manual run; CI syntax/build success alone is not runtime GPU evidence.
2. Replace the sample-owned near/far refresh heuristic with actual Renderer observer/horizon/bucket invalidation and reuse decisions, preserving the same workload arrays and deterministic observer trace. Keep Renderer core independent of Visualize and graphics APIs.
3. Feed the resulting platform-neutral frame/resource descriptions through Visualize's existing resource planner and GPU foundation. Resolve logical geometry/material/instance IDs to actual data; never treat ID preservation as resource transfer.
4. Use the browser WebGPU path as the first shared-workload GPU execution target where runtime support exists, with known-output checks/readback where the implementation supports it. Record unavailable hardware-dependent checks as skipped/not-run, not passed.
5. Only after this end-to-end path is reproducible, implement an equivalent native consumer of the exact same arrays and observer trace. Compare Vulkan, native WebGPU/wgpu, or another suitable native option against host compatibility, runtime capability, integration cost, correctness, packaging, and measured performance—not familiarity or vendor identity.
6. Keep offscreen pixel readback, actual host presentation, compute-to-render, device-loss/resource recreation, and physical-GPU benchmarks as separate evidence gates.

This ordering gives **both Renderer and Visualize** an immediate common workload: Renderer owns which work is due, invalidated, reused, or omitted; Visualize owns resource resolution, GPU execution, and presentation; the host owns surface lifecycle. The GPU can later accelerate eligible data-parallel Renderer workloads through an explicit neutral compute boundary, without making Renderer depend on Visualize or forcing semantic policy onto the GPU. That extension should follow a measured workload and a concrete contract, not a premature API expansion.

No permanent backend is selected. Browser WebGPU and native graphics APIs are distinct host execution paths over the same logical workload. A software adapter can establish limited correctness evidence but cannot establish physical-GPU performance.

## Explicit non-claims

- No GPU output or pixel readback has been produced by this research.
- No physical GPU, adapter, driver or native runtime was tested by this comparison.
- No benchmark, speedup, zero-copy upload or performance claim is supported.
- Passing a CI build or running on a software adapter would establish only the specific correctness gate it actually exercises.

## Sources and freshness

Package version, archive size and license details above were observed on NuGet pages on 2026-10-20. Recheck versions and license/deployment details at implementation time; package metadata is not a substitute for auditing the resolved runtime graph.
