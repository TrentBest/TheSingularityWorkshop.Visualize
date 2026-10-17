# ADR-0005: GPU Candidate Shortlist Before a Native Proof

- **Status:** Investigation snapshot; no backend selected.
- **Date:** 2026-10-17
- **Scope:** Narrow the next experiment without adding a graphics dependency to Visualize.GPU or extending the Direct3D 12 sample.

## Decision

Do not add a graphics binding yet. Start the native comparison with **Vulkan through Silk.NET** and compare it with a native .NET WebGPU/wgpu path. Keep browser WebGPU as a separate JavaScript/browser-host implementation; Silk.NET is a .NET binding and does not itself turn into the browser's WebGPU API. The existing Direct3D 12 sample remains an isolated reference only.

This is a shortlist for a small proof, not a permanent backend choice. The deciding evidence must come from the same array-first workload and the actual WPF/browser host constraints.

## Initial evidence from upstream project documentation

| Candidate | Evidence supporting investigation | Unresolved costs / risks | Current disposition |
| --- | --- | --- | --- |
| Vulkan + Silk.NET | The NuGet gallery lists **2.23.0** as a stable published version (last updated 2026-01-23); Silk.NET is MIT-licensed and under the .NET Foundation. [Project](https://github.com/dotnet/Silk.NET), [NuGet package](https://www.nuget.org/packages/Silk.NET.Vulkan) | Explicit device, swap-chain, synchronization, shader, and lifetime work; WPF surface integration and deployment of native dependencies must be proven. Silk.NET 3.x package feeds contain build/prerelease labels, so do not select a version from the latest label alone. | First native candidate to evaluate; 2.23.0 is an audit baseline only, not an approved dependency. |
| Native WebGPU + Silk.NET / wgpu-native | NuGet lists Silk.NET.WebGPU and Silk.NET.WebGPU.Native.WGPU 2.23.0 as published packages; upstream wgpu-native has its own release and native-asset matrix. [WebGPU bindings](https://github.com/dotnet/Silk.NET/tree/main/src/WebGPU), [Silk.NET.WebGPU](https://www.nuget.org/packages/Silk.NET.WebGPU), [Native WGPU package](https://www.nuget.org/packages/Silk.NET.WebGPU.Native.WGPU), [wgpu-native releases](https://github.com/gfx-rs/wgpu-native/releases) | Exact binding-to-native ABI compatibility, binary deployment, licensing/notices, and WPF surface creation must be verified together. The browser's JavaScript WebGPU path is separate and should not depend on these native packages. | Native .NET comparator only; package versions are audit candidates, not a compatibility claim or selection. |
| wgpu-native via WGPU.NET | The project describes cross-platform raw bindings and optional wrappers around wgpu-native, and documents that window creation is outside its scope. [Project](https://github.com/Trivaxy/WGPU.NET) | Release cadence, native binary distribution, supported target matrix, maintenance, and package/transitive dependency footprint require direct verification before adoption. | Keep as a comparator, not a dependency. |
| Direct3D 12 / Vortice | An isolated WPF clear/present sample already exists in this repository. | Windows-specific; the sample does not draw Visualize geometry or prove the target architecture. It waits for GPU completion each frame and has no real-hardware capture recorded. | Do not extend by default; retain as a baseline/reference only. |

This table is a desk-research snapshot, not a completed legal, dependency, or runtime audit. Upstream project statements do not establish that a specific package version, native binary, or license combination is suitable for distribution. Verify exact versions, transitive packages, native assets, and license notices at the point a proof dependency is proposed.

## Proof gate before choosing a backend

A candidate is eligible for a decision only after it can demonstrate:

1. Create a device and a real host surface, report API/device/driver/runtime details, and present actual pixels.
2. Consume the canonical workload files and verify their manifest hashes and byte counts before upload; do not regenerate equivalent-looking data in the adapter.
3. Upload contiguous vertex/index/instance data and issue indexed plus instanced drawing.
4. Dispatch a compute workload whose output is consumed by a render pass, or record a concrete blocker with an alternative test plan.
5. Exercise resize, repeated submissions, resource revision changes, synchronization, and deterministic disposal.
6. Separate CPU conversion/allocation, upload, CPU submission, GPU execution, synchronization stalls, presentation interval, and frame-time percentiles. Queue-submit call duration is not GPU execution time.
7. Record what CI verifies versus what requires a real device. A headless build or mocked adapter is not evidence of rendered pixels.

The canonical benchmark remains the array-first workload in the separate workload PR. Do not duplicate its generator or loosen its hashes for a backend-specific format; if an adapter requires a conversion, report that cost separately.

## Next implementation step

Build a small **candidate audit and surface-lifecycle proof plan** first. The lifecycle and evidence checklist is now recorded in [GPU Surface Lifecycle Plan](../GPU_SURFACE_LIFECYCLE_PLAN.md). Confirm the intended desktop and browser support matrix, WPF surface ownership/lifecycle constraints, package and native-runtime contents, shader tooling, debugging support, and CI versus hardware-only evidence. Then select one proof dependency in an isolated adapter/sample project; keep API-specific types out of Visualize core and the backend-neutral GPU contracts.

No dependency, API commitment, package publication, or merge is authorized by this ADR.
