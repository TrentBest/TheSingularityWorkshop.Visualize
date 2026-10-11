# ADR-0001: Separate Renderer, Visualizer, Backend, and Host

- **Status:** Accepted as an architectural direction; implementation must validate it.
- **Date:** 2026-10-11

## Context

Renderer selects observer-relative representations and computational detail through Event Horizons. A concrete graphics API handles device resources and command execution. WPF and browsers expose different host surfaces and lifecycles.

## Decision

Keep four responsibilities distinct:
1. Renderer decides what representation is appropriate.
2. Visualizer orchestrates presentation and resource contracts.
3. A graphics backend maps work to a specific API.
4. A host adapter integrates that backend with WPF, browser canvas, or another surface.

## Consequences

- World and Renderer contracts remain free of WPF/native graphics handles.
- A new backend can be evaluated without rewriting Gesture definitions or Experience data.
- Backend-specific features must be capability-reported.
- More interfaces are introduced, so each abstraction must be justified by a real integration and not expanded speculatively.
