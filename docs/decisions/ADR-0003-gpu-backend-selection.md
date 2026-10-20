# ADR-0003: Measure Before Selecting the GPU Backend

- **Status:** Accepted.
- **Date:** 2026-10-11

## Context

The project needs strong GPU control and future native/browser support. No one API offers the same capabilities and integration path on every target. Low-level APIs expose control but require explicit synchronization, lifetime, and device management.

## Decision

Do not select a permanent graphics API from reputation, assumptions about deprecation, or a triangle-only benchmark. Compare viable native candidates on a real WPF-hosted surface, indexed drawing, instancing, and a compute-to-draw workload. Evaluate browser compatibility separately.

## Current investigation

The initial candidate shortlist and the evidence required before adding a dependency are recorded in [ADR-0005: GPU Candidate Shortlist Before a Native Proof](ADR-0005-gpu-candidate-shortlist.md). This is an investigation snapshot, not a backend selection.

## Consequences

- The initial public contract must report capabilities.
- Backend-specific implementations may differ in shader targets and advanced features.
- Performance reports must include hardware, driver, workload, quality, and frame-time distribution.
- No backend should be described as implemented until it creates and presents a real GPU frame.
