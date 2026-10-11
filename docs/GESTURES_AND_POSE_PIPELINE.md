# Gestures and the Pose Pipeline

**Status:** Cross-repository design alignment. The proposed Gestures package does not yet exist.

## 1. The Workshop meaning of a Gesture

A Gesture is not merely an animation file. It is a procedural physical action expressed as a sequence of poses plus mathematical transitions, progression rules, completion conditions, and relevant spatial anchors or constraints.

The ideal Gesture says what physical action is intended. A soldier's stable identity, current state, context, equipment, and bounded variation determine how that soldier performs it.

A vehicle may provide the Gesture for entering its cockpit. The soldier discovers the need, moves to the interaction point, asks the vehicle's capability for the procedure, and executes it. The vehicle owns its own physical interaction knowledge; the soldier does not contain a giant table of every vehicle's boarding animation.

See [raWWar Gestures Design](https://github.com/TrentBest/raWWar/blob/development/docs/GESTURES_DESIGN.md) and [Renderer Integration](https://github.com/TrentBest/raWWar/blob/development/docs/RENDERER_INTEGRATION.md).

## 2. Separate the procedure from its presentation

~~~text
Need / order / current FSM state
              |
              v
Domain + FSM_API select and progress procedure
              |
              v
Gesture Provider supplies the physical procedure
              |
              v
Gesture evaluation
  - pose sequence
  - transition mathematics
  - progress / rate
  - attachment anchors / constraints
  - stable identity + contextual variation
              |
              v
Compact evaluated pose / instance data
              |
              v
Renderer selects representation for observer
              |
              v
Visualizer submits GPU work
              |
              v
Visible motion
~~~

This is an intended division of responsibility, not a claim that all stages are implemented.

## 3. What the GPU should receive

The GPU should receive compact, batchable values, not one high-level animation controller object per soldier. Depending on the eventual pose representation, a batch may include:
- stable entity or instance identity;
- root/world transform;
- current and target pose indices or palette offsets;
- transition progress and interpolation parameters;
- bounded variation parameters derived from stable identity and context;
- relevant equipment or attachment configuration;
- selected fidelity/update class from Renderer policy.

This list is illustrative, not a frozen binary schema. We must choose layouts from actual Gesture math, access patterns, precision needs, and measured bandwidth. The earlier four-8-bit-channel-per-texel idea is a candidate packing strategy, not a universal storage rule.

## 4. Data-parallel versus context-sensitive work

Good candidates for GPU evaluation:
- the same transition formula across large batches;
- deterministic variation from precomputed identity/context values;
- pose interpolation or palette blending;
- repeated transform composition;
- GPU-side preparation of instance data;
- reduced-detail pose updates at farther Event Horizons.

Work that must not be moved to the GPU merely to claim GPU usage:
- authoritative FSM transitions;
- choosing whether a soldier is qualified to perform a job;
- deciding whether an interaction point is valid;
- domain-level collision outcomes or irreversible world events;
- arbitrary branches that perform poorly as a parallel workload;
- any result that would become authoritative without a clear synchronization and determinism contract.

A hybrid pipeline is expected. Measure the complete cost, including transfers and synchronization.

## 5. Individualization without random noise

Ten thousand soldiers should not look like one soldier duplicated ten thousand times. Variation should be bounded and driven by stable identity and relevant context, rather than per-frame uncontrolled randomness.

Examples include stride phase, timing, head direction, posture, small offsets, hesitation, recovery, and correction. Variations must respect physical constraints: they cannot place a hand through a cockpit frame or randomize a foot into an impossible support position.

A deterministic seed alone does not solve all context changes. The Gesture contract still needs to define which inputs remain stable, which are time-varying, and how variation changes across interruptions, equipment changes, injuries, and different procedures.

## 6. Event Horizon fidelity

| Observation level | Gesture / pose work | What must survive |
| --- | --- | --- |
| Near | richer pose progression, equipment interactions, secondary motion, bounded individual variation | physical credibility and readable action |
| Middle | fewer pose details, reduced secondary motion, lower update frequency | role, direction, major action, unit coherence |
| Far | simplified pose or silhouette, aggregate motion, sparse updates | identity class, formation, tactical readability |
| Semantic / not visually represented | no per-frame pose required unless another observer needs it | authoritative world state continues |

Event Horizons select the necessary computational representation. Visualizer executes the selected representation; it does not independently decide that distant soldiers stop existing.

## 7. Why a Gesture package may be warranted

A standalone **TheSingularityWorkshop.Gestures** package/repository is a strong candidate because procedural motion is a reusable capability, not merely a raWWar animation subsystem.

Its responsibility would be to define and evaluate Gesture procedures independently of WPF, a specific graphics API, and a specific Experience. It should be consumable by domain providers and produce data that a CPU reference evaluator and future GPU evaluators can agree on.

Candidate contents:
- canonical pose/anchor abstractions;
- Gesture definition and transition model;
- progression/completion/interruption semantics;
- bounded, deterministic individualization inputs;
- a CPU reference evaluator for correctness tests;
- compact evaluated output suitable for batching;
- test vectors that GPU implementations must match within documented tolerances.

It should not depend on Visualizer or Renderer. The dependency direction should be:
- Experience / interaction provider defines or selects the physical procedure;
- Gestures defines and evaluates the procedure;
- Renderer decides the observer-relative fidelity;
- Visualizer consumes the presentation projection and executes GPU work.

**Recommendation:** do not put Gesture semantics inside Visualizer. First use the cross-repository contract here to clarify the pose model, then create the independent Gestures package when its public data model is sufficiently clear. This keeps the GPU project from becoming the accidental owner of motion semantics.

## 8. Decisions that remain open

- canonical joint/skeleton/pose representation;
- how a pose references equipment-specific attachment points;
- how transitions and physical constraints are expressed;
- how Gesture interruption and failure behave;
- how root motion interacts with authoritative movement;
- what the CPU evaluator guarantees deterministically;
- what precision and tolerance the GPU evaluator must match;
- whether state is represented as buffers, textures, or a hybrid;
- when a result is read back, if ever;
- how Gesture providers package versioned data through MicroBundles.
