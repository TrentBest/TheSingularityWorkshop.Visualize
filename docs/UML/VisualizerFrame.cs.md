# UML — `VisualizerFrame.cs`

**Source:** [`VisualizerFrame.cs`](../../VisualizerFrame.cs)

## Responsibility

Defines the backend-neutral snapshot passed to Visualizer: view transforms, ordered indexed draw commands, representation labels, and semantic entity identity.

## Class diagram

```mermaid
classDiagram
    class VisualizerFrame {
      +VisualizerView View
      +IReadOnlyList~VisualizerDrawCommand~ Commands
    }
    class VisualizerView {
      <<record struct>>
      +Vector3 ObserverPosition
      +Matrix4x4 View
      +Matrix4x4 Projection
    }
    class VisualizerDrawCommand {
      <<record struct>>
      +VisualizerResourceId Geometry
      +VisualizerResourceId Material
      +Matrix4x4 WorldTransform
      +int IndexCount
      +int FirstIndex
      +int BaseVertex
      +VisualizerRepresentation Representation
      +VisualizerResourceId Instances
      +int InstanceCount
      +int FirstInstance
      +VisualizerEntityId Entity
    }
    class VisualizerRepresentation {
      <<enumeration>>
      Semantic
      Aggregate
      Distant
      Near
      Interactive
    }
    class VisualizerEntityId {
      <<record struct>>
      +ulong Value
      +bool IsValid
      +Invalid
    }
    VisualizerFrame *-- VisualizerView
    VisualizerFrame o-- VisualizerDrawCommand
    VisualizerDrawCommand --> VisualizerRepresentation
    VisualizerDrawCommand --> VisualizerEntityId
```

## Observed behavior

- The constructor snapshots the incoming command sequence into an array-backed read-only list, preserving command order even if the producer later mutates or reuses its sequence.
- `VisualizerDrawCommand` rejects invalid geometry/material IDs, non-positive index/instance counts, negative range starts, and non-default instance ranges without a valid instance resource.
- `VisualizerEntityId.Value == 0` is the invalid/unassigned sentinel.
- The representation enum records the upstream Renderer choice; it does not itself perform LOD selection.

## Boundaries and non-claims

This file defines data contracts only. It does not resolve resources, choose representations, submit GPU work, or prove a presentation backend exists.
