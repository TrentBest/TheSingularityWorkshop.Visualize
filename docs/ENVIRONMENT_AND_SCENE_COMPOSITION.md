# Environment and Scene Composition

**Status:** Architecture direction. This document defines ownership and seams; it does not claim that every modality or volumetric technique is implemented.

## 1. The scene is more than a picture

The Workshop needs a composable **Environment**: the coherent scene experienced by an observer, assembled from world meaning and whichever sensory and interaction capabilities that experience requires.

A scene may include geometry and materials, light, air and weather, sound, haptics, motion, spatial relationships, and interaction affordances. A book may primarily need authored text and illustrations; a film may compose image, sound, and timing; a game may additionally need physical interaction, changing world state, and haptic feedback. These are different compositions of capabilities, not reasons to force every experience through one monolithic runtime.

**Visualize remains the image-producing capability. Environment is the scene-level composition concept.** It brings modality-specific outputs into a shared authored and runtime context without requiring Visualize core to own audio engines, haptic devices, story logic, physics, or persistence.

## 2. Ownership boundaries

| Concern | Primary owner |
| --- | --- |
| Persistent authored scene, story, stage, timeline, and experience configuration | Forge / authoring and persistence layer |
| Authoritative world state and domain meaning | Experience and domain models |
| Physical consequences: forces, collisions, material state, energy transfer, weather dynamics where simulated | Physics and relevant domain capabilities |
| Observer-relative relevance, Event Horizons, representation choice, and computational detail | Renderer |
| Images: geometry manifestation, materials, lighting/shading, shadows, volumetrics, visual atmosphere, composition, and visual backend execution | Visualize |
| Sound sources, propagation approximations, mixing, spatial audio, playback | Audio capability / backend |
| Contact, vibration, force, and device-specific feedback | Haptics capability / backend |
| State coordination, dependency ordering, deferred execution, and lifecycle | FSM_API and composed runtime |
| Which capabilities are installed and how compatible components are assembled | FSM_COS / MicroBundle composition |

These boundaries are deliberately directional. Physics must not depend on Visualize. Renderer must not depend on Visualize. Visualize must not become the authority for story state or physical consequences. Audio and haptics are optional siblings, not mandatory dependencies of an image renderer.

## 3. A shared scene, multiple manifestations

The shared scene should preserve identity and meaning while each output capability builds the representation appropriate to its channel and observer.

```text
Forge-authored Environment / Experience
  ├── persistent scene, story, stage, timeline, identity
  ├── domain state and interactions
  └── composed capabilities
        ├── Physics ── physical state and consequences
        ├── Renderer ── observer, relevance, horizons, detail
        ├── Visualize ── image / volumetric presentation
        ├── Audio ── sound and spatial mix
        └── Haptics ── contact and device feedback
```

The shared model should not require every capability to materialize every object. A storm might be one authored phenomenon whose physical model updates pressure, moisture, wind, and precipitation; Visualize renders cloud volume, fog, rain streaks, and surface wetness; Audio produces wind and thunder; Haptics may provide optional impact or environmental feedback. Each capability consumes only the data it needs and may use a different update cadence.

Modality outputs must remain synchronized through shared identities, timestamps or simulation ticks, coordinate conventions, and revisions—not through a giant object that owns every implementation.

## 4. Volumetric rendering belongs in the visual capability set

Visualize should be designed to accommodate volumetric representations as a first-class path alongside surfaces and lines. This is an architectural target, not a claim that production volume rendering already exists.

Potential volume families include:

- **Participating media:** fog, smoke, haze, dust, steam, gas, and cloud.
- **Weather and precipitation:** rain, snow, spray, and storm structure.
- **Particles and motes:** dust motes in a light beam, embers, debris, droplets, and fine effects.
- **Fields:** density, temperature, humidity, velocity, pressure, emission, and other sampled scalar/vector quantities when supplied by a domain capability.
- **Authored volumes:** fog banks, force fields, bounded effects, invisible trigger volumes, and stylized story-space effects.
- **Mixed scenes:** opaque/translucent surfaces, particles, volumes, and light sources interacting in a single composition.

Do not force these into a single storage or rendering technique. Depending on workload, a capability may use sparse grids, tiled or brick data, particles, analytic volumes, ray-marched representations, impostors, or another supported technique. Selection must follow measured needs, platform capability, representation fidelity, and observer relevance.

A volume description should state its coordinate frame and bounds, semantic kind, data representation and units, density/field meaning, time or revision, and any emission/absorption/scattering parameters that are actually defined. Missing physical quantities must not be silently invented. Artistic controls may supplement physical data but must remain distinguishable from authoritative simulation inputs.

## 5. Environment phenomena have several owners

A useful example is dust in a shaft of light:

