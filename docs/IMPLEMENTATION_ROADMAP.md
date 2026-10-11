# Implementation Roadmap

This roadmap separates architectural preparation from evidence of a functioning renderer. A contract compiling is not a GPU milestone.

## Milestone 0 — Repository and dependency audit

- Verify the existing Renderer public API and package version.
- Verify AnyApp's actual WPF lifecycle and target framework.
- Record existing dependencies, licensing, CI, and release-gate conventions.
- Agree on the Gesture output data needed by the first visualizer scene.
- Choose candidate graphics bindings based on current support and actual integration constraints.

**Exit evidence:** checked-in audit with links to code and a backend decision record.

## Milestone 1 — GPU surface proof

- AnyApp opens a WPF window containing a true GPU-backed surface.
- The backend clears and presents frames.
- Resize, DPI, shutdown, and resource disposal are exercised.
- Device/backend/driver details and failure behavior are recorded.

**Exit evidence:** reproducible run instructions and an actual capture. Do not call a WPF DrawingVisual or placeholder rectangle a GPU backend.

## Milestone 2 — First indexed draw

- Upload a known vertex and index buffer.
- Draw a triangle or simple test mesh.
- Supply view/projection and world transform.
- Resolve material/shader and geometry IDs without exposing raw native handles to Renderer.
- Validate resource lifetime and recreate resources after device loss.

**Exit evidence:** known image, resource validation, test coverage, and timings.

## Milestone 3 — Resource and frame contracts

- Validate the initial Visualizer resource provider and backend contract against the working implementation.
- Define revisions, cache invalidation, disposal, and capability reporting.
- Make invalid/missing resource behavior explicit.
- Add diagnostics for device creation, shader/pipeline failures, and frame submission.

**Exit evidence:** backend-independent contract tests and a concrete backend passing them.

## Milestone 4 — Instancing

- Render a repeated population from compact instance data.
- Preserve stable entity identity across updates.
- Avoid one WPF visual or one CPU animation controller per soldier.
- Add camera movement and deterministic per-instance variation.
- Benchmark at multiple population counts and resolutions.

**Exit evidence:** repeatable population measurements, including frame-time distribution, CPU/GPU times, allocations, and hardware.

## Milestone 5 — Gesture pose path

- Define a minimal, documented Gesture-evaluated pose output.
- Use a CPU reference evaluator as a correctness baseline.
- Upload batched pose/instance data.
- Prototype GPU transition/pose evaluation where operations are parallel.
- Compare CPU and GPU output within declared tolerances.
- Keep FSM transitions, authoritative movement, and interaction validity outside presentation-only results.

**Exit evidence:** deterministic test vectors and a visible moving squad.

## Milestone 6 — Renderer integration

- Consume Renderer RenderFrame and RenderDrawCommand.
- Preserve Event Horizon policy as Renderer-owned.
- Support a near/middle/far representation transition without identity discontinuity.
- Render a physical squad standard as a world object.
- Test occlusion and silhouette through dust/smoke.

**Exit evidence:** a visible scene using the real Renderer output path, not a separately hard-coded mock that bypasses the contract.

## Milestone 7 — Living battlefield slice

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

## Milestone 8 — Browser path

- Render the same conceptual scene using a browser-compatible path, likely WebGPU.
- Keep Blazor/DOM types out of core scene and Gesture contracts.
- Report browser/device limits and define fallback support.
- Compare shared semantics and data layouts without claiming pixel-identical native/browser output unless verified.

## Milestone 9 — Scale and advanced GPU work

Only after measuring the living battlefield slice:
- GPU-driven culling and indirect drawing;
- broader pose evaluation;
- particle systems for dust, smoke, sparks, debris, and weather;
- material/lighting and shadow systems;
- streaming and resource residency;
- asynchronous compute where measured beneficial;
- device-loss recovery;
- regression benchmarks for population, memory, and frame latency.

## Release gates

- Tests and CI pass.
- Public APIs and package dependencies are reviewed.
- Supported backends and hosts are explicitly named.
- Performance claims include repeatable hardware/workload details.
- NuGet publishing remains disabled by default and is enabled only with explicit release approval.
