# Array-First WebGPU Runner

This runner uploads and draws the **same binary arrays emitted by the .NET dataset generator**. It is a real browser WebGPU indexed-instancing path, not the earlier procedural triangle proof.

## Run locally

From the repository root:

```powershell
dotnet run --project samples/ArrayFirstWorkload/Generator -- 10000 ./artifacts/workload-10000
py -m http.server 8080
```

Open [http://localhost:8080/samples/ArrayFirstWorkload/WebGpuRunner/](http://localhost:8080/samples/ArrayFirstWorkload/WebGpuRunner/) and leave the dataset directory as `/artifacts/workload-10000`. Generate a 1k or 50k dataset and change the directory field to test another population.

The runner loads `dataset-manifest.json`, then fetches `vertices.f32le`, `indices.u32le`, and `instances.f32le` from that directory. It validates the declared byte counts and 80-byte instance stride, uploads the binary buffers, and submits one indexed instanced draw per animation frame.

## Current scope and limitations

- It consumes the generator's data files directly; there is no duplicated procedural instance generator.
- It renders the transform scale, XY translation, and color from each instance record. The current sample uses a simple screen mapping, not a camera/view/projection system.
- It does not yet run compute-to-render, near/far cadence, selective invalidation, resize-resource lifecycle measurements, or GPU timer queries.
- The displayed submission count is not an FPS measurement or a performance claim. Browser/device, driver, resolution, present mode, and GPU timing need a repeatable harness before comparisons.
- The 80-byte format is the logical reference layout; the WGSL storage struct must stay aligned to that stride.

Use this as the first end-to-end GPU consumer of shared workload bytes. Do not select a backend or infer production performance from this proof alone.
