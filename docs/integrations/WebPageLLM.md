# WebPageLLM — Renderer / Visualize Coordination Handoff

**Audience:** the LLM working on `TheSingularityWorkshop.WebPage`  
**From:** Renderer / Visualize workstream  
**Purpose:** exchange the concrete integration facts and questions needed to make Singularity City one coherent, observer-relative world rather than separate demos.

This is a living handoff. Please append answers under **WebPageLLM response** or update the relevant sections as implementation decisions become clear. The repository owner will relay this document between workstreams.

## The shared objective

Build Singularity City from an inspectable plan / map-like view toward district and aerial views, street-level experiences, building and venue plans, vehicles, and interiors. The same places and objects must retain stable identity as the observer moves between representations.

The north star is not a collection of disconnected illustrations. It is one world with many observer-relative representations, whose detail and computational cost change with the observer and the experience.


## Observer choice: plan view, first-person view, and the hidden Magic switch

The user should be able to choose how to observe the same city. The top-down plan/map is one observer mode, not the entire product. The long-term target includes a progressively realistic first-person/FPS view of the same world, with stable place/entity identity and no duplicated city database or disconnected game level.

A deliberately hidden **Magic switch** belongs in the laboratory as a physical 3D interactable for inquisitive visitors. Flip once to switch to FPS observation; flip again to return to the top-down plan. Keep it a discoverable secret rather than a prominent global UI control. WebPage owns the lab interaction and camera/mode transition; the world model owns the switch's stable identity and placement; Renderer handles observer-relative representation/work decisions; Visualize manifests the selected frame. Mode switching must preserve world identity and relevant state. If a fully developed FPS scene is not ready, expose the contract and shared-scene prototype honestly rather than faking a finished realistic experience.

This changes the integration questions and acceptance gate:
- What existing WebPage camera/navigation state can support switching observer mode while retaining world coordinates and selection?
- Can the same stable city entities be represented in plan view and in a first-person scene, and what geometry/content is missing for street-level use?
- Where can the laboratory and its hidden switch be authored within the composed WebPage experience/microbundle model?
- The vertical slice should prove real Renderer decisions for a city scene and Visualize output; the next gate should prove a reversible plan ↔ FPS observer transition over shared world identity. Do not require the hidden surprise to be revealed in ordinary UI.

## Current ownership boundaries

- **WebPage:** host experience, camera/navigation input, user interaction, selection, overlays, and experience-level orchestration. It should consume rendering capabilities rather than duplicate Renderer policy or Visualize backends.
- **World / experience content:** authoritative city facts and authored content—stable IDs, placements, coordinate frame, units, topology, routes, and source revisions. A rendered mesh is not the city database.
- **Renderer:** observer-relative representation decisions, event horizons, freshness / invalidation policy, bucket reuse, and platform-neutral render-frame assembly. It should not own city-authoring rules or the WebPage UI.
- **Visualize:** platform-neutral visualization contracts and backend manifestation (currently including GUI.SVG integration). It should not become a duplicate world simulation or own WebPage navigation.
- **FSM_API / domain owners:** state and domain semantics at their appropriate layer. Physics and other domain packages remain independently owned; coordinate contracts rather than editing their repositories from this workstream.
- **FSM_COS:** composition boundary for capabilities, not an application loop.

## What is already evidenced in the repositories

- Visualize has a deterministic city-massing proof that sends a shared geometry / material catalog and draw commands through Visualize to GUI.SVG. Its current commands use the `Distant` representation; this is a useful visual/backend proof, **not yet proof of Renderer-driven city LOD**.
- Renderer has platform-neutral view/frame/draw-command contracts, observer representation policy, event horizons, and a coordinator that combines representation state and horizon freshness. The host still has responsibility for preparing the representation and completing refreshes after output is ready.
- A small Renderer-to-Visualize adapter exists, but the current city-massing proof project does not yet demonstrate the complete city scene passing through actual Renderer policy and then through Visualize.
- The existing proof is not, by itself, evidence of GPU rendering, persistent live city state, full street view, or city-scale performance.

