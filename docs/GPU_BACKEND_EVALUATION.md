# GPU backend evaluation gate

**Status: evaluation plan only. No backend is selected.**

See the [dated candidate comparison](GPU_BACKEND_CANDIDATE_COMPARISON.md) for current binding/package evidence and the rationale for the proposed first experiment.

The first GPU path must prove correctness before speed. A surface-clearing demo is a useful host/device smoke test, but it is not enough to validate Visualize's array-first architecture or make performance claims.

## Candidate shortlist

| Candidate | Why evaluate it | What must be verified |
| --- | --- | --- |
| **WebGPU / WGSL** | Relevant to browser presentation and available through native implementations/bindings; one shader/data model may be reusable across browser and native hosts. | Native binding and runtime maintenance, browser host integration, adapter/device availability, readback, compute-to-render, and package/runtime footprint. Browser and native APIs are related, but their host integration is not interchangeable. |
| **Vulkan** | Cross-platform, cross-vendor native graphics and compute with explicit control over memory, queues, and synchronization. | Surface integration for desktop hosts, setup/maintenance cost, device/driver coverage, and whether its extra control produces measurable value for our workload. |
| **Direct3D 12** | Existing isolated Windows surface experiment can inform host-lifecycle requirements. | Windows-only portability boundary and package/deployment cost. Do not extend the existing proof by default; it is not selected. |

Research starting points:
- [Official Vulkan documentation](https://docs.vulkan.org/) describes Vulkan as a cross-platform graphics and compute API.
- [Silk.NET](https://github.com/dotnet/Silk.NET) exposes Vulkan and WebGPU bindings. Its project notes that 2.x maintenance investment is limited while 3.0 development proceeds, so package/version and native-runtime maintenance must be an explicit dependency gate.
- [Silk.NET.WebGPU on NuGet](https://www.nuget.org/packages/Silk.NET.WebGPU/) is a concrete native .NET binding candidate, not a backend decision.
- [wgpu-native](https://github.com/gfx-rs/wgpu) provides a native WebGPU implementation and lists .NET bindings, including Silk.NET.

## Required proof ladder

1. **Device identity:** enumerate/request an adapter and device; record adapter/vendor/device/driver/backend when the API exposes them. Distinguish physical hardware from software rendering.
2. **Known output:** render a deterministic indexed triangle or marker into a GPU-owned render target, copy it to a readback buffer, and assert selected pixel values. Save the readback image and metadata as artifacts. A CPU-generated SVG does not satisfy this gate.
3. **Array-first resources:** upload known vertex/index arrays, render multiple entities, then update only a changed range/revision. Record bytes transferred and resource creations.
4. **Compute-to-render:** dispatch a small compute workload whose GPU-produced output is consumed by a render pass. Verify output through readback.
5. **Observer-driven workload:** feed the same deterministic scene and observer transitions used by Renderer proofs. Keep representation decisions upstream and compare equivalent visible work.
6. **Repeatable benchmark:** warm up separately, repeat enough frames to report median/p95/p99 CPU submission and GPU pass times, allocations, transfer volume, synchronization stalls, resource churn, output correctness, and cold-start cost.
7. **Hardware confirmation:** run the same tagged workload on a named physical GPU/device/driver and preserve raw measurements and screenshots/readback. CI success on a software adapter is useful correctness evidence, not physical-GPU performance evidence.

## Current bulk-data boundary

`VisualizerGeometry` and `VisualizerInstanceBuffer` now expose `ReadOnlyMemory<T>` views (`VertexMemory`, `IndexMemory`, and `InstanceMemory`) over the same private snapshots as their existing read-only list properties. This gives a backend a contiguous bulk-data path without requiring it to enumerate the list API or make another copy just to obtain memory.

This is an API boundary improvement, **not yet a GPU upload implementation or a zero-copy guarantee**. Construction still snapshots caller data; the GPU backend must validate byte layout and alignment, decide when revision changes require uploads, measure conversion and transfer costs, and prove that unchanged revisions do not trigger redundant resource creation or upload. Do not assume `Memory<T>` means the data can be used directly by a device or pinned safely for arbitrary GPU lifetime.

## Acceptance rules

- Keep the backend implementation behind Visualize's backend boundary; do not add graphics API types to Renderer or FSM_API.
- Keep the first prototype isolated and opt-in. Do not add a runtime dependency to the core package until portability, license, deployment footprint, and host lifecycle are reviewed.
- Compare the same scene, resolution, visible detail, workload seed, and output correctness.
- Never publish an FPS or speedup claim without raw data, named hardware/backend, source revision, and reproducible commands.
- A successful device/pixel proof establishes GPU execution correctness only. It does not establish that the architecture is performant.
