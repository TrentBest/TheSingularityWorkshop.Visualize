# FSM_API boundary in Visualize

## Finding

The Visualize core project does **not** directly consume FSM_API.

Evidence on `development`:

- `TheSingularityWorkshop.Visualize.csproj` targets .NET 10 and declares no `PackageReference` or `ProjectReference`.
- `Visualizer` coordinates a backend-neutral `VisualizerFrame`, validates referenced resources, and submits the frame to `IVisualizerBackend`. It contains no FSM_API calls or FSM context ownership.
- `VisualizerFrame` carries the observer/view snapshot, ordered draw commands, stable entity identity, and the selected representation. Those are inputs to presentation, not the authority that chooses the representation.

## Where FSM_API actually enters the repository

The optional `integrations/RendererAdapter` integration depends on `TheSingularityWorkshop.Renderer`.

In the current development source-build path, the adapter project references the sibling Renderer project. In package mode, it references Renderer package `0.1.0-alpha.2`. Renderer itself directly references `TheSingularityWorkshop.FSM_API` `1.0.13` in package mode, or the FSM_API source project in repository-source mode.

Renderer's `RendererComputationMachine` is the direct FSM_API consumer. It creates a finite state machine, creates per-entity instances, advances them with `Update`, reads the active representation, and destroys its owned handle. The adapter then carries Renderer frame data into Visualize's neutral contracts.

The relationship is therefore:

```text
FSM_API
  ↓ direct state-machine use
Renderer
  ↓ observer-selected RenderFrame
RendererFrameAdapter (integration boundary)
  ↓ VisualizerFrame
Visualize
  ↓ backend submission
Presentation backend
```

This is an integration/transitive dependency, not a direct FSM_API dependency of the Visualize core assembly.

## Naming decision

Do not rename this repository or assembly to `FSM_Visualize` merely because an upstream dependency uses FSM_API.

The `FSM_` prefix should continue to communicate tight FSM_API coupling. Visualize deliberately consumes a selected frame and executes presentation; it does not own the state machine that decides what representation is required. Adding FSM_API directly to Visualize would invert that separation without adding a necessary capability.

If a future Visualize feature genuinely owns an FSM-driven lifecycle or state machine, isolate that responsibility and establish the direct dependency explicitly rather than introducing an incidental dependency across the entire presentation core.

## What remains incomplete

The Renderer adapter proof establishes a CPU-side frame handoff. It does not establish that Visualize is acquired, materialized, and composed as an executable MicroBundle by FSM_COS.

That is a separate capability-delivery boundary tracked in [issue #49](https://github.com/TrentBest/TheSingularityWorkshop.Visualize/issues/49). The target is to keep portable capability identity and composition in FSM_COS, host/repository acquisition at the host boundary, and Visualize presentation behavior in its own capability artifact.

## Evidence distinctions

- Direct FSM_API call: Renderer source.
- Visualize core API: backend-neutral frame validation and submission.
- Integration proof: Renderer frame adapted into Visualize.
- FSM_COS MicroBundle resolution and host composition: not yet proven by the adapter project reference.
- GPU presentation: not established by CPU/SVG artifacts or by the browser experiments.
