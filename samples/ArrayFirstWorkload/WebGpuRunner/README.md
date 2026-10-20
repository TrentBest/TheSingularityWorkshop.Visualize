# Array-First WebGPU Runner

This runner uploads and draws the **same binary arrays emitted by the .NET dataset generator**. It is a live browser WebGPU indexed-instancing scene with visibly distinct near/far cohorts, not the earlier procedural triangle proof.

## Run locally

From the Visualize repository root:

```powershell
dotnet run --project samples/ArrayFirstWorkload/Generator -- 10000 ./artifacts/workload-10000
py -m http.server 8080
```

Open [http://localhost:8080/samples/ArrayFirstWorkload/WebGpuRunner/](http://localhost:8080/samples/ArrayFirstWorkload/WebGpuRunner/) and leave the dataset directory as `/artifacts/workload-10000`. Generate a 1k or 50k dataset and change the directory field to test another population.

## Use actual Renderer decisions

The runner can optionally consume the versioned JSON trace produced by the Renderer repository's `samples/RendererArrayFirstWorkloadTrace` sample.

1. From the Renderer repository root, generate the trace:
   ```powershell
   dotnet run --project samples/RendererArrayFirstWorkloadTrace -- artifacts/renderer-workload-trace.json
   ```
2. Copy that generated file into the Visualize repository's served `artifacts` directory (create the directory if necessary). Do not commit generated binary datasets or traces unless a specific reproducible fixture is needed.
3. In the runner's **Renderer decision trace (JSON URL)** field, enter `/artifacts/renderer-workload-trace.json`, then load the dataset.

The runner validates the trace schema and ordered frame records. It uses the selected `Horizon` to identify the near or far cohort and uploads that cohort only when Renderer reports `RefreshRequired` and `RefreshAccepted`. Frames without an accepted refresh leave the current GPU instance data in place, matching the trace's cache-reuse intent. Leave the trace field empty to compare against the original sample-owned cadence.

**Important evidence boundary:** the trace generator exercises the actual Renderer coordinator and bucket policy, but simulates refresh completion immediately; it does not produce geometry. The browser consumes those policy decisions and applies them to the existing near/far GPU cohorts, but the current trace is synthetic deterministic input, not a live cross-process Renderer instance. This establishes a reproducible policy-to-upload bridge, not yet live world state, a native-vs-browser benchmark, GPU timer-query data, or physical transfer volume.

## Dataset verification

The runner loads `dataset-manifest.json`, then fetches `vertices.f32le`, `indices.u32le`, and `instances.f32le`. It checks schema version, cohort counts, byte order, stride, declared vertex/index/instance counts, binary lengths, index bounds, and each binary array's SHA-256 against the manifest before creating GPU buffers. A size-correct but altered dataset or out-of-range index is rejected.

## Executed workload

- Without a Renderer trace, the near cohort is updated/uploaded every animation frame and the far cohort every 12 frames.
- With a Renderer trace, cohort uploads follow the trace's actual Renderer refresh/acceptance decisions; unchanged cohorts keep their last GPU data.
- Both cohorts use the same 80-byte instance records as the generated dataset; there is no second instance generator.
- The scene visually distinguishes near and far instances. Near records render larger with a cyan tint and far records smaller with a violet tint.
- The page reports cumulative near upload bytes, far refresh count, far upload bytes, command submissions, frame-interval p50/p95, and JavaScript-side encode/submit-call p50/p95 over a rolling sample window. Frame intervals include browser scheduling; JS submit time ends when `queue.submit` returns. Neither is GPU execution time.
- `Upload avoided vs full update` compares bytes submitted by this sample with a simple every-instance-every-frame baseline. It is not a measurement of physical bus traffic or a performance claim.
- Reloading the dataset cancels the previous animation loop, releases its buffers, unconfigures the canvas context, and destroys the previous device before starting another run. Failed initialization also releases partially created resources.
- One indexed instanced draw consumes the uploaded instance storage buffer each frame.

## Current scope and limitations

- The sample uses a simple XY screen mapping, not a camera/view/projection system.
- It does not yet run compute-to-render, selective dependency invalidation on real scene resources, resize-resource lifecycle measurements, or GPU timer queries.
- Browser/device, driver, resolution, present mode, and GPU timing need a repeatable harness before performance comparisons.
- The 80-byte format is the logical reference layout; the WGSL storage struct must stay aligned to that stride.

Use this as the first end-to-end GPU consumer of shared workload bytes and the first policy-driven near/far upload-cadence experiment. Do not select a backend or infer production performance from this proof alone.
