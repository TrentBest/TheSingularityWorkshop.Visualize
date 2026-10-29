![The Singularity Workshop animated moniker](https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Visualize/development/docs/assets/moniker.svg)

# Projection transforms

Visualize provides a small, deterministic point-transform helper for adapters that need to map source-space coordinates to destination coordinates. It is a math contract only: it does not own surfaces, route frames, clip geometry, or render a scene.

## Convention

\`VisualizerProjectionTransform.TryTransformPoint\` accepts a 2D source point and a \`System.Numerics.Matrix4x4\`.

- The source point is promoted to homogeneous \`(x, y, 0, 1)\`.
- Transformation follows \`Vector4.Transform(vector, matrix)\` and the System.Numerics row-vector convention. Translation therefore uses the fourth matrix row.
- The resulting \`(x, y, z)\` is divided by homogeneous \`w\`.
- The method returns \`false\` and a default output for non-finite source or matrix values, non-finite transformed values, a homogeneous \`w\` whose absolute value is at most \`1e-6\`, or a non-finite divided result.

Example:

\`\`\`csharp
using System.Numerics;
using TheSingularityWorkshop.Visualize;

var mapping = Matrix4x4.CreateTranslation(100f, 40f, 0f);
if (VisualizerProjectionTransform.TryTransformPoint(
        new Vector2(10f, 20f), mapping, out var destination))
{
    // destination is (110, 60, 0)
}
\`\`\`

## Responsibility boundary

The caller supplies the matrix and is responsible for ensuring that its input coordinates match the declared source coordinate system. The helper does not enforce viewport bounds, clipping, aspect ratio, letterboxing, depth conventions, or screen-vs-world policy. A \`true\` result means only that the homogeneous transform was numerically usable; it does not mean the point is visible or in front of a camera.

For multi-target presentation, use the ordered \`VisualizerProjectionBatch\` to describe destinations. A host or adapter still owns target lifecycle, frame scheduling, routing, resize handling, and per-target failure isolation. This helper does not turn that request batch into a routing engine.

## Verification

The projection transform tests cover identity, row-vector translation, homogeneous division, invalid source values, invalid matrices, near-zero homogeneous \`w\`, and non-finite results. These tests validate math behavior only; an adapter still needs integration tests for its real coordinate and surface conventions.

TheSingularityWorkshop
