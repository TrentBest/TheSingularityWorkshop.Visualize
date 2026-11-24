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

The tablet offers a sternum-plate impact, thermal-dissipation prototype, and non-firing cutaway. Each entry has a stable ID, part reference, fictional research-credit price, setup-time estimate, event sequence, and research question. The player orders the test, advances the simulated setup clock, and runs the demonstration. The impact scene shows a crater and hot edge, a simulated hot barrel, a localized animated shader shimmer/glow around the weapon muzzle and chassis heat exchangers, and the head scientist's explanation. The main soldier renderer consumes the same component-condition data each frame for the thermal shader; per-horizon instance-data refreshes remain independent, and thermal-work scheduling is not yet wired to Renderer bucket decisions. Funding cooling research changes the next simulated thermal result and unlocks a nearby trooper's dry joke about the rifle transferring heat into the exoskeleton.

The research board retains each run so the baseline and funded cooling iteration can be compared. The sternum plate is tagged as the fictional most-hit candidate, but its hit share is null and explicitly unmeasured; campaign simulation should eventually produce the distribution. The costs, temperatures, hit-rate premise, cooling gains, and dialogue are gameplay parameters, not measured engineering data. The selected specimen now uses the same procedural geometry source as the soldier mesh and is rendered as an isolated 3D WebGPU component. Impact events are retained by stable part ID; each adds a deterministic crater/ring overlay to that part's render geometry, and the experiment ledger preserves the matching event ID and condition snapshot. This is still a render-space damage overlay, not a volumetric fracture or a physically validated material response. The impact requests a short rumble through the browser gamepad vibration API when a connected controller exposes compatible support, and synthesizes a short shot report, low impact thump, breath intake, and exertion-grunt cue through Web Audio. Unsupported haptics/audio are logged without breaking the trial. Neither the audio cue nor the haptic path has been validated against physical controller hardware. Direct VR hand tracking, grab constraints, controller pose, synthesized breath/grunt audio, and a fully physical hole cut through the plate remain future presentation/simulation work.

## What it does

- Renders a procedural 3D exoskeleton mesh through WebGPU.
- Uses instancing for a selectable population from 1 to 100,000 soldiers per horizon workload.
- Gives eight horizons independent instance buffers, field depths, tints, and Renderer-derived refresh cadences.
- Uses the generated Renderer/FSM decision to draw one selected horizon by default; all-horizon stress mode remains available.
- Allocates 0 / 64 / 256 / 1,024 / 4,096 dust motes for Semantic / Aggregate / Distant / Near / Interactive states. Interactive detail draws 4,096 octahedra (32,768 triangles).
- Includes ×10 and ×100 population steps.
- Reports frame interval p50/p95, JavaScript encode/submit-call p50/p95, CPU-side instance-update timing, bytes submitted through instance-buffer writes, per-horizon update/reuse/policy-skip counters, main-field draw calls, and the active dust-mote/triangle budget.

## Renderer policy bridge

The browser runner no longer owns its horizon names or refresh intervals as independent constants. The checked-in `renderer-policy.json` is generated by `samples/RendererPolicyManifest` using the real .NET `ObserverRepresentationPolicy` and `EventHorizon` contracts from the Renderer repository, then advances each selected representation through `RendererComputationMachine`/FSM_API and records the resulting state.

The generated manifest carries:
- the eight horizon distance bands;
- Renderer-selected representation labels;
- preferred updates per second, converted to frame intervals for a 60 Hz presentation target;
- nine deterministic observer-distance probes across the eight horizon bands;
- a 9 × 5 decision matrix covering distance and minimum representation;
- an attention-floor example in which an entity 64 m away is promoted to the `Interactive` representation by the policy.

