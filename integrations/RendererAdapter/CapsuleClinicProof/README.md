# Capsule Clinic → Renderer → Visualize → GUI.SVG proof

This executable integration proof captures a seeded, authoritative `CapsuleClinicLocalSession` snapshot, maps the four arena panes to deterministic Renderer draw commands, adapts the `RenderFrame` through the existing Renderer-to-Visualize adapter, and renders it through the existing GUI.SVG backend.

It deliberately keeps puzzle rules and command authority in Experiences. This proof is a CPU/SVG integration check, not a browser interaction runtime, GPU renderer, network host, or performance benchmark. The empty board cells are presentation tiles; settled cells and active capsule segments use color-specific materials. Draw-command ordering is deterministic: pane order, bottom-up row-major cells, then active segments.

## Run

From the Visualize repository root, with Experiences checked out into `ExperiencesSource/`, Renderer source checked out into `integrations/RendererAdapter/RendererSource/`, and GUI source checked out into `integrations/GuiSvg/GuiSource/`:

```bash
dotnet run --project integrations/RendererAdapter/CapsuleClinicProof/CapsuleClinicFrameProof.csproj --configuration Release -- ./artifacts/capsule-clinic-frame.svg
```

The command produces the SVG and a JSON manifest recording pane/board dimensions, command counts, and the CPU/SVG backend.
