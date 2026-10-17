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
- The Renderer-to-Visualize proof is a small specimen mapping that demonstrates a CPU-side frame handoff. It is not the city-massing scene driven by Renderer policy.

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
