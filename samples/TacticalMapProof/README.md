# Tactical Map Authoring Proof

This small, domain-neutral sample demonstrates a Visualize-owned SVG authoring surface suitable for maps, encounter layouts, diagrams, and other structured illustrations.

## Run

```sh
dotnet run --project samples/TacticalMapProof/TacticalMapProof.csproj --configuration Release -- ./artifacts/tactical-map
```

The sample writes `tactical-map.svg` and a machine-readable manifest. The SVG is generated entirely from the typed `VisualizerSvgDocument` primitive model—rectangles, polygons, circles, and text. Text is XML-escaped by the serializer. Stable author-supplied entity IDs are emitted as `data-entity-id` metadata so downstream authoring tools can inspect and correlate visual elements.

The generated SVG and manifest are uploaded by CI as the `visualize-tactical-map-authoring-proof` artifact.

## What this proves

- Semantic circles and text can be authored alongside existing rectangle and polygon primitives.
- Stable location, door, token, and objective identities survive into the SVG.
- Identical input produces byte-identical SVG output.
- The output is a deterministic CPU-side static SVG artifact.

## What this does not prove

This is not a live editor, authoritative encounter state, game rules engine, interactive browser application, or GPU renderer. The C# source is the authoring model in this first proof. A future editor can manipulate that model and regenerate the SVG while keeping SURPS or another consuming domain authoritative for meaning and rules.
