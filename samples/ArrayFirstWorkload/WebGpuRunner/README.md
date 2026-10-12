# Array-First WebGPU Runner

This runner uploads and draws the **same binary arrays emitted by the .NET dataset generator**. It is a live browser WebGPU indexed-instancing scene with visibly distinct near/far cohorts, not the earlier procedural triangle proof.

## Run locally

From the repository root:

```powershell
dotnet run --project samples/ArrayFirstWorkload/Generator -- 10000 ./artifacts/workload-10000
py -m http.server 8080
```

Open [http://localhost:8080/samples/ArrayFirstWorkload/WebGpuRunner/](http://localhost:8080/samples/ArrayFirstWorkload/WebGpuRunner/) and leave the dataset directory as `/artifacts/workload-10000`. Generate a 1k or 50k dataset and change the directory field to test another population.

The runner loads `dataset-manifest.json`, then fetches `vertices.f32le`, `indices.u32le`, and `instances.f32le`. It checks schema version, cohort counts, byte order, stride, declared vertex/index/instance counts, binary lengths, index bounds, and each binary array's SHA-256 against the manifest before creating GPU buffers. A size-correct but altered dataset or out-of-range index is rejected.

## Executed workload

- The near cohort is updated and uploaded every animation frame.
- The far cohort remains cached between refreshes and is updated/uploaded every 12 frames.
- Both cohorts use the same 80-byte instance records as the generated dataset; there is no second instance generator.
- The scene visually distinguishes near and far instances. Near records render larger with a cyan tint and move/upload every frame; far records render smaller with a violet tint and reuse their last GPU data between 12-frame refreshes.
- The page reports cumulative near upload bytes, far refresh count, far upload bytes, command submissions, frame-interval p50/p95, and JavaScript-side encode/submit-call p50/p95 over a rolling sample window. Frame intervals include browser scheduling; JS submit time ends when `queue.submit` returns. Neither is GPU execution time.
- `Upload avoided vs full update` compares actual near/far upload bytes with a simple baseline that uploads every instance record every frame. It is an arithmetic comparison of bytes submitted by this sample, not a measurement of physical bus traffic or a performance claim.
- Reloading the dataset cancels the previous animation loop, releases its buffers, unconfigures the canvas context, and destroys the previous device before starting another run. Failed initialization also releases partially created resources, and the load control is disabled while initialization is in progress.
- One indexed instanced draw consumes the uploaded instance storage buffer each frame.

This makes the cadence executable rather than merely describing it in the workload contract. It is still a focused harness: the cadence is currently a deterministic sample policy, not yet wired to the Renderer Event Horizon bucket/dependency APIs.

## Current scope and limitations

- The current sample uses a simple XY screen mapping, not a camera/view/projection system.
- It does not yet run compute-to-render, selective dependency invalidation, resize-resource lifecycle measurements, or GPU timer queries.
- Browser/device, driver, resolution, present mode, and GPU timing need a repeatable harness before performance comparisons.
- The 80-byte format is the logical reference layout; the WGSL storage struct must stay aligned to that stride.

Use this as the first end-to-end GPU consumer of shared workload bytes and the first executable near/far upload-cadence experiment. Do not select a backend or infer production performance from this proof alone.
