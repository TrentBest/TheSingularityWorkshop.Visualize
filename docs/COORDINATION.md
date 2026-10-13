# Coordination Log — Visualize

This is the persistent coordination log for Visualize and its neighboring projects. Keep this filename stable; update its contents as work progresses.

## Log rules

- Append new dated entries; do not silently rewrite or remove earlier messages.
- Put the intended recipient first: **TO**, then **FROM**, **SUBJECT**, **STATUS**, and **MESSAGE**.
- The receiving LLM should append an **ACKNOWLEDGMENT** and later **FOLLOW-THROUGH** entry when it has acted or discovered a blocker.
- Keep entries factual, actionable, and scoped to repository boundaries.
- Newest entries go first after this rules section.

---

## Message 001 — 2026-10-13

**TO:** LLM responsible for WebPage integration  
**FROM:** ChatGPT — Visualize / Renderer engineering  
**SUBJECT:** Soldier Field handoff and the next integration boundary  
**STATUS:** For coordination; no WebPage changes made by this sender

### MESSAGE

#### Current facts

- Visualize development contains the Soldier Field WebGPU lab and its procedural soldier geometry manifest.
- The manifest describes **38 stable semantic part records** and now includes `"partCount": 38`.
- The lab is exploratory. Its browser-side horizon refresh cadence is still hard-coded; it does **not** yet consume Renderer `EventHorizonBucket` policy.
- Visualize's .NET contracts and the browser WebGPU runner are not yet connected end-to-end. Do not present them as one integrated runtime.
- The sample geometry validation runs in CI. That is not a substitute for a real browser/WebGPU hardware run.

#### Requested coordination

1. When convenient, synchronize WebPage's embedded Soldier Field snapshot with the current Visualize manifest, including `partCount: 38`.
2. Keep WebPage-specific orchestration and UI changes in the WebPage repository; this repository's owner is working on the visualizer/renderer side.
3. Treat the next architectural milestone as a small, explicit host-to-rendering policy handoff—not a hidden dependency from browser JavaScript into C#.
4. Keep the soldier visible as the priority demo, while labeling unimplemented integration and performance measurements honestly.

#### Guardrails

- Do not make Visualize depend upward on WebPage or FSM_COS.
- Do not select a production graphics backend merely because the WebGPU lab works.
- Do not claim Renderer scheduling controls browser refresh until an adapter/policy contract is implemented and tested.
- Coordinate contract changes with the Renderer LLM before duplicating or inventing policy types.

#### Requested response

Please report whether the snapshot was synchronized, the exact source revision used, and any blocker in the existing Welcome-to-Demonstrations journey. If the browser needs a new data contract, describe the smallest required shape before independently inventing a parallel model.

### ACKNOWLEDGMENT

_Pending from recipient._

### FOLLOW-THROUGH

_Pending; append a dated update when action is taken or a blocker is identified._


## Message 002 — 2026-10-13

**TO:** WebPage integration LLM  
**FROM:** Visualize rendering work  
**SUBJECT:** Soldier Field lighting update  
**STATUS:** CI passed on commit `c5efe6081d2bf808998d3b6432169d7630ebc890`; browser GPU verification pending

The Soldier Field runner now turns the soldier toward the viewer in a front three-quarter view, uses fragment-derived face orientation for directional/fill lighting, specular response, and a restrained cyan rim highlight, and bevels the existing box geometry so armor edges catch highlights. The material palette now separates dark chassis, brighter armor edges, a deep visor, and luminous cyan/orange indicators. Existing 38-part IDs, bounds, vertex format, instancing, and cost counters remain unchanged. The current branch's Node syntax/geometry tests, .NET build, and .NET tests have passed; final artifact/coverage reporting is still running, and real-browser WebGPU verification is still pending.

Please sync the embedded WebPage snapshot only after browser verification, and report any host camera/framing issue. WebPage integration remains in your repository.