The WebGPU runner loads this manifest and uses its 9 × 5 distance/attention decision matrix to select the exact Renderer decision and verified FSM state for the chosen observer distance and minimum representation. In default observer mode, only the selected horizon is refreshed and drawn; the other horizon workloads are skipped and counted. An explicit all-horizon stress toggle restores the eight-workload comparison. Visualize then maps the selected FSM state to a concrete dust-detail budget: 0 motes at Semantic, 64 at Aggregate, 256 at Distant, 1,024 at Near, and 4,096 at Interactive. Each mote is an eight-triangle octahedron, so Interactive allocates 32,768 detail triangles. This mapping belongs to Visualize, not Renderer; Renderer chooses the state, Visualize spends the detail budget. CI regenerates the manifest from the current Renderer `development` source and compares it byte-for-byte with the committed browser input.

This is a real **Renderer policy → Visualize WebGPU workload** bridge, but it is intentionally a first integration step:
- Renderer policy and `RendererComputationMachine`/FSM_API transitions are evaluated in the .NET generator, not executed inside the browser.
- The browser consumes the generated policy snapshot; it does not call Renderer dynamically as the camera moves.
- The soldier mesh is still one geometry across all representation states; the state currently changes the active horizon and dust-mote budget, not soldier mesh LOD. All-horizon stress mode draws all eight workloads.
- The focus override can promote a distant observer sample to Interactive and its 4,096-mote budget, but it is a manual observer control, not eye tracking or a live focal-beam geometry promotion.
- The existing browser cadence loop remains Visualize presentation code; it is not the Renderer/FSM_API bucket scheduler.

The next integration should replace the repeated-per-horizon stress fixture with observer-positioned entities, feed their distance/attention requirements through the same Renderer policy, and prove that promotion, demotion, visibility, and cached reuse change actual submitted geometry and work while preserving stable entity identity.

## Deliberate stress-test behavior

In all-horizon stress mode, the selected population is repeated independently in every horizon workload. At population 1, the harness processes eight soldier instances across eight horizons; at population 100, it processes 800. Default observer-selected mode submits only the one horizon returned by the generated Renderer decision matrix. The repeated eight-horizon layout is a stress model, not a physically valid occupancy simulation.

Cadence intervals (1, 1, 2, 4, 8, 16, 32, and 64 frames at the 60 Hz presentation target) are derived from Renderer Event Horizon update frequencies by the .NET policy generator. The browser consumes a deterministic 9-distance × 5-minimum-representation decision matrix. This is evidence that Renderer policy selects the active WebGPU workload, but it is not evidence that the live Renderer/FSM_API bucket scheduler is executing in the browser.

## Measurement limits

Frame interval includes browser scheduling. JavaScript encode/submit timing is not GPU execution time. Upload counts represent data submitted by this sample, not measured physical memory-bus traffic. GPU timestamp queries, skeletal animation, production camera behavior, screen-space dust budgeting, particle lifetime/physics, focal-beam quality allocation, soldier mesh LOD, and repeatable benchmark automation are not yet implemented. The current dust count is a fixed Visualize-owned budget per FSM representation, not a screen-space measurement. Treat the numbers as initial diagnostic telemetry, not a performance claim.

## Next engineering steps

1. Validate the first soldier on actual WebGPU hardware and tune the mesh/camera.
2. Add controlled quality variants and focal-beam detail selection.
3. Replace the static generated policy matrix with a live host handoff from actual Renderer Event Horizon bucket decisions, including invalidation and safe cached reuse; do not duplicate Renderer policy inside the browser.
4. Connect the selected part to a VR hand/grab adapter when a spatial host exists; keep the experiment record and part identity host-independent.
5. Replace the render-space crater overlay with a geometry/damage representation that can feed collision, inspection, and cutaway views.
6. Add procedural breath/grunt audio, a visibly glowing barrel, animated heat distortion, and sensor-aware presentation without confusing these effects with measured temperatures.
7. Capture GPU timestamps where supported, warm-up runs, repeated trials, and initial ×10/×100 scaling curves.
5. Increase practical population limits only as buffer/memory constraints and measurements justify it.
