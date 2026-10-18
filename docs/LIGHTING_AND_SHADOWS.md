# Physics-Based Lighting, Shadows, and Extensible Math

**Status:** Design direction and staged investigation. No lighting model, graphics backend, math package, or optimization technique has been selected.

## 1. The goal: believable light, efficient presentation

Visualize should support lighting grounded in physical principles rather than treating illumination as arbitrary decoration. The scene may include direct light sources, emitted intensity, distance and direction, surface response, occlusion, atmospheric scattering, and indirect illumination. The exact model and its level of approximation depend on the scene, the selected backend, and measured constraints.

This matters especially for **raWWar**: a dark, gritty battlefield can still have strongly illuminated military installations, pools of light around structures, moving beams from soldiers' headlamps, deep occlusion, dust and smoke that scatter or obscure light, and shadows that make terrain and silhouettes legible. Darkness is not simply lowering the screen brightness; it is a lighting environment whose sources, falloff, materials, and visibility must work together.

The goal is not to implement a complete physically based renderer before the first visible scene. The goal is to preserve the right boundaries so we can introduce physical fidelity progressively.

## 2. Separate light meaning, scheduling, and execution

| Responsibility | Owner |
| --- | --- |
| Authoritative lights, emitters, material meaning, world changes, and gameplay state | Experience / domain |
| Observer-relative representations, Event Horizon membership, and what visual work is warranted | Renderer |
| Capability composition and MicroBundle arbitration | FSM_COS / capability composition |
| Stateful coordination, dependency ordering, due-work scheduling, and deferred evaluation | FSM_API |
| GPU resources, lighting/shadow passes, compute/graphics execution, composition, and diagnostics | Visualize and its selected backend |
| Optional reusable math algorithms, approximation tables, and backend-specific implementations | A future math capability/package, if evidence justifies it |

A lighting capability may contribute lighting work and requirements without dictating the number or names of Event Horizons. A shadow capability should likewise attach a shadow work bucket to each applicable horizon through a general contract. The capability describes its behavior and constraints; the active runtime supplies the horizons and resolves the compatible work.

Visualize should not become the owner of authoritative light sources or world physics. It consumes presentation-ready source/material data and revisions, then executes the selected calculations. GPU results remain derived and reconstructable.

## 3. Independent work buckets and cadence

A displayed frame does not require every visual subsystem to recompute. Geometry, motion samples, lighting, shadows, atmosphere, and composition may have different dependencies and refresh schedules.

Conceptual example only:

~~~text
Event Horizon (any applicable horizon)
  ├── Representation / geometry bucket
  ├── Motion presentation bucket
  ├── Lighting bucket
  ├── Shadows bucket
  ├── Atmosphere / dust bucket
  └── Composition bucket
           |
           v
  Dependency-aware arbitration and scheduling
           |
           v
  Reuse valid results; execute only due or invalidated work
~~~

A MicroBundle such as **CrispShadows** could contribute a shadow capability and scheduling policy. Arbitration would apply it to compatible horizons automatically, regardless of how many horizons exist. Another bundle might provide a cheaper shadow method, an optional ray-traced path, or a deliberate no-shadow policy for a particular representation. The runtime chooses only among capabilities that are installed, supported, and compatible with the scene and quality/performance policy.

Cadence is a freshness policy, not a physical law. For example, testing motion presentation at 30 updates per second and shadows at 15 is a reasonable experiment, not a universal constant. A changed light source, newly exposed surface, moving shadow caster, observer transition, or other correctness-relevant dependency may require an earlier refresh. Conversely, a stable distant result can be reused. Visual scheduling must never reduce authoritative simulation, collision, damage, or other gameplay update requirements.

## 4. Light sources across horizons

Lighting work should scale with what a horizon's representation can meaningfully express.

- **Immediate horizon:** nearby sources and casters may need frequent, detailed treatment.
- **Intermediate horizons:** group or cull irrelevant sources, reuse stable lighting data, and choose approximations appropriate to the representation.
- **Distant horizons:** represent broad illumination, large shadow structure, or baked/static results where the source representation supports it; do not simulate every nearby light's full cost at world scale.
- **Transitions:** prevent obvious pops when a representation moves between horizons; invalidate or blend only the dependent results needed for a correct transition.

Consider a lit military base at night and 1,000 marching soldiers, each with two headlamps. The base can use mostly static or slowly changing lighting where justified. Headlamps are dynamic sources, but not every lamp needs an equally expensive shadow calculation at every display frame. Renderer can identify relevant representations; Visualize can batch source data and select supported passes; the scheduler can update buckets at different rates and aggregate distant sources when the representation allows it.

The two headlamps per soldier are a scene requirement, not a demand for 2,000 independent CPU objects or draw calls. Source records, relevance lists, and GPU work should be laid out and processed in batches where practical.

## 5. Invalidation follows dependencies

A single global "scene changed" flag is too coarse for the intended architecture. Cache validity should track the inputs that actually affect each result. Potential dependency classes include:

- light-source transform, intensity, color/spectrum representation, range, or enablement;
- geometry, caster, material, and surface-response revisions;
- observer pose and horizon representation;
- atmosphere, dust, smoke, and relevant visibility changes;
- backend resources, shader/pipeline revisions, and capability changes;
- a freshness deadline or explicit quality-policy change.

A change to a light may invalidate its affected lighting and shadow results without forcing unrelated geometry to be rebuilt. A geometry change may invalidate shadows cast by that geometry, but not every horizon's lighting. The exact dependency graph is an implementation contract to test, not a reason to build a sophisticated cache before we have a simple end-to-end scene.

## 6. Physical model first; optimizations remain optional

