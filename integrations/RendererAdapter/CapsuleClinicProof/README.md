# Capsule Clinic → Renderer → Visualize → GUI.SVG proof

**Trent Best**

This executable integration proof captures a seeded, authoritative `CapsuleClinicLocalSession` snapshot and consumes the reusable `CapsuleClinicRenderFrameFactory` owned by the Experiences repository. Experiences owns the game-specific mapping; Visualize owns the generic renderer pipeline. The proof adapts the resulting `RenderFrame` through the Renderer-to-Visualize adapter and renders through GUI.SVG.

The integration is presentation mapping only: it does not advance game rules, mutate the snapshot, or authorize intents. This is a CPU/SVG integration check, not a browser interaction runtime, GPU renderer, network host, or performance benchmark.

## Run

From the Visualize repository root, with Experiences checked out into `ExperiencesSource/`, Renderer source checked out into `integrations/RendererAdapter/RendererSource/`, and GUI source checked out into `integrations/GuiSvg/GuiSource/`:

```bash
export WorkshopVisualizeRoot="$PWD"
export WorkshopGuiSvgProjectPath="$PWD/integrations/GuiSvg/GuiSource/src/SVG/TheSingularityWorkshop.GUI.SVG.csproj"
dotnet run --project integrations/RendererAdapter/CapsuleClinicProof/CapsuleClinicFrameProof.csproj --configuration Release -- ./artifacts/capsule-clinic-frame.svg
```

The command produces the SVG and a JSON manifest recording pane/board dimensions, command counts, and the CPU/SVG backend. The shared Experiences integration project requires the Visualize source checkout; the environment variable above points it at this repository root.

![Trent Best avatar](../../../Avatar.jpg)
