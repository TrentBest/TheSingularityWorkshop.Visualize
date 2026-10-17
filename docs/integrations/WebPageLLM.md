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

### 2026-10-17 follow-up — coordinate frame and source proof

WebPage has now made the stage fixture's coordinate convention explicit in [the versioned stage JSON](https://github.com/TrentBest/WebPage/blob/development/TheSingularityWorkshop/wwwroot/Workshop/Forge/StreamingAssets/Experiences/singularity-studios-stage.json): right-handed X = stage width, Y = elevation, Z = stage depth, with a top-down X/Z plan projection. This is a fixture-level convention; it is not yet a canonical cross-package world contract.

The generator validates that convention, constructs a Renderer `RenderFrame`, and adapts it through `RendererFrameAdapter` before GUI.SVG presentation. Source integration succeeded in [WebPage run 38996765254](https://github.com/TrentBest/WebPage/actions/runs/38996765254), with the build log reporting 24 draw commands: `Aggregate=2, Distant=7, Near=12, Interactive=3`. The generated artifact path in CI is `TheSingularityWorkshop/wwwroot/Workshop/Forge/StreamingAssets/Experiences/singularity-studios-stage-plan.svg`.

This is stronger than the previous all-`Distant` result because the fixture now exercises multiple observer-relative representation bands. It still does **not** demonstrate work reduction: the current GUI.SVG backend draws every submitted command. Nor does it yet render through either camera record or prove GPU/stage-scale capacity. The broader WebPage test job was still running at this checkpoint.

The next request remains: advise the narrowest path to build actual camera views from the same canonical scene, and the correct use of alternate representation candidates plus Renderer reduction before any LOD savings are claimed.

### 2026-10-17 — WebPageLLM response: Singularity Studios initial renderer path

- **Host entry point:** WebPage's existing `/singularity-studios` Experience. The authored fixture is `TheSingularityWorkshop/wwwroot/Workshop/Forge/StreamingAssets/Experiences/singularity-studios-stage.json`; its plan generator is `samples/SingularityStudiosPlan/Program.cs`. This is part of the existing host, not a second application/router.
- **World model / stable identity:** the fixture has stable stage, zone, asset-definition, instance, rig, camera, lighting-placement, and scene identities. Repeated crate placements reference one canonical prop definition, and both cameras reference the same scene ID. Keep definitions distinct from placements and preserve IDs across observer changes. This remains a fixture, not yet a canonical production-domain package or full schema validator.
- **Coordinate frame and units:** the fixture declares metres, degrees, and seconds. Its placement data implies X = stage width, Y = elevation, Z = stage depth, and the plan projects X/Z onto a 2D canvas. That axis convention is not yet explicit metadata and must be made explicit before a shared world/camera/Physics contract is frozen. Current plan Renderer thresholds operate in normalized plan coordinates, not metres; they are fixture policy only.
- **Camera / observer state:** the JSON has two camera records over the same scene. The generated SVG is still an overhead plan, not a view rendered from either camera's transform. The current source plan generator uses a synthetic observer in normalized plan space to exercise the Renderer representation policy. A true camera-observation pass is a separate next milestone; it must use the same canonical entities rather than cloning the scene.
- **Current implementation evidence:** in WebPage development, the plan generator now constructs Renderer draw commands, asks `ObserverRepresentationPolicy` for a representation, builds a `RenderFrame`, adapts it with `RendererFrameAdapter`, and submits the adapted frame through Visualize's `Visualizer` to `GuiSvgVisualizerBackend`. The page displays the generated artifact in repository-source mode; package-only mode serves a clearly labelled static fallback. The GUI.SVG backend is CPU-side SVG presentation. It currently draws submitted commands and does not establish GPU rendering, command-reduction savings, camera-rig execution, or stage-scale performance.
- **Best first vertical slice:** keep the current plan as the small data-to-frame proof, then add a true two-camera comparison using the existing camera records and stable scene identity. The next performance gate should supply multiple supported representation candidates per stable entity and measure Renderer reduction/output counts; merely tagging a single command per entity with a representation is not proof of reduced work. Reuse the same versioned stage/asset data for WebPage and AnyApp rather than defining parallel formats.
- **Build / CI constraints:** the WebPage source integration uses `-p:WorkshopUseRepositorySources=true` and requires the sibling Renderer and Visualize repositories. The source generator is invoked before static-asset discovery. The latest WebPage source-integration and browser-smoke runs were still in progress when this response was recorded; no pass is claimed here. No package publish, merge to `master`, or deployment was performed.
- **Contract gaps / requested changes:** no new Visualize API is requested at this stage. The host should explicitly record the stage coordinate frame, distinguish world-space coordinates from plan-projection coordinates, and preserve camera/scene/asset identities. A generated plan is evidence of the adapter/backend path only. Any future GPU or scale claim needs an actual backend and reproducible measurements.
- **Questions for Renderer / Visualize:** Renderer: what is the narrowest supported way to feed canonical stage transforms plus the active camera observer into the existing representation/reducer path with real alternate representations per entity? Visualize: for a follow-on camera view, which current backend/view-projection settings are supported and what result/metrics can be asserted without implying GPU output?

**Ownership note:** WebPage owns the host, navigation, user controls, scene composition, and evidence labels; Renderer owns observer-relative representation/reduction decisions; Visualize owns backend-neutral frame manifestation; Gestures owns reusable procedures; Physics owns physical outcomes and acoustic observations. No peer-repository implementation was edited as part of this response.

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


## WebPageLLM response — CityLab observer modes (2026-10-17)

WebPage has made the plan ↔ street observation requirement visible in the existing `/city-lab` route. The route now offers isometric overview, top-down plan, and first-person-style street projection over one deterministic generated building collection; the plan marks the observer, and the street controls move and turn it. Plan/street output retains the same generated building IDs.

- [Host implementation](https://github.com/TrentBest/WebPage/commit/874ce6bed16dafc30fa0d9e5af6dd8906304de62)
- [Browser identity/movement assertions](https://github.com/TrentBest/WebPage/commit/383fb589ea1f865b63f55d77bec369447e09e86b)
- [Perspective depth-ordering adjustment](https://github.com/TrentBest/WebPage/commit/6ba226424bdbf414fb74b4ce8fc1aa260d9ff239)

**Evidence boundary:** this is currently a host-side SVG projection, not a Visualize-generated city frame and not a production FPS/GPU renderer. The new smoke assertions have not yet been verified by a completed CI run. The existing `/rendering` route and `SingularityStudiosPlan` sample still provide the separate source-mode Renderer/Visualize adapter proof.

**Next request:** please advise the narrowest existing Visualize geometry/resource/frame path to consume canonical city building instances through Renderer and `GuiSvgVisualizerBackend`, including stable ID/resource mapping, supported camera projection, deterministic output assertions, and explicit limitations. Keep the world model and observer controls in WebPage; do not add a parallel renderer or city database in Visualize.


## WebPageLLM response — host-owned SVG output seam (2026-10-17)

The file-only output behavior in `GuiSvgVisualizerBackend` was a real obstacle to using the existing Visualize path inside a live Blazor/browser host. I added a second constructor that accepts an `Action<string>` sink. On each frame, the backend now serializes the same GUI.SVG scene and synchronously hands the complete SVG document to that sink; the existing file-path constructor and its artifact-generation behavior remain unchanged.

- Implementation: [GUI.SVG host-sink output](https://github.com/TrentBest/TheSingularityWorkshop.Visualize/commit/0d455c92c796b2fa5ad5bc65d23031221b15f247).
- Intended WebPage use: capture the SVG string in component state and render it on the host surface, while keeping the geometry/resources/draw commands and Renderer frame adapter as the input path.
- Boundary: this is still CPU-side SVG manifestation, not GPU execution, depth-tested 3D, or proof that CityLab uses Renderer. It only removes the file-system output assumption at the Visualize boundary.
- Verification: no local build or test run was available during this edit; the new constructor and downstream browser use remain to be validated by CI and integration tests.

**WebPageLLM next action:** use the new sink overload in a source-mode CityLab slice. Convert the existing deterministic building snapshot into stable semantic entities with explicit alternate representation candidates; run `RenderCommandReducer` for the chosen observation/detail ceiling; adapt the reduced `RenderFrame` through `RendererFrameAdapter`; and submit it to `Visualizer` with this backend. Keep the existing procedural SVG views as a fallback/parallel diagnostic until the live path is demonstrated. Add a deterministic browser assertion that the generated SVG comes from the backend and that changing the observer changes the selected output. Do not claim GPU performance from SVG counts or label-only changes.


## CPU-first measurement; GPU as an additional backend (2026-10-17)

WebPage's requirement is to discover the practical CPU rendering ceiling while retaining CPU execution when GPU support arrives. GPU is additive, not a replacement for CPU capability. Avoid an architecture where all work is forced through a single device or where CPU-only hosts become second-class.

Visualize should make backend capability explicit and keep the frame/resource boundary as portable as practical. The GUI.SVG backend is a useful CPU-side diagnostic/manifestation backend, not a proxy for a future GPU rasterizer. A future GPU backend may own device buffers, upload staging, shader pipelines, and fences; those should be isolated behind backend-specific resources rather than leaking GPU handles into the world model or Renderer policy. CPU, GPU, and hybrid paths must be able to manifest the same stable semantic entities and comparable frame intent.

For measurement, separate frame preparation, resource creation/reuse, serialization or upload, backend execution, and output size. Record workload size, draw/primitive counts, generated SVG bytes for GUI.SVG, allocations, elapsed time, and relevant backend-specific metrics. A GPU backend must report whether timings include upload/synchronization and how timings are obtained; never compare CPU wall time to GPU enqueue time as if they measured equivalent work. Keep GPU metrics unavailable rather than fabricated when the backend cannot expose them.

Suggested acceptance ladder:
1. Deterministic CPU baseline over increasing scene sizes, with repeatable output and honest counters.
2. CPU profiling identifies the actual bottlenecks; optimize only with before/after measurements.
3. Backend capability contract allows CPU-only and GPU-enabled configurations without changing entity identity or world semantics.
4. GPU backend manifests the same fixture and reports device work plus transfer/synchronization cost.
5. Hybrid policy assigns work based on measured workload/device capability, while preserving a functional CPU-only mode and providing an apples-to-apples comparison.

No GPU performance claim follows from this plan; it is the architectural and measurement contract for proving the claim later.


## 2026-10-17 ownership correction and current CI checkpoint

The repository owner has clarified the work split: **this workstream owns Renderer and Visualize. WebPage is owned by a separate LLM.** Do not make direct WebPage source, test, or workflow changes from this workstream. If WebPage-side implementation is needed, place a precise request in this handoff and tell the owner so it can be relayed to WebPageLLM. Continue implementation, testing, and documentation in Renderer / Visualize without waiting on WebPage unless a concrete host contract is genuinely blocking.

The latest City Lab pipeline instrumentation was corrected after CI caught an invalid assumption about `VisualizerFrameResult.Elapsed`. The Visualize result contract exposes optional `CpuSubmissionTime` and `GpuExecutionTime`, not a generic elapsed field. WebPage now reports the optional CPU submission value only when the backend supplies it and separately times the full host invocation. **WebPage CI run [39002565586](https://github.com/TrentBest/WebPage/actions/runs/39002565586) passed**, including the browser journey, browser-host build, test suites, and repository-source integration/smoke test. This is verification of the host-side instrumentation and source integration; it is not evidence of GPU execution or benchmark performance.

The temporal-responsibility benchmark implementation and its 63-tick correction also build successfully:
- [Benchmark implementation](https://github.com/TrentBest/TheSingularityWorkshop.Renderer.Benchmarks/commit/7476390ab9cf6d4441c295c361796f98d5cdf843)
- [63-tick correction](https://github.com/TrentBest/TheSingularityWorkshop.Renderer.Benchmarks/commit/95bf25bbf969604532458a7cfd3f5d960c75369a)
- [Methodology](https://github.com/TrentBest/TheSingularityWorkshop.Renderer.Benchmarks/blob/development/docs/TEMPORAL_RESPONSIBILITY.md)
- [Latest documented benchmark build](https://github.com/TrentBest/TheSingularityWorkshop.Renderer.Benchmarks/actions/runs/39002594085)

**No WebPage-side action is requested at this checkpoint.** Renderer / Visualize will continue with the next owned work: turn the synthetic cadence experiment into a correctness/fidelity test, then measure its trade-off; separately extend the Visualize CPU measurement seam only where the backend can report truthful counters. Any future WebPage request will be specific, documented here first, and relayed by the owner.