1. The domain or Physics capability decides whether dust particles or a density field exist and how they evolve, to the fidelity required by the experience.
2. Renderer determines which portion and representation matters to the current observer and horizon.
3. Visualize decides how that data contributes to the image: visible motes, volume density, light scattering, depth fade, and temporal reuse where supported.
4. Audio may optionally represent wind or machinery; haptics may optionally represent a physical contact or event. Neither is required for the visual effect.
5. The composed runtime coordinates revisions and timing while each subsystem keeps its own budget.

The same separation applies to storms, rain, fog, smoke, snow, underwater turbidity, dust clouds, sparks, and glowing arcs. A presentation-only effect can be cheap and stylized. If a phenomenon changes gameplay or physical world state, those consequences must be computed by the appropriate domain/Physics capability, not inferred from pixels.

## 6. Composition and modularity

Capabilities should be independently installable and removable. Examples of possible bundles—not existing implementation claims—include:

- `SurfaceRendering`
- `VolumetricMedia`
- `DustMotes`
- `WeatherVisuals`
- `Lighting`
- `Shadows`
- `SpatialAudio`
- `HapticFeedback`
- `TimelinePlayback`
- `ObserverCamera`

A bundle declares its contracts, dependencies, supported data forms, resource requirements, and graceful fallback. A scene that has no audio capability must still render. A headless simulation must still advance authoritative state without a visual backend. A book-like experience can omit physical simulation; a game can compose it in.

FSM_COS/MicroBundle composition selects compatible capabilities. FSM_API can coordinate lifecycle and due work, but neither should hard-code a fixed list of visual effects or a universal frame loop. Do not create a separate package for every noun prematurely: start with contracts and vertical proofs, then split packages when independent lifecycle, dependencies, or consumers justify it.

## 7. Persistence: save the authored world, not GPU accidents

The Forge needs durable authored Environment artifacts. Persist semantic and reconstructable inputs, not transient GPU handles or backend-owned memory.

The persistent form should be able to describe, as appropriate:

- stable scene/entity/resource identities and relationships;
- transforms, geometry references, material intent, lights, and volume definitions;
- environmental phenomena and their parameters;
- modality-specific tracks and event timing;
- references to external assets with version/content identity;
- capability requirements and optional fallbacks;
- authored versus derived values, revisions, and schema versions.

GPU buffers, shader instances, shadow maps, transient particles, audio-device handles, and haptic-device state are runtime resources or derived caches unless a specific authoring feature explicitly makes a snapshot part of the artifact. They should be recreated from durable inputs and invalidated when their dependencies change.

Persistence format and storage implementation should be decided with the Workshop's existing repository and MicroBundle conventions. This document does not introduce a new persistence dependency or claim an existing Environment file format.

## 8. First vertical proofs

Build evidence in small slices rather than declaring the entire environment solved:

1. **Scene contract:** one persistent, backend-neutral scene with stable IDs, transforms, one material, one light, and explicit coordinate conventions.
2. **Visible surface:** submit a known scene through the existing Visualize API and retain the generated artifact/reference output.
3. **Volume proof:** add one bounded fog or dust volume, a simple density representation, and a reproducible image; clearly label the rendering path and limitations.
4. **Lighting interaction:** demonstrate a light visible through the volume and document which effects are approximations.
5. **Weather slice:** animate a small rain or dust field from deterministic input data; keep presentation updates separate from authoritative simulation.
6. **Multimodal composition:** attach a timestamped sound cue to the same scene event; verify that the audio capability is optional and independently replaceable.
7. **Persistence round-trip:** save and reload the authored scene, reconstruct derived resources, and compare stable semantic content and output.
8. **Scale and scheduling:** measure visible volume/particle work, memory, CPU preparation, GPU execution when a real backend exists, and update cadence across observer horizons.

Every proof must say whether it is a diagram, browser-side experiment, CPU-side artifact, or real GPU/audio/haptic execution. A storyboard is not a renderer; a successful frame conversion is not proof of pixels on a device.

## 9. Acceptance principles

- The Environment is a composition of capabilities, not a monolithic subsystem.
- Visualize can evolve toward surfaces and volumes without taking ownership of physics, story logic, audio, haptics, or persistence.
- Renderer selects relevant representation and work; Visualize executes visual presentation.
- Physics owns authoritative physical consequences where modeled; visual effects cannot silently mutate simulation state.
- Audio and haptics are optional, independently composable manifestations of shared scene events.
- Persistent artifacts contain stable authored meaning and reconstructable inputs, not backend handles.
- Units, coordinates, timing, revisions, resource lifetime, and fallbacks are explicit.
- Implemented behavior, experiments, and future targets are labelled honestly.

---

*The Singularity Workshop — Tools for the curious, the bold, and the systemically inclined.*
