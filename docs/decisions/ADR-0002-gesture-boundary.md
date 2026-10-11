# ADR-0002: Keep Gesture Semantics Outside Visualizer

- **Status:** Accepted as an architectural direction; the standalone package remains proposed.
- **Date:** 2026-10-11

## Context

Workshop Gestures are procedural physical actions: pose sequences plus mathematical transitions, progression, constraints, and bounded individual variation. They are not merely baked animation clips.

## Decision

Gesture semantics and reference evaluation belong in a reusable Gestures capability. Visualizer consumes evaluated presentation data and may execute data-parallel pose work on a GPU when the operation and backend support it.

## Consequences

- Visualizer does not define what “enter cockpit,” “march,” or “operate hatch” means.
- Gesture providers can belong to the MicroBundle that owns the physical interaction.
- A CPU reference evaluator can establish correctness before GPU kernels are optimized.
- The future TheSingularityWorkshop.Gestures package must not depend on Visualizer or Renderer.
- GPU-generated pose data remains presentation state and cannot silently alter authoritative FSM/domain state.
