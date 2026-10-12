# Array-First GPU Workload Contract

**Status: workload definition; not yet an executable benchmark.** This contract is shared ground for the native WPF path and the browser/WebGPU path. It prevents a backend decision from being made by comparing a clear-only surface with a triangle-only demo.

## What this workload must prove

The target is the Workshop's actual data flow: Renderer selects observer-relative representations and freshness; Visualize consumes compact geometry and instance arrays; the backend uploads or reuses GPU resources, executes indexed instancing, and presents the result. A later phase adds compute output consumed by rendering. The workload does not replace Renderer or FSM_API and does not make GPU state authoritative.

## Required profiles

The adjacent `workload.json` is the machine-readable profile definition.

1. **Static scale:** same indexed geometry, growing contiguous instance array. Record 1k, 10k, and 50k instances.
2. **Near-field update:** update a bounded near cohort every frame while leaving the far cohort unchanged. Record bytes prepared, bytes submitted for upload, and whether the far resource was reused.
3. **Far-field cadence:** keep the far representation valid across frames where its refresh cadence is not due. Record reuse and refresh decisions separately from frame presentation.
4. **Invalidation:** change one source revision and prove only dependent representations are refreshed; unrelated valid resources remain reusable.
5. **Compute-to-render (required before backend selection):** write a deterministic per-instance or visibility result in a compute pass, then consume that output in the same frame's render pass. Record synchronization and read/write transitions.
6. **Resize / lifecycle:** resize and recreate presentation resources without rebuilding unchanged world arrays unnecessarily.

## Data and equivalence rules

- Generate geometry and instances deterministically from the same documented seed and profile values on every backend.
- Keep geometry, indices, instance transforms/colors, and visibility/update membership in contiguous arrays. No per-object draw submission for the instanced profile.
- Use identical geometry, instance count, near/far membership, update cadence, observer changes, and invalidation events across candidates.
- Declare all byte counts and formats. Distinguish bytes prepared by the CPU, bytes submitted to an API, and physical transfer volume (the last may be unavailable).
- Preserve a CPU reference for generated values and visibility decisions. Compare output/counters for correctness before timing.
- Report warm-up separately; collect frame-time percentiles (p50/p95/p99), CPU preparation/submission time, GPU execution time when timer queries are available, allocation volume, resource memory, and dropped/late frames.
- Include hardware, OS, driver, runtime, browser/API version, build configuration, resolution, vsync/present mode, and power mode with each result.

## Decision guardrails

A passing build is not a runtime proof. A visible frame is not a performance result. FPS displayed by a demo is useful feedback but not a substitute for repeatable timing. Do not choose Vulkan, WebGPU, Direct3D 12, or a cross-backend layer until the same workload has been exercised on real target hardware and package footprint, licensing, diagnostics, host integration, and data movement have been reviewed.

## Current limitations

This file defines the comparison contract only. It does not yet generate buffers, execute a GPU workload, capture timings, or select a backend. The existing browser WebGPU instancing proof and WPF/D3D12 clear-present experiment are useful isolated experiments but are not equivalent benchmark implementations. The next implementation step is to make each candidate consume the same deterministic arrays and profiles, then record results rather than infer them.
