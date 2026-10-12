# Array-First GPU Workload Contract

**Status: deterministic dataset generation is implemented; GPU execution and benchmarking are not.** This contract is shared ground for native and browser candidates so we do not choose a backend by comparing a clear-only surface with a triangle-only demo.

## Generate common input arrays

The [dataset generator](Generator/README.md) creates deterministic contiguous vertex, index, and instance arrays from a fixed seed. It emits SHA-256 hashes and a manifest so candidate runs can verify that they used equivalent input bytes.

```powershell
dotnet run --project samples/ArrayFirstWorkload/Generator -- 1000 ./artifacts/workload-1000
dotnet run --project samples/ArrayFirstWorkload/Generator -- 10000 ./artifacts/workload-10000 20261012
dotnet run --project samples/ArrayFirstWorkload/Generator -- 50000 ./artifacts/workload-50000 20261012
```

The binary layout is documented in the generator README. It is a logical reference layout; graphics APIs may require a physical layout conversion, which must be recorded and checked.

## Required profiles

The adjacent `workload.json` is the machine-readable target profile definition.

1. **Static scale:** same indexed geometry, growing contiguous instance array. Record 1k, 10k, and 50k instances.
2. **Near-field update:** update a bounded near cohort every frame while leaving the far cohort unchanged. Record bytes prepared, bytes submitted for upload, and whether the far resource was reused.
3. **Far-field cadence:** keep the far representation valid across frames where its refresh cadence is not due. Record reuse and refresh decisions separately from frame presentation.
4. **Invalidation:** change one source revision and prove only dependent representations are refreshed; unrelated valid resources remain reusable.
5. **Compute-to-render (required before backend selection):** write a deterministic per-instance or visibility result in a compute pass, then consume that output in the same frame's render pass. Record synchronization and read/write transitions.
6. **Resize / lifecycle:** resize and recreate presentation resources without rebuilding unchanged world arrays unnecessarily.

## Data and equivalence rules

- Generate data from the same documented seed and profile values on every backend; compare dataset hashes before comparing results.
- Keep geometry, indices, transforms/colors, and cohort membership contiguous. Do not use one draw submission per instance for the instanced profile.
- Use identical geometry, instance count, near/far membership, update cadence, observer changes, and invalidation events across candidates.
- Distinguish bytes prepared by the CPU, bytes submitted to an API, and physical transfer volume (the last may be unavailable).
- Preserve a CPU reference for generated values and visibility decisions. Compare output/counters for correctness before timing.
- Report warm-up separately; collect frame-time percentiles (p50/p95/p99), CPU preparation/submission time, GPU execution time when timer queries are available, allocation volume, resource memory, and dropped/late frames.
- Include hardware, OS, driver, runtime, browser/API version, build configuration, resolution, vsync/present mode, and power mode with each result.

## Decision guardrails

A passing build is not a runtime proof. A visible frame is not a performance result. FPS displayed by a demo is useful feedback but not a substitute for repeatable timing. Do not choose Vulkan, WebGPU, Direct3D 12, or a cross-backend layer until the same workload has been exercised on real target hardware and package footprint, licensing, diagnostics, host integration, and data movement have been reviewed.

## Remaining limitations

The dataset generator does not create a graphics device, upload resources, draw, dispatch compute, replay update traces, capture GPU timings, or select a backend. Existing browser WebGPU instancing and WPF/D3D12 clear-present experiments are still isolated proofs, not equivalent benchmark implementations. Next: add reproducible update traces and CPU-side validation, then implement actual GPU execution for a candidate while preserving identical inputs for the other candidate.
