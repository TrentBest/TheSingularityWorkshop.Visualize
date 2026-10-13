# Implementation Roadmap

This roadmap separates architectural preparation from evidence of a functioning renderer. A contract compiling is not a GPU milestone, and a working experiment does not automatically select an architecture.

## Milestone 0 — Array-first backend decision

- Verify the existing Renderer public API and package version.
- Verify AnyApp's actual WPF lifecycle and target framework.
- Record dependencies, licensing, CI, release-gate conventions, and current package/runtime footprints.
- Define the initial data-oriented workload: contiguous geometry/index arrays, revisioned instance-transform/color arrays, visibility/material indices, bulk updates, and compute output consumed by rendering. The initial backend-neutral geometry and instance-buffer contracts now exist; concrete GPU upload, indexed/instanced execution, and compute-to-render remain unimplemented.
- Model Renderer Event Horizons as the first work-selection boundary: distinguish representations that are valid and reusable from those requiring recomputation.
- Define FSM_API's role in coordinating horizon processing groups, dependency order, update cadence, and invalidation; keep GPU-parallel arithmetic and rasterization in the GPU backend.
- Include camera/observer movement, near-field changes, distant-horizon cache reuse, and deliberate invalidation in the benchmark workload.
- Compare native and browser-capable backend candidates against the same conceptual workload.
- Record explicit trade-offs, evidence gaps, and the reason for the selected direction before extending any one prototype.

**Exit evidence:** checked-in array/resource workload, candidate comparison, and a backend decision record. The existing Direct3D 12 experiment does not count as a selection.

## Milestone 1 — Candidate GPU proof

- Prove a real GPU-backed surface for the selected native host.
- Upload known contiguous vertex and index arrays without per-object conversion/allocation.
- Render an indexed object and a repeated population from compact instance data.
- Prove a compute pass whose output is consumed by a graphics pass where the chosen path supports it.
- Exercise resize, DPI, shutdown, resource disposal, and device failure behavior.
- Keep the browser/WebGPU path in the candidate design from the beginning; document capability differences rather than assuming parity.

**Exit evidence:** reproducible run instructions, real hardware capture, device/driver details, array-upload and compute-to-render correctness, and diagnostics. Do not call a WPF DrawingVisual or placeholder rectangle a GPU backend.

## Milestone 2 — Data movement and frame scheduling

- Compare persistent GPU residency, incremental updates, mapped/upload buffers, storage buffers, textures, and hybrid layouts against actual update patterns.
- Define resource revisions, cache invalidation, disposal, and capability reporting.
- Replace any blocking per-frame synchronization with a measured, bounded resource/fence strategy.
- Make invalid/missing resource behavior explicit.
- Add diagnostics for device creation, shader/pipeline failures, uploads, and frame submission.

**Exit evidence:** backend-independent contract tests and a concrete backend passing them, plus measurements of allocations, copies, transfer volume, CPU submission, GPU timings, and frame-time distribution.

## Milestone 3 — Instancing and scaling

- Render a repeated population from compact instance arrays.
- Preserve stable entity identity across updates.
- Avoid one WPF visual or one CPU animation controller per entity.
- Add camera movement and deterministic per-instance variation.
- Benchmark multiple population counts, visible-detail levels, and resolutions.

**Exit evidence:** repeatable population measurements with frame-time percentiles, CPU/GPU timings, allocations, upload volume, memory, and hardware.

## Milestone 4 — Gesture pose path

- Define a minimal, documented Gesture-evaluated pose output.
- Use a CPU reference evaluator as a correctness baseline.
- Upload batched pose/instance data.
- Prototype GPU transition/pose evaluation where operations are parallel.
- Compare CPU and GPU output within declared tolerances.
- Keep FSM transitions, authoritative movement, and interaction validity outside presentation-only results.

**Exit evidence:** deterministic test vectors and a visible moving squad.

## Milestone 5 — Renderer integration

- Implement and test a narrow adapter from Renderer RenderFrame and RenderDrawCommand to VisualizerFrame and VisualizerDrawCommand.
- Preserve Event Horizon policy as Renderer-owned.
- Support a near/middle/far representation transition without identity discontinuity.
- Render a physical squad standard as a world object.
- Test occlusion and silhouette through dust/smoke.

**Exit evidence:** a visible scene using the real Renderer output path, not a separately hard-coded mock that bypasses the contract.

## Milestone 6 — Living battlefield slice

First visually meaningful raWWar scene:
- ground and horizon;
- camera movement;
- a moving squad of recognizable soldiers;
- the physical unit standard carried by the designated soldier;
- dust/smoke and lighting sufficient to test silhouette and occlusion;
- a vehicle or structure;
- near/far representation transitions;
- one real Gesture, such as marching or a vehicle interaction.

**Exit evidence:** reproducible build/run, screenshot or capture, workload definition, performance data, and a clear list of simulated versus visual-only details.

## Milestone 7 — Browser path and parity

- Render the same conceptual scene using a browser-compatible path, likely WebGPU.
- Keep Blazor/DOM types out of core scene and Gesture contracts.
- Report browser/device limits and define fallback support.
- Compare shared semantics and data layouts without claiming pixel-identical native/browser output unless verified.

The browser path constrains the shared data model from the beginning; this milestone measures how much functionality and performance can be shared in practice.

## Milestone 8 — Scale and advanced GPU work

Only after measuring the living battlefield slice:
- GPU-driven culling and indirect drawing;
- broader pose evaluation;
- particle systems for dust, smoke, sparks, debris, and weather;
- physics-grounded lighting and independently scheduled shadow work, with explicit light-source conventions and dependency-aware invalidation;
- lighting and shadow capability experiments across applicable Event Horizons, including a dark base, moving headlamps, and atmospheric occlusion;
- optional LUT/native-math/approximation benchmarks only when profiling identifies a relevant bottleneck;
- streaming and resource residency;
- asynchronous compute where measured beneficial;
- device-loss recovery;
- regression benchmarks for population, memory, and frame latency.

## Lighting and math: deliberately staged

See [Physics-based lighting, shadows, and extensible math](LIGHTING_AND_SHADOWS.md). First establish a small correct lighting path and its source conventions. Then separate the work descriptions, test invalidation, and introduce independent cadence. Only after a representative scene exists should we compare more expensive lighting techniques or alternative math implementations. No standalone math package or LUT dependency is required for the first visible scene.

## Release gates

- Tests and CI pass.
- Public APIs and package dependencies are reviewed.
- Supported backends and hosts are explicitly named.
- Performance claims include repeatable hardware/workload details.
- NuGet publishing remains disabled by default and is enabled only with explicit release approval.
