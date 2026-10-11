# Visualizer Architecture

**Status:** Architecture proposal with an initial contract implementation. GPU execution remains future work.

## 1. Why this project exists

Renderer and Visualizer solve different problems.

Renderer asks: *What must computationally exist for this observer to perceive and interact with the world?*

Visualizer asks: *How do we efficiently execute the resulting presentation work on a GPU and present the result through this host?*

This separation is important to the Workshop's core hypothesis: reduce cost by reducing unnecessary computational detail before drawing, then execute the remaining parallel presentation work efficiently.

## 2. Data flow

~~~text
Authoritative Experience state
        |
        v
FSM_API: stateful coordination and scheduled evaluation
        |
        v
Gesture procedures / evaluated pose data
        |
        v
Renderer: observer perception + Event Horizon policy
        |
        | horizon representations + render work
        v
FSM_API: schedule horizon work, dependencies, cadence, invalidation
        |
        v
Visualizer contracts
  - batched arrays / resource revisions
  - device capabilities
  - backend execution / diagnostics
        |
        v
GPU backend
  - reuse valid distant representations
  - upload changed arrays
  - compute / graphics passes
  - synchronization and composition
        |
        v
Host surface -> pixels
  - AnyApp + WPF first
  - browser + WebGPU path
~~~

This is the intended responsibility flow, not a claim that all stages are implemented. Renderer determines what representation is warranted for an observer. FSM_API coordinates stateful work and scheduling; Visualizer and its GPU backend execute the due presentation work.

### Event Horizons as the work boundary

Event Horizons are observer-relative computational boundaries, not simply conventional mesh LOD bands. They determine which representation is required and how often it needs evaluation. A far horizon may retain a valid cached representation while near horizons change every frame. A horizon's representation can be a texture, impostor, mesh/instance buffer, or another suitable cached form; it does not have to be a separate full-screen image.

The intended per-frame sequence is:

1. Determine observer/camera state and the current perception budget.
2. Have Renderer classify the required representations and expose the work that is newly needed or invalidated.
3. Use FSM_API processing groups to coordinate dependencies, cadence, and which horizon work is due.
4. Reuse valid horizon output; recompute and transfer only what changed or must be refreshed.
5. Execute compact arrays and batched work on the GPU, then compose near and far representations correctly.

Cache invalidation must account for relevant world changes, observer movement/orientation, resource revisions, lighting, and horizon-specific freshness rules. The implementation must define and test these rules; distance alone is not a sufficient invalidation policy.

**Role separation:** FSM_API is the stateful coordination/scheduling layer, not the GPU rasterizer. The GPU performs data-parallel arithmetic and graphics work suited to it. The backend should receive batched arrays and compact work descriptions, not require one managed wrapper or draw submission per world entity. The optimization target is the complete perception-to-presentation path, including cache reuse and final composition.

## 3. Ownership boundaries

| Concern | Owner |
| --- | --- |
| World identity, soldier history, ranks, medals, equipment, orders | Experience / domain |
| Stateful procedure and schedule | FSM_API and domain procedure |
| Ideal physical procedure, pose sequence, transition mathematics, bounded variation | Gestures capability (proposed package) |
| Observer-relative representation and Event Horizon policy | Renderer |
| Resource descriptions, GPU execution, frame submission, backend diagnostics | Visualizer |
| Native window, input lifecycle, application startup | Host |
| Composition and capability assembly | FSM_COS |

These are conceptual ownership boundaries. They do not require every concept to become a separate assembly immediately.

## 4. Resource identity and lifetime

Renderer commands identify geometry and materials by RenderResourceId. Visualizer currently defines its own VisualizerResourceId and VisualizerFrame so its core can build and evolve independently of the Renderer package release cadence. A GPU backend must not assume that an ID is a raw device pointer or that the ID alone describes all required data.

Visualizer needs a resource-provider boundary that resolves logical IDs to immutable or versioned resource descriptions. A backend can then map those descriptions to native GPU objects and maintain a device-local cache.

Resource identity must be stable enough for caching, but device resources must be disposable. On device loss or backend replacement, GPU allocations can be recreated from the resource provider and source assets.

Resource revisions, ownership, eviction, upload staging, and residency policy are still design work. Do not turn the initial registry into a permanent content-pipeline API without evidence.

## 5. Frame contract

A frame contains observer-relative view data and an ordered snapshot of draw commands. The backend is responsible for translating that snapshot into supported GPU work.

The current Renderer command model includes geometry and material IDs, world transform, index count, first index, base vertex, and selected representation. It does not by itself provide vertex buffers, textures, shaders, material parameters, or a native surface. Visualizer therefore must not claim that accepting a RenderFrame alone is enough to draw it.

The initial contracts introduce a resource-provider interface and backend capability report. The first concrete backend must define and validate:
- geometry layout and index format;
- material/shader mapping;
- resource upload and revision behavior;
- unsupported representation handling;
- frame synchronization and presentation;
- device loss and cleanup.

## 6. Backend and host are different axes

A graphics backend is not the same thing as a host adapter.

The backend handles device, queue, resources, shader/pipeline execution, and synchronization. The host adapter connects the backend to a WPF window or browser canvas and handles resize, DPI/device scale, focus/input coordination, lifecycle, and presentation integration.

A native Windows backend may be the first implementation. The browser path should use browser-exposed APIs—most plausibly WebGPU—rather than pretending that native Direct3D/Vulkan handles can cross into the browser.

The public contract must expose capabilities instead of assuming every backend supports every feature. It must also avoid becoming a lowest-common-denominator that prevents GPU-driven techniques where available.

## 7. Gestures are procedures, not baked animation clips

Gesture definitions describe a physical action through poses and mathematical transitions. They may be provided by the capability that owns the interaction, such as a vehicle providing its own cockpit-entry procedure.

Visualizer must not decide what “enter cockpit” means or own the state machine for climbing into a vehicle. It consumes evaluated presentation data: transforms, pose selections or palettes, blend/progression values, and per-instance variation. The exact canonical pose schema belongs in the Gestures design, not in the graphics backend.

The GPU may evaluate a shared Gesture over many entities when its mathematics and constraints are data-parallel. Context-sensitive decisions, collision resolution, FSM transitions, and authoritative state changes remain outside a GPU visual result unless a deliberate deterministic contract says otherwise.

## 8. Event Horizons apply before and during presentation

Renderer's Event Horizons are changes in computational representation, not merely mesh simplification. A distant squad may need formation motion, faction identity, silhouette, and a visible unit standard, but not detailed finger articulation for each soldier.

Visualizer should support multiple representations and update strategies without redefining the policy that selects them. It may use instancing, compact buffers, GPU culling, compute-generated pose data, or indirect drawing when supported and measured.

## 9. Non-goals

Visualizer is not:
- a new game engine or world simulation;
- an authoritative soldier database;
- an FSM replacement;
- the owner of Gesture semantics;
- a WPF-only control toolkit;
- a requirement that every entity have a CPU object updated every frame;
- a claim that “GPU accelerated” automatically means fast.

## 10. Architectural invariants

1. The Experience remains authoritative.
2. GPU state is derived, disposable, and rebuildable.
3. Renderer owns observer-relative representation decisions.
4. Gestures own procedural motion meaning; Visualizer consumes evaluated motion.
5. Host-specific APIs do not leak into the backend-neutral contract.
6. Backend-specific features are capability-reported, not silently assumed.
7. Performance claims require repeatable measurements on stated hardware and workload.
