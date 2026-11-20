# MyCity block proof

This is a small, deterministic, domain-neutral presentation fixture for the MyCity pipeline. It contains two districts, eight parcels, and four buildings. It is not MyCity's authoritative geography, ownership, resident, institution, or economy model.

Run from the Visualize repository root:

```sh
dotnet run --project samples/MyCityBlockProof/MyCityBlockProof.csproj --configuration Release -- ./artifacts/mycity-block
```

The program creates two complete revisions from the same stable scene identity:

- `mycity-block.svg` — near building representations, including roofs.
- `mycity-block-distant.svg` — distant building representations, with reduced building detail.
- `mycity-block-manifest.json` — stable district, parcel, and building IDs; revisions; command counts; and the explicit backend boundary.

Both revisions use `VisualizerSceneSnapshot`, `VisualizerFrame`, the public resource catalog, and Visualize's `SvgVisualizerBackend`. Stable entity IDs survive the representation transition. The backend produces deterministic CPU/static SVG artifacts; this is not GPU execution, a live surface, an interactive city, or an authoritative MyCity domain model.

The fixture intentionally uses no Renderer dependency. Renderer policy remains upstream and is already exercised by the separate shared-frame tri-view proof; this sample isolates Visualize's direct scene-to-artifact path.
