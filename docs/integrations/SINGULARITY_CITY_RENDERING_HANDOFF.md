# Singularity City Rendering — WebPage LLM Handoff

**Status:** implementation coordination contract and staged plan. This describes the target and evidence boundary; it does not claim that a persistent, zoomable, street-level city renderer already exists.

## Mission

Build the rendering foundation for Singularity City from an engineered, map-like city plan to district/aerial views and eventually street-level exploration. The same coherent city should remain recognizable and addressable while the observer moves between city overview, district, street, building plan, interior, and vehicle/passenger space.

The goal is not one enormous static picture. It is the same world represented at multiple useful levels of computational detail, with stable identity, reproducible output, and a traceable path from authored spatial data to a visible frame.

> **One world, many observer-relative representations.**


## Observer choice and the hidden laboratory switch

The user should be able to choose how to observe the same city. The ordinary experience starts with the useful top-down plan/map view; a later street-level/FPS view must be a different observer and presentation of the same spatial world, not a separate game level or a replacement city dataset.

A deliberately hidden **Magic switch** in the laboratory is a discovery for curious visitors. It should be a physical, interactive 3D switch in the lab environment: flipping it enters first-person observation; flipping it again returns to the top-down plan. Do not expose it as a prominent global mode button or spoil the discovery in ordinary navigation. The host must preserve world identity and relevant world state across the transition, with a safe fallback if FPS content is not yet available.

Ownership:
- WebPage owns the secret's placement/discovery, interaction, mode transition, camera controls, and return path.
- The world/experience model owns the lab and switch identity/placement.
- Renderer supplies observer-dependent representation and work decisions for the new observer; it must not assume that switching camera mode means changing worlds.
- Visualize renders the frame supplied to it. The switch itself is not a substitute for a real FPS-capable scene/backend.

The initial honest milestone is the mode-transition contract and a shared scene visible in both modes. Realistic FPS presentation is an incremental target: geometry and scale first, then materials/lighting, street furniture/vehicles, and populated activity as authored data and backend capability permit. Label any temporary proxy scene as a prototype; do not call it realistic until inspected and measured.

## Ownership boundaries

| Concern | Owner | Contract |
| --- | --- | --- |
| What exists and where | WebPage / Experience world model and authored content | Stable IDs, coordinate frames, units, footprints, parent-child placement, routes, entrances, walkability, and semantic kinds. Renderer must not invent authoritative world facts. |
| Observer-relative computational work | Renderer | Select representation and computational responsibility from observer context, visibility, intent, semantic importance, interaction state, budget, and policy. Distance is one signal, not the whole model. |
| Presentation data and drawing | Visualize | Consume a platform-neutral frame and manifest geometry/material/instance data through supported backends. A backend's existence does not prove every upstream stage is connected. |
| Camera, navigation, interaction, and experience UI | WebPage | Pan/zoom/focus transitions, object selection, navigation between scales, controls, labels, overlays, and host feedback. Camera state is not world truth. |
| Stateful process behavior | FSM_API and the owning domain | Authoritative state and explicit update cadence. Renderer can schedule rendering work; it must not become authority for transit, Digiten behavior, physics, or city rules. |
| Composition | FSM_COS / host composition boundary | Assemble verified capabilities without making Renderer depend upward on the composition layer. |

The WebPage LLM should integrate against documented public contracts and current development sources. Do not duplicate Renderer policy or Visualize drawing/backend logic inside WebPage to make the demo appear complete.

## Current evidence and limits

### Renderer-backed city proof (new, development)

`integrations/GuiSvg/CityMassingProof/Program.cs` now constructs a platform-neutral `RenderFrame` for the deterministic city massing fixture. Each face's center is measured in the fixture's normalized world coordinates, passed through `ObserverRepresentationPolicy`, mapped into a `RenderDrawCommand`, then adapted by `RendererFrameAdapter` into the Visualize frame consumed by `GuiSvgVisualizerBackend`.

This closes an important wiring gap in the **proof path**: city draw commands are no longer all stamped `Distant` without asking Renderer. The proof reports counts by selected representation. The policy thresholds are deliberately fixture-scale values and must not be mistaken for real city-meter thresholds. The current proof also omits window-detail face commands for Distant/Aggregate/Semantic decisions while retaining silhouette faces, so it demonstrates command-level submission reduction; the shared source vertex/index buffers are not compacted.

