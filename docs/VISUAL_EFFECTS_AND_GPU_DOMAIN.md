# Visual Effects, Environmental Media, and GPU Domain

**Status:** Domain definition and implementation direction. This document defines the visual capability's scope and seams; it does not claim the listed techniques are already implemented or that a production GPU backend is selected.

## 1. The domain is not “draw some fog”

Visualize is the Workshop capability that turns a scene's visual meaning and presentation inputs into images. Its domain includes static and animated surfaces, light transport approximations, visibility-dependent visual representation, particles, volumetric media, weather presentation, transient effects, material response, camera-facing composition, and the GPU execution path that realizes those images.

A gameplay phenomenon may cross several ownership boundaries. Consider a large fungus that senses prey, releases compressed gas, and creates a slowly spreading lethal cloud:

- The experience/organism domain defines sensing, intent, prey criteria, and biological state.
- Physics or a specialized environmental simulation defines reservoir release, transport, mixing, flow, and any modeled physical interactions.
- Gameplay/domain rules define exposure, incapacitation, death, decomposition, and nutrient absorption.
- Renderer selects what phenomenon representation and fidelity are needed for each observer and budget.
- Visualize turns the selected representation into a readable image: eruption, plume, turbulence cues, cloud body, light scattering, ground interaction, obscuration, and dissipation.
- Optional Audio/Haptics capabilities manifest the same event through their own channels.

Pixels do not decide whether the gas is lethal. A visual effect must not silently become authoritative simulation.

## 2. Visualize owns image-producing work

Visualize is responsible for the visual representation and execution of:

- **Surfaces and geometry presentation:** meshes, lines, points, billboards, impostors, instances, procedural detail, and surface attributes.
- **Materials:** base appearance, opacity/transmission intent, emission, roughness/metalness or other supported response models, sidedness, surface normals, and shader parameters.
- **Illumination presentation:** direct and indirect-light approximations, ambient terms, shading, reflection/refraction when supported, and light-volume interactions.
- **Visibility effects:** depth testing, occlusion presentation, transparency ordering/approximation, clipping, fogging, and compositing.
- **Volumetric media:** gas, fog, smoke, haze, dust clouds, steam, clouds, underwater turbidity, spores, aerosols, and authored volumes.
- **Particles and transient effects:** motes, droplets, sparks, embers, fragments, debris, splashes, arcs, bursts, trails, wakes, and impact effects.
- **Environmental presentation:** rain, snow, wind cues, blowing leaves, airborne material, ground wetness, drifting dust, heat shimmer, and environmental transitions.
- **Animated and event-driven visuals:** emission bursts, expansion, advection supplied by a simulation, fading, dissolution, impact response, deformation cues, and state-driven visual transitions.
- **Image-space and post-process operations:** tone mapping, color grading, exposure, bloom, depth of field, motion blur, anti-aliasing, upscaling, and other optional composition passes.
- **Observer-specific output:** camera/view transforms and view-dependent effects as consumed from Renderer/host contracts; Visualize does not own world truth or observer policy.
- **GPU resource/execution implementation:** visual resource layouts, shader inputs, pipeline execution, batching, synchronization and device-specific paths behind explicit backend contracts.

This is a domain inventory, not a requirement that every project load every capability. A text experience may need none of the GPU visual stack; a game may compose most of it.

## 3. Visual phenomena taxonomy

A phenomenon may combine multiple categories; do not force it into one “particle effect” abstraction.

| Family | Examples | Typical input | Typical visual output |
| --- | --- | --- | --- |
| Opaque/translucent surfaces | terrain, fungus, armor, glass, water surface | geometry, transforms, material intent | rasterized or otherwise rendered surfaces |
| Point/particle populations | spores, dust motes, embers, droplets, debris | instances or particle state | points, sprites, instanced geometry, trails |
| Participating media | gas, fog, smoke, haze, steam | density and optical parameters; optional fields | volume integration, slices, ray marching, or a cheaper fallback |
| Flowing media | wind-blown gas, smoke, water spray | velocity/advection fields or trajectories | evolving plume and coherent motion cues |
| Weather | rain, snow, hail, storm clouds | event/field data and local conditions | precipitation, clouds, wetness, visibility changes |
| Energy/emission | sparks, arcs, flame, glowing spores | emission events, intensity, lifetime | emissive geometry/particles, light contribution where supported |
| Contact/impact | dust puff, splash, fragments, deformation | event position, impulse/energy descriptor, materials | burst, reaction, trails, surface response |
| Optical atmosphere | shafts of light, haze, heat distortion | media, light, depth, temperature-like cue | scattering, attenuation, refraction approximation |
| Persistent environmental state | accumulated ash, wet surfaces, stains, rot visuals | durable state or material parameter updates | surface/terrain/material changes |
| Stylized/nonphysical effects | magic, authored aura, UI/world-space glyphs | artist-authored parameters and timeline | intentionally nonphysical visual language |
| Image-space composition | exposure, bloom, color transform, depth effects | rendered image and camera data | final image processing |

