<!-- The Singularity Workshop moniker -->
<p align="center">
  <a href="https://github.com/TrentBest">
    <img src="https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Visualize/development/docs/assets/moniker.svg" alt="The Singularity Workshop — animated workshop moniker" width="720" />
  </a>
</p>

# ✳️ 00 TheSingularityWorkshop.Visualize

This README follows the Workshop's shared [Documentation Standard](https://github.com/TrentBest/TheSingularityWorkshop.FSM_COS/blob/master/DOCUMENTATION_STANDARD.md), while keeping this repository's own purpose and implementation authoritative.

## Environment and scene composition

Visualize is designed to be the image-producing capability within a larger, composable scene—not a monolithic owner of every modality. The Environment architecture describes how authored scene state can bring together surfaces, volumetric media, weather, sound, haptics, persistence, and staged proofs while keeping Physics, Renderer, Visualize, and optional sensory capabilities independently bounded.

- [Environment and Scene Composition](docs/ENVIRONMENT_AND_SCENE_COMPOSITION.md) — ownership boundaries, volumetric direction, persistent authored inputs, and first vertical proofs.
- [Physics-Based Lighting, Shadows, and Extensible Math](docs/LIGHTING_AND_SHADOWS.md) — the relationship between physical light meaning, observer relevance, visual execution, independent work cadence, and invalidation.

**Implementation status:** architecture direction and documentation. This does not claim production volumetric rendering, a selected GPU backend, or working audio/haptic integration.
