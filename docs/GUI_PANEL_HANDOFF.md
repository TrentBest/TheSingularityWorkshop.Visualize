![The Singularity Workshop animated moniker](https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Visualize/development/docs/assets/moniker.svg)

# Visualize → GUI panel handoff

> Visualize describes and submits presentation work. GUI owns panel structure and layout. An adapter connects the two without turning Visualize core into a GUI framework.

**Status:** Domain scaffold and integration contract. The current GUI.SVG path is an executable CPU-side manifestation proof; the multi-panel and world-projection host contract below is a target for audit and staged implementation.

## 1. Responsibility map

~~~mermaid
flowchart TD
    W[Experience / authoritative state] --> R[Renderer: observer-relative representation]
    R --> F[Renderer-neutral frame]
    F --> A[RendererFrameAdapter]
    A --> VF[VisualizerFrame + resource catalog]
    VF --> B[Visualizer backend]
    B --> O[Presentation output]
    O --> H[GUI / host routes output to panel targets]
~~~

| Concern | Owner | Contract boundary |
|---|---|---|
| World identity and meaning | Experience | Never inferred from pixels |
| Observer-relative representation | Renderer | Frame/command input |
| VisualizerFrame, resources, draw commands | Visualize | Platform-neutral presentation data |
| Backend capabilities and submission | Visualize backend contract | Device/presentation-specific implementation |
| GUI node tree and panel arrangement | GUI / host | Adapter consumes Visualize output |
| Source rectangle | Presentation request / adapter boundary | Explicit logical content region; independent of destination size |
| Destination surface and projection mapping | GUI / host / presentation adapter | Screen-space or world-space mapping, including required transforms |
| Native surface lifecycle, docking, input, resize events | Host / GUI adapter | Passed in as explicit context where needed |
| Composition of capabilities | FSM_COS / application | Above individual packages |

## 2. What works today

The existing [Renderer → Visualize → GUI.SVG proof](../integrations/RendererAdapter/Proof/README.md) exercises real Renderer distance/horizon selection, FSM-backed representation state, frame adaptation, Visualizer resource validation/submission, and SVG output.

The [GUI.SVG backend](../integrations/GuiSvg/GuiSvgVisualizerBackend.cs) supports a file destination and an in-memory SVG sink. The latter is a useful seam for a host-owned surface. The backend remains a CPU-side SVG manifestation path, not a GPU renderer.

Visualize core should remain independently usable. GUI.SVG is one optional adapter; it must not become a mandatory dependency of the Visualize core package.

## 3. Panel and projection handoff: target contract

A panel is a GUI/host concept, not a Visualize resource. The clean direction is:

1. The host owns a stable destination identity and its layout/lifecycle.
2. Every projection request supplies an explicit **source rectangle** describing which logical GUI/content region is projected. Do not derive it implicitly from the destination's dimensions.
3. The host supplies destination context: screen-space or world-space target, viewport dimensions, and any required transform/camera information.
4. Renderer determines observer-relative representation; Visualize receives presentation data and validates resource references.
5. An adapter maps the source rectangle to the destination surface and produces output suitable for that surface.
6. GUI/host routes output to the chosen panel or panel set and owns resizing, focus, docking, input, and native-control disposal.

Keep the concepts distinct:

- **Source rectangle** answers *which region of the source is presented?*
- **Destination surface** answers *where does it appear?*
- **Projection transform** answers *how is source space mapped to destination space?*

World-space projection may involve a transform, scale, orientation, and perspective; ordinary screen panels may use a simpler mapping. These are adapter/host capabilities, not reasons to introduce a mandatory GUI or 3D-engine dependency into Visualize core. A single source rectangle may be routed to multiple destinations with different mappings.

Do not add a Panel class to Visualize core merely to make the handoff convenient. First establish a narrow adapter contract and prove it with the existing GUI.SVG path, then add the smallest missing API.

### Multi-target questions the implementation must settle

- **Identity:** Are target IDs stable across frames and independent of GUI node IDs?
- **Source region:** Is the rectangle expressed in logical GUI units, normalized coordinates, or source pixels, and how are origin and coordinate orientation defined?
- **View independence:** Can each panel have a separate camera/view/viewport over the same authoritative scene?
- **Resource sharing:** Which immutable geometry/material resources can be shared, and which backend resources are device-local?
- **Mapping:** How are clipping, aspect ratio, letterboxing, and coordinate conversion handled?
- **Ordering:** Is submission ordered per target, globally, or both?
- **Freshness:** How does the host reject a late frame after a newer frame or resize?
- **Failure isolation:** Can one panel fail or be disposed without corrupting another panel's submission?
- **Cancellation and disposal:** Who owns cancellation, backend lifetime, and surface teardown?
- **Diagnostics:** Can the consumer correlate a frame, target, backend, and validation failure without parsing a display string?

These are audit criteria, not claims that the current API already supports every case.

## 4. Layered implementation plan

1. **Inventory:** document the actual public frame, resource, backend, capability, validation, diagnostic, cancellation, and disposal contracts.
2. **Single-target proof:** formalize the existing SVG sink as a host-owned output seam; keep GUI-specific routing outside Visualize core.
3. **Projection scaffold:** document source-rectangle semantics and the minimum target-neutral mapping context, without prematurely committing to a particular coordinate unit.
4. **Multi-target proof:** submit one source scene to two independent targets and prove distinct source rectangles/mappings plus explicit shared-resource behavior.
5. **World-space adapter:** prove a source rectangle can be mapped to a world-space surface with an explicit transform; keep engine/device-specific execution in an optional adapter.
6. **Native/GPU backend:** implement separately, with device lifecycle and performance evidence; do not infer GPU behavior from SVG.
7. **Consumer quick start:** show the smallest experience-owned composition that passes a frame to an adapter and routes the result to a GUI panel.

## 5. Evidence boundaries

- A generated SVG proves the described CPU-side data path and serialized SVG content.
- A test can prove validation, ordering, resource identity, cancellation, and disposal only to the extent those cases are explicitly exercised.
- A panel mock can prove routing logic, but not a real native host lifecycle or world-space projection.
- World-space projection requires a testable transform/mapping path; GPU claims require a GPU backend and measurements on that backend.

See [Visualizer Architecture](ARCHITECTURE.md), [Renderer adapter proof](../integrations/RendererAdapter/Proof/README.md), [GUI architecture](https://github.com/TrentBest/TheSingularityWorkshop.GUI/blob/development/docs/ARCHITECTURE.md), and the [living SVG documentation guide](SVG_LIVING_DOCUMENTATION.md).

TheSingularityWorkshop
