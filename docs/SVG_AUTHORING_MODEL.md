![The Singularity Workshop animated moniker](assets/moniker.svg)

# Visualize SVG authoring model

**Purpose:** provide a small, deterministic vector-document model for authored scene illustrations. It is separate from both the GUI semantic tree and the projected triangle output of `SvgVisualizerBackend`.

## Choose the correct authoring surface

| Input and intent | Owner / API |
|---|---|
| Semantic interface tree, panels, controls, layout, GUI-specific SVG behavior | GUI.Core + GUI.SVG |
| Authored visual illustration, map, labeled scene, markers, and stable visual identities | Visualize `VisualizerSvgDocument` |
| A 3D-style `VisualizerFrame` with indexed geometry/material resources projected into a static artifact | Visualize `SvgVisualizerBackend` |

The domain remains authoritative for meaning. A consuming experience supplies its own entity IDs, relationships, and authored layout. Visualize serializes the visual description; it does not decide game rules or encounter outcomes.

## Supported primitives

- `VisualizerSvgRectangle` — filled rectangular regions and panels.
- `VisualizerSvgPolygon` — filled polygons for irregular authored shapes.
- `VisualizerSvgCircle` — filled circular markers, tokens, and nodes.
- `VisualizerSvgText` — accessible, XML-escaped text with validated size, family, weight, and anchor.

Every primitive has a unique validated SVG ID. Optional metadata is restricted to the documented data attributes and fill opacity. Geometry, IDs, colors, and text presentation values are validated before serialization. Primitive order is preserved and the document snapshots both the primitive list and metadata, so later caller mutation cannot change an already-authored document.

## Minimal usage

```csharp
var document = new VisualizerSvgDocument(
    800,
    500,
    [
        new VisualizerSvgRectangle("room-lab", 40, 40, 320, 220, "#344B5A",
            new Dictionary<string, string> { ["data-entity-id"] = "1201" }),
        new VisualizerSvgCircle("actor-token", 160, 150, 16, "#55B8A6",
            new Dictionary<string, string> { ["data-entity-id"] = "4101" }),
        new VisualizerSvgText("room-label", 56, 72, "LABORATORY", "#FFFFFF", 14,
            "sans-serif", "bold")
    ],
    background: "#101923",
    role: "img");

string svg = document.ToSvg();
```

The serializer uses invariant numeric formatting and `XmlWriter` for element and text encoding. Repeated serialization of the same immutable document is deterministic.

## Tactical map proof

The [Tactical Map Authoring Proof](../samples/TacticalMapProof/README.md) builds a six-location encounter map with doors, character tokens, and an objective marker. Its manifest records stable IDs, primitive counts, output size, and SHA-256. CI regenerates and verifies the artifact as `visualize-tactical-map-authoring-proof`.

This is a typed C# authoring model and deterministic CPU-side SVG artifact. It is not yet an interactive editor, an authoritative game-state model, a general-purpose vector editor, or a GPU renderer.

## Deliberate limits and next steps

The first authoring vocabulary is intentionally compact. Lines, curves, paths, gradients, clipping, reusable symbols, and in-place editing are not part of this initial contract. Add new primitives only with clear validation, stable serialization, focused tests, and a consuming authoring scenario. Keep GUI-specific layout and controls in GUI; keep domain meaning in the experience or rulebook.

Related: [Visualize Gallery](GALLERY.md) · [Visualize-owned static SVG scene output](STATIC_SVG_BACKEND.md) · [GUI panel handoff](GUI_PANEL_HANDOFF.md)
