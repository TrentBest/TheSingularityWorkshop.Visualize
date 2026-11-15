# Gallery recipe: Renderer → Visualize city massing

## What this exhibit means

The CityMassingProof fixture creates a real Renderer `RenderFrame`, adapts its ordered commands through `RendererFrameAdapter`, then emits three material skins through the optional GUI.SVG integration.

## Find the generated output

Open the [Visualize proof workflow](https://github.com/TrentBest/TheSingularityWorkshop.Visualize/actions/runs/39369513107), then download `visualize-city-massing-proof` from **Artifacts**. It contains the three skins and a JSON report.

## What is checked

The proof asserts command count and order, geometry/material IDs, world transforms, index ranges, representation, and building entity IDs survive the adapter. It also validates a deterministic district → parcel → building hierarchy: identities are unique, each parcel references an existing district, and each building references exactly one existing parcel and its matching district. District and parcel IDs are serialized as SVG root metadata and every building membership is recorded in the JSON report. These are synthetic fixture identities, not authoritative MyCity records. The report marks `adapterCommandOrderAndIdentityPreserved` and `semanticHierarchyValidated` only after their respective assertions pass.

## Evidence boundary

The distant representation omits selected window-detail face commands. This is command-level work reduction, not GPU buffer compaction or a production city renderer. The SVG skins are snapshots from the named CI run.

## Next recipe

Let an actual observer/policy input change the selected work, assert the resulting command-count difference, and capture a paired baseline/detail SVG plus measured CPU work. That is the bridge from attractive output to a defensible performance demonstration.
