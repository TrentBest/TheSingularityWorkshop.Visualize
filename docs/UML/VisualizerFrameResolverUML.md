# VisualizerFrameResolver — class diagram

The resolver validates a frame against host-owned resource snapshots. It returns resolved commands in frame order and does not submit native GPU work.

```mermaid
classDiagram
    class VisualizerFrameResolver {
      <<static>>
      +Resolve(VisualizerFrame frame, IVisualizerResourceProvider resources) IReadOnlyList~ResolvedVisualizerDrawCommand~
      -ValidateIndexRange(int commandIndex, VisualizerDrawCommand command, VisualizerGeometry geometry)
      -MissingResource(int commandIndex, string role, ulong resourceId) InvalidOperationException
    }
    VisualizerFrameResolver ..> VisualizerFrame : reads ordered commands
    VisualizerFrameResolver ..> IVisualizerResourceProvider : resolves resources
    VisualizerFrameResolver ..> VisualizerDrawCommand : validates ranges
    VisualizerFrameResolver ..> VisualizerGeometry : checks indices and vertices
    VisualizerFrameResolver ..> VisualizerMaterial : resolves material
    VisualizerFrameResolver ..> VisualizerInstanceBuffer : validates optional instances
    VisualizerFrameResolver ..> ResolvedVisualizerDrawCommand : returns read-only ordered results
```

## Use case

The GPU adapter resolves a CPU-side frame before native submission. Missing resources and invalid index, base-vertex, or instance ranges are rejected with command-indexed diagnostics.

## Boundary

This class is a validation/resolution boundary, not a GPU device, resource allocator, command queue, or proof of hardware execution.
