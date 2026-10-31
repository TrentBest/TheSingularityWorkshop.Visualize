# Drone Construction → Renderer → Visualize → GUI.SVG proof

This proof uses the actual `ConstructionSimulation` and `ConstructionSnapshotObserver` contracts from the Experiences repository. It publishes one committed snapshot, filters agents by qualified simulation-space identity, maps stable `AgentId` values to Renderer `RenderEntityId` values, adapts the resulting `RenderFrame` through Visualize, and submits the frame to the GUI.SVG backend.

The proof now uses a top-down indexed quadcopter glyph (central body, four arms, and four rotor discs) for each snapshot-selected drone. Each aircraft remains one stable entity and one draw command. It also draws three **illustrative scenario-fixture overlays**: two directed corridor centerlines plus a sloped transition, a protected VTOL-volume outline, and a perimeter bypass/rejoin path. These overlays are authored fixture geometry, deliberately separate from the authoritative `ConstructionSimulation` snapshot; they do not claim that continuous lanes, traffic admission, queue behavior, or protected-volume enforcement are implemented. The proof remains a tiny three-drone CPU/SVG integration check, not a scale benchmark.

The CI artifact includes:
- `drone-construction-frame.svg` — the CPU/SVG manifestation.
- `drone-construction-frame.manifest.txt` — run ID, committed tick, observer sequence, qualified space, authoritative population, total submitted draws, authoritative drone draws, fixture overlay count/source, stable entity IDs, geometry/material resource counts, snapshot capture timestamp, and elapsed snapshot age at submission. Snapshot age is measured as wall-clock elapsed time within this proof process; it is not a simulation-time age or a stale-snapshot policy.

Run from the Visualize repository root after checking out Experiences into `ExperiencesSource/` and Renderer/GUI into the paths used by the integration workflow:

```bash
dotnet run --project integrations/RendererAdapter/DroneConstructionProof/DroneConstructionFrameProof.csproj --configuration Release -- ./artifacts/drone-construction-frame.svg
```

This is a small correctness proof, not a scale benchmark, live hosted view, GPU proof, or claim of starvation-freedom. It intentionally keeps the cross-repository mapper at the proof/integration boundary; the Experiences simulation remains renderer-independent.
