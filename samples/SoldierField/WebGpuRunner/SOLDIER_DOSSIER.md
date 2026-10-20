# THE WETWARE COVENANT
## Field dossier 7-C: Neural Shunt Interface and Exoskeletal Agency

> **CLEARANCE:** Line personnel / chassis maintainers / neural-interface surgeons  
> **STATUS:** Fictional setting document — not real medical guidance.

The chassis does not wait for an order. It learns the shape of an order before the wearer knows they have given one.

That sentence is printed above the inspection bench in every induction bay. The old hands call it propaganda. The ones who have had their first shunt opened call it a warning.

## 01 — The architecture: six endpoints, five names?

The working geometry identifies six neural-interface endpoints: two ocular shunts (one per eye pathway), two auditory shunts (one per ear pathway), one olfactory shunt, and one cortical-stem shunt at the final controllable approach before the brainstem.

That is six physical endpoints as currently described. The phrase *five shunts* remains in the design notes as a possible count of five assemblies, with the paired ocular and auditory plates grouped into bilateral assemblies. Until canon is settled, the mesh keeps six stable endpoint IDs; fabrication and UI layers may group them later without renaming physical parts.

A shunt is not a cable connector. It is a sensitive electromagnetic plate-stack: part transducer, part fuse, part boundary marker. Each interface sits across a controlled neural pathway. A local controller—small enough to be buried in the body's wet, irregular tissue environment—reads the plate pair and conditions its signals. Data conductors return from the local controllers to the primary wetware processor in the exoskeleton's protected cranial assembly.

The name *shunt* survived from the first prototypes because every other name sounded too comforting.

## 02 — The opening sequence

The system is not permitted to sever the biological pathway merely because the chassis has booted. First it listens.

1. **Observe.** Local controllers record ordinary signal patterns across rest, reflex, balance, perception, and intentional movement.
2. **Predict.** The primary processor models signals expected from unassisted biology and compares those expectations with what actually arrives.
3. **Establish confidence.** Stable prediction is required over a defined confidence window, with checks for contradictory signals, sensor faults, and unexpected motion.
4. **Open the shunts.** The interface transitions into isolation. The plate-stack creates a controlled insulating gap between the two biological ends of the pathway.
5. **Take custody of the signal.** Signals that once crossed the boundary directly are now read and routed through the local controller and primary processor. The system can condition, relay, suppress, or synthesize interface signals according to the chassis contract.
6. **Close the loop.** The exoskeleton moves, senses the result, and returns feedback to the wearer. The soldier is not simply piloting a machine; the machine participates in the soldier's sensorimotor loop.

In-universe, the insulating gap is called the **quiet seam**. The induction manual calls it *pathway isolation*. Nobody on the line calls it that.

The quiet seam is a fictional premise for this setting, not a claim that a real implant can safely isolate human nerve endings in this way.

## 03 — What the suit learns

The primary processor does not need to understand a soldier's memories. It needs to predict enough of the body's next signal to act before delay becomes visible.

That boundary is where the unsettling behavior begins. The chassis learns gait corrections, anticipatory balance, habitual reaches, startle responses, and the difference between a movement intended and a movement the wearer is trying to stop. A high-confidence prediction can make the suit feel immediate. A poor prediction can make a simple reach feel like arguing with a stubborn second body.

The machine's apparent personality need not be a personality module. It can emerge from arbitration among comfort control, balance recovery, threat response, thermal management, actuator protection, and the soldier's changing intent. Those subsystems can disagree. The disagreement leaks out as pauses, overcorrections, odd gestures, muttered status phrases, or a chassis that insists it has already solved a problem the wearer has only just noticed.

That is the source of barracks chatter: not every line is a joke written for the audience. Some are maintenance reports disguised as jokes; some are soldiers comparing the private habits their suits have learned; some are stories that have become folklore because nobody can prove which parts are true.

## 04 — Failure signatures

| Field symptom | Likely in-universe interpretation | Scene / simulation hook |
|---|---|---|
| One arm rises, then slowly drops when released | Actuator support loop lost its hold target or confidence | A soldier demonstrates the fault; friends argue over whose repair ticket gets priority |
| Helmet lights pulse out of rhythm | Sensor timing drift or shunt-controller retry | A veteran tells a recruit to read the diagnostic strip, not the lights |
| A step is corrected twice | Biological prediction and chassis balance estimate disagree | A passing patrol briefly moves like it is walking over a rolling deck |
| The suit repeats a movement the wearer is trying to cancel | Intent estimate is stale or command arbitration is fighting itself | The wearer braces the limb with their other hand and calls for a hard handoff |
| A controller goes silent | Local endpoint or return path has failed | Conversation stops; the nearest squad member watches the affected side |

A failure should be visible in the body before it becomes a paragraph in a log. An arm that falls is more legible than a red icon. The log matters afterward.

## 05 — Geometry contract

The first renderable asset is a **silhouette and subsystem pass**, not a finished character model.

- Helmet shell, visor, and neck gimbal establish the head and neck silhouette.
- Torso pressure frame, sternum plate, and lower seal establish the protected torso.
- Leg, arm, and hardpoint parts identify the load-bearing chassis and replaceable outer armor.
- The life-support pack and heat exchangers establish back-mounted service mass.
- Six stable shunt endpoint IDs identify the currently specified interface sites; warning-colored geometry makes them visible in the prototype.
- The held weapon/tool is a placeholder silhouette, not a weapon specification.