The architecture must leave room for multiple ways to evaluate lighting and related math:

- native CPU or GPU math;
- physically based shading models and progressively richer approximations;
- shadow maps, ray queries/tracing, or other backend-supported methods;
- baked or precomputed lighting where scene and usage permit;
- lookup tables (LUTs), polynomial approximations, and specialized algorithms;
- different implementations selected by capability, hardware support, workload, or quality policy.

These are options to investigate, not promises that every backend supports them and not a requirement to build a separate math package immediately.

A future math capability could expose an operation or approximation set and advertise its supported domain, precision/error bounds, storage cost, and execution targets. A lighting capability could declare that it can use such an implementation. Arbitration could select it only when compatible and beneficial. Avoid a hard dependency from Visualize core onto a hypothetical math library before a real workload demonstrates the need.

### LUT caution

Replacing every square root with a lookup is not automatically faster. Native instructions may be highly optimized; a LUT may add memory traffic, cache misses, interpolation, upload cost, and numerical error. Any comparison must use the actual CPU/GPU and access pattern, and must record both throughput and visual/physical error. Test native math, LUTs, and polynomial or other approximations before selecting one.

LUTs and baked lighting are different things:

- **Math LUT:** reusable approximation of a mathematical function over a defined domain.
- **Baked lighting data:** precomputed result tied to particular scene geometry, materials, lights, or assumptions.

Each needs provenance and versioning appropriate to its type. Scene-specific baked data also needs clear invalidation when its source assumptions change.

## 7. Baby-step implementation path

1. **Document the boundary** between light-source meaning, Renderer relevance/horizon decisions, scheduling, and Visualize execution.
2. **Build one visible scene** with a small number of explicit light sources and a basic supported lighting path; establish correct source units/conventions and a reproducible reference image.
3. **Separate the lighting and shadow work descriptions** from geometry work, even if the first backend initially executes them together.
4. **Introduce explicit revisions and invalidation tests** for a moving light, moving caster, and unchanged distant representation.
5. **Add independent refresh policies** and measure stale-result artifacts, CPU scheduling, GPU pass time, and frame-time variance.
6. **Scale the workload** toward a lit base, dust/smoke, and many headlamps; compare batched source handling with naive per-source work.
7. **Evaluate optional math and lighting capabilities** only after profiling identifies a real bottleneck. Benchmark candidate methods for performance, precision, memory, integration cost, and target compatibility.

Do not start by implementing a universal math library, every lighting algorithm, or every MicroBundle. Preserve the extension points, make the first slice correct and measurable, then add complexity in response to evidence.

## 8. Physics integration: one phenomenon, two responsibilities

Lighting should be a shared physical concept across the Workshop, but **Physics and Visualize must not become the same subsystem**.

Physics should own physical consequences where the simulation needs them: energy emitted or absorbed, radiative heating/cooling when modeled, material/medium interactions, and any light-related quantities that affect authoritative world state. It should expose those results as backend-neutral data with explicit units, coordinate conventions, and revisions. A light that heats a surface, an arc that deposits energy, or hot material that radiates should be representable as physical cause-and-effect—not merely as a color chosen by a shader.

Visualize owns the image-producing approximation: how relevant sources illuminate visible surfaces, how material response is shaded, how shadows and occlusion are approximated, and how atmosphere, dust, and smoke affect the displayed image. It may consume source and medium data from Physics, but it must not be the authority for gameplay heat, damage, or other simulated outcomes.

The distinction matters because visual and physical fidelity have different budgets. A far-away lamp may be represented by a cheap visual aggregate while the physical simulation uses its own valid energy-transfer model; conversely, a visual shadow may be refreshed less often without slowing collision, thermal, or other authoritative simulation. **Never use the lighting shader as the thermal solver, and never require Physics to reference Visualize or a graphics API.**

Suggested neutral handoff:

~~~text
Experience / world state
   ├── Physics: source energy, absorption/emission, physical consequences
   │       └── neutral source / medium / energy snapshots + revisions
   └── Renderer: observer relevance, representation, horizon and cadence policy
                   |
                   v
             Visualize: shading, shadows, atmosphere, GPU execution
~~~

The data contract should start small and explicit: source identity, source kind, position/direction, emitted intensity or radiometric quantity with documented units, spectrum/color representation, range or attenuation model where applicable, enabled state, and a content revision. Physics should publish only the quantities it actually models; Visualize may derive display color, artistic grading, and backend-specific parameters without rewriting physical state. Surface material data should likewise distinguish physical properties (where modeled) from presentation controls (such as a deliberately stylized tint).

A shared neutral contract can live in a lower-level domain/math package only when both consumers genuinely need it. Do not make Physics depend on Visualize, and do not add a new shared package just to speculate. First agree on units, ownership, and a small testable source snapshot; then implement the smallest vertical slice and verify that physical changes update presentation while presentation-only changes cannot alter authoritative simulation.

## 9. Acceptance principles

- Light sources and surface response have documented physical meaning and consistent units/conventions.
- Renderer and Visualize retain separate semantic and execution responsibilities.
- Lighting and shadows can be scheduled independently without hard-coded horizon names or counts.
- Cache invalidation follows dependencies; important changes cannot be hidden indefinitely by a refresh interval.
- Visual cadence never substitutes for authoritative simulation cadence.
- Optional math/lighting capabilities do not become mandatory core dependencies without evidence.
- Benchmark reports state hardware, backend, workload, quality, numerical error where relevant, CPU/GPU cost, memory, transfers, and frame-time distribution.
- The current implementation status is stated honestly; a design possibility is not presented as implemented functionality.

---

*The Singularity Workshop — Tools for the curious, the bold, and the systemically inclined.*
