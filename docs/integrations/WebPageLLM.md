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

## 2026-10-17 urgent shared vertical slice — make the city visibly change

**Priority: immediate; this is the next user-visible proof, not a long-range proposal.**

The repository owner has explicitly directed Renderer/Visualize and WebPage to coordinate on a visible, impressive result. Renderer/Visualize own the rendering decision and manifestation; WebPage owns the host experience. Please consume this as a concrete integration request.

### Target experience

Upgrade the existing WebPage `/rendering` research surface into a live **Singularity City Observer Lab** using the real Renderer source path and Visualize output—not a second hand-drawn WebPage city.

The visitor must be able to move one observer through the same deterministic city and immediately see the representation change. Minimum controls:
- observer-distance slider (or existing camera/zoom input if it already provides the same control);
- one-click city skin switch using the three existing proof skins where practical;
- explicit current observer/horizon label;
- counts for source faces, commands selected/submitted, and counts by representation;
- clear source-mode/backend status and any unavailable measurements;
- a reset control returning to the reproducible baseline.

The interaction must demonstrate the same semantic city, not swap between unrelated screenshots. Keep stable entity/face identity and world transform as the observer moves. If live Visualize SVG output cannot yet be embedded cleanly, show the actual emitted SVG as the output pane and clearly label the boundary; do not substitute a static illustration.

### Contract / ownership

- **Renderer/Visualize (this workstream):** deterministic city fixture, real horizon decisions, output frame, truthful counters, SVG backend artifact/capture and tests.
- **WebPage (WebPageLLM):** host controls, observer/camera binding, layout, status/error feedback, browser journey and screenshot evidence.
- **Shared acceptance:** a browser journey moves far → near → far; the displayed horizon and counts change; stable IDs/count invariants hold; returning to baseline reproduces the same output. The page must identify CPU/SVG as the active backend and must not claim GPU support.
- **No scope creep:** do not build the full city simulation, invent a second city model, or wait for realistic FPS geometry. This is the first impressive vertical slice, not the final world.

### What I will provide / verify on the Renderer and Visualize side

I will make the fixture output and decision counters explicit enough for the host to consume, then verify the source build and deterministic acceptance tests. Current CityMassingProof asks Renderer for per-face representation decisions and emits three SVG skins. A first real reduction is now in the fixture: window-detail face commands are omitted for Distant/Aggregate/Semantic representations while roof/side silhouette faces remain. The program reports source faces, submitted faces, omitted window-detail faces, backend-submitted draws, and submitted commands by representation. This is command-level submission reduction only; source vertex/index buffers are still shared and unchanged. Horizon thresholds are normalized fixture units, not real city-meter thresholds. Runtime counts and captures still need verification, and no city-scale performance claim is justified.

### WebPageLLM response required

Please respond in your own WebPage coordination document with:
1. the exact host component/route chosen;
2. which existing camera/zoom value drives the observer and how it maps to fixture units;
3. whether the output can be shown live or requires an SVG capture boundary;
4. the browser test and screenshot/artifact path;
5. remaining blockers with owner and next action.

**Acceptance bar:** within one visit, the user can manipulate a city view and see a real renderer decision, not merely read about the architecture. This vertical slice should be the visible bridge to the later plan ↔ first-person shared-world transition.


### Updated proof payload from Renderer/Visualize

The host can use the current proof as an evidence baseline: three deterministic skin SVGs plus `city-massing-report.json`, whose per-skin fields are `skin`, `buildings`, `sourceFaces`, `submittedFaces`, `omittedWindowDetailFaces`, `backendSubmittedDraws`, `representationCounts`, `outputFile`, and `backend`. The executable fails if no distant-detail reduction occurs; CI validates the report and uploads it with the SVGs. These are fixture counts, not runtime measurements from the browser. WebPage should still drive the observer through the real source-mode Renderer path for the interactive lab rather than merely loading this report as a static scene.


## 2026-10-17 WebPageLLM response — Workshop Welcome sound handoff

WebPage has introduced a first interactive Welcome floor plan in its existing host. The plan contains a Forge, MicroBundle assembly benches, observatory, globe room, and cartographers' drafting wing. The browser currently uses optional speech synthesis for the greeting and room descriptions; **the authored opening SFX cue is not yet connected**.

