![The Singularity Workshop animated moniker](https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Visualize/development/docs/assets/moniker.svg)

# ✳️ 00 The Singularity Workshop — Visualize

[![Build Status](https://img.shields.io/github/actions/workflow/status/TrentBest/TheSingularityWorkshop.Visualize/build.yml?branch=development&style=flat-square&logo=github)](https://github.com/TrentBest/TheSingularityWorkshop.Visualize/actions/workflows/build.yml)
[![Coverage](https://img.shields.io/codecov/c/github/TrentBest/TheSingularityWorkshop.Visualize/development?style=flat-square&logo=codecov)](https://codecov.io/gh/TrentBest/TheSingularityWorkshop.Visualize/branch/development)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg?style=flat-square)](LICENSE)

![](docs/assets/dividers/star-gold.svg)

> **Make the living world visible. Keep the world independent of its display.**

## Living visual showcase

[![Animated connected-point geometry emitted by the C# Visualizer SVG proof backend](samples/PrimitiveGallery/generated/03-four-connected-points.svg)](samples/PrimitiveGallery/README.md)

This is the first living visual in the documentation sequence whose **geometry is submitted through the actual C# Visualizer contracts and written by its SVG proof backend**. The connected points and segments are real backend output; the moving marker and dashed trace are SVG presentation effects layered over that output. The animation does not pretend to be an FSM update loop or a production GPU renderer.

### Follow the actual API consumption

The color-coded [API-to-SVG proof page](samples/PrimitiveGallery/api-consumption.html) pairs the live SVG with the C# call path. The code below is taken from the proof harness itself.

- 🟦 **Scene and contract data** — build geometry, materials, instances, and a `VisualizerFrame`.
- 🟩 **API consumption** — instantiate `Visualizer` with a backend/resource provider, then submit the frame through `RenderAsync`.
- 🟨 **Presentation-specific effect** — the SVG backend adds the moving marker and dashed trace after consuming the submitted geometry.

```csharp
// 🟦 The example has already built its geometry and VisualizerFrame.
foreach (var example in examples)
{
    var litStudy = example.Title.StartsWith("Lit hemisphere", StringComparison.Ordinal);
    var animateConnectedPoints = example.Title.StartsWith("Four connected points", StringComparison.Ordinal);

    // 🟨 Choose the proof backend and its SVG-only presentation effect.
    // 🟩 This is the actual Visualizer API consumption.
    await using var visualizer = new Visualizer(
        new SvgProofBackend(example.Path, example.Title, litStudy, litStudy, animateConnectedPoints),
        example.Resources);
    var result = await visualizer.RenderAsync(example.Frame);
    Console.WriteLine($"{example.Title}: Visualizer submitted {result.SubmittedDrawCount} draw command(s) -> {example.Path}");
}
```

[Open the exact C# source](samples/PrimitiveGallery/VisualizeSvgProof/Program.cs) · [Inspect the color-coded presentation](samples/PrimitiveGallery/api-consumption.html)

The larger [living SVG illustration](docs/assets/living-showcase.svg) shows the direction for documentation visuals. The [interactive SVG Primitive Lab source](samples/PrimitiveGallery/index.html) is the browser-only sandbox for changing primitive type, count, positions, colour, and spread; it is intentionally a separate execution path. The [Primitive Gallery guide](samples/PrimitiveGallery/README.md) puts the contract-backed output, browser experiment, and their evidence boundaries side by side.

Next, we will replace more illustrative motion with behavior and scene data controlled by the Workshop's own code, keeping the SVG surface subordinate to your state and composition rules. Consumer-specific scenes, such as raWWar's soldier and exoskeleton, belong in the consuming Experience and should exercise Visualize through its public contracts, not define Visualize's own sample identity.

TheSingularityWorkshop.Visualize is the GPU-facing presentation project for The Singularity Workshop. It turns observer-aware presentation data from [TheSingularityWorkshop.Renderer](https://github.com/TrentBest/TheSingularityWorkshop.Renderer) into pixels without making a particular windowing toolkit or graphics API the definition of the world.

The first desktop host is **AnyApp on WPF**. **No production graphics API has been selected.** The next milestone is to connect the Workshop's array-first data model to a backend without binding world meaning to one display technology. A real Experience such as raWWar should provide the domain-specific scene and demonstrate consumption of Visualize; this repository's examples should stay small, neutral, and contract-focused.

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

### What is real, what is illustrative, and what remains to build

To keep the living documentation honest, examples use these distinctions:

- **Implemented contract:** code and tests in this repository establish the behavior explicitly described by the contract. A passing contract test does not prove GPU execution.
- **Browser experiment:** an HTML/SVG/WebGPU page may animate or render its own demonstration logic. Unless a documented bridge says otherwise, it must not be described as executing the C# Visualize or FSM_API implementation.
- **Concept / proposed integration:** an architectural direction or illustrative behavior that still needs implementation and verification. It is intent, not a capability claim.
- **Hardware-verified:** reserve this for a path actually exercised on identified hardware/browser or host—not merely one that compiles or passes CI.

In particular, the interactive Primitive Gallery and lighting lab are browser-side experiments, while the SVG proof harness exercises the C# Visualizer contracts. They are related demonstrations, but they are not the same execution path. Likewise, the intended FSM_API → motion/pose → Renderer policy → Visualize presentation flow is an architectural direction wherever an end-to-end, tested connection has not yet been demonstrated. We should promote a claim from concept to implementation only when the corresponding code and evidence exist.

An isolated WPF/Direct3D 12 clear-and-present experiment exists under `samples/WpfD3D12Proof`. It compiles in Windows CI, but a real hardware/runtime capture has not yet been recorded. It draws no geometry, uses correctness-first per-frame synchronization, and is **not a selected backend**.

## 🟢 04 See the work and explore it

![](docs/assets/dividers/green.svg)

The illustration above is the no-launch overview. For deeper inspection, the documentation links below take you directly to the implementation and the current visual experiments. GitHub displays this README and its SVG artwork in place; the browser labs are separate HTML/WebGPU or SVG experiments, not embedded executions of the C# assembly.

- **[Primitive Gallery source and guide](samples/PrimitiveGallery/README.md)** — the interactive point/line progression and what each step demonstrates.
- **[Primitive Gallery page](samples/PrimitiveGallery/index.html)** — inspect the experiment's actual HTML, SVG, and JavaScript.
- **[Surface Normals & Light lab](samples/NormalLightingLab/index.html)** — change light direction, ambient contribution, diffuse strength, sample density, and normal overlays in a live SVG study.

To run the current .NET test suite, you need the **.NET 8 SDK**. From the repository root:

```powershell
dotnet restore TheSingularityWorkshop.Visualize.slnx
dotnet build TheSingularityWorkshop.Visualize.slnx --configuration Release
dotnet test TheSingularityWorkshop.Visualize.slnx --configuration Release
```

The tests verify the current contracts. To explore the first visible SVG sequence, run `py -m http.server 8080` from the repository root and open [Primitive Gallery](http://localhost:8080/samples/PrimitiveGallery/); it steps from one point through 10,000 points and connected/independent lines. The gallery's [README](samples/PrimitiveGallery/README.md) lists expected observations and explains the timing counter. The showcase remains deliberately neutral. Domain-specific scene and workload demonstrations should live with their consuming Experience—for example, raWWar—and call Visualize through its supported contracts.

## 🟪 05 Available documentation and theory

![](docs/assets/dividers/purple.svg)

- [Architecture](docs/ARCHITECTURE.md) — follow data from authoritative state to the host surface and understand which layer owns each decision.
- [Visual primitives](docs/PRIMITIVES.md) — start with the [interactive SVG Primitive Gallery](samples/PrimitiveGallery/index.html), then explore [surface normals and live diffuse lighting](samples/NormalLightingLab/index.html), and distinguish each browser experiment from the current C# contracts.
- [Gestures and procedural motion](docs/GESTURES_AND_POSE_PIPELINE.md) — see how physical procedures become evaluated pose data without making Visualize the owner of motion semantics.
- [GPU backend strategy](docs/GPU_BACKEND_STRATEGY.md) — compare backend responsibilities and the intended native/browser directions, with array-first execution as a constraint.
- [Isolated WPF/Direct3D 12 experiment](samples/WpfD3D12Proof/README.md) — inspect the exploratory native-surface proof and its limitations.
- [Implementation roadmap](docs/IMPLEMENTATION_ROADMAP.md) — see the engineering milestones and their current boundaries.
- [Performance and benchmark rules](docs/PERFORMANCE.md) — understand how performance evidence must be collected and reported.
- [Physics-based lighting, shadows, and extensible math](docs/LIGHTING_AND_SHADOWS.md) — understand physical light-source meaning, independent horizon work buckets, MicroBundle arbitration, cache invalidation, and the staged plan for optional math optimizations.
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
