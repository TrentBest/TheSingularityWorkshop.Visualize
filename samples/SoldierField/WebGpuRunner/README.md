# Soldier Field — Event Horizon Scaling Lab

This is a visible soldier-first rendering target, not a triangle placeholder. The page procedurally constructs an original low-poly science-fiction exoskeleton trooper: modular armor, articulated chassis, helmet/visor, backpack/life-support pack, and a held tool silhouette. It avoids contemporary national military imagery and requires no downloaded model or external image.

## Run

From the repository root:

    py -m http.server 8080

Open http://localhost:8080/samples/SoldierField/WebGpuRunner/ in a browser with WebGPU enabled.

## What it does

- Renders a procedural 3D exoskeleton mesh through WebGPU.
- Uses instancing for a selectable population from 1 to 100,000 soldiers per horizon workload.
- Gives eight horizons independent instance buffers, field depths, tints, and refresh cadences.
- Includes ×10 and ×100 population steps.
- Reports frame interval p50/p95, JavaScript encode/submit-call p50/p95, CPU-side instance-update timing, bytes submitted through instance-buffer writes, and per-horizon update/reuse counters.

## Deliberate stress-test behavior

The selected population is repeated independently in every horizon workload. At population 1, the harness processes eight soldier instances across eight horizons; at population 100, it processes 800. This deliberately makes per-horizon cost visible even when a real near-field volume could not physically contain that many full-size soldiers. It is a stress model, not a physically valid occupancy simulation.

Cadences are demonstration values (1, 1, 2, 4, 8, 16, 32, and 64 frames). They are not imported from the Renderer bucket model yet. This page is not evidence that Renderer already drives the draw schedule.

## Measurement limits

Frame interval includes browser scheduling. JavaScript encode/submit timing is not GPU execution time. Upload counts represent data submitted by this sample, not measured physical memory-bus traffic. GPU timestamp queries, skeletal animation, production camera behavior, focal-beam quality allocation, and repeatable benchmark automation are not yet implemented. Treat the numbers as initial diagnostic telemetry, not a performance claim.

## Next engineering steps

1. Validate the first soldier on actual WebGPU hardware and tune the mesh/camera.
2. Add controlled quality variants and focal-beam detail selection.
3. Feed refresh, invalidation, and reuse decisions from actual Renderer Event Horizon bucket APIs rather than a duplicate page-owned cadence policy.
4. Capture GPU timestamps where supported, warm-up runs, repeated trials, and initial ×10/×100 scaling curves.
5. Increase practical population limits only as buffer/memory constraints and measurements justify it.
