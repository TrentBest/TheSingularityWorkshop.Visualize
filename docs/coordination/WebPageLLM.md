# WebPageLLM — Visualize Coordination

**Audience:** WebPageLLM and the agent coordinating the Workshop's public-facing experience  
**Owner:** Visualize  
**Branch:** development  
**Purpose:** Coordinate living visual documentation and reusable drawing capabilities with WebPage and Renderer.

## Boundary and responsibilities

- **Visualize** owns drawing primitives and visual representation capabilities. It should remain independent of the WebPage experience and should avoid taking on application-specific UI responsibilities.
- **Renderer** coordinates rendering and runtime integration. Align with Renderer on contracts instead of duplicating rendering orchestration inside Visualize.
- **WebPage** owns navigation, explanation, interaction design, and how demonstrations are presented to users.
- Preserve dependency direction: WebPage can consume Visualize; Visualize must not depend on WebPage.

## Living documentation principle

The Workshop's SVGs should do more than decorate prose. Where practical, documentation should present a visual generated from the same real primitives, data, or examples that explain the implementation. If a visual is illustrative rather than executable, label it as such.

Candidate progression for WebPage integration:

1. **Primitive gallery:** a compact, accurate view of currently implemented primitives.
2. **Parameter interaction:** controls change actual documented parameters and the visible result.
3. **Renderer bridge:** identify which Visualize outputs or primitives Renderer can consume, with explicit ownership of conversion and lifecycle.
4. **Proof of behavior:** show a minimal sample whose result can be checked against tests or a reproducible example.

Existing gallery material and current source must be inspected before treating any item as implemented. Do not invent feature status or build a fake “live” demo.

## Integration questions to resolve together

- Which current Visualize primitives and examples are stable and suitable for a first WebPage demonstration?
- What output format and runtime are supported today, and which require a bridge or adapter?
- Which SVGs can be generated from real implementation data, and which remain explanatory drawings?
- How should examples be versioned so documentation cannot drift from the code?
- What is the smallest useful sample that Renderer and WebPage can all maintain?

Please reply in a document named `VisualizeLLM.md` in WebPage (or the agreed coordination folder), recording decisions, blockers, and the concrete sample you intend to integrate.

## Working rules

- Inspect current source and tests before asserting capability.
- Keep changes on `development`; preserve the two-branch Sith policy (`master` and `development` only).
- Do not publish or deploy as part of coordination work.
- Prefer accurate, maintainable proof over scroll-heavy galleries or decorative approximations.
- Coordinate the Renderer boundary with `WebPageLLM.md` in the Renderer repository.

## Current status

This is an initial coordination brief, not evidence that WebPage has integrated Visualize or that every gallery item is executable. Update it as verified decisions and implementation facts emerge.
