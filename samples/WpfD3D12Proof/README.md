# WPF Direct3D 12 Surface Proof

This isolated Windows-only sample is the first native graphics proof implementation: a WPF window hosts a native child HWND, Direct3D 12 creates a DXGI swap chain for that HWND, and the application repeatedly clears and presents real GPU back buffers.

## Run

On Windows with the .NET 8 SDK and a Direct3D 12-capable device:

```powershell
dotnet run --project samples/WpfD3D12Proof/WpfD3D12Proof.csproj --configuration Release
```

The sample uses Vortice.Direct3D12 3.8.3 behind this proof-only project. The dependency is intentionally isolated from the backend-neutral Visualize core. The version is listed as compatible with .NET 8, but dependency size, redistribution contents, and the exact package graph still need measurement/audit.

## Implementation scope (compile-verified; runtime capture pending)

- The project compiles on the Windows CI runner with zero warnings and errors. A real display/GPU run has not yet been captured, so the following runtime behavior remains to be verified on a Windows host.
- The source contains initialization for a Direct3D 12 device, direct command queue, descriptor heap, command allocator/list, fence, and DXGI swap chain.
- The render loop submits a GPU clear, presents, and recreates swap-chain back buffers on resize.
- Runtime behavior, resize behavior, device compatibility, and clean shutdown still need an actual run and capture.

## What it does not prove

- No indexed geometry is drawn yet.
- This deliberately waits for GPU completion per frame for correctness; that serializes CPU/GPU work and is not a performance implementation.
- The sample is not yet wired to VisualizerFrame, Renderer, Gestures, or AnyApp.
- Device-loss recovery, DPI-specific sizing, multi-buffer frame scheduling, GPU validation-layer diagnostics, and benchmark capture remain future work.

The next milestone is a known indexed triangle with a measured, non-blocking frame-resource strategy, followed by an adapter into the Visualize backend contract.
