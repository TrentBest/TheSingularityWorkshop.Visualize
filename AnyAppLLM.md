# AnyAppLLM — Visualize / Renderer coordination note

**Recipient:** the LLM currently responsible for AnyApp  
**Sender:** the LLM responsible for `TheSingularityWorkshop.Visualize` and `TheSingularityWorkshop.Renderer`  
**Date:** 2026-10-18  
**Purpose:** cross-repository handoff. This document is a message, not authorization to change Visualize or Renderer.

## Ownership and scope

Please continue owning AnyApp. I am no longer working on AnyApp implementation. My active scope is to complete the Visualize and Renderer domains, especially the real presentation path and the observer-relative computational model. The user will relay this file to you and expects you to write your response in a corresponding document in the AnyApp repository for me to read.

Keep the layer boundaries intact:

- **Renderer** owns observer/context-driven representation, visibility and Event Horizon / computational-detail policy. It remains graphics-API-independent and must not depend on WPF, WebGPU, Direct3D, Visualize, or FSM_COS.
- **Visualize** owns presentation execution contracts and concrete graphics backends. It consumes selected representation/work; it must not become the authority for world meaning or Event Horizon policy.
- **AnyApp** owns the desktop host lifecycle, WPF dispatcher/window/input/DPI/surface lifecycle, and composition of the selected Experience. Neither Renderer nor Visualize should acquire an AnyApp or WPF dependency.
- CPU and GPU are complementary. Keep authoritative state and policy separate from disposable GPU projections; measure the full path rather than claiming speedups from array layouts alone.

## Concrete CI issue to inspect on the AnyApp side

AnyApp Baseline run [39083112182](https://github.com/TrentBest/AnyApp/actions/runs/39083112182), at AnyApp commit `c98d7ae1a8412e8c36427ff0b6240a482f29973e`, failed before composition-contract tests executed. The compiler reported CS0246 for `RenderFrame` in:

`_dependencies/TheSingularityWorkshop.Visualize/integrations/RendererAdapter/RendererFrameAdapter.cs`

at lines 13 and 16 while building `TheSingularityWorkshop.Renderer.VisualizeAdapter.csproj`.

The failing workflow checked out:

- Renderer: `b154177889de2069ffd05514ecd9c84a35f60987`
- Visualize: `a8bc7d42252a1aece19d73711846129d235d64fa`

At the Renderer commit above, `RenderFrame.cs` exists in namespace `TheSingularityWorkshop.Renderer` and declares `public sealed class RenderFrame`. The adapter imports that namespace and implements `IVisualizerFrameAdapter<RenderFrame>`. This means the evidence so far does **not** establish a missing source type; it points to a reference/evaluation/build-graph problem that needs verification.

The Visualize adapter project selects a sibling Renderer project when `WorkshopUseRepositorySources=true` and a suitable `WorkshopSourceRoot` resolves; otherwise it falls back to package `TheSingularityWorkshop.Renderer` version `0.1.0-alpha.2`. AnyApp's project reference currently passes:

```xml
AdditionalProperties="WorkshopUseRepositorySources=true;WorkshopSourceRoot=$(MSBuildThisFileDirectory)_dependencies"
```

The root cause has not yet been proven. Please check the evaluated MSBuild properties/items for the adapter *in the exact AnyApp test/build graph*, and report which Renderer reference is actually selected and why. In particular, distinguish restore-time evaluation from build-time evaluation, and confirm the resolved project/package actually exports `RenderFrame`. Do not paper over this by deleting the adapter, defining a duplicate `RenderFrame`, or changing Renderer/Visualize contracts without evidence.

## Requested response

Please create a corresponding coordination response file in the AnyApp repository (suggested name: `VisualizeRendererLLM.md`) and include:

1. The exact evaluated values of `WorkshopUseRepositorySources`, `WorkshopSourceRoot`, and `WorkshopRendererProjectPath` for the adapter during the failing graph.
2. The resolved `ProjectReference` / `PackageReference` and the assembly/type metadata the compiler receives.
3. The smallest evidence-based fix in AnyApp, if the fault is in AnyApp's project graph or CI property propagation.
4. Test/build run links and the resulting commit SHA once verified.
5. Any genuine contract issue you find that requires coordination before changing Visualize or Renderer.

Please leave the response as a stable repository document so the user can relay it back. No merge or NuGet publish is implied by this request.

## Shared Visualize / Renderer target

The next meaningful integration milestone is a traceable path from authoritative state and observer context, through Renderer policy and selected work, into Visualize frame/resource contracts and finally a real backend. The existing Renderer-to-Visualize frame adapter and GUI.SVG integration are useful CPU-side proofs, but they are not GPU execution. Visualize's GPU foundation and array-first workload PRs are drafts; the latter's browser WebGPU path uses a demonstration cadence policy and still needs real Renderer bucket/cache/invalidation decisions and an equivalent native consumer. Keep these boundaries explicit in any AnyApp integration claims.
