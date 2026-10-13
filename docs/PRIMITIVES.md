# Visual Primitives — From Meaning to Pixels

**Status:** Living guide. Visualize's examples should remain small, reusable, and neutral: they demonstrate presentation contracts without making a consuming Experience's domain objects part of this library.

![Lit hemisphere and projected surface normals](assets/hemisphere-normals-preview.svg)

The preview is visible without building or launching anything. The [Primitive Gallery](../samples/PrimitiveGallery/README.md) distinguishes the browser-side SVG experiment from the C# proof harness, which submits indexed geometry, material resources, instance data, and frame commands through Visualizer contracts to an SVG proof backend. The exact output of the latest successful CI run is available in the [CI artifact](https://github.com/TrentBest/TheSingularityWorkshop.Visualize/actions/runs/38453100266/artifacts/11841960173).

## 0. See the primitives, not just read about them

Open the [interactive SVG Primitive Gallery](../samples/PrimitiveGallery/index.html). It begins with one point, then lets you explore point clouds, connected points, one line, and many lines. Change the count, point size, line width, position spread, and colour style; randomize the point data and watch the element counts and local build-time observation update.

The interactive page is a browser-side SVG experiment. It does not execute the C# Visualize assembly. The separate C# proof harness creates actual Visualizer resources and frame commands, submits them to a small SVG backend, and writes inspectable SVG output. Neither path is a production GPU backend.

## 1. Three meanings of “primitive”

A *primitive* can mean different things at different layers. Keeping those meanings separate prevents an example from accidentally defining the architecture.

| Layer | What a primitive means here | Example |
| --- | --- | --- |
| Domain / semantic description | A meaningful, stable piece of an object's description | A named component in a consuming Experience |
| Geometry generator | A rule that produces vertices and indices from a compact description | Box, ellipsoid, hemisphere |
| Presentation backend | A way to turn prepared geometry into pixels | SVG proof backend, future GPU backend |

A semantic component is not the same thing as a shape. A box is a geometric approximation; the component's identity and role belong to the consuming Experience. The same semantic description might later be represented by a mesh, an outline, a cutaway, or a detailed model without transferring domain ownership into Visualize.

## 2. Current C# geometry contract

The current proof harness submits VisualizerGeometry with vertices and indexed triangles, VisualizerMaterial resources, optional VisualizerInstanceBuffer data, and VisualizerFrame draw commands. The Visualizer validates the frame/resource relationships and passes the work to the selected backend.

The proof backend translates indexed triangles into SVG paths. For the lit hemisphere example it additionally computes a simple Lambert response from vertex normals and draws projected normal vectors. This lighting calculation belongs to the proof backend; it is not a production shader or a promise that the current material contract implements GPU lighting.

The current contract draws indexed triangles. Point-like marks and line-like segments in the gallery are therefore represented by small triangle meshes, not native point-list or line-list topology.

## 3. The proof sequence

| Step | What it demonstrates |
| --- | --- |
| One point | A mesh-backed marker submitted in a Visualizer frame |
| Point cloud | A marker mesh reused across a contiguous instance buffer |
| Four connected points | Marker and segment-mesh draws composed into a simple outline |
| One line | A segment represented by indexed triangles |
| Many lines | One segment mesh reused across multiple instance transforms |
| Lit hemisphere + normals | Indexed surface geometry, a simple Lambert response, and projected vertex normals |

The checked-in SVG snapshots are useful for review and visual regression. CI also generates the complete set of SVGs from the C# harness and uploads them as an artifact. The [Primitive Gallery guide](../samples/PrimitiveGallery/README.md) records the run command and the boundaries of each proof.

## 4. SVG is a representation target, not the architecture

SVG describes visible geometry through retained elements such as paths, rectangles, ellipses, and groups. A GPU backend typically consumes prepared vertex/index buffers and draw commands. These approaches can share source descriptions or semantic identity, but they are not interchangeable backends by default.

The [Primitive Gallery](../samples/PrimitiveGallery/index.html) and [Surface Normals & Light](../samples/NormalLightingLab/index.html) are browser SVG experiments. The C# SVG proof backend is a separate implementation that consumes Visualizer contracts. Keeping the paths distinct is important: a compelling browser drawing is not evidence that the C# path rendered it.

## 5. How a consumer should extend the examples

A consuming Experience should own its domain-specific scene and feed presentation data through Visualize's public contracts. For example, raWWar should own soldier and exoskeleton meaning, geometry descriptions, simulation state, and workload choices; its integration should demonstrate that Visualize can present that data without taking ownership of the game domain.

When adding a neutral Visualize example, keep these together:

- a small, inspectable input description;
- deterministic geometry or an explicit conversion rule;
- a visible demonstration;
- automated tests for dimensions, topology, resource ranges, and invalid inputs;
- a clear statement of which execution path produced the image;
- an explanation of the representation's limits.

Candidate investigations—not implemented primitives—include cylinders/capsules, reusable indexed meshes, and 2D SVG projections for diagrams. Promote a candidate to “implemented” only when code and tests support the claim.

## 6. Related reading

- [Architecture](ARCHITECTURE.md) — layer ownership and data flow.
- [GPU backend strategy](GPU_BACKEND_STRATEGY.md) — backend responsibilities and the exploratory browser/native directions.
- [Lighting and shadows](LIGHTING_AND_SHADOWS.md) — how visual work can be separated and scheduled.
- [Primitive Gallery guide](../samples/PrimitiveGallery/README.md) — generated SVG proof and browser experiment boundaries.
- [Surface Normals & Light](../samples/NormalLightingLab/README.md) — interactive SVG lighting study and its limitations.