## 4. Effect descriptions need explicit contracts

A reusable visual effect should be data-driven and declarative. The precise C# types should follow repository conventions and vertical proofs, but the semantic contract should cover the following when relevant:

- Stable effect/phenomenon identity and source identity.
- Coordinate frame, transform, bounds, orientation, and units.
- Start time/tick, duration or termination rule, current revision, and deterministic seed where reproducibility is required.
- Semantic family and rendering intent: surface, particle, volume, optical, post-process, or a composition of these.
- Inputs and provenance: authoritative simulation fields, authored controls, derived values, and presentation-only parameters must be distinguishable.
- Resource references and version identity for geometry, materials, textures, noise, and shader programs.
- Density/opacity, color/emission, temperature-like visual cue, optical depth, scattering/absorption parameters, and velocity where the chosen model defines them. Never assume all values exist.
- Interaction inputs: light, shadow, depth, terrain, obstacles, weather, camera, and other media when supported.
- Quality/fallback policy, bounds on memory/work, and observer relevance.
- Ownership/lifetime, reset/disposal behavior, and what must be reconstructed after reload or device loss.

A visual effect is not required to be physically accurate. It is required to declare whether it is physically driven, artistically authored, derived from a simulation, or a cheap perceptual approximation.

## 5. Event and field are different forms

An event is a cause or transition: a fungus opens a vent, a pipe bursts, a rock impacts, a spell ignites. A field describes state over space and time: gas density, wind velocity, humidity, temperature, pressure, or light attenuation.

The same event may create several field and presentation products:

```text
Release event
  ├── physical source / reservoir state
  ├── simulation field or particle trajectories
  ├── gameplay exposure / hazard state
  ├── Visualize plume and particles
  ├── optional sound event
  └── optional haptic cue
```

Visualize should accept events for immediate transient presentation and evolving fields for coherent environmental effects. An event can be rendered before a high-resolution field exists; a field can continue evolving after the initiating event has ended.

## 6. The gas-spore example, from source to readable hazard

A scalable composition could use the following stages:

1. **Trigger:** The domain emits a stable release event when its sensing and rules permit it.
2. **Source:** The physical model provides release rate, source transform, and any defined pressure, temperature, or composition data. If no physical model exists, an authored source curve can be used and labelled as such.
3. **Transport:** A domain simulation may provide a velocity/density grid, particles, coarse cells, or another representation. Visualize must not require a particular solver representation.
4. **Representation:** Renderer determines which sources, field regions, boundaries, and threat cues are observable and at what fidelity.
5. **Visual realization:** Visualize creates an initial jet/burst, turbulent-looking plume, spreading body, drifting edge, light attenuation, partial occlusion, and dissipation cues. A single effect may combine a coarse volume with sparse particles and a few local high-detail emitters.
6. **Gameplay readability:** The experience exposes hazard boundaries or cues appropriate to the game design. The actual hazard remains a domain/simulation value; visible opacity alone is not a reliable toxicity metric.
7. **Lifecycle:** The source can stop while the cloud persists; regions can advect, mix, settle, or decay; resources can be reclaimed when no observer or simulation consumer needs detailed presentation.
8. **Many sources:** Multiple mushrooms may share shader programs, material definitions, volume atlases, noise resources, and execution batches. Source identity and physical state remain distinct even when visual work is aggregated.

The architecture must support a coherent cloud generated by dozens or hundreds of sources without forcing one expensive, separately allocated volumetric simulation and draw per source.

## 7. GPU-first, not GPU-at-all-costs

The target is to put suitable high-volume, parallel, image-producing work on the GPU while retaining CPU/domain ownership of authoring, coordination, discrete state transitions, resource lifetime, and correctness.

### Strong GPU candidates

- Vertex/instance transforms and procedural visual detail.
- Particle integration for presentation-only particles or simulation data explicitly delegated to GPU.
- Field sampling, volume ray marching/integration, optical attenuation and scattering approximations.
- Surface shading, material evaluation, lighting and shadow passes.
- Depth/visibility operations, compositing, temporal accumulation, and post-processing.
- Large uniform workloads over pixels, voxels, tiles, or instances.
- GPU-side indirect command generation or culling when a concrete backend and measured workload justify it.

### Keep CPU-side unless evidence says otherwise

- Authoritative game rules and biological lifecycle decisions.
- Persistence, stable identity, schema migration, resource ownership, and capability composition.
- Irregular low-population state transitions and orchestration where transfer/synchronization costs dominate.
- Renderer policy that selects representation and computational responsibility, while allowing later GPU-assisted visibility paths.
- Small effects whose upload, dispatch, readback, or synchronization costs exceed the benefit of parallel execution.
- Validation and deterministic reference calculations used to test GPU output.

