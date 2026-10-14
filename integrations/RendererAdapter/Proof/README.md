# Renderer → Visualize → GUI.SVG proof

This proof exercises the real .NET contracts across the integration boundary:

1. Observer distances are passed through Renderer’s real `ObserverDistance.Select` and ordered `EventHorizon` policies.
2. The selected horizon chooses a desired representation; `RendererComputationMachine` and FSM_API advance to that representation. The resulting state is recorded in a platform-neutral `RenderDrawCommand` and `RenderFrame`.
3. `RendererFrameAdapter` translates the frame to Visualize contracts while preserving representation and resource identity.
4. `Visualizer` validates the frame's resources and submits it to `GuiSvgVisualizerBackend`.
5. GUI.SVG emits the SVG output.

Run from the Visualize repository root:

```bash
dotnet run --project integrations/RendererAdapter/Proof/RendererFrameToSvgProof.csproj --configuration Release -- ./samples/PrimitiveGallery/generated/10-renderer-selected-frame.svg
```

The project uses the source checkout path in CI when available, the shared sibling Renderer repository when `WorkshopUseRepositorySources=true`, and the declared Renderer package when neither source path is available.

## What this proves—and what it does not

This is a first executable **CPU-side integration proof**: actual Renderer distance-based horizon selection and FSM-backed representation state reach a real Visualize frame and a real GUI.SVG backend. The sample deliberately maps each selected state to a simple specimen shape and color so the states can be distinguished.

It is not yet the production observer → Event Horizon/bucket → selected-work → frame-assembly pipeline, not a GPU backend, and not a benchmark. The sample's geometry mapping is demonstration code, not a universal representation policy. Those are separate milestones in Renderer’s implementation roadmap.
