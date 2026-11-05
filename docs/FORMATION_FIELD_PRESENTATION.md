![The Singularity Workshop animated moniker](https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Visualize/development/docs/assets/moniker.svg)

# Formation field: isometric assembly and cohort presentation

> **Status: visual architecture direction.** This document describes a first presentation target for a player-led formation experience. It does not claim an implemented battlefield, soldier renderer, instancing backend, formation simulation, or military command system.

## 1. The first scene should explain itself

Start with a broad, flat drill field viewed from an isometric-feeling camera. The player should immediately understand:

- where soldiers arrive from;
- which formation is being assembled;
- where its front and rear are;
- which positions are planned, occupied, or still waiting;
- how a soldier moves into position;
- how a formed unit can later fall out without disappearing.

Keep the first environment deliberately restrained: flat ground, a readable field boundary, a subtle grid or guide marks, one staging lane, one formation footprint, and a clear egress path. Do not begin with a detailed battlefield, terrain obstacles, combat effects, or elaborate scenery. The first visual proof is about formation choreography.

The camera may be isometric or orthographic with an isometric composition. That is a presentation decision. The authoritative world and formation coordinate frames must remain independent of camera orientation and screen pixels.

## 2. Establish a formation-relative field

A formation has a stable world-space origin, facing direction, and local coordinate frame. Define front/rear/left/right relative to that facing. Never derive those meanings from screen-up, screen-left, or the current camera.

The visual field should present:
- a **formation footprint** showing the planned bounds;
- **rank/file guides** or restrained slot markers;
- a **rear staging lane** from which the arrival queue approaches;
- **flank egress lanes** reserved for release;
- optional labels for formation name, unit identity, current phase, and readiness.

The guides should be visible enough to communicate intent but quiet enough that the soldiers remain the focus. They may fade as the formation becomes established, while remaining available in an inspection mode.

## 3. Fall In: show the formation being built

For the first line formation, use a single-file arrival queue from the rear. Soldiers should not teleport into a grid or all snap into place on one frame.

The formation plan assigns stable slot identities before the visual movement begins. Fill the rear-most rank first, across its width, then advance rank by rank toward the front. Each soldier:
1. leaves the queue;
2. follows a clear approach path to its reserved slot;
3. decelerates before reaching the slot;
4. turns to the formation facing;
5. makes a small final positional adjustment;
6. settles into the standing pose;
7. becomes visually marked as in position only after the authoritative state confirms occupancy.

This creates a satisfying visible progression: a line appears, then gains depth one rank at a time. The player can see the organization being built rather than being shown only the final result.

Conceptual top-down layout (the rendered camera is angled):

~~~text
                       FORMATION FRONT
             [ ][ ][ ][ ][ ][ ][ ][ ]   next rank
             [ ][ ][ ][ ][ ][ ][ ][ ]   next rank
             [ ][ ][ ][ ][ ][ ][ ][ ]   first rank filled
                         |
                         | rear arrival lane
                         |
                    single-file queue
                         REAR
~~~

The rank labels here describe the proposed fill order, not universal military naming. A formation template supplies its own geometry and terminology.

## 4. Fall Out: show an orderly release

Falling out is not despawning. A soldier leaves an assigned slot, moves through a reserved egress route, and reaches an explicit rally/staging state. Visually:
- the formation changes to a releasing state;
- a soldier is selected for release;
- the soldier steps clear of the slot and moves toward a flank egress lane;
- the slot guide becomes available only after the soldier physically clears it;
- the soldier reaches the rally area and joins the released group;
- rank and formation progress update as release completes.

The initial egress policy should avoid soldiers crossing through occupied slots. Release order is determined by the formation layout and available lanes; it must not be assumed that reversing the arrival queue is always safe. A partially released formation should remain legible and recoverable.

## 5. The same visual grammar scales to squads and platoons

Do not try to draw every soldier with equal detail at every camera distance. Preserve identity and organization while changing representation.

- **Close view:** individual soldiers, recognizable poses, slot guides, approach paths, and clear state transitions.
- **Squad view:** individuals remain distinguishable; squad boundary and readiness are visible.
- **Platoon view:** squads read as subordinate blocks with their own orientation and spacing; selected squads can be expanded.
- **Large cohort view:** platoons and larger formations become coherent visual masses with readable frontage, depth, facing, gaps, and movement. Individual soldiers need not each consume a full skeletal-rendering path.
- **Inspection/selection:** zooming in or selecting a unit reveals the subordinate elements and ultimately individual soldiers without changing their authoritative identities.

The presentation must distinguish represented population from rendered detail. A million commanded soldiers must not be described as a million fully animated, fully simulated, individually visible soldiers unless the system actually performs that work. Track commanded, materialized, actively simulated, individually evaluated, and visibly presented populations separately.

## 6. Visual state language

Use a small, consistent visual vocabulary:
- planned slot: faint guide or ghost marker;
- assigned slot: subtle assignment indicator;
- approaching: soldier visibly moving on its route;
- occupied: settled soldier with no persistent distracting marker;
- blocked/vacant: explicit exception marker in inspection mode;
- releasing: soldier moving toward an egress lane;
- released: soldier at the rally/staging area.

Avoid relying on color alone. Shape, position, motion, and optional concise labels should make state understandable. The normal view should remain clean; detailed diagnostics can be toggled or shown for the selected unit.

## 7. Ownership boundaries

| Concern | Owner |
|---|---|
| Formation geometry, slot assignment, membership, readiness, release lifecycle | Formation / command-coordination domain |
| Soldier pose procedure and per-soldier gesture evaluation | Gestures |
| Actual movement, collision, clearance, and authoritative occupancy observations | Physics / world |
| Observer relevance, representation choice, and detail level | Renderer |
| Field, soldier instances, slot guides, visual motion, lighting, and image output | Visualize |
| Player commands, selection, inspection panels, and controls | Experience / GUI |
| Runtime composition and compatible capability scheduling | FSM_COS / Foundry as appropriate |

Visualize receives stable identities, transforms, presentation state, and representation instructions. It must not become the authority for unit membership, command completion, or physical slot occupancy. The GUI presents controls and diagnostics; it does not own the battlefield simulation.

## 8. A staged visual proof

1. Render the empty field, formation footprint, staging lane, and egress lanes.
2. Render one soldier and one slot; show approach, alignment, occupancy, and release.
3. Show a small squad filling two ranks one soldier at a time.
4. Show the squad falling out without slot/path collisions.
5. Compose multiple squads into a platoon, with each squad treated as a subordinate visual element.
6. Add camera zoom, selection, and drill-down.
7. Measure instance count, CPU submission cost, GPU cost, memory, animation update cadence, and visual quality at each cohort tier.

The first success criterion is not a record population count. It is a small, visually clear formation whose Fall In and Fall Out can be watched, understood, interrupted safely, and replayed consistently. Scale follows from that grammar.

---

<p align="center">
  <a href="https://github.com/TrentBest/FSM_API">
    <img src="https://raw.githubusercontent.com/TrentBest/FSM_API/master/Documentation/Branding/TheSingularityWorkshop.png" alt="The Singularity Workshop" height="180">
  </a>
</p>
