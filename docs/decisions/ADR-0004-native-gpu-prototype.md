# ADR-0004: Isolated Direct3D 12 Surface Experiment

- **Status:** Superseded as a backend-selection direction; retained only as a record of an isolated experiment.
- **Date:** 2026-10-11

## Context

Visualize needs an efficient GPU execution path for AnyApp/WPF while preserving a browser/WebGPU path and keeping graphics API details out of the core contracts. The Workshop is data-oriented and array-friendly: the architecture should make bulk geometry, transforms, pose data, visibility, and instance data natural to upload and process, rather than centering the design on per-object CPU submission.

## What was tried

An isolated WPF sample was created using Vortice.Direct3D12. It hosts a native child window and clears/presents a DXGI swap chain. This verifies only that a particular native-surface experiment can be compiled in CI; a real hardware capture is still outstanding. It does not render geometry, establish performance, or demonstrate that Direct3D 12 is the right foundation.

## Decision

**No graphics API has been selected. Do not extend the Direct3D 12 sample by default.** Treat it as a disposable reference experiment, not the architecture or the next mandatory milestone.

The next step is to compare native and browser-capable paths against the same array-first workload. Candidates include Vulkan, WebGPU, and a cross-backend implementation if it preserves the control and data flow Visualize requires. Direct3D 12 may only return to consideration if the comparison gives a reason to do so; its existing implementation is not a reason by itself.

## Evaluation requirements

- Represent hot data as contiguous arrays/streams with explicit layouts and stable resource revisions.
- Measure conversion, allocation, copy, upload, CPU submission, GPU execution, synchronization, and presentation costs separately where practical.
- Include indexed and instanced rendering plus a compute result consumed by a render pass.
- Preserve backend-neutral core contracts and allow backend-specific shaders/capabilities.
- Compare AnyApp/WPF lifecycle and resize behavior with the browser/WebGPU path.
- Audit .NET bindings, transitive packages, runtime footprint, licensing, supported devices, debugging, and shader tooling.
- Record real hardware/device/driver evidence and use equivalent workloads before making performance claims.

## References

- [GPU backend strategy](../GPU_BACKEND_STRATEGY.md)
- [Open questions](../OPEN_QUESTIONS.md)
- [Vulkan](https://www.vulkan.org/)
- [WebGPU specification](https://www.w3.org/TR/webgpu/)
- [wgpu project](https://github.com/gfx-rs/wgpu)
- [WPF and Win32 interoperation](https://github.com/dotnet/docs-desktop/blob/main/dotnet-desktop-guide/wpf/advanced/wpf-and-win32-interoperation.md)
- [Isolated WPF/Direct3D 12 experiment](../../samples/WpfD3D12Proof/README.md)
