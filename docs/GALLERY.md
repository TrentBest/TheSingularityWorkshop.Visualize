# Visualize Gallery

**Make the living world visible. Keep the world independent of its display.**

This is the short visual index. Each entry tells you what to look at and links to a focused document for the implementation, recipe, and evidence. The [SVG living-documentation guide](SVG_LIVING_DOCUMENTATION.md) collects the current proofs and explains the boundaries between SVG output, CPU-side projection, and GPU execution. The [per-file UML source index](UML/README.md) tracks audited C# types and explicitly lists coverage still pending.

## 01 — Geometry and connectivity

![Connected points and line meshes](../samples/PrimitiveGallery/generated/07-gui-svg-connected-points.svg)

**See:** how point/line geometry, materials, draw commands, and a Visualizer frame become a GUI.SVG scene.

**Recipe and evidence:** [Connected geometry → SVG](gallery/connected-geometry-svg.md) · [Primitive Gallery guide](../samples/PrimitiveGallery/README.md) · [C# proof source](../integrations/GuiSvg/Proof/Program.cs)

## 02 — Primitive scale

![One point](../samples/PrimitiveGallery/generated/01-one-point.svg)

![100-point cloud](../samples/PrimitiveGallery/generated/02-point-cloud-100.svg)

**See:** the same kind of primitive contract at small and larger counts. These are correctness illustrations, not throughput benchmarks.

**Recipe and evidence:** [Primitive-scale recipe](gallery/primitive-scale.md) · [Primitive Gallery guide](../samples/PrimitiveGallery/README.md) · [Visual primitives](PRIMITIVES.md)

## 03 — Normals and light

![Hemisphere normals preview](assets/hemisphere-normals-preview.svg)

**See:** indexed surface geometry, normal direction, and a CPU-side lighting study. This is not a production GPU shader.

**Recipe and evidence:** [Normals and light recipe](gallery/normals-and-light.md) · [Lighting and shadows](LIGHTING_AND_SHADOWS.md)

## 04 — Perspective mesh

The CI proof turns an indexed octahedron into a perspective-projected SVG using CPU transforms and painter ordering.

**See:** [Perspective mesh recipe and evidence limits](gallery/perspective-mesh.md). Download `visualize-svg-primitive-proof` from the [proof workflow](https://github.com/TrentBest/TheSingularityWorkshop.Visualize/actions/runs/39369513107).

## 05 — City massing: the cross-repository proof

The city fixture sends a real Renderer `RenderFrame` through `RendererFrameAdapter`, into Visualize, and then through the optional GUI.SVG path. Three material skins make the same command stream easier to inspect.

**See:** [City massing recipe, adapter assertions, and limits](gallery/city-massing.md). Download `visualize-city-massing-proof` from the [proof workflow](https://github.com/TrentBest/TheSingularityWorkshop.Visualize/actions/runs/39369513107).

## 06 — Observer-driven frame: the world responds to movement

Two generated frames run the actual Renderer distance policy at observer X=0 and X=50, preserve entity identity and representation through `RendererFrameAdapter`, and draw the resulting detail choices through Visualize and GUI.SVG. This is an executable CPU/SVG integration proof, not GPU output or a performance benchmark.

**See:** [Observer-driven presentation recipe and evidence boundary](gallery/observer-driven-presentation.md). CI uploads `visualize-observer-driven-presentation-proof`.

## 07 — Crowd instances

The deterministic crowd fixture makes instance data visible as a generated artifact. It is a contract illustration, not a measured GPU crowd renderer.

**See:** [Crowd-instance recipe and evidence boundaries](gallery/crowd-instances.md). Download `visualize-extremus-crowd-proof` from the [proof workflow](https://github.com/TrentBest/TheSingularityWorkshop.Visualize/actions/runs/39369513107).

## 08 — GUI.SVG feature language

Gradients, clipping, patterns, markers, symbols, animation, filters, and lighting definitions demonstrate the expressiveness of the optional SVG manifestation path.

**See:** [GUI.SVG feature-gallery recipe](gallery/gui-svg-features.md); download `gui-svg-feature-gallery` from the [proof workflow](https://github.com/TrentBest/TheSingularityWorkshop.Visualize/actions/runs/39369513107).

## What “alive” means here

SVG animation can make a picture feel alive, but animation alone is not a live simulation. A genuinely living Visualize proof must show a real input changing the frame or submitted work, with reproducible measurements and explicit backend status. See [Performance and benchmark rules](PERFORMANCE.md) and the [SVG living-documentation guide](SVG_LIVING_DOCUMENTATION.md).

---

[Architecture](ARCHITECTURE.md) · [GPU backend strategy](GPU_BACKEND_STRATEGY.md) · [Implementation roadmap](IMPLEMENTATION_ROADMAP.md)
