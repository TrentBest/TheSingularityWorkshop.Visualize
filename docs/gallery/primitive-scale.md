# Gallery recipe: primitive scale

![One point](../../samples/PrimitiveGallery/generated/01-one-point.svg)

![100-point cloud](../../samples/PrimitiveGallery/generated/02-point-cloud-100.svg)

## What this exhibit means

These generated SVGs make a minimal primitive and a larger repeated point set easy to compare. They help inspect the shape of the contract and its output at different counts.

## Source and reproduction

- [Primitive Gallery guide](../../samples/PrimitiveGallery/README.md)
- [Visual primitives](../PRIMITIVES.md)

From the Visualize repository root:

```bash
dotnet run --project samples/PrimitiveGallery/VisualizeSvgProof/VisualizeSvgProof.csproj --configuration Release
```

## Evidence boundary

Visual correctness is not throughput. SVG element count, file size, and apparent smoothness are not GPU draw-call or frame-time measurements. A performance claim needs a controlled workload, warmup, repetitions, allocations, timings, and the selected backend identified.
