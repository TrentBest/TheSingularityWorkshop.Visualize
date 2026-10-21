# VisualizerFrameResolver.cs

**User moniker:** Visualize GPU-frame validation boundary

## Purpose

Resolve each immutable `VisualizerFrame` command against a host-owned `IVisualizerResourceProvider` before a native submission layer consumes it. The resolver validates resource existence and indexed geometry/instance ranges, returning resolved commands in the same order as the frame.

## Source and executable evidence

- Implementation: [src/GPU/VisualizerFrameResolver.cs](../../src/GPU/VisualizerFrameResolver.cs)
- Tests: [VisualizerFrameResolverTests.cs](../../tests/TheSingularityWorkshop.Visualize.Tests/VisualizerFrameResolverTests.cs)
- GPU contract and release boundaries: [Release readiness](../RELEASE_READINESS.md)

## Class card

| Element | Contract |
|---|---|
| Type | `public static class VisualizerFrameResolver` |
| Entry point | `Resolve(VisualizerFrame, IVisualizerResourceProvider)` |
| Input | Immutable frame plus host-owned geometry, material, and optional instance snapshots |
| Output | Read-only ordered list of `ResolvedVisualizerDrawCommand` |
| Failure behavior | Null arguments raise `ArgumentNullException`; missing resources or out-of-range geometry/instance data raise `InvalidOperationException` with command-indexed diagnostics |
| Ownership | Does not allocate native GPU resources or cache native handles |

## Observed constraints

- Geometry and material resources are resolved for every command.
- The index interval is checked against the geometry index count, then each selected index plus `BaseVertex` is checked against the vertex count using a widened integer calculation.
- Instance resources are resolved only when the command's instance identifier is valid; the requested instance interval must fit the resource.
- The output preserves frame command order.
- Resolution is a CPU-side validation step. It does not submit work to a GPU or prove a Vulkan, Direct3D, or WebGPU backend is executing.

## Use case

A backend adapter calls `Resolve` before translating frame commands into native API calls. Invalid references fail at a deterministic boundary with the offending command index, instead of becoming opaque native submission errors.

## UML

See [VisualizerFrameResolverUML.md](VisualizerFrameResolverUML.md) for the dedicated class diagram.

## Evidence boundary

This page describes the implementation and links to its test suite. Release claims about hardware-GPU execution, performance, or production scheduling require separate reproducible evidence.
