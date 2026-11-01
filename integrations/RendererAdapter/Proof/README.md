# Renderer → Visualize tri-view proof

This executable proof emits three inspectable views from **one canonical Renderer frame**:

1. **Documentation SVG** — the existing GUI.SVG integration emits the SVG intended for documentation and human inspection.
2. **Renderer structure** — a deterministic JSON projection of the actual Renderer `RenderFrame`, including ordered commands, stable entity IDs, resource IDs, selected representations, transforms, and draw ranges. This is the AI-/tool-inspectable composition view.
3. **Visualization output** — the Visualize-owned `SvgVisualizerBackend` consumes the exact same adapted `VisualizerFrame` and resource catalog and emits its own CPU/SVG backend result.

The proof does not independently redraw three scenes. A single Renderer frame is adapted once; the same immutable adapted frame is submitted to both SVG backends. Runtime assertions verify command count, ordering by position, entity identity, geometry/material resource identity, and representation mapping before output generation.

## Run

From the Visualize repository root:

```bash
dotnet run --project integrations/RendererAdapter/Proof/RendererFrameToSvgProof.csproj --configuration Release -- ./samples/PrimitiveGallery/generated/10-renderer-selected-frame.svg
```

The command emits:

- `10-renderer-selected-frame.svg` — documentation-facing GUI.SVG artifact;
- `10-renderer-selected-frame.renderer.json` — structured Renderer composition;
- `10-renderer-selected-frame.visualize.svg` — Visualize backend output.

## What this proves—and what it does not

This is an executable **CPU-side tri-view integration proof**. Renderer performs observer-distance horizon selection and FSM-backed representation selection. The adapter preserves command meaning and stable identities. GUI.SVG and Visualize's own SVG/CPU backend each consume the same adapted frame.

The Visualize SVG backend is a CPU artifact backend, not GPU execution, an interactive display surface, or a benchmark. The specimen geometry mapping is demonstrator code, not a universal representation policy. This proof is also not yet the production observer → Event Horizon/bucket → selected-work → frame-assembly pipeline. Those boundaries remain separate work items.

The proof reuses the existing Renderer adapter and Visualize backend; it does not add a second renderer or make Renderer depend on Visualize.
