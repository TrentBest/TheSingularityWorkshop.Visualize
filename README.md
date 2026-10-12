# ✳️ 00 The Singularity Workshop — Visualize

[![Build Status](https://img.shields.io/github/actions/workflow/status/TrentBest/TheSingularityWorkshop.Visualize/build.yml?branch=development&style=flat-square&logo=github)](https://github.com/TrentBest/TheSingularityWorkshop.Visualize/actions/workflows/build.yml)
[![Coverage](https://img.shields.io/codecov/c/github/TrentBest/TheSingularityWorkshop.Visualize/development?style=flat-square&logo=codecov)](https://codecov.io/gh/TrentBest/TheSingularityWorkshop.Visualize/branch/development)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg?style=flat-square)](LICENSE)

![](docs/assets/dividers/star-gold.svg)

> **Make the living world visible. Keep the world independent of its display.**

TheSingularityWorkshop.Visualize is the GPU-facing presentation project for The Singularity Workshop. It turns observer-aware presentation data from [TheSingularityWorkshop.Renderer](https://github.com/TrentBest/TheSingularityWorkshop.Renderer) into pixels without making a particular windowing toolkit or graphics API the definition of the world.

The first desktop host is **AnyApp on WPF**. A small browser/WebGPU proof now consumes the same generated binary workload as the dataset generator. This is evidence that shared arrays can reach an actual GPU draw path—not a production backend or a performance result. **No graphics API has been selected.**

## 🟦 01 What

![](docs/assets/dividers/blue.svg)

Visualize owns the execution of presentation work: GPU resource descriptions, backend execution, frame submission, and diagnostics. It is designed to consume the representation selected by Renderer, not to decide what the world means or how much detail an observer needs.

The GPU holds disposable projections of the world. GPU buffers, pose samples, visibility lists, and render targets must be reconstructable from authoritative state and presentation inputs.

## 🟣 02 Why

![](docs/assets/dividers/violet.svg)

A rendering architecture becomes difficult to evolve when world meaning, observer-relative detail, graphics execution, and window lifecycle are tangled together. Visualize gives GPU-facing execution a focused boundary so hosts and graphics backends can evolve without becoming the authority over the world.

The Workshop's central performance hypothesis is to reduce unnecessary computational detail before drawing, then execute the remaining parallel presentation work efficiently. Array-first data layouts, bulk transfer, instancing, and GPU compute are candidates to measure—not performance claims by themselves.

## 🔺 03 How

![](docs/assets/dividers/triangle-red.svg)

The responsibility flow is deliberate:

- **Experience / raWWar** owns world meaning and authoritative state.
- **FSM_API** supplies stateful behavior and scheduling.
- **[Gestures](https://github.com/TrentBest/TheSingularityWorkshop.Gestures)** defines reusable procedural motion and CPU reference evaluation.
- **Renderer** selects observer-relative representation and computational detail through Event Horizons.
- **Visualize** executes presentation work and produces pixels through a selected graphics backend and host surface.
- **AnyApp, WebPage, and other hosts** own host lifecycle and choose which Experience to compose.
- **FSM_COS** composes capabilities; it is not a graphics API, renderer, visualizer, or game loop.

### 🟢 Current implementation status

![](docs/assets/dividers/green.svg)

This repository has backend-neutral frame contracts, revisioned geometry/material resources, and an immutable instance-buffer contract for indexed instancing. Draw commands can address an explicit range in a contiguous instance array, and Visualizer validates resource/range correctness before backend submission. A narrow adapter maps Renderer output into VisualizerFrame; the contract is deliberately independent of the Renderer package version. These are data/API contracts—not yet a concrete GPU upload or draw implementation.

An isolated WPF/Direct3D 12 clear-and-present experiment exists under `samples/WpfD3D12Proof`. It compiles in Windows CI, but a real hardware/runtime capture has not yet been recorded. It draws no geometry, uses correctness-first per-frame synchronization, and is **not a selected backend**. Do not extend or integrate it by default; first compare backend candidates against an array-first workload.

## 🟢 04 See it in a minute

![](docs/assets/dividers/green.svg)

You need the **.NET 8 SDK**, Python 3, and a browser with WebGPU enabled. To see the current GPU demo from the repository root:

```powershell
dotnet run --project samples/ArrayFirstWorkload/Generator -- 10000 ./artifacts/workload-10000
py -m http.server 8080
```

Open [http://localhost:8080/samples/ArrayFirstWorkload/WebGpuRunner/](http://localhost:8080/samples/ArrayFirstWorkload/WebGpuRunner/). Keep the dataset path as `/artifacts/workload-10000` and select **Load dataset and render**. The animated scene renders near and far cohorts from the generated, hash-verified arrays, updates/uploads the near cohort each frame, and refreshes the far cohort every 12 frames. It reports upload accounting and browser/JavaScript timing samples; these are **not GPU execution timings**. See the [runner guide](samples/ArrayFirstWorkload/WebGpuRunner/README.md) for requirements and limitations.

To verify the .NET contracts and tests:

```powershell
dotnet restore TheSingularityWorkshop.Visualize.slnx
dotnet build TheSingularityWorkshop.Visualize.slnx --configuration Release
dotnet test TheSingularityWorkshop.Visualize.slnx --configuration Release
```

The live browser runner is an end-to-end GPU draw proof. It is **not yet** wired to Renderer Event Horizon bucket/dependency APIs, and it is not a native-vs-browser benchmark. Those are the next integration and measurement milestones.

## 🟪 05 Available documentation and theory

![](docs/assets/dividers/purple.svg)

- [Architecture](docs/ARCHITECTURE.md) — follow data from authoritative state to the host surface and understand which layer owns each decision.
- [Gestures and procedural motion](docs/GESTURES_AND_POSE_PIPELINE.md) — see how physical procedures become evaluated pose data without making Visualize the owner of motion semantics.
- [GPU backend strategy](docs/GPU_BACKEND_STRATEGY.md) — compare backend responsibilities and the intended native/browser directions, with array-first execution as a constraint.
- [Array-first GPU workload contract](samples/ArrayFirstWorkload/README.md) — generate deterministic contiguous inputs and inspect the shared comparison profiles.
- [Dataset generator](samples/ArrayFirstWorkload/Generator/README.md) — run the .NET 8 tool to produce vertex, index, and instance binary arrays with SHA-256 manifests.
- [Shared-data WebGPU runner](samples/ArrayFirstWorkload/WebGpuRunner/README.md) — upload those exact arrays and draw indexed instances in a browser supporting WebGPU.
- [Isolated WPF/Direct3D 12 experiment](samples/WpfD3D12Proof/README.md) — inspect the exploratory native-surface proof and its limitations.
- [Implementation roadmap](docs/IMPLEMENTATION_ROADMAP.md) — see the engineering milestones and their current boundaries.
- [Performance and benchmark rules](docs/PERFORMANCE.md) — understand how performance evidence must be collected and reported.
- [Open questions](docs/OPEN_QUESTIONS.md) — review unresolved design decisions.
- [Decision records](docs/decisions/README.md) — follow architectural choices and their rationale.

## 🟢 Build and test

![](docs/assets/dividers/green.svg)

The commands above use the solution file and the installed .NET 8 SDK. CI also collects Cobertura coverage and uploads build, test, and package artifacts for review.

## 🟣 Release discipline

![](docs/assets/dividers/violet.svg)

The package is not ready for NuGet merely because its contracts compile. A selected and supported GPU backend, host integration, tests, documented device requirements, performance evidence, package review, and dependency/license audit are required. Any NuGet publishing workflow must remain disabled by default and may only be enabled with explicit approval.

---

<p align="center">
  <em>The Singularity Workshop — Tools for the curious, the bold, and the systemically inclined.</em>
</p>
