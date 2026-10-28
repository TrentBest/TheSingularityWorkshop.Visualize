![The Singularity Workshop animated moniker](assets/moniker.svg)

# Visualize-owned static SVG scene output

**Purpose:** document the CPU-side SVG artifact backend owned by Visualize. SVG is a visualization format; it does not imply a GUI tree, a GPU render, or an interactive host.

## What is implemented

`SvgVisualizerBackend` consumes a `VisualizerFrame` and an `IVisualizerResourceProvider` directly. It serializes indexed triangle-list draws to deterministic SVG markup without depending on `GUI.Core` or `GUI.SVG`.

- Frame view, world transforms, and projection matrices determine projected vertex positions.
- Geometry indices and base-vertex offsets are validated before use.
- Material base color, vertex colors, and optional instance tint contribute to the triangle fill.
- Representation and valid entity identity are emitted as SVG data attributes to support inspection.
- The output sink is caller-owned. The backend does not create a browser, window, Word surface, or GPU device.

## Boundaries and limitations

- **Static artifact:** output is SVG text. The backend reports `SupportsPresentationSurface = false` and `WasPresented = false`; it does not claim that an artifact was shown to a user.
- **CPU only:** the backend does not use GPU compute, native graphics resources, or GPU timing.
- **Triangle lists only:** each draw's index count must be divisible by three. Other primitive topologies fail explicitly rather than being silently reinterpreted.
- **No clipping or depth buffer:** triangles are projected, but this first implementation does not implement a depth test, back-face culling, full clip-volume clipping, lighting, or a physically based material model.
- **No GUI semantics:** controls, layout, panels, and interaction remain GUI concerns. GUI.SVG remains an optional adapter when the input is specifically a semantic GUI tree.
- **Host behavior is separate:** embedding SVG in Markdown, Word, WPF, or a browser does not establish that animations, scripting, or interactions are supported in that host.

## Minimal usage

```csharp
string? svg = null;
await using var backend = new SvgVisualizerBackend(text => svg = text);
var result = await backend.RenderAsync(frame, resources, cancellationToken);

// svg is a static SVG artifact. result.WasPresented is false.
```

Use `BuildSvg(frame, resources)` when the caller needs the SVG string directly and does not want to invoke the configured output sink.

## Verification

The unit tests cover stable output for identical inputs, triangle and identity metadata, sink emission, explicit CPU/not-presented reporting, cancellation, and rejection of unsupported index counts. The repository's current CI result is the authoritative evidence for whether these tests pass on the latest commit.

Related: [SVG living documentation](SVG_LIVING_DOCUMENTATION.md) · [GUI panel handoff](GUI_PANEL_HANDOFF.md) · [Issue #34: move general SVG output ownership into Visualize](https://github.com/TrentBest/TheSingularityWorkshop.Visualize/issues/34)

---

![The Singularity Workshop avatar](../Avatar.jpg)