These are defaults, not ideological rules. A field solver may eventually run on GPU if Physics explicitly owns that solver and defines the data contract. Visualize must not secretly become the authoritative solver simply because compute shaders are available.

### GPU execution principles

- Keep data resident on the GPU across frames when it is reused; avoid CPU readback in the normal presentation path.
- Prefer compact, contiguous, batched data and stable resource IDs over per-object managed allocations.
- Separate persistent resources from transient frame data; track lifetime and invalidation.
- Batch compatible work, but do not merge semantic identities or authoritative state just to reduce draw calls.
- Use asynchronous compute/transfer only where synchronization and overlap are measured.
- Treat vendor-specific compute features as optional capabilities with a baseline path.
- Measure CPU preparation, upload, dispatch, GPU execution, synchronization, memory residency, and presentation separately.
- Never call a CPU-generated image, SVG mock, or neutral command packet “GPU rendering.” Record the exact stage proven.

## 8. Scale, aggregation, and event horizons

A thousand visual sources do not necessarily mean a thousand independent GPU resources or dispatches. Useful mechanisms include:

- shared pipelines, textures, noise fields, materials, and geometry;
- instancing and compact source records;
- tiled or brick-based sparse volumes;
- particle pools and bounded append/consume buffers;
- screen-space or world-space clustering where perceptually valid;
- temporal reuse and reduced update frequency for slowly changing media;
- coarse far-field fields with high-detail near-field reconstruction;
- frustum/occlusion and observer-horizon rejection before expensive shading;
- pooled transient resources and explicit budgets.

Aggregation is permitted only when it preserves the distinctions needed by the consumer. Overlapping gas sources may share a visual density field while retaining separate source IDs, source lifecycles, and per-target exposure calculations in their owning domain.

Renderer selects relevant representation and detail. Visualize realizes the selected representation. A visual fallback may be cheaper than the physical representation, but it must not alter gameplay semantics.

## 9. Persistence and reconstruction

Persist authored intent and reconstructable source data, not GPU accidents. Depending on the feature, durable artifacts may contain source parameters, volume definitions, timeline events, seeds, material references, environmental presets, and references to authoritative simulation snapshots.

Do not persist device handles, command buffers, shader instances, transient GPU addresses, swapchain objects, or frame-local caches as durable meaning. They are rebuilt after load, backend changes, or device loss. Transient effects should only be persisted when the experience explicitly requires replay, save/load continuity, or historical reconstruction.

## 10. Proof ladder and acceptance

The following is a proposed proof ladder, not a claim that these stages already exist:

1. **Contract proof:** serialize/reload a scene and a visual phenomenon with stable IDs, coordinates, revisions, and provenance.
2. **Surface proof:** submit one known surface/material/light through the Visualize-to-Renderer boundary.
3. **Transient proof:** render one event-driven burst with deterministic lifetime and resource reclamation.
4. **Field proof:** render a bounded density field with explicit units/parameters and a reproducible reference image.
5. **Light/media proof:** demonstrate a light interacting with the field and label approximations.
6. **Transport proof:** animate a field or particle representation from deterministic input data without Visualize owning the solver.
7. **Hazard proof:** run the mushroom scenario with a simulation-owned hazard field and a separately derived visual cloud; test that changing visual quality does not change gameplay results.
8. **Crowd proof:** use many sources, measure allocations, CPU time, GPU time, memory, upload traffic, dispatch/draw count, and image quality across observer horizons.
9. **Persistence/device proof:** reload authored state and reconstruct resources; test fallback and device-loss paths.
10. **Composition proof:** install/remove volume, weather, particle, lighting, and post-process capabilities independently.

Each proof must state whether it is design, CPU model, browser experiment, software rasterization, GPU compute, or actual GPU image output. Do not claim a capability based on a diagram.

## 11. Acceptance principles

- Visualize covers the image-producing domain, not the authority for physical or narrative truth.
- Events, evolving fields, surfaces, particles, volumes, and image-space operations are distinct composable forms.
- Physical inputs, artistic controls, derived fields, and presentation approximations remain identifiable.
- The architecture supports mixed representations and many concurrent sources.
- GPU work is selected by parallelism, data locality, reuse, and measured cost—not by ideology.
- CPU/GPU boundaries minimize needless transfers and readback while preserving explicit ownership.
- Event Horizon quality reduction changes presentation cost, not authoritative consequences.
- All effects have explicit identity, time/revision, bounds, resource lifetime, and fallback expectations where relevant.
- Implemented code, experimental evidence, and future direction are clearly distinguished.

---

*The Singularity Workshop — Tools for the curious, the bold, and the systemically inclined.*