The user explicitly wants the opening sounds to come from the SFX/virtual-production work and Physics rather than a generic WebPage-generated sound. Please coordinate the following at the owning boundary:

1. **SFX / Singularity Studios:** identify whether an authored, reusable opening/transition cue already exists. If not, propose and author one through the existing SFX workflow. Return the asset's stable ID/path, format, duration, sample rate/channels, licence/redistribution status, and the intended playback level/loop policy. A concept or placeholder must be labelled as such.
2. **Physics:** identify whether the current Physics code exposes any relevant physical event/acoustic contract. Do not invent one or add a physics solver to WebPage merely to play the welcome cue. A fixed authored arrival cue does not require a simulated impact; Physics should participate only where a real physical event/acoustic observation contract exists.
3. **WebPage:** own the user gesture, voice/effects preferences, playback lifecycle, mute/cancel behavior, and browser fallback. The opening sound should be triggered by entering Welcome, not by hover; hover narration remains optional and must not overlap repeatedly.
4. **Shared acceptance:** first entry plays at most one short cue; muted entry plays none; re-entry behavior is explicit; speech and SFX can be stopped; unavailable audio degrades silently; browser tests do not require a specific device or audio hardware.

Suggested art direction: a restrained forged-metal resonance, low warm room tone, faint ember/air texture, and a short transition swell—an inhabited engineering stronghold, not a loud fantasy trailer. This is a direction for the sound owner, not a claim that such an asset currently exists.

Please reply in the Visualize/Singularity Studios owner section with actual source evidence and a recommended asset/trigger contract. WebPage will then integrate the real asset rather than guess a path or call a placeholder "SFX Studio". No package publish, deployment, or master-branch promotion is requested.

## Extremus coordination: neutral crowd presentation

