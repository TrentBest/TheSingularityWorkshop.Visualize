# Soldier Field — Event Horizon Scaling Lab

This is a visible soldier-first rendering target, not a triangle placeholder. The WebGPU page imports a separate, deterministic geometry asset whose stable semantic part IDs describe the modular armor, articulated chassis, helmet/visor, six provisional neural-shunt endpoints, life-support pack, and held tool silhouette. It avoids contemporary national military imagery and requires no downloaded model or external image. The current mesh is an intentional silhouette blockout; its component inventory is ready to refine without rewriting the renderer.

## Run

From the repository root:

    py -m http.server 8080

Open http://localhost:8080/samples/SoldierField/WebGpuRunner/ in a browser with WebGPU enabled.

## Geometry asset

- `soldier-geometry.js` owns the semantic part inventory, deterministic full-soldier mesh, and `buildPartGeometry(partId, damageEvents)` for isolating one component in local coordinates with persistent impact overlays.
- `soldier-geometry.json` declares the asset ID, coordinate system, vertex format, and shunt endpoint IDs.
- `soldier-part.schema.json` defines the versioned engineering record contract for dimensions, materials, interfaces, manufacturing, inspection, research trials, and traceability.
- `soldier-part-records.json` carries records for all 38 current semantic parts. Unknown tolerances, physical material properties, mating interfaces, and acceptance criteria remain explicitly unresolved rather than invented.
- `soldier-cut-sheet.mjs` derives a stable machine-readable cut sheet from a part record. The lab lets you select any part and download its derived JSON cut sheet.
- `SOLDIER_DOSSIER.md` is the in-universe field dossier for neural shunts, the quiet seam, fabrication, failure signatures, and ambient soldier culture.
- `research-demonstrations.json` is the priced, setup-timed catalog offered through the head scientist's helmet tablet.
- `research-demonstrations.schema.json` defines the versioned catalog contract, including explicit unmeasured status for the fictional most-hit plate premise.
- `soldier-research-simulation.mjs` models ordering, setup advancement, stable per-part damage events, persistent component condition, thermal response, and funding a cooling iteration.
- `research-trial.schema.json` defines the traceable experiment record: trial/catalog identity, part ID, price paid, setup duration, credits remaining, damage event, component condition, and thermal observations.
- `soldier-research-simulation.test.mjs` verifies the state transitions, cost accounting, traceable experiment records, damage persistence, component isolation, and the follow-on trooper's heat complaint after research investment.
- `soldier-geometry.test.mjs` verifies deterministic isolated part geometry and ensures an impact event changes only the geometry for its own part ID.
- Each part has a stable ID, primitive, local-space position/dimensions, and material key. The generated vertex stream is contiguous float32 position + colour data for triangle-list rendering.
- The lore currently enumerates six endpoints (paired ocular and auditory plus olfactory and cortical-stem). The possible five-assembly grouping is explicitly unresolved rather than silently choosing a canon.

## Engineering data and cut sheets

The current records are BIM-inspired engineering data, not a claim of certified or fabrication-ready BIM. Geometry values are nominal render-blockout values in metres. Each part carries a stable ID, revision/maturity, geometry, material status, interfaces, manufacturing status, inspection checks, research questions, and traceability/open issues. Cut sheets are derived from these records so the laboratory view, later drawings, and research-board comparisons can share one source of truth.

## Research-lab demonstration loop

The tablet offers a sternum-plate impact, thermal-dissipation prototype, and non-firing cutaway. Each entry has a stable ID, part reference, fictional research-credit price, setup-time estimate, event sequence, and research question. The player orders the test, advances the simulated setup clock, and runs the demonstration. The impact scene shows a crater and hot edge, a simulated hot barrel, a chassis thermal halo, and the head scientist's explanation. Funding cooling research changes the next simulated thermal result and unlocks a nearby trooper's dry joke about the rifle transferring heat into the exoskeleton.

The research board retains each run so the baseline and funded cooling iteration can be compared. The sternum plate is tagged as the fictional most-hit candidate, but its hit share is null and explicitly unmeasured; campaign simulation should eventually produce the distribution. The costs, temperatures, hit-rate premise, cooling gains, and dialogue are gameplay parameters, not measured engineering data. The selected specimen now uses the same procedural geometry source as the soldier mesh and is rendered as an isolated 3D WebGPU component. Impact events are retained by stable part ID; each adds a deterministic crater/ring overlay to that part's render geometry, and the experiment ledger preserves the matching event ID and condition snapshot. This is still a render-space damage overlay, not a volumetric fracture or a physically validated material response. The impact requests a short rumble through the browser gamepad vibration API when a connected controller exposes compatible support; otherwise recoil remains visual and logged. This best-effort path has not been validated against physical controller hardware. Direct VR hand tracking, grab constraints, controller pose, synthesized breath/grunt audio, and a fully physical hole cut through the plate remain future presentation/simulation work.

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
4. Connect the selected part to a VR hand/grab adapter when a spatial host exists; keep the experiment record and part identity host-independent.
5. Replace the render-space crater overlay with a geometry/damage representation that can feed collision, inspection, and cutaway views.
6. Add procedural breath/grunt audio, a visibly glowing barrel, animated heat distortion, and sensor-aware presentation without confusing these effects with measured temperatures.
7. Capture GPU timestamps where supported, warm-up runs, repeated trials, and initial ×10/×100 scaling curves.
5. Increase practical population limits only as buffer/memory constraints and measurements justify it.
