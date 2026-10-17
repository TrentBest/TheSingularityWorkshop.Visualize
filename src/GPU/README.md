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
