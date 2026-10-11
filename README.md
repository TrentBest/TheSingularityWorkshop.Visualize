# ✳️ 00 The Singularity Workshop — Visualize

[![Build Status](https://img.shields.io/github/actions/workflow/status/TrentBest/TheSingularityWorkshop.Visualize/build.yml?branch=development&style=flat-square&logo=github)](https://github.com/TrentBest/TheSingularityWorkshop.Visualize/actions/workflows/build.yml)
[![Coverage](https://img.shields.io/codecov/c/github/TrentBest/TheSingularityWorkshop.Visualize/development?style=flat-square&logo=codecov)](https://codecov.io/gh/TrentBest/TheSingularityWorkshop.Visualize/branch/development)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg?style=flat-square)](LICENSE)

> **Make the living world visible. Keep the world independent of its display.**

TheSingularityWorkshop.Visualize is the GPU-facing presentation project for The Singularity Workshop. It turns observer-aware presentation data from [TheSingularityWorkshop.Renderer](https://github.com/TrentBest/TheSingularityWorkshop.Renderer) into pixels without making a particular windowing toolkit or graphics API the definition of the world.

The first integration target is **AnyApp on WPF**. A browser/WebGPU path is part of the architecture from the beginning, but it is not claimed to exist yet.

## 🟦 01 What

![](docs/assets/dividers/blue.svg)

Visualize owns the execution of presentation work: GPU resource descriptions, backend execution, frame submission, and diagnostics. It is designed to consume the representation selected by Renderer, not to decide what the world means or how much detail an observer needs.

The GPU holds disposable projections of the world. GPU buffers, pose samples, visibility lists, and render targets must be reconstructable from authoritative state and presentation inputs.

## 🟣 02 Why

![](docs/assets/dividers/violet.svg)

A rendering architecture becomes difficult to evolve when world meaning, observer-relative detail, graphics execution, and window lifecycle are tangled together. Visualize gives GPU-facing execution a focused boundary so hosts and graphics backends can evolve without becoming the authority over the world.

The Workshop's central performance hypothesis is to reduce unnecessary computational detail before drawing, then execute the remaining parallel presentation work efficiently. A GPU backend alone does not prove that a workload is faster; performance claims require measurements on a stated workload and hardware.

## 🩵 03 How

![](docs/assets/dividers/teal.svg)

The responsibility flow is deliberate:

- **Experience / raWWar** owns world meaning and authoritative state.
- **FSM_API** supplies stateful behavior and scheduling.
- **[Gestures](https://github.com/TrentBest/TheSingularityWorkshop.Gestures)** defines reusable procedural motion and CPU reference evaluation.
- **Renderer** selects observer-relative representation and computational detail through Event Horizons.
- **Visualize** executes presentation work and produces pixels through a graphics backend and host surface.
- **AnyApp, WebPage, and other hosts** own host lifecycle and choose which Experience to compose.
- **FSM_COS** composes capabilities; it is not a graphics API, renderer, visualizer, or game loop.

### Current implementation status

This repository has backend-neutral frame contracts, resource descriptions, tests, and architecture documentation. A narrow adapter maps Renderer output into VisualizerFrame; the contract is deliberately independent of the Renderer package version.

An isolated WPF/Direct3D 12 clear-and-present proof exists under `samples/WpfD3D12Proof`. It implements a native child HWND, Direct3D 12 device/queue, DXGI swap chain, and clear/present submission. Windows CI builds it with zero warnings and errors, but a real hardware/runtime capture has not yet been recorded. It is not connected to `VisualizerFrame`, AnyApp, or Renderer, does not draw geometry, and uses correctness-first rather than performance-ready per-frame synchronization.

## 🟢 04 See it in a minute

![](docs/assets/dividers/green.svg)

You need the **.NET 8 SDK**. From the repository root, restore, build, and run the current test suite:

```powershell
dotnet restore TheSingularityWorkshop.Visualize.slnx
dotnet build TheSingularityWorkshop.Visualize.slnx --configuration Release
dotnet test TheSingularityWorkshop.Visualize.slnx --configuration Release
```

This verifies the current contracts and test suite; it does **not** demonstrate an end-to-end rendered scene. The next engineering milestone is a real GPU-backed WPF surface in AnyApp, followed by a known test scene, resource upload, indexed drawing, instancing, and measured compute-to-draw.

## 🟪 05 Available documentation and theory

![](docs/assets/dividers/purple.svg)

- [Architecture](docs/ARCHITECTURE.md) — follow data from authoritative state to the host surface and understand which layer owns each decision.
- [Gestures and procedural motion](docs/GESTURES_AND_POSE_PIPELINE.md) — see how physical procedures become evaluated pose data without making Visualize the owner of motion semantics.
- [GPU backend strategy](docs/GPU_BACKEND_STRATEGY.md) — compare backend responsibilities and the intended native/browser directions.
- [WPF Direct3D 12 surface proof](samples/WpfD3D12Proof/README.md) — inspect exactly what the isolated Windows proof implements and what remains unproven.
- [Implementation roadmap](docs/IMPLEMENTATION_ROADMAP.md) — see the engineering milestones and their current boundaries.
- [Performance and benchmark rules](docs/PERFORMANCE.md) — understand how performance evidence must be collected and reported.
- [Open questions](docs/OPEN_QUESTIONS.md) — review unresolved design decisions.
- [Decision records](docs/decisions/README.md) — follow architectural choices and their rationale.

## Build and test

![](docs/assets/dividers/green.svg)

The commands above use the solution file and the installed .NET 8 SDK. CI also collects Cobertura coverage and uploads build, test, and package artifacts for review.

## Release discipline

![](docs/assets/dividers/violet.svg)

The package is not ready for NuGet merely because its contracts compile. A supported GPU backend, host integration, tests, documented device requirements, performance evidence, package review, and dependency/license audit are required. Any NuGet publishing workflow must remain disabled by default and may only be enabled with explicit approval.

---

<p align="center">
  <em>The Singularity Workshop — Tools for the curious, the bold, and the systemically inclined.</em>
</p>
