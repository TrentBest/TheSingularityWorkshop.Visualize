# TheSingularityWorkshop.Visualize

[![Build Status](https://img.shields.io/github/actions/workflow/status/TrentBest/TheSingularityWorkshop.Visualize/build.yml?branch=development&style=flat-square&logo=github)](https://github.com/TrentBest/TheSingularityWorkshop.Visualize/actions/workflows/build.yml)
[![Coverage](https://img.shields.io/codecov/c/github/TrentBest/TheSingularityWorkshop.Visualize/development?style=flat-square&logo=codecov)](https://codecov.io/gh/TrentBest/TheSingularityWorkshop.Visualize/branch/development)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg?style=flat-square)](LICENSE)

> **Make the living world visible. Keep the world independent of its display.**

TheSingularityWorkshop.Visualize is the GPU-facing presentation project for The Singularity Workshop. Its purpose is to turn observer-aware presentation data from [TheSingularityWorkshop.Renderer](https://github.com/TrentBest/TheSingularityWorkshop.Renderer) into pixels without making a particular windowing toolkit or graphics API the definition of the world.

The first integration target is **AnyApp on WPF**. A browser/WebGPU path is part of the architecture from the beginning, but it is not claimed to exist yet.

## The boundary

- **Experience / raWWar** owns world meaning and authoritative state.
- **FSM_API** supplies stateful behavior and scheduling.
- **[Gestures](https://github.com/TrentBest/TheSingularityWorkshop.Gestures)** defines the reusable procedural-motion model and CPU reference evaluator: poses, normalized progression, semantic anchors, and bounded individual variation.
- **Renderer** decides which representation and computational detail an observer needs through Event Horizons.
- **Visualize** executes presentation work and produces pixels through a graphics backend and a host surface.
- **AnyApp, WebPage, and other hosts** own their host lifecycle and choose which Experience to compose.
- **FSM_COS** composes capabilities; it is not a graphics API, renderer, visualizer, or game loop.

The GPU holds disposable projections of the world. GPU buffers, pose samples, visibility lists, and render targets must be reconstructable from authoritative and presentation inputs.

## Current implementation status

This repository begins with a backend-neutral frame contract, resource descriptions, tests, and architecture documentation. The contract is deliberately independent of the Renderer package version; a narrow adapter maps Renderer output into VisualizerFrame. **An isolated WPF/Direct3D 12 clear-and-present proof is now checked in under `samples/WpfD3D12Proof`.** It creates a native child HWND, Direct3D 12 device/queue, DXGI swap chain, and clears/presents back buffers. It is not yet connected to `VisualizerFrame`, AnyApp, or the Renderer; it does not draw geometry and its per-frame synchronization is correctness-first, not performance-ready.

The first engineering milestone is a real GPU-backed WPF surface in AnyApp, followed by a known test scene, resource upload, indexed drawing, instancing, and measured compute-to-draw.

## Documentation

- [Architecture](docs/ARCHITECTURE.md)
- [Gestures and procedural motion](docs/GESTURES_AND_POSE_PIPELINE.md)
- [GPU backend strategy](docs/GPU_BACKEND_STRATEGY.md)
- [WPF Direct3D 12 surface proof](samples/WpfD3D12Proof/README.md)
- [Implementation roadmap](docs/IMPLEMENTATION_ROADMAP.md)
- [Performance and benchmark rules](docs/PERFORMANCE.md)
- [Open questions](docs/OPEN_QUESTIONS.md)
- [Decision records](docs/decisions/README.md)

## Build and test

Requires the .NET 8 SDK.

~~~powershell
dotnet restore TheSingularityWorkshop.Visualize.slnx
dotnet build TheSingularityWorkshop.Visualize.slnx --configuration Release
dotnet test TheSingularityWorkshop.Visualize.slnx --configuration Release
~~~

## Release discipline

The package is not ready for NuGet merely because its contracts compile. A supported GPU backend, host integration, tests, documented device requirements, performance evidence, package review, and dependency/license audit are required. Any NuGet publishing workflow must remain disabled by default and may only be enabled with explicit approval.

---

<p align="center">
  <em>The Singularity Workshop — Tools for the curious, the bold, and the systemically inclined.</em>
</p>