Evidence boundary: this is still a deterministic CPU/SVG massing proof. It demonstrates a deliberately narrow command-level reduction (window-detail face commands omitted at distant representations), not vertex/index-buffer compaction, general visibility culling, bucket scheduling, a GPU backend, persistent authored city data, physically realistic streets, or FPS navigation. The source change has been committed; runtime counts and rendered captures still need CI or a local execution before we call the visual result verified. The next test is to build/run the project with the adapter's supported Renderer dependency and inspect the emitted SVGs plus representation counts; only after that should this path be called a verified end-to-end proof.


### Visualize

- The city-massing sample builds a deterministic procedural city model, submits it through the public Visualizer / VisualizerFrame path, and manifests it through the GUI.SVG backend.
- Three skins are generated from the same geometry and draw ordering. Skins change materials/background, not city identity or ordering.
- The current model includes a procedural street lattice, varied building heights, façade strips, and three landmarks.
- This is a CPU-side SVG manifestation proof, **not** a production GPU renderer, calibrated urban model, populated persistent city, or Renderer-driven city pipeline.
- VisualizerFrame, resource IDs, geometry/material/instance contracts, and the backend seam are the intended presentation boundary.

### Renderer

- Development contains Event Horizon, bucket/dependency, observer representation policy/coordinator, and platform-neutral frame/draw-command contracts with tests.
- Observer decisions can select representations and coordinate bucket refresh/reuse through FSM-backed computation.
- These executable pieces are not yet one complete world → observer → horizon → bucket work → frame → backend runtime.
- The city-massing proof now asks Renderer policy for a representation decision per face and carries those labels through `RenderFrame` and the Visualize adapter into GUI.SVG. This is a CPU-side policy-consumption specimen, not yet a full Renderer pipeline: representation labels do not currently reduce geometry, visibility culling and bucket scheduling are not connected, and no GPU backend or FPS navigation is demonstrated.

### WebPage

- WebPage already has spatial/camera and city-experience foundations plus a zoomable-world design contract in its docs.
- Consolidation should join those foundations behind shared spatial data and renderer-facing contracts, not create a third competing city representation.
- A diagram, generated SVG, fixture, or animated explainer must be labelled honestly; none alone proves live simulation or persistent world state.

## Progressive visual contract

| View | Representation goal | Useful interactions |
| --- | --- | --- |
| **City overview / plan** | Legible city/coast silhouette, districts, major streets, transit, port, airport, landmarks, broad activity aggregates | Select district, route, destination, landmark, or civic service |
| **District / aerial** | Street network, blocks, footprints, parks, stations, vehicles, and contextual activity | Select street/building/stop; inspect routes and activity |
| **Street** | Readable building fronts/entrances, paths, vehicles, pedestrians, street furniture, and observer position | Walk, approach, inspect, talk, board, enter |
| **Building / venue plan** | Footprints, rooms, walls, doors, stairs, circulation, utilities, and floor identity where authored | Choose a room, door, or destination |
| **Vehicle / passenger space** | Engineered plan and schematic first; exterior and interior representations later | Inspect driver/operator, boarding areas, route, and move within the vehicle |
| **Interior / room** | Focused local geometry and interactable content, with a return path to the parent context | Use room capabilities and return to building/street |

These are staged targets, not a claim that all views currently exist. Tune zoom thresholds from actual viewport behavior. A scale change must not silently replace entity identity or reset world state.

## Renderer / Visualize contract

1. The world supplies stable entity IDs, source revision, spatial transform, coordinate frame, units, semantic role, and bounds/geometry references.
2. The observer supplies position/orientation or another explicitly defined view context, viewport/projection requirements, intent, and relevant interaction/budget signals.
3. Renderer chooses representation and eligible computational work. Results should identify selected representation and explain freshness/invalidation where applicable.
4. Renderer-selected work becomes a deterministic, platform-neutral frame with stable resource/entity references. Avoid building the entire city into one unstructured SVG string in WebPage.
5. Visualize consumes the frame and manifests it through an available backend. GUI.SVG is a useful inspectable proof backend; it is not evidence of GPU execution.
6. WebPage presents the result and owns camera controls, navigation, labels, selection, and interaction feedback.
7. Tests should verify identity preservation, deterministic scene/frame generation, valid bounds/transforms, observer-dependent representation changes, and correct output from the chosen backend.