Reference:
- [Visualize city handoff](SINGULARITY_CITY_RENDERING_HANDOFF.md)
- [Renderer implementation roadmap](https://github.com/TrentBest/TheSingularityWorkshop.Renderer/blob/development/docs/IMPLEMENTATION_ROADMAP.md)

## Decisions we need from WebPageLLM

Please answer with **current code evidence** (paths, types, and relevant behavior) where possible. Mark proposals as proposals, not implemented facts.

1. **Current host entry point**
   - What is the intended WebPage experience / microbundle entry point for Singularity City?
   - Which component currently owns the camera/observer position, view/projection data, zoom, and navigation state?
   - Which existing route, panel, or experience should host the first end-to-end city proof?

2. **World and identity contract**
   - Does WebPage already have a city/world model with stable IDs for districts, roads, buildings, lots, entrances, transit, vehicles, and Digitens?
   - What coordinate frame and units does it use? What is the origin / axis convention?
   - How are scene changes and source revisions represented?
   - If this model does not exist yet, what is the smallest truthful contract we can agree on without building a second city database in Renderer or Visualize?

3. **Existing spatial/camera work**
   - Which current docs and code implement the zoomable-world, plan/map view, spatial index, camera transforms, and navigation?
   - What is implemented and tested versus design-only?
   - Are there existing controls or interactions the Renderer/Visualize proof must preserve?

4. **Renderer → Visualize integration**
   - Can the host provide observer position plus a deterministic list of city candidates with stable IDs, world bounds/centers, semantic roles, and a source revision?
   - Can the host accept a platform-neutral `RenderFrame` and send it through the existing Renderer-to-Visualize adapter?
   - If not, what exact API gap or dependency boundary blocks this?
   - Which project should own the integration proof so the WebPage app remains an experience host, not a duplicate rendering engine?

5. **First vertical slice**
   - Recommend one small, visible slice: for example, city plan with roads and building footprints; observer movement changes representation decisions; stable IDs survive frame rebuilds; GUI.SVG output is a CI artifact.
   - What existing WebPage experience can host or link to this proof?
   - What is the smallest user-visible interaction that should be included now, and what must remain explicitly out of scope?

6. **Build and CI**
   - What commands/workflows currently build and test the relevant WebPage experience?
   - Can the integration produce a deterministic, inspectable artifact without requiring secrets, network services, or publishing a package?
   - Which CI constraints or project conventions must the Renderer/Visualize workstream respect?

7. **Coordination**
   - Please identify any existing WebPage work that would conflict with this proposed boundary or duplicate an existing capability.
   - If WebPage needs a Renderer or Visualize API change, state the exact contract and use case rather than asking for an app-specific special case.

## Proposed first acceptance gate

A first vertical slice is complete only when all of the following are demonstrated:

- A deterministic city candidate source with stable identity and explicit coordinate frame / units.
- WebPage supplies an observer state through an agreed host boundary.
- Renderer makes actual observer-relative representation / refresh decisions; the proof does not hard-code every city command as `Distant`.
- The resulting platform-neutral frame passes through the existing Visualize path.
- A deterministic artifact and tests make the behavior inspectable in CI.
- The documentation clearly labels what is implemented, simulated, and still future work.

A rendered SVG is valuable evidence of the GUI.SVG path, but is not proof of GPU execution or a fully persistent living city.

## Constraints

- Sith policy: keep only `master` and `development`; do not create another long-lived branch.
- Do not publish packages, deploy, or merge to `master` without the owner's explicit approval.
- This workstream does not edit the Physics repository. Coordinate any physics-facing needs with its owner.
- Prefer one composed WebPage experience / manifest and reusable microbundles over standalone demo pages that duplicate the host.
- Keep ownership clear: WebPage owns the experience; Renderer owns observer-relative decisions; Visualize owns visualization/backend manifestation.

## WebPageLLM response

*Please add your findings and answers here, preferably with repository paths, API names, and links to relevant documentation or commits.*

- **Host entry point:**
- **World model / stable identity:**
- **Coordinate frame and units:**
- **Camera / observer state:**
- **Current implementation evidence:**
- **Best first vertical slice:**
- **Build / CI constraints:**
- **Contract gaps / requested changes:**
- **Questions for Renderer / Visualize:**


## Additional scene candidate: impossible-scale monarch migration

A second proving ground is proposed alongside Singularity City: an immersive natural environment during a monarch butterfly migration, at a deliberately impossible population far beyond what nature could sustain. The intent is to let the observer experience a phenomenon larger than people ordinarily witness and better understand something lost to us. This is an artistic/technical stress test inspired by migration, not a claim of ecological realism at that density.

This scene should use the same world/observer/rendering contracts, not introduce a separate rendering engine. It stresses moving populations, overlapping trajectories, depth, occlusion, density, temporal coherence, and attention-driven detail in ways static city massing cannot.

**WebPageLLM coordination questions for this scene:**
- What current WebPage host/camera/input architecture can place an observer inside a natural environment and record reproducible observer traces?
- Is there an existing world/scene composition model suitable for terrain, vegetation, and deterministic migration cohorts, or should the first proof live as an isolated Visualize fixture until a host contract is agreed?
- How should an experience select between overview, walking-through, and attention-focused observation while preserving the same migration state?
- What is the narrowest host-facing contract for observer position/orientation, intent, and viewport requirements, without making Renderer own experience UI or world content?
- Can the host eventually supply attention/interaction signals (for example, tracking one butterfly or looking toward a sunbeam) as observer inputs, rather than Renderer guessing what the user intends?

The initial engineering proof should start with deterministic terrain and a density field, then add a CPU reference with a manageable set of individual butterflies plus a much larger cohort representation. It must measure represented population separately from actual individual evaluations, cohort updates, generated geometry/instances, reuse/skips, allocations, and elapsed time. A larger count alone is not proof of performance, and aggregate representations must preserve visible density, direction, motion, and identity through observer changes.

Renderer owns representation and computation decisions; Visualize manifests the selected frame; WebPage owns the experience, camera, navigation, and interaction; the natural-world content owner owns terrain and migration state. Physics remains separately owned and should be coordinated by contract if needed. This is proposed work, not an implemented feature.


## User-confirmed experience requirement — migration cloud and avatar contact

The user confirmed that the migration must deliver **both** a vast migration flowing across the landscape and dense clouds that overtake and envelop the observer. The observer should see butterflies land on the avatar, flap their wings while perched, and take flight again.

This changes the acceptance criteria from density-only to a linked scale-to-contact transition:
- preserve a coherent large-scale migration direction and cloud front while walking into and out of it;
- promote nearby butterflies to stable individual identities without visible popping or duplicates;
- represent landing/perched wing animation/takeoff as explicit state transitions attached to an avatar surface or stable contact anchor;
- keep authoritative landing and takeoff state with the world/behavior owner; WebPage supplies avatar pose/surface and interaction context, Renderer handles observer-relative representation and work scheduling, Visualize manifests the frame;
- measure individual updates, cohort updates, contact candidates, landings/takeoffs, representation changes, and reuse/skips separately.

The Renderer response/acknowledgement is recorded in [Renderer `docs/WebPageLLM.md`](https://github.com/TrentBest/TheSingularityWorkshop.Renderer/blob/development/docs/WebPageLLM.md). This remains a proposed acceptance contract, not an implemented scene or benchmark.
