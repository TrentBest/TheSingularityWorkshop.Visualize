# Capsule Clinic → Renderer → Visualize → GUI.SVG proof

This executable integration proof captures a seeded, authoritative `CapsuleClinicLocalSession` snapshot and calls the reusable `CapsuleClinicRenderFrameFactory` from the separate integration library. That library maps presentation data to deterministic Renderer draw commands and a Visualize resource catalog; the proof adapts the `RenderFrame` through the existing Renderer-to-Visualize adapter and renders through GUI.SVG.

It deliberately keeps puzzle rules and command authority in Experiences. The integration library is presentation mapping only: it does not advance game rules, mutate the snapshot, or authorize intents. This proof is a CPU/SVG integration check, not a browser interaction runtime, GPU renderer, network host, or performance benchmark.

## Run

From the Visualize repository root, with Experiences checked out into `ExperiencesSource/`, Renderer source checked out into `integrations/RendererAdapter/RendererSource/`, and GUI source checked out into `integrations/GuiSvg/GuiSource/`:

```bash
dotnet run --project integrations/RendererAdapter/CapsuleClinicProof/CapsuleClinicFrameProof.csproj --configuration Release -- ./artifacts/capsule-clinic-frame.svg
```

The command produces the SVG and a JSON manifest recording pane/board dimensions, command counts, and the CPU/SVG backend. The integration library is reusable within this source-checkout build, but the existing Experiences browser host does not yet reference it; see [Experiences issue #33](https://github.com/TrentBest/TheSingularityWorkshop.Experiences/issues/33) for the host migration and cross-repository dependency decision.
