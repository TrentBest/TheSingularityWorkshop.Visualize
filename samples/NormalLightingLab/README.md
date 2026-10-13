# Surface Normals & Light — Visual Experiment 02

This is a self-contained, interactive SVG study of surface normals and Lambert diffuse lighting. It uses inline SVG and browser JavaScript; there is no build step, external library, or network request.

## Run it

From the Visualize repository root:

```powershell
py -m http.server 8080
```

Open [http://localhost:8080/samples/NormalLightingLab/](http://localhost:8080/samples/NormalLightingLab/), or open `index.html` directly.

## Experiments

- **Light azimuth and elevation:** move the light around the sphere and observe the bright region move.
- **Ambient contribution:** raise the baseline illumination or lower it to reveal directional contrast.
- **Diffuse strength:** change the contribution from the Lambert term.
- **Surface sample density:** change the number of sampled positions.
- **Normal vectors and sample points:** toggle the projected normal directions and sample markers.
- **Lambert shading:** disable the directional term to compare it with uniform ambient-plus-diffuse illumination.
- **Reverse light / reset:** quickly compare opposite directions or return to the initial settings.

## Model and boundaries

For a sampled unit sphere, the outward normal at a point is the normalized position vector. The directional diffuse term is:

```text
diffuse = max(0, dot(normal, lightDirection))
intensity = ambient + diffuseStrength * diffuse
```

The implementation clamps intensity for display. It projects a sampled 3D direction into a 2D SVG view, so a normal's visible arrow shows its screen-plane components rather than a full 3D arrow. The sphere is a teaching model; it is not a GPU shader, a production material system, or a physically complete lighting implementation.

This page generates and updates SVG elements from the controls; it does not swap screenshots. Its counters report the sampled surface points and average diffuse term for the current sample grid. They are explanatory values, not performance benchmarks.

The page does **not** execute the C# Visualize contracts. It is a browser-side experiment that makes a lighting concept observable while the .NET execution backend remains separate. See [the lighting design notes](../../docs/LIGHTING_AND_SHADOWS.md), [the primitive guide](../../docs/PRIMITIVES.md), and [the repository README](../../README.md).
