# Gallery recipe: connected geometry → SVG

![Generated connected-points SVG](../../samples/PrimitiveGallery/generated/07-gui-svg-connected-points.svg)

## What this exhibit means

The C# proof creates Visualizer geometry, material resources, draw commands, and a `VisualizerFrame`. The optional `GuiSvgVisualizerBackend` consumes that public frame and uses GUI.SVG to serialize the neutral scene.

## Source and reproduction

- [Scene builder](../../integrations/GuiSvg/Proof/Program.cs)
- [Optional backend](../../integrations/GuiSvg/GuiSvgVisualizerBackend.cs)
- [Reusable GUI.SVG renderer](https://github.com/TrentBest/TheSingularityWorkshop.GUI/blob/development/src/SVG/SvgGuiRenderer.cs)

From the Visualize repository root:

```bash
dotnet run --project integrations/GuiSvg/Proof/GuiSvgProof.csproj --configuration Release
```

## What it proves

The Visualizer frame/resource contract can be consumed by an optional presentation backend and manifested as SVG.

## What it does not prove

The animation is presentation-only; the proof uses a simple XY mapping and is not perspective-correct, depth-tested GPU rendering.

## Recipe pattern

Build scene data through the public API → construct a frame → pass the frame to the optional backend → serialize the resulting scene → check deterministic output. Keep the output generator in C# rather than redrawing the scene in browser JavaScript.
