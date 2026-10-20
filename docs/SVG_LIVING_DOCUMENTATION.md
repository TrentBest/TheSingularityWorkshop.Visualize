# SVG living documentation

**Purpose:** make Visualize's real contracts observable through generated SVG while keeping SVG output distinct from production GPU execution.

## Executable proof ladder

| Proof | Source of truth | Output and boundary |
| --- | --- | --- |
| 01–05 Primitive Gallery | `samples/PrimitiveGallery/VisualizeSvgProof/Program.cs` | Generated point, instance, connected-point, and line SVGs from Visualizer geometry/resources/frames. |
| 06 Hemisphere normals | C# lighting/normal proof | SVG preview of indexed geometry, CPU-side normal projection, and a simple Lambert study; not a production shader. |
| 07 GUI.SVG connected points | `integrations/GuiSvg/Proof/Program.cs` | Visualizer frame → `GuiSvgVisualizerBackend` → neutral GUI scene → GUI.SVG serialization. Motion is SVG presentation, not simulation. |
| 08 Perspective mesh | `integrations/GuiSvg/Proof/PerspectiveMeshProof.cs` | CPU world/view/projection transforms and painter ordering for a convex mesh; no GPU depth test or general clipping. |
| 09 GUI.SVG feature gallery | GUI.SVG C# scene builder in the optional integration | Generated gradients, clipping, patterns, markers, symbols, animation, filter graphs, and lighting definitions. |
| City massing | `integrations/GuiSvg/CityMassingProof/Program.cs` | Renderer representation policy → actual `RenderFrame` → Renderer adapter → Visualizer → GUI.SVG; three skins plus machine-readable counts. Detail reduction is command-level, not buffer compaction. |

## Run the proofs

From the Visualize repository root:

```bash
dotnet run --project samples/PrimitiveGallery/VisualizeSvgProof/VisualizeSvgProof.csproj --configuration Release
dotnet run --project integrations/GuiSvg/Proof/GuiSvgProof.csproj --configuration Release
dotnet run --project integrations/GuiSvg/CityMassingProof/CityMassingProof.csproj --configuration Release
```

The optional GUI.SVG integrations use the GUI repository's source project in development CI. They are downstream consumers; Visualize core should not require GUI.SVG.

## Evidence rules

- Generated SVGs must come from the C# contract path they claim to show; don't redraw output in HTML/JavaScript and call it backend output.
- Keep code, source input, rendered result, and measurable assertions linked from the documentation.
- Identify CPU projection, SVG manifestation, and GPU execution separately.
- Keep SVG animation explicitly presentational unless a real data/control loop drives it.
- Preserve a static, inspectable artifact in README/docs and use CI to regenerate and compare deterministic snapshots.
- Do not describe a proof fixture as production rendering. For GPU claims, require an actual selected backend, device/surface creation, submitted work, verified GPU-produced pixels, and exercised resize/shutdown paths.

## Cross-repository boundary

Renderer decides which representation and draw commands are appropriate; Visualize consumes presentation contracts and resources; GUI.SVG serializes a neutral GUI scene to SVG. Renderer remains independent of both downstream packages. The [Renderer SVG guide](https://github.com/TrentBest/TheSingularityWorkshop.Renderer/blob/development/docs/SVG_LIVING_DOCUMENTATION.md) documents the Renderer-owned state snapshot and the next frame-inspector proof.
