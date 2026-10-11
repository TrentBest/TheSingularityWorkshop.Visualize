# Isolated WPF / Direct3D 12 Surface Experiment

**Status: exploratory only. Direct3D 12 has not been selected as Visualize's graphics backend.**

This Windows-only sample is retained as a small native-surface experiment. It hosts a native child HWND in WPF, creates a Direct3D 12 device and DXGI swap chain, and repeatedly clears and presents GPU back buffers. Its existence must not be mistaken for an architectural commitment or a performance result.

## Run the experiment

On Windows with the .NET 8 SDK and a Direct3D 12-capable device:

```powershell
dotnet run --project samples/WpfD3D12Proof/WpfD3D12Proof.csproj --configuration Release
```

The sample uses Vortice.Direct3D12 3.8.3 inside this proof-only project. The package graph, deployed footprint, licensing, redistribution contents, and actual device/runtime behavior have not been fully audited.

## What it demonstrates—and what it does not

- Windows CI compiles the proof with warnings treated as errors.
- The source initializes a Direct3D 12 device, direct queue, descriptor heap, command allocator/list, fence, and DXGI swap chain.
- The render loop clears and presents a GPU back buffer and recreates swap-chain buffers on resize.
- A real hardware/runtime capture has not yet been recorded.
- It does not draw indexed geometry, demonstrate array-first uploads, prove compute-to-render flow, or provide meaningful performance evidence.
- It waits for GPU completion each frame; this serializes CPU/GPU work and is explicitly not performance-ready.
- Device-loss recovery, DPI-specific sizing, multi-buffer frame scheduling, validation-layer diagnostics, and benchmark capture remain unproven.

## Architectural rule

Do not extend this experiment with an indexed triangle or connect it to VisualizerFrame, Renderer, Gestures, or AnyApp by default. First evaluate the GPU backend candidates against Visualize's array-first workload and choose based on portability, data movement, compute-to-render flow, synchronization, host integration, measured performance, and footprint.

See [GPU Backend Strategy](../../docs/GPU_BACKEND_STRATEGY.md) and [ADR-0004](../../docs/decisions/ADR-0004-native-gpu-prototype.md).
