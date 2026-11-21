# Renderer → Visualize tri-view proof

This executable proof emits three views from **one canonical Renderer frame**:

1. **Documentation SVG** — GUI.SVG output for human-facing documentation.
2. **Renderer structure** — JSON projection of the actual Renderer `RenderFrame`, including ordered commands, stable entity IDs, resource IDs, selected representations, transforms, and draw ranges.
3. **Visualization output** — Visualize's own `SvgVisualizerBackend` consumes the exact same adapted `VisualizerFrame` and resource catalog and emits CPU/SVG output.

The proof adapts the Renderer frame exactly once. Runtime assertions verify command count, order, entity identity, geometry/material resource identity, and representation mapping before either backend receives it. Both backends must report the same submitted draw count as the source frame.

## Run

From the Visualize repository root:

```bash
dotnet run --project integrations/RendererAdapter/Proof/RendererFrameToSvgProof.csproj --configuration Release -- ./samples/PrimitiveGallery/generated/10-renderer-selected-frame.svg
```

The command emits:

- `10-renderer-selected-frame.svg` — documentation-facing GUI.SVG artifact;
- `10-renderer-selected-frame.renderer.json` — structured Renderer composition;
- `10-renderer-selected-frame.visualize.svg` — Visualize backend output.

The adapter project requires the Renderer source checkout (the shared sibling repository or CI's `RendererSource` checkout). Its source `ProjectReference` exists only to compile and test the adapter against Renderer contracts. There is no Renderer NuGet fallback, and this source reference is not runtime MicroBundle composition.

## Evidence boundary

This is an executable **CPU-side tri-view integration proof**. It exercises Renderer observer-distance horizon selection and FSM-backed representation selection, verifies the adapter's semantic handoff, and submits the same adapted frame to GUI.SVG and Visualize's own SVG/CPU backend.

It is not GPU execution, an interactive desktop surface, a benchmark, or proof of the production observer → Event Horizon/bucket → selected-work → frame-assembly pipeline. The specimen geometry mapping is demonstrator code, not a universal representation policy.