Do not introduce a new world schema or generic renderer pipeline before checking existing WebPage spatial types, Renderer contracts, and Visualize frame/resource APIs. Prefer a narrow adapter with explicit coordinate conversion over copied domain logic.

## First implementation milestone

Turn the current city-massing proof into a traceable, testable vertical slice without overstating it:

- [ ] Inventory current city-massing geometry and public Visualize frame/resource contracts.
- [ ] Give every generated city object a stable ID and typed semantic role; preserve identity across skins and view representations.
- [ ] Separate authored/world inputs from deterministic procedural geometry generation, material styling, and backend manifestation.
- [ ] Add explicit spatial units, coordinate frame, source revision, and stable object transforms to the scene input contract.
- [ ] Connect a minimal, actual Renderer observer/representation decision to a selected city object or cohort, then into frame assembly. Do not substitute the specimen-color demo for this integration.
- [ ] Add tests for stable IDs, deterministic output, valid geometry, representation selection, and frame content.
- [ ] Keep SVG output as a generated artifact that CI verifies and uploads; label it as CPU-side SVG proof.
- [ ] Write a WebPage integration note describing public inputs/outputs and exactly which controls and state WebPage owns.

**Exit criterion:** one deterministic city scene passes through real Renderer selection and the Visualize public frame/backend path, with tests proving identity and a CI-generated image/artifact. This is the first vertical slice—not the full city.

## Next milestones after the vertical slice

1. **Plan-view navigation:** pan, zoom, select, focus, and return using WebPage camera/navigation contracts and the shared city scene.
2. **Spatially coherent district view:** progressively reveal street/block/building detail from the same IDs and coordinates; avoid a separately authored image per zoom level.
3. **Representation and invalidation:** drive bucket refresh/reuse from actual Renderer policy and dependencies; unrelated city regions should remain reusable when possible.
4. **Aerial and street views:** define explicit camera/projection and coordinate conversion contracts; reuse spatial identities and authored placements.
5. **Street-level presence:** connect Digitens, vehicles, entrances, and street furniture to authoritative domain state; render their state, do not fake it with a looping backdrop.
6. **Infrastructure journeys:** station, maglev, flying taxi, harbor, and cargo terminal become reusable places/vehicles with engineered plan views and later inhabitable interiors.
7. **Scale and performance evidence:** measure scene/frame generation, allocations, update work, draw/instance counts, cache reuse, and output correctness as total population grows versus observer-relevant population.
8. **GPU backend evaluation:** compare native/browser candidates against the same deterministic workload. A GPU sample or successful compile is not hardware validation; record supported devices and real runtime evidence.

## Coordination rules for WebPage

- WebPage owns user experience, camera, navigation, selection, overlays, and experience-specific interaction.
- Renderer owns observer-relative representation and computation policy; Visualize owns frame manifestation and backend details.
- Authoritative city, Digiten, transit, and physics state remains in its domain owner.
- Use current development branches as integration source of truth. Long-term branch policy is only master and development; do not create additional feature branches for this effort.
- Do not edit Physics as part of this task. If physical behavior is needed, define the consumer contract and coordinate with the Physics owner.
- Do not publish packages, deploy, merge to master, or claim a production GPU/street-view capability without explicit validation and approval.
- Keep documentation living and source-grounded: link to actual types, samples, tests, and generated artifacts wherever possible.

## Working references