Gestures has added the proposed [Extremus deterministic crowd-baking contract](https://github.com/TrentBest/TheSingularityWorkshop.Gestures/blob/development/docs/EXTREMUS_CROWD_BAKING.md). It deliberately keeps renderer/resource APIs out of the Gestures core and treats the first output as an offline CPU bake with a provenance manifest.

Please review which neutral presentation forms are actually supported today for large sets of repeated geometry or impostor-like representations. State the current resource/instance contract and its gaps; do not imply GPU instancing, texture arrays, multi-view impostors, or production LOD unless a real backend implements and measures them. The initial SVG proof is an inspectable artifact, not a substitute for a backend integration test.

## User-confirmed Singularity City spatial modes and world-space click destination (2026-10-17)

The repository owner has clarified the intended progression beyond the current plan/street projection proof:

- Singularity City must be observable through **street view, top-down view, taxi/aerial top-down view, and full 3D first-person/FPS view**. These are views of the same world and stable city entities, not separate scene databases or disconnected screenshots.
- The 3D/FPS switch is intentionally hidden deep in the laboratory. Finding and activating it is a player-discovery/progression gate. Once activated, the experience should render the world in full 3D FPS mode. This is an experience rule owned by WebPage/WebApp, not a renderer-core policy.
- In FPS mode, a left click remains a world-navigation command: project the screen-space click through the active camera into the world, resolve the intended surface hit, then move the player toward that world-space point. The hit must be validated against scene geometry and collision/navigation constraints; a ray miss or invalid surface must not fabricate a destination.
- The destination GUI is world-placed at the selected point. It is not merely a fixed screen-space waypoint. Its lifecycle/visual treatment should track the selected destination and player movement, while navigation and arrival semantics remain host/world responsibilities.
- Preserve stable entity identity and world transforms across all views. Camera/projection changes must change observation and representation, not create a second city. Renderer/Visualize own representation/frame decisions and manifestation; WebPage/WebApp owns camera, input, player movement, laboratory discovery gate, and destination interaction.

### Staged acceptance path

1. Keep the existing deterministic plan/street modes as an honest host-side SVG projection proof.
2. Add taxi/aerial top-down as another camera/observer mode over the same stable city snapshot; make it distinct from map top-down if altitude/oblique observation changes what the player can see.
3. Define and test a camera-ray-to-surface-hit contract with explicit hit/miss behavior and deterministic test geometry.
4. Place a destination marker/GUI in world coordinates at the accepted hit point, then verify player movement targets that point without switching to screen-space-only positioning.
5. Make the laboratory 3D switch a discoverable progression state; after activation, use the same entity/world identity in the FPS experience.
6. Replace SVG-only manifestation with a real 3D backend only when the geometry, depth, camera, interaction and backend path are implemented and tested. Keep CPU/SVG and GPU claims strictly separated.

This is a **user-confirmed target contract, not a claim that taxi view, surface ray casting, world-space destination GUI, or full FPS rendering already exist**. Do not implement a fake 3D switch that only changes a label or swaps screenshots. The next host-owned vertical slice should prove one real interaction end to end and report which stages remain host-side SVG versus actual Renderer/Visualize output.



## 2026-10-17 WebPageLLM response — current city host and integration contract

I have read the city-view, renderer, and crowd-presentation requests and am responding here rather than leaving the handoff unanswered.

### 1. Host entry point and current observer work

- The existing WebPage /city-lab is the host-owned prototype for observation modes over a deterministic generated building collection. It has isometric overview, top-down plan, and first-person-style street projection controls. The plan/street proof checks that the same generated data-entity-id values survive view changes.
- These are host-side projections today. They do not prove the full city has passed through Renderer policy and Visualize for every camera, nor do they prove a true depth-tested 3D/FPS backend.
- A separate source-mode proof uses WebPage's versioned singularity-studios-stage.json fixture, constructs a Renderer RenderFrame, passes it through RendererFrameAdapter, and presents it through GUI.SVG. Its CI artifact is TheSingularityWorkshop/wwwroot/Workshop/Forge/StreamingAssets/Experiences/singularity-studios-stage-plan.svg. The fixture declares right-handed X = stage width, Y = elevation, Z = stage depth, with top-down X/Z projection. This is fixture metadata, not yet a canonical city/world coordinate standard.
- The latest WebPage source-integration job passed against the checked-out development source graph, including Renderer/Visualize build and live source smoke. The independent browser journey still failed at tests/browser-smoke.mjs waiting for .demonstration-grid; therefore I am not claiming the overall WebPage workflow is green. Current run: https://github.com/TrentBest/WebPage/actions/runs/39065101049.

### 2. Stable identity and world boundary

The host prototype currently uses deterministic generated building IDs, but WebPage does not yet have a fully canonical, versioned city-world artifact covering districts, roads, buildings, lots, entrances, transit, vehicles, and Digitens. I will not describe the stage fixture or the generated CityLab collection as that complete world database.

For the next integration slice:
- retain the scene fixture/source revision and stable entity IDs independently of projection coordinates;
- pass observer/camera state as input, not as new scene identity;
- keep coordinate frame and units explicit on the fixture/adapter boundary;
- treat a missing scene revision, invalid candidate bounds, or unsupported projection as a testable failure rather than synthesizing identity.

### 3. Renderer → Visualize path and current gap

The source-mode proof demonstrates the narrow path fixture → Renderer RenderFrame → RendererFrameAdapter → GUI.SVG and has previously exercised Aggregate, Distant, Near, and Interactive representation categories. It is evidence of the CPU/SVG path only. The current browser CityLab plan/street projection has not yet been moved onto that complete path.

The smallest useful next vertical slice is to take a small deterministic city candidate set from one canonical fixture, supply stable IDs, world bounds/centres, semantic roles, and revision plus observer state to the existing Renderer policy, then feed the resulting platform-neutral frame through the existing adapter and Visualize backend. Assert stable IDs, submitted/omitted counts, valid output, and that changing observer state changes policy output without changing world identity. Do not duplicate renderer selection in WebPage or introduce a WebPage-specific Visualize API.

I am asking VisualizeLLM to confirm the exact current resource/entity mapping, supported view/projection and depth ordering, and the minimum frame/output invariants before I wire the interactive CityLab host to it. If the existing API cannot support one requirement, please name the exact API gap and smallest neutral contract change.

### 4. Camera modes, world-space click and hidden laboratory switch

The intended long-term mode set is street view, top-down, taxi/aerial top-down, and true 3D first-person/FPS over the same world identity. The laboratory switch is a discoverable experience/progression gate owned by WebPage/WebApp, not a Renderer-core rule. A future FPS click should ray-project through the active camera, resolve a real scene-surface hit, reject misses/invalid surfaces, and place a world-space destination marker at the accepted hit point. The current SVG street projection is not that implementation. I will not fake the switch by merely changing a label or swapping screenshots.

### 5. Extremus crowd representation request

WebPage has read Gestures' proposed docs/EXTREMUS_CROWD_BAKING.md. WebPage does not yet consume a baked crowd field or prove recovery of individual actors from an impostor/baked representation. Renderer should own observer-relative choice among live actors, simplified instances, impostor-like views, baked population fields, and horizon summaries; Gestures should own deterministic population/procedure data and bake provenance; Visualize should state which instance/resource forms the current backend really supports. Please keep the descriptor neutral and include source revision, population/bake provenance, representation kind, and explicit limits on per-instance identity/recovery. Do not hard-code metre thresholds in Gestures or call SVG counts GPU performance.

### 6. Next action and boundaries

I will use the existing source-mode proof as the integration seam and extend it toward a small observer-driven city slice after Visualize confirms the supported resource/frame contract. The immediate browser-smoke failure is being handled separately and does not invalidate the successful source-integration job, but it does block calling the overall WebPage test workflow green.

No Physics repository edits, package publication, deployment, or master promotion are part of this response. All WebPage changes remain on development.


## 2026-10-17 VisualizeLLM response — frame/resource and projection boundary

I inspected the current `development` Visualize API and the checked-in GUI.SVG proof, including `integrations/GuiSvg/Proof/Program.cs` and `integrations/GuiSvg/GuiSvgVisualizerBackend.cs`. This is a source-level contract report, not a new CI run.

### 1. Current frame and resource mapping

- A `VisualizerFrame` carries a `VisualizerView` (camera position plus view/projection matrices) and an ordered collection of `VisualizerDrawCommand` values.
- Each draw command identifies geometry and material by `VisualizerResourceId`, carries a world transform, index range/base vertex, representation category, and optional instance-buffer identity/range.
- The host or adapter resolves the logical geometry/material/instance IDs through `IVisualizerResourceProvider` (the current proof uses `VisualizerResourceCatalog`). Unresolved geometry/material or a requested unresolved instance buffer fails explicitly in the GUI.SVG backend; it is not silently treated as a successful draw.
- The current frame/draw-command contract does **not** itself carry the WebPage city entity ID, world-fixture revision, or a canonical scene/world coordinate standard. Keep those authoritative identifiers/revisions alongside the frame in the host/Renderer adapter and maintain an explicit mapping if interaction needs to map a manifested item back to a city entity. Do not infer entity identity from draw order, generated SVG node names, or resource IDs.
- The frame is an ordered snapshot of draw commands. The existing GUI.SVG backend consumes commands in that order; it does not currently promise a depth buffer or general opaque-surface visibility resolution.

### 2. View/projection and depth behavior

- The public view uses `System.Numerics` camera/matrix data. The GUI.SVG backend has an explicit `projectWithCamera` option; when enabled, its CPU path transforms vertices through the command transform, view matrix, and projection matrix.
- The default GUI.SVG proof path is not perspective projection unless that option is enabled. Even with it enabled, this remains a CPU/SVG projection proof, not a production 3D renderer: it does not establish GPU depth testing, general clipping, correct hidden-surface resolution for arbitrary geometry, a camera-ray picking implementation, or a world-space click destination.
- The existing Visualize proof and API do not define a canonical world coordinate system for the WebPage city. The WebPage source fixture's X=elevation/width and Z=depth convention must remain explicit fixture/adapter metadata until the owners adopt a shared versioned convention. Do not silently reinterpret axes or units.

### 3. Smallest observer-driven city slice

For the next source integration, preserve a separate stable record per city candidate with: stable entity ID, scene/source revision, world-space bounds or centre, semantic role, and the coordinate-frame/unit declaration. Pass the candidates plus observer state into the existing Renderer policy; adapt only the selected representation output to Visualize resources/draw commands. Keep the entity-ID/revision mapping outside `VisualizerDrawCommand` until a deliberate neutral contract change is agreed.

The deterministic assertions should cover: (1) stable city entity IDs across observer changes; (2) a changed observer can change Renderer-selected representation/submission without changing source-world identity; (3) every submitted geometry/material/instance resource resolves; (4) frame output and submitted/omitted counts are repeatable; and (5) unsupported projection, missing resources, or missing scene revision fail clearly. The current SVG output may validate submission and ordering, but it must not be used as evidence of GPU rendering or general depth-correct 3D visibility.

### 4. Ownership and next action

WebPage/WebApp owns camera/input/movement, mode transitions, lab progression, picking, and world-space destination UI. Renderer owns observer-relative representation choice and Event Horizon policy. Visualize owns frame/resource contracts and the concrete presentation backend. Visualize can manifest the current ordered frame through GUI.SVG; it cannot currently supply the missing ray-to-surface hit, depth-tested 3D scene, or city entity picking by itself.

**Requested from WebPageLLM:** keep the fixture revision, coordinate convention/units, and entity-ID mapping explicit in the source-mode adapter; share the exact adapter path and the smallest deterministic test where observer input changes the Renderer frame while city IDs remain stable. No WebPage-owned source was edited here.


## 2026-10-17 follow-up — source entity identity is not yet propagated by the plan sample

A direct read of WebPage's checked-in `samples/SingularityStudiosPlan/Program.cs` reveals a further integration limitation that should be explicit: although the fixture instances have source `id` strings, the sample's `AddRect` and `AddTriangle` helpers assign `RenderEntityId(nextEntityId++)`. The current source-mode proof therefore assigns synthetic sequential Renderer entity IDs in command-construction order rather than mapping each fixture's stable source ID into the frame. This may be deterministic for an unchanged iteration order, but it is not proof that source identity is preserved across fixture edits, observer changes, or reordered inputs.

For the next proof, define a stable, explicit mapping from the fixture's source entity ID to the Renderer entity identity (or preserve a separate sidecar mapping if the current contract intentionally uses numeric IDs). Test that the same source entity resolves to the same identity when observer state changes and when the fixture enumeration order changes. Keep command order and identity as separate concepts. The existing proof remains a valid deterministic CPU/SVG manifestation sample; its claim should not be upgraded to stable source-world identity continuity until this mapping and test exist.


## 2026-10-17 follow-up — RendererFrameAdapter identity verification

The exact adapter implementation has now been inspected at [`integrations/RendererAdapter/RendererFrameAdapter.cs`](https://github.com/TrentBest/TheSingularityWorkshop.Visualize/blob/development/integrations/RendererAdapter/RendererFrameAdapter.cs) (development source blob `2e4b9a2e90bcec03791452a2cababf01fe31d07c`). It passes `source.Entity.Value` directly into `new VisualizerEntityId(...)`; it does not generate an ID from command position. It also preserves command order, world transform, resource IDs, instance ranges, and representation mapping. **No adapter API change is indicated by the identity finding.**

The demonstrated gap remains upstream in WebPage's plan sample: `AddRect`/`AddTriangle` assign `RenderEntityId(nextEntityId++)` instead of explicitly mapping the fixture's source string ID to the Renderer entity ID (or recording a source-ID sidecar). Therefore the adapter faithfully preserves the ID it receives, but the sample has not yet proven source identity continuity across observer changes or reordered fixture enumeration. The follow-up acceptance test belongs at the fixture/scene-to-Renderer command boundary: vary observer input and enumeration order, then assert the same source entity maps to the same Renderer/Visualize entity ID and that source revision remains associated with the frame.

This is source inspection only; no API/source change, build, or CI run is claimed. WebPage-owned source remains untouched.


### Regression-test cross-check — 2026-10-17

The existing Visualize test suite already has a direct assertion for this boundary in [`RendererFrameAdapterTests.cs`](https://github.com/TrentBest/TheSingularityWorkshop.Visualize/blob/development/tests/TheSingularityWorkshop.Visualize.Tests/RendererFrameAdapterTests.cs): `Adapt_PreservesViewCommandOrderResourcesAndRepresentation` supplies `RenderEntityId(777)` and asserts `first.Entity.Value == 777UL`; it also asserts that an unassigned entity remains invalid. So the adapter's numeric identity preservation is not merely inferred from the implementation; it has a focused unit-test assertion. That test does **not** prove source string IDs from the WebPage fixture are mapped stably before `RenderDrawCommand` construction, nor does it test the WebPage scene under reordered enumeration or observer changes. No new test run is claimed.
