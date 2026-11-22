# FSM-driven execution in Visualize

## Architectural decision

Visualize is an FSM-driven execution capability. Its execution contexts and lifecycle belong to Visualize; the host supplies the update heartbeat by advancing the Visualize processing group through FSM_API.

The intended relationship is:

```text
Host / FSM_COS composition
  │ owns scheduling boundary and calls the group update
  ▼
FSM_API processing group
  │ advances Visualize-owned contexts
  ▼
Visualize execution contexts
  │ drive work through explicit lifecycle states
  ▼
Frame preparation → resource validation → backend submission
  │
  ▼
SVG / GPU / other replaceable presentation backends
```

This is not a second scheduler inside Visualize. Visualize may own execution contexts, FSM definitions, and their handles, but FSM_API remains the mechanism that advances those contexts. A host does not need to know Visualize's internal state machine or backend details; it only needs the processing-group contract.

## What the current implementation proves

The current `development` implementation has a backend-neutral `VisualizerFrame`, resource validation, serialized asynchronous submission, and an `IVisualizerBackend` contract. Those are useful execution operations, but the current `Visualizer` API exposes them as direct `RenderAsync` calls.

The core now has a direct project reference to the FSM_API development source and a `VisualizerExecution` owner. Each accepted frame receives a Visualize-owned `IStateContext` and `FSMHandle`; the context advances through Queued, Preparing, Submitting, and a terminal state. The backend task is observed by later FSM updates rather than awaited by the FSM update action.

## Required execution model

Visualize should introduce meaningful contexts for independently advancing work. A context should carry the state and references required to make progress through a real visualization operation; it must not be a decorative wrapper around each public method.

A first end-to-end context should own a concrete frame submission lifecycle, for example:

- **Queued:** a frame and its required references have been accepted for processing.
- **Preparing:** the context validates or prepares the frame for submission.
- **Submitting:** the selected backend is executing the frame.
- **Completed:** the backend result has been recorded and can be observed.
- **Failed / Cancelled:** terminal outcomes preserve diagnostics and cancellation semantics.

The exact state graph must match the actual asynchronous contract. In particular, an FSM update is synchronous: it must not block waiting for GPU/device completion. A state action may initiate asynchronous work and record its completion; subsequent FSM updates observe that completion and advance the context. The backend's existing submission gate and disposal guarantees remain authoritative for device access.

Visualize owns the contexts it creates and the FSM handles/definitions it registers. The application or host owns the source scene/model and decides when to update the processing group. Invalid contexts must stop participating in computation without silently destroying caller-owned data or conflating validity with handle destruction. Explicit Visualize lifecycle APIs must unregister handles when their work is finished or cancelled.

## FSM_API contract and version boundary

The FSM_API `development` source exposes the concise `FSM_API.FsmApi` facade, including:

```csharp
FsmApi.Create.CreateFiniteStateMachine(...);
FsmApi.Create.CreateInstance(...);
FsmApi.Interact.Update("Visualize");
```

The string-backed context contract is `TheSingularityWorkshop.FSM_API.IStateContext`. The host advances the group; Visualize does not run a competing update loop.

Important version boundary: the published `TheSingularityWorkshop.FSM_API` 1.0.13 package remains on the legacy API surface and does not provide the current concise facade or the current context-lifecycle semantics. Visualize therefore references `FSM_API/FSM_API/FSM_API.csproj` directly. Its build workflows check out `TrentBest/FSM_API` on `development` at the matching workspace path. This is a source-level development dependency, not a NuGet package fallback; a standalone source build must provide that sibling checkout. Renderer integration builds also set `WorkshopUseFsmApiRepositorySource=true` and `WorkshopSourceRoot` to the workspace root, so Renderer binds to the same FSM_API project rather than loading its legacy 1.0.13 package alongside the development assembly. No NuGet package is published by this integration.

## Delivery boundary

Visualize is delivered as an executable MicroBundle capability, not as a NuGet package consumed by applications. Ordinary library dependencies may use NuGet where their published API is sufficient; the Visualize capability itself is discovered, resolved, integrity-checked, loaded, and composed by FSM_COS.

A source `ProjectReference` used to compile or test an adapter is not proof of runtime MicroBundle composition. The FSM_COS loader/composition path must resolve the Visualize capability artifact and establish its lifecycle explicitly.

## Evidence required before calling this complete

1. Visualize has a direct, version-correct FSM_API dependency and a meaningful context implementation.
2. Visualize registers its definitions and contexts into a named processing group and owns the resulting handles.
3. One host call to `FsmApi.Interact.Update("Visualize")` advances real Visualize work through observable state transitions.
4. Asynchronous backend completion is observed on later FSM updates without blocking the update call or introducing a second scheduler.
5. Invalid, failed, cancelled, completed, and disposed contexts have tested, explicit lifecycle behavior.
6. Tests prove that context and handle ownership is correct, including cleanup and no accidental destruction of caller-owned scene/model data.
7. FSM_COS resolves and composes the executable Visualize MicroBundle; a project reference or SVG artifact alone is not accepted as that proof.
8. GPU execution claims are backed by a real GPU-backed backend and captured execution evidence. CPU/SVG and browser experiments retain their narrower evidence labels.

## Current status

The first FSM-driven execution owner and frame-submission context are implemented on `development`. The required automated proof must still pass on the current commit. FSM_COS runtime MicroBundle discovery/composition remains a separate integration milestone; a project reference is not accepted as runtime-composition proof.
