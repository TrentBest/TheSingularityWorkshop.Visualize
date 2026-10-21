# Visualizer — UML

**Source:** [`Visualizer.cs`](../../Visualizer.cs)  
**Companion and use case:** [Visualizer.cs.md](Visualizer.cs.md)

```mermaid
classDiagram
    direction TB
    class IVisualizerBackend {
        <<interface>>
        +Capabilities VisualizerCapabilities
        +RenderAsync(frame, resources, cancellationToken) ValueTask~VisualizerFrameResult~
        +DisposeAsync() ValueTask
    }
    class IVisualizerResourceProvider {
        <<interface>>
        +TryGetGeometry(id, geometry) bool
        +TryGetMaterial(id, material) bool
        +TryGetInstances(id, instances) bool
    }
    class IVisualizerFrameAdapter~TSourceFrame~ {
        <<interface>>
        +Adapt(sourceFrame) VisualizerFrame
    }
    class Visualizer {
        +Capabilities VisualizerCapabilities
        +RenderAsync(frame, cancellationToken) ValueTask~VisualizerFrameResult~
        +RenderAsync~TSourceFrame~(sourceFrame, adapter, cancellationToken) ValueTask~VisualizerFrameResult~
        +DisposeAsync() ValueTask
        -submissionGate SemaphoreSlim
        -disposed int
    }
    Visualizer --> IVisualizerBackend : submits to
    Visualizer --> IVisualizerResourceProvider : validates against
    Visualizer ..> IVisualizerFrameAdapter~TSourceFrame~ : optionally adapts
```

## Structural contract

Visualizer serializes submissions, validates geometry/index/vertex/material/instance references before invoking the backend, and coordinates backend disposal after active work finishes. Pixel and GPU behavior are properties of the injected backend, not of the orchestration class itself.
