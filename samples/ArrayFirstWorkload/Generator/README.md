# Array-First Workload Dataset Generator

This .NET 8 console tool turns the shared workload's logical instance format into deterministic, contiguous binary arrays. It is the first executable step toward feeding the same inputs to different GPU backends.

From the repository root:

```powershell
dotnet run --project samples/ArrayFirstWorkload/Generator -- 10000 ./artifacts/array-first-10000
dotnet run --project samples/ArrayFirstWorkload/Generator -- 50000 ./artifacts/array-first-50000 20261012
```

Each output directory contains:

- `vertices.f32le`: three XYZ float32 vertices (36 bytes).
- `indices.u32le`: three uint32 indices (12 bytes).
- `instances.f32le`: one contiguous 80-byte record per instance: 16 row-major float32 transform values (64 bytes), then four float32 color values (16 bytes).
- `dataset-manifest.json`: seed, counts, layouts, byte counts, and SHA-256 hashes for the binary files.

The first 10% of instances (rounded up, at least one) form the near cohort by array index. The remaining records form the far cohort. A fixed xorshift32 generator makes the output deterministic across runtimes; the optional seed is an unsigned 32-bit value (negative integer arguments are accepted and interpreted as their unsigned bit pattern).

The output is a **dataset**, not a benchmark. This generator does not create a graphics device, upload resources, draw, dispatch compute, or measure CPU/GPU time. Candidate backends must consume these same bytes or document a verified conversion to their physical layout. A later runner must add update traces, correctness checks, GPU timings, lifecycle handling, and repeatable measurements before any backend selection.