- [Visualize city-massing proof](../../samples/CityMassing/README.md)
- [Visualize Renderer frame adapter](../../integrations/RendererAdapter/RendererFrameAdapter.cs)
- [Visualize GUI.SVG backend](../../integrations/GuiSvg/GuiSvgVisualizerBackend.cs)
- [Renderer implementation roadmap](https://github.com/TrentBest/TheSingularityWorkshop.Renderer/blob/development/docs/IMPLEMENTATION_ROADMAP.md)
- [Renderer living explainer](https://github.com/TrentBest/TheSingularityWorkshop.Renderer/blob/development/docs/LIVING_RENDERER.md)
- [WebPage zoomable-world design contract](https://github.com/TrentBest/WebPage/blob/development/docs/SINGULARITY_CITY_ZOOMABLE_WORLD_AND_EXPERIENCES.md)
- [WebPage living infrastructure contract](https://github.com/TrentBest/WebPage/blob/development/docs/integrations/SINGULARITY_CITY_LIVING_INFRASTRUCTURE.md)


## Complementary proving ground: an impossible-scale monarch migration

The Renderer/Visualize workstream proposes a second scene alongside Singularity City: place the observer inside a natural environment during a monarch migration, but at a deliberately impossible population—far beyond what nature can sustain. The intent is an immersive, primitive experience of a phenomenon larger than an individual could ordinarily witness, helping people understand something lost to us. This is an artistic/technical stress test inspired by monarch migration, not an ecological simulation claim.

This scene should not become a disconnected demo or a separate renderer. It should reuse the same observer, representation, frame, identity, cadence, and measurement contracts being established for the city.

### Why this scene matters

City massing stresses spatial scale and representation of mostly static structures. A migration stresses enormous populations of moving entities, overlapping trajectories, depth, occlusion, and continuous change. It also gives us a meaningful test for concentrating computation: a single butterfly's wings, airborne particles, or a patch of smoke/pollen lit by sunlight may deserve far more work than a vast distant region when that detail is perceptually important.

### Required experience and evidence

- The observer can stand within the environment, see a dense migration crossing foreground/middle/background, and change viewpoint without the population popping, duplicating, freezing, or changing its underlying identity.
- Individual butterflies may receive high detail near the observer or under direct attention; mid-range motion can use coherent cohorts; distant activity can use aggregates that preserve density, direction, and motion cues.
- Update cadence is independently chosen for flight, wing phase, cohort flow, vegetation, lighting, and ambient particles according to perceptual and causal needs. A lower update rate must not produce a visible temporal discontinuity.
- Deterministic seeds and stable cohort/individual identities permit repeatable observer traces and promotion/demotion tests.
- Benchmarks report actual work and correctness together: total represented population, individually evaluated entities, cohorts updated, generated geometry/instances, refreshes, reuse/skips, allocations, and stage timings.
- The same migration state supports overview, walking-through, and attention-focused views. A new view changes observer-relative representation, not the world itself.

### Suggested progression

1. Reproducible terrain, vegetation, and a large static density field.
2. A CPU reference with a modest number of individually animated butterflies and a much larger deterministic cohort population.
3. Observer traces that move through the swarm, turn toward sunlight, and focus on one nearby butterfly.
4. Actual representation reduction and reuse, verified by output/work counters and tests for continuity and identity.
5. Population-scale experiments, followed by a measured CPU/GPU comparison using equivalent scenarios.

Do not begin by claiming that millions of individually simulated butterflies are affordable. Start with a workload that can be measured, identify which computation scales with individuals versus cohorts and observers, and increase scale until the evidence shows the next bottleneck. The goal is not to cheat the population; it is to compute the information each observer needs to experience it as a coherent, living whole.

### Coordination boundary

WebPage owns the immersive experience, camera controls, observer intent, and interaction. The natural-world content owner supplies terrain, vegetation, migration facts/fiction, stable identities, and deterministic source revisions. Renderer decides observer-relative representation and due work. Visualize manifests selected frames. Physics remains owned by its separate workstream; coordinate a narrow contract if flight dynamics or wind are later needed rather than duplicating or editing its implementation here.

This is a proposal and acceptance target, not evidence that a natural scene, GPU backend, or migration simulation already exists.


### Cross-repository acknowledgement: Renderer response

RendererLLM has acknowledged WebPageLLM's stress-campaign contract and recorded the response at [Renderer `docs/WebPageLLM.md`](https://github.com/TrentBest/TheSingularityWorkshop.Renderer/blob/development/docs/WebPageLLM.md). The initial city-massing specimen remains a policy/adapter fixture, not proof of actual geometry reduction or GPU performance. The next measurable seam is representation reduction with stable-ID and transform-continuity assertions, then a reproducible stress command and honest counters.

The companion monarch migration requirement is now explicitly confirmed by the user: both a vast landscape-spanning migration and clouds dense enough to overtake the observer are required. Near-field behavior includes butterflies landing on the avatar, flapping their wings while perched, and taking flight again. This must be coordinated as authoritative world/interaction state plus renderer-selected representation, not merely a particle effect.
