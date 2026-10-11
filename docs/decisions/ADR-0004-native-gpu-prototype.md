# ADR-0004: Native GPU Prototype Candidate

- **Status:** Provisional investigation direction; not a permanent backend selection.
- **Date:** 2026-10-11

## Context

The first host is AnyApp/WPF, and the project targets .NET 8. We need a real GPU-backed surface and must keep the core free from WPF/native handles. A browser/WebGPU path must remain feasible.

## Investigation findings

- Silk.NET.Direct3D12 2.23.0 is a stable published .NET Standard-compatible binding and is a plausible candidate for a .NET 8 native prototype.
- Current Vortice.Windows package targeting must be checked carefully against the Workshop's .NET baseline; current versions may target newer .NET versions.
- WPF can host a native child HWND through HwndHost, which offers a plausible place to attach a DXGI swap chain without representing scene objects as WPF controls.
- A browser surface must use browser-compatible GPU APIs. Native Direct3D 12 handles cannot be reused directly in the browser.

## Decision

Prototype Direct3D 12 as the first native candidate using a maintained .NET binding isolated behind a backend-specific project. Prototype browser WebGPU separately. Do not claim D3D12 is the final universal API, and do not commit to a binding until the actual package footprint and host integration are measured.

## Required evidence

- clear-and-present on a real WPF-hosted GPU surface;
- resize and clean shutdown;
- indexed draw;
- resource upload and device-resource lifetime;
- compute-to-draw path;
- measured CPU/GPU timings and package/runtime footprint;
- documented device/driver requirements.

## References

- [Silk.NET.Direct3D12 2.23.0](https://www.nuget.org/packages/Silk.NET.Direct3D12/2.23.0)
- [Vortice.Direct3D12](https://www.nuget.org/packages/Vortice.Direct3D12)
- [WPF and Win32 interoperation](https://github.com/dotnet/docs-desktop/blob/main/dotnet-desktop-guide/wpf/advanced/wpf-and-win32-interoperation.md)
- [Silk.NET Direct3D 11 example](https://github.com/dotnet/Silk.NET/blob/main/examples/CSharp/Direct3D11%20Tutorials/Tutorial%201.3%20-%20Textures/Program.cs)
