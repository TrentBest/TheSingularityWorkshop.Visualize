# WPF Direct3D 12 Surface Proof

This isolated Windows-only sample validates the first native graphics milestone: a WPF window hosts a native child HWND, Direct3D 12 creates a DXGI swap chain for that HWND, and the application repeatedly clears and presents real GPU back buffers.

## Run

On Windows with the .NET 8 SDK and a Direct3D 12-capable device:

```powershell
dotnet run --project samples/WpfD3D12Proof/WpfD3D12Proof.csproj --configuration Release
```

The sample uses Vortice.Direct3D12 3.8.3 behind this proof-only project. The dependency is intentionally isolated from the backend-neutral Visualize core. The version is listed as compatible with .NET 8, but dependency size, redistribution contents, and the exact package graph still need measurement/audit.

## What it proves

- WPF can host a native child HWND through HwndHost.
- A Direct3D 12 device, direct command queue, descriptor heap, command allocator/list, fence, and DXGI swap chain can be created for that surface.
- A GPU clear is submitted and presented repeatedly.
- Resize recreates swap-chain back buffers.
- GPU work is synchronized before allocator reuse and disposal.

## What it does not prove

- No indexed geometry is drawn yet.
- This deliberately waits for GPU completion per frame for correctness; that serializes CPU/GPU work and is not a performance implementation.
- The sample is not yet wired to VisualizerFrame, Renderer, Gestures, or AnyApp.
- Device-loss recovery, DPI-specific sizing, multi-buffer frame scheduling, GPU validation-layer diagnostics, and benchmark capture remain future work.

The next milestone is a known indexed triangle with a measured, non-blocking frame-resource strategy, followed by an adapter into the Visualize backend contract.