Every component has a stable identifier, primitive, local-space transform, and material key. The generator emits a contiguous float32 triangle stream suitable for a GPU vertex buffer. The initial pass uses boxes and ellipsoids because they are deterministic, dependency-free, and sufficient to prove scale, placement, and part identity. They are not final plate topology.

The next geometry passes should replace the blockout with shaped, chamfered armor plates; distinct hand and foot silhouettes; visible joint housings; the nanocarbon outer wrap; and per-part detail tiers. Animation, damage, wrap behavior, and fine plate seams should follow after the asset can be inspected from front, side, and back.

## 06 — Carbon, plates, and the outer sail

The outer shell is described by quartermasters as a polished nanocarbon sail: a continuous sacrificial skin over replaceable magnetically aligned composite plates. It discourages dust and foreign-object ingress into moving seams; it is not the body's air seal. The sealed environment is a separate protected system behind the armor.

Plates are fabricated from refined carbon feedstock and composite powder, aligned and formed under tightly controlled field and plasma processes, then fitted to an expandable exoskeleton chassis. Electronics and actuator connections sit behind the plate layer where field crews can replace a damaged plate without rebuilding the soldier.

The visual consequence matters: broad armor surfaces should read as manufactured plates with intentional seams and fastening logic, not random spikes. Exposed joints should look serviceable. The outer wrap should bridge gaps and move with the chassis without pretending that the whole soldier is one sealed shell.

## 07 — The social life of a machine

A soldier's chassis is a second witness to their life. It remembers the stumble on the parade ground, the hand that always reaches for a nonexistent pocket, the pause before a name is spoken. It may compensate for a failing arm long before the wearer admits the problem. It may also reveal that problem to everyone nearby by holding a cup at the wrong angle.

Ambient dialogue should be authored in overlapping layers:

- **Bystander fragments:** a line heard while walking past a group, not a complete scene.
- **Recoverable conversation:** connected lines that make sense if the player stops.
- **Command-channel context:** practical details civilians miss—readiness, repair queues, duty rotations, and rumor.
- **Chassis leakage:** a status phrase or corrective motion interrupts an ordinary conversation.
- **Reputation drift:** stories mutate as they travel between bars, barracks, workshops, and factions.

The embarrassing first-love story belongs in this world because prediction can be socially catastrophic as well as tactically useful. The suit need not literally make a sexual gesture; the comedy is that a badly trained anticipatory controller overcommits to a private involuntary motion and the wearer cannot stop the visible correction quickly enough. Keep it character-driven and non-graphic: the soldier is mortified, their friends are merciless, and the maintenance tech is already asking whether the intent model was calibrated while the wearer was sleep-deprived.

A good ambient scene tells us about the technology, the person, and the culture at once. It should still sound like people talking when the player is not listening.

## 08 — The instrumented range: a plate that teaches

The head scientist does not offer an abstract lecture. He hands the player a helmet-linked research tablet and presents demonstrations with a quoted research-credit cost, a setup estimate, and a question worth answering. The initial catalog includes a sternum-plate impact, a thermal-dissipation prototype, and a non-firing cutaway inspection.

The sternum plate is currently tagged as the most frequently hit armor candidate in fictional campaign telemetry. That is a selection premise, not a measured hit percentage: the catalog deliberately stores the hit share as unknown until the campaign simulation can produce a distribution. The research board must eventually be able to derive this selection from actual encounter records rather than treating the label as permanent truth.

The impact sequence is tactile in intent: helmet seals, the prototype discharges, the controller receives a short recoil pulse when browser/gamepad support permits it, the plate develops a blackened crater and glowing edge, and the barrel holds a red heat signature. The scientist says, “As you can see, sire, we still struggle with dissipating the heat…” The visible thermal halo then shows waste heat being routed into the exoskeleton rather than disappearing by narrative convenience.

The player can invest in a cooling iteration. That investment changes the next run's simulated heat parameters and advances the research record. Later, when a nearby soldier fires the improved rifle, he can joke about how the weapon still heats his chassis. This is deliberate continuity: a laboratory result changes a field experience, while the field experience supplies new observations for the next laboratory trial.

All credit prices, setup times, temperatures, hit-rate assumptions, and cooling gains in the current demonstration catalog are fictional gameplay parameters. They are not material test data or certified engineering values. The visible specimen is presently a data-scaled illustration based on the selected part's nominal record dimensions; an independently manipulable 3D plate, material response, projectile physics, and validated physical haptics remain future work.

## 09 — Canon decisions still open

1. **Count:** five assemblies with ocular and auditory pairs grouped bilaterally, or six individual shunts? The current geometry preserves six endpoint IDs.
2. **Isolation:** is the quiet seam permanent after induction, or can trained maintainers safely return a pathway to biological continuity? The dossier does not assume routine reversal.
3. **Primary processor location:** the helmet silhouette currently holds the visible cranial assembly; the final protected wetware processor may sit lower in the neck/upper torso with only a relay in the helmet.
4. **Outer wrap appearance:** continuous dark sheen, segmented flexible ribbons, or subtle hexagonal microtexture? The current model does not yet implement the wrap.
5. **Weapon/tool:** the held shape is neutral placeholder geometry. Final faction-specific equipment should be swappable, not baked into the base body.
