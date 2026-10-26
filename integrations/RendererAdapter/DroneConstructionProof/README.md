# Drone Construction → Renderer → Visualize → GUI.SVG proof

This proof uses the actual `ConstructionSimulation` and `ConstructionSnapshotObserver` contracts from the Experiences repository. It publishes one committed snapshot, filters agents by qualified simulation-space identity, maps stable `AgentId` values to Renderer `RenderEntityId` values, adapts the resulting `RenderFrame` through Visualize, and submits the frame to the GUI.SVG backend.

The proof now uses a top-down indexed quadcopter glyph (central body, four arms, and four rotor discs) for each snapshot-selected drone. Each aircraft remains one stable entity and one draw command; the mesh detail does not multiply the authoritative agent or draw-command count. This is still a deliberately tiny three-drone CPU/SVG proof. The next visual slice should add real lane centerlines, sloped transition segments, protected VTOL volumes, and queue/conflict overlays from explicit snapshot/traffic data—not inferred animation.

The CI artifact includes:
- `drone-construction-frame.svg` — the CPU/SVG manifestation.
- `drone-construction-frame.manifest.txt` — run ID, committed tick, observer sequence, qualified space, authoritative population, submitted draw count, and entity IDs.

Run from the Visualize repository root after checking out Experiences into `ExperiencesSource/` and Renderer/GUI into the paths used by the integration workflow:

```bash
dotnet run --project integrations/RendererAdapter/DroneConstructionProof/DroneConstructionFrameProof.csproj --configuration Release -- ./artifacts/drone-construction-frame.svg
```

This is a small correctness proof, not a scale benchmark, live hosted view, GPU proof, or claim of starvation-freedom. It intentionally keeps the cross-repository mapper at the proof/integration boundary; the Experiences simulation remains renderer-independent.
