# TheSingularityWorkshop.Visualize.GPU

**Own the GPU-facing path. Keep the world and the host independent of it.**

This project is the GPU execution boundary inside the Visualize repository. It is deliberately separate from the backend-neutral Visualize contracts and has no third-party graphics dependency in its first milestone.

## What

A small C#-friendly layer for device discovery, resource allocation, explicit submission, synchronization, and diagnostics. Native graphics/compute APIs are adapters behind this boundary; they are not the public data model of the Workshop.

## Why

The Workshop already owns its state, scheduling, observer-relative representation, and array-first presentation contracts. The GPU layer must consume contiguous workloads efficiently without forcing those layers to adopt an engine-specific object model or lifetime rules.

We will measure allocation, transfer, submission, synchronization, and end-to-end frame costs separately. “Own GPU-facing technology” does not mean claiming to replace vendor kernel drivers or undocumented hardware interfaces. We own the software architecture and can implement the adapter down to native API and carefully selected unsafe/assembly paths when measurements justify it.

## How

- **Visualize** owns renderer-facing presentation contracts and resource meaning.
- **Visualize.GPU** owns GPU resource lifetimes, device capabilities, transfers, submissions, synchronization, and GPU diagnostics.
- **Native adapters** translate our small contracts to a selected explicit graphics/compute API.
- **The OS and vendor driver** still perform hardware initialization, scheduling, and device-specific operations.

The core contract should stay dependency-light. Optional API adapters may carry platform-specific dependencies in isolated projects. No Unity dependency is introduced.

## Renderer integration boundary

The Renderer-to-Visualize adapter is a command bridge, not a resource-content bridge. Both models use numeric IDs, but `RenderResourceId` and `VisualizerResourceId` are different contracts: the adapter must map them deliberately, not assume equal numbers prove equal ownership or content.

Visualize already defines the resource-resolution seam:

- `IVisualizerResourceProvider` resolves geometry, material, and optional instance data.
- `VisualizerResourceCatalog` is the current thread-safe bootstrap implementation.
- `VisualizerGeometry`, `VisualizerMaterial`, and `VisualizerInstanceBuffer` expose immutable CPU-side snapshots with caller-managed content revisions.
- `IVisualizerFrameAdapter<TSourceFrame>` is the renderer-package boundary; it must preserve command ordering, transforms, index/instance ranges, resource identity mapping, and the representation decision.

Before native submission, the frame consumer must resolve each command's geometry and material, and resolve its instance resource whenever `Instances.IsValid`. It must validate requested index and instance ranges against the resolved snapshots. A missing resource or invalid range must produce a useful diagnostic; it must not silently allocate an empty placeholder.

Keep responsibilities separate:

- **Renderer** selects observer-relative representations and emits platform-neutral draw commands.
- **Renderer.VisualizeAdapter** maps source command fields and logical identities without owning native GPU handles.
- **Visualize resource provider / host** owns the CPU-side descriptions, revisions, and provider lifetime.
- **Visualize.GPU adapter** owns native allocations, uploads, synchronization, and submission. GPU copies are derived cache entries, keyed by stable resource identity plus revision, and never authoritative world state.

The native proof must exercise geometry, material, and instanced draws; verify resource revision changes trigger the required upload/update; and deliberately test missing resources and out-of-range index/instance ranges. Numeric ID coincidence alone is not an acceptance test.

## First proof

1. Enumerate the selected physical/logical device and report its capabilities.
2. Allocate and dispose a GPU buffer with explicit size, usage, and lifetime.
3. Upload one contiguous array and submit one indexed draw through the existing Visualize resource/frame contracts.
4. Present actual pixels to a real host surface.
5. Verify resize, repeated submission, device/resource teardown, and diagnostics.
6. Record the backend, adapter/device, driver/runtime versions, build configuration, and measurements.

A clear-screen test is useful for bring-up, but it is not the end-to-end proof. A successful milestone must resolve real Visualize geometry, material, and instance data into GPU resources and render a frame.

## Non-goals for this first milestone

- Replacing the operating-system graphics stack or vendor kernel driver.
- Introducing CUDA as a mandatory dependency.
- Making GPU memory authoritative world state.
- Selecting a production graphics API before the adapter requirements and hardware target are verified.
- Claiming performance wins before reproducible measurements exist.

## Status

**Foundation only.** This project establishes the boundary and plan. It does not yet contain a working native GPU backend and does not claim to render pixels.
