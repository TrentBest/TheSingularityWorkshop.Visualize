# Array-First WebGPU Runner

This runner uploads and draws the **same binary arrays emitted by the .NET dataset generator**. It is a real browser WebGPU indexed-instancing path, not the earlier procedural triangle proof.

## Run locally

From the repository root:

```powershell
dotnet run --project samples/ArrayFirstWorkload/Generator -- 10000 ./artifacts/workload-10000
py -m http.server 8080
```

Open [http://localhost:8080/samples/ArrayFirstWorkload/WebGpuRunner/](http://localhost:8080/samples/ArrayFirstWorkload/WebGpuRunner/) and leave the dataset directory as `/artifacts/workload-10000`. Generate a 1k or 50k dataset and change the directory field to test another population.

The runner loads `dataset-manifest.json`, then fetches `vertices.f32le`, `indices.u32le`, and `instances.f32le`. It checks schema version, cohort counts, byte order, stride, and binary lengths before creating GPU buffers.

## Executed workload

- The near cohort is updated and uploaded every animation frame.
- The far cohort remains cached between refreshes and is updated/uploaded every 12 frames.
- Both cohorts use the same 80-byte instance records as the generated dataset; there is no second instance generator.
- The page reports cumulative near upload bytes, far refresh count, far upload bytes, and GPU command submissions. These are work counters, **not** timing or performance claims.
- One indexed instanced draw consumes the uploaded instance storage buffer each frame.

This makes the cadence executable rather than merely describing it in the workload contract. It is still a focused harness: the cadence is currently a deterministic sample policy, not yet wired to the Renderer Event Horizon bucket/dependency APIs.

## Current scope and limitations

- The current sample uses a simple XY screen mapping, not a camera/view/projection system.
- It does not yet run compute-to-render, selective dependency invalidation, resize-resource lifecycle measurements, or GPU timer queries.
- Browser/device, driver, resolution, present mode, and GPU timing need a repeatable harness before performance comparisons.
- The 80-byte format is the logical reference layout; the WGSL storage struct must stay aligned to that stride.

Use this as the first end-to-end GPU consumer of shared workload bytes and the first executable near/far upload-cadence experiment. Do not select a backend or infer production performance from this proof alone.
