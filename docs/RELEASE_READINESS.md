# Visualize Release Readiness

This checklist records evidence required before a Visualize release. A passing build or attractive SVG is not sufficient by itself; release claims must match reproducible behavior and package evidence.

## Current scope

Visualize turns geometry, materials, draw commands, and frame data into inspectable visual output. Current proofs establish CPU-side rendering and SVG manifestation, including explicit integrations with Renderer and GUI.SVG. They do not by themselves establish hardware-GPU rendering or production-scale scheduling.

## Verification checklist

### Build, tests, and package

- [x] CI restores and builds the solution in Release configuration.
- [x] CI runs the Visualize test suite with Cobertura coverage.
- [x] CI runs the GUI.SVG integration test project using the checked-out GUI source.
- [x] CI packs the Visualize project and uploads a NuGet package artifact.
- [ ] Re-run the complete workflow against the exact release candidate commit; record the run URL and confirm every required job passes.
- [ ] Inspect the generated package contents, package metadata, target frameworks, README/license inclusion, and dependency graph.
- [ ] Verify a clean external/package-only consumer can restore, compile, and exercise the package version from the intended feed.
- [ ] Confirm version, release notes, repository metadata, and compatibility claims agree across project file, README, and package.

### Executable visual evidence

- [x] CI validates deterministic array-first workload manifests and file hashes.
- [x] CI checks browser WebGPU runner module syntax and selected behavior-contract tests.
- [x] CI generates Visualize primitive SVG proofs, GUI.SVG integration proofs, a perspective-projected mesh proof, and a Renderer-selected frame proof.
- [x] CI generates three city massing skins and verifies a machine-readable report with CPU/SVG backend assertions.
- [x] CI generates deterministic Extremus crowd SVG/manifest evidence and asserts population, instance, draw-command, repeatability, and backend properties.
- [x] CI builds and tests a package-only GUI.SVG consumer and verifies package-embedded documentation/assets.
- [ ] Re-run documented proof commands from a clean checkout and ensure each command/path still matches the workflow.
- [ ] Keep all published scale/performance statements tied to repeatable workload inputs, correctness assertions, output artifacts, and measured costs.
- [ ] Review docs/UML/README.md and document each relevant production C# file under docs/UML; explicitly account for sample, integration, and test sources.

### CI and supply-chain hardening

- [ ] Pin every third-party GitHub Action to a verified full commit SHA; keep the human-readable release tag in a comment.
- [ ] Verify each pinned SHA corresponds to the intended upstream release and inspect action permissions and inputs.
- [ ] Validate workflow syntax and run all affected workflows after pinning.
- [ ] Review GITHUB_TOKEN permissions, third-party action permissions, secret exposure, artifact paths, and untrusted pull-request input handling.
- [ ] Confirm Renderer and GUI source checkouts use intentional refs and that release packaging consumes the intended dependency versions.

### Evidence boundaries

- [x] Current Renderer-to-Visualize proof explicitly describes CPU/SVG integration and does not claim GPU submission.
- [x] Current crowd proof asserts hardwareGpuInstancing=false and CPU execution.
- [ ] If claiming GPU rendering, add a reproducible test that verifies actual GPU work and resulting output—not just WebGPU API presence or module syntax.
- [ ] If claiming production scheduler/cache behavior, add executable integration evidence for those specific paths.
- [ ] If claiming performance or scale, publish reproducible workload details, correctness checks, environment, timings/allocations or other resource measurements.

### Release authorization

- [x] NuGet publishing is gated behind manual workflow dispatch, explicit publish=true, and the development ref; Visualize publication also depends on the GUI.SVG package-proof job.
- [ ] Before any release, inspect the exact commit, package artifact, workflow results, version collision risk, dependency compatibility, and release notes.
- [ ] Obtain explicit release authorization. Readiness is not authorization to publish.

## Release decision

**Not yet certified release-ready.** The repository has unusually useful executable SVG and integration evidence, but immutable action pinning, full UML coverage review, and a fresh release-candidate/package-consumer audit remain open. GPU and production-scheduler claims must remain bounded by the evidence currently present.