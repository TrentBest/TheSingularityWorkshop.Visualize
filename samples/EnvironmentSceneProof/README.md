# Environment Scene Proof

Run the deterministic CPU/static-SVG environment fixture:

```bash
dotnet run --project samples/EnvironmentSceneProof/EnvironmentSceneProof.csproj --configuration Release -- ./artifacts/environment-scene
```

The program writes:
- `environment-scene.svg` — complete revision 1.
- `environment-scene-moved.svg` — revision 2, with only the ball transform changed.
- `environment-scene-manifest.json` — stable scene/entity identities, revisions, execution boundary, and SHA-256 hashes.

The scene contains a floor, raised platform, box, ball, and a multi-command articulated placeholder. Revision 1 is rendered twice and compared for deterministic output. Revision 2 preserves the scene identity and all entity IDs while moving the ball.

This is a renderer-neutral scene-contract and CPU/SVG manifestation proof. The articulated figure is not a Gestures controller or kinematic simulation; the ball transform is not Physics output; floor/platform geometry is not collision state. No GPU execution, interactive authoring, or FSM_COS MicroBundle composition is claimed.
