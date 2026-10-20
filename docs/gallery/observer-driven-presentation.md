# Observer-driven presentation: Renderer → Visualize → GUI.SVG

**Purpose:** prove that changing an observer position changes Renderer representation decisions and that those decisions survive the adapter into generated presentation output.

## Run

From the Visualize repository root, with the sibling Renderer and GUI repositories available:

```sh
dotnet run --project integrations/GuiSvg/ObserverFrameProof/ObserverFrameProof.csproj --configuration Release -- ./samples/ObserverFrameProof/generated
```

CI checks out Renderer and GUI source into the integration locations expected by the project references. The proof writes two SVGs and a machine-readable report.

## What is exercised

- The actual `ObserverDistance.Select` policy chooses Interactive, Near, or Distant representation for four stable entities.
- Two real Renderer `RenderFrame` snapshots are built at observer X=0 and X=50.
- `RendererFrameAdapter` preserves command count, entity identity, and representation.
- The Visualize resource catalog selects a detailed eight-segment marker for Interactive/Near and a compact diamond for Distant.
- `Visualizer` submits the adapted frames to the GUI.SVG backend. Assertions require one output draw per entity and different representation decisions between observer positions.

## Evidence boundary

This is an executable **CPU/SVG integration proof**, not a GPU benchmark. Marker geometry is deliberately a small fixture, and index counts are fixture workload values. The proof does not submit GPU commands, measure GPU pixels, timing, allocation rate, transfer volume, or frame-time percentiles. It does not claim a production observer scheduler or event-horizon cache.

The next performance gate must feed equivalent geometry and workloads to a real GPU backend and measure CPU submission, GPU pass time, transfers, synchronization, allocations, and output correctness on a named device/driver/backend.
