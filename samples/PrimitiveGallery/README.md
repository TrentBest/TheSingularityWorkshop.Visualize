# Primitive Gallery — Visual Test Sequence 01

This is a browser-run visual experiment for the first drawing primitives. It uses inline SVG and JavaScript, with no external assets, build step, graphics library, or server API.

## Run it

From the Visualize repository root:

```powershell
py -m http.server 8080
```

Open [http://localhost:8080/samples/PrimitiveGallery/](http://localhost:8080/samples/PrimitiveGallery/) in a browser. You can also open `index.html` directly; the page has no module imports.

## Progression and expected observations

| Step | Action | Expected visible result |
| --- | --- | --- |
| 01 · One point | Keep the default mode | One coloured point appears at the center |
| 02 · Point cloud | Select Point cloud; try 1, 100, 1,000, and 10,000 | The selected number of independently positioned points appears |
| 03 · Four connected points | Select Four connected points | Four point markers and a closed, four-segment outline appear |
| 04 · One line | Select One line | A line connects two endpoints |
| 05 · Many lines | Select Many lines; vary the count | The selected number of line segments appears |
| Randomize | Press Randomize positions + colours | The current arrangement changes without changing its selected mode/count |
| Presentation | Change point size, line width, colour style, or spread | Mark appearance or distribution changes |

## What the counters measure

- **Visible marks** counts the generated primitive marks in the scene.
- **SVG elements created** currently reports those generated marks; the fixed background grid and axes are not included.
- **Build time** is the local JavaScript generation and SVG DOM-insertion pass, measured with `performance.now()`.

This is a visual experiment, not yet an automated rendering test for the C# Visualize contracts. The .NET test project currently verifies resource catalog behavior, geometry/index validity, draw and instance ranges, backend submission safety, and disposal. It does not run this SVG page. The build-time number is not a GPU timing, a cross-device benchmark, or a performance claim.

The next progression should be derived from the actual C# contract tests and the drawing capabilities that are implemented, adding assertions and visible examples together rather than documenting planned primitives as if they already exist.
