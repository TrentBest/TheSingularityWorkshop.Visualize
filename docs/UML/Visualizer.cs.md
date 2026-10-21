# Visualizer — source companion

**Purpose:** Orchestrate frame validation and serialized submission to an injected presentation backend using an injected logical-resource provider.

**Implementation source:** [`Visualizer.cs`](../../Visualizer.cs)  
**Class diagram:** [`Visualizer.cs.svg`](Visualizer.cs.svg)  
**Backend contract:** [`IVisualizerBackend.cs`](../../IVisualizerBackend.cs)  
**Frame adapter contract:** [`IVisualizerFrameAdapter.cs`](../../IVisualizerFrameAdapter.cs)  
**Tests and executable evidence:** [Visualize test suite](../../tests) · [observer-driven presentation proof](../gallery/observer-driven-presentation.md)

## Class card

```text
Visualizer : IAsyncDisposable
  + Visualizer(backend, resources)
  + Capabilities : VisualizerCapabilities
  + RenderAsync<TSourceFrame>(sourceFrame, adapter, cancellationToken)
  + RenderAsync(frame, cancellationToken) : ValueTask<VisualizerFrameResult>
  + DisposeAsync() : ValueTask
```

## Contract and lifecycle

- The constructor rejects null backend and resource-provider dependencies.
- Generic submission adapts a source frame, then uses the same validated submission path as a `VisualizerFrame`.
- Submission is serialized through a semaphore.
- Before backend submission, every command's geometry, index range, effective vertex indices, material, and optional instance range are validated against the resource provider.
- Cancellation is observed while waiting for submission and before validation/submission continues.
- Disposal is idempotently represented by one cached task, marks the instance disposed, waits for active submission to finish, and then disposes the backend.
- This class is orchestration. Whether real pixels or GPU work occur depends on the injected backend and its capability report.

## Use case — safely submit a frame

1. Create a backend and resource provider that share a compatible resource namespace.
2. Construct `Visualizer` with those dependencies.
3. Submit a `VisualizerFrame` or adapt a source Renderer frame.
4. On missing resources or invalid index/vertex/instance ranges, expect validation to fail before backend submission.
5. Await `DisposeAsync` when the visualizer's work is complete; it waits for an active submission before disposing the backend.

## Evidence boundary

The observer-driven Renderer-to-Visualize proof validates a CPU/SVG manifestation path. It does not establish hardware-GPU execution unless a specific backend demonstrably submits GPU work.
