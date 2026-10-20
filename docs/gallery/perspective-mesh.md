# Gallery recipe: perspective-projected mesh

## What this exhibit means

The CI proof submits an indexed octahedron through Visualizer's geometry, material, draw-command, and frame contracts. CPU world/view/projection transforms and painter ordering produce the SVG.

## Find the generated output

Open the [Visualize proof workflow](https://github.com/TrentBest/TheSingularityWorkshop.Visualize/actions/runs/39369513107), then download `visualize-svg-primitive-proof` from **Artifacts**.

## Evidence boundary

This demonstrates a perspective-projection path, not a finished 3D renderer. The fixture does not provide GPU depth testing, general clipping, or production lighting. The workflow artifact is a snapshot from that run.

## Recipe progression

Keep mesh input, transforms, projection, and ordering independently inspectable. Add difficult cases such as near-plane crossings and intersecting geometry only when the implementation actually supports them; do not let the illustration imply capabilities not covered by tests.
