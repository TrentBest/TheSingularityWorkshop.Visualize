# CiAiLLM — Help Needed: Diagnose Visualize CI and Verify Drone Construction Presentation Proof

## Why I am asking for help

Please help diagnose and repair a GitHub Actions workflow-definition failure in **TrentBest/TheSingularityWorkshop.Visualize**, then help verify the intended end-to-end Drone Construction snapshot presentation proof. This is a focused CI-debugging request, not a request to publish a package or merge branches.

## Target and current state

- Repository: `TrentBest/TheSingularityWorkshop.Visualize`
- Working branch: `development`
- Latest known commit: `b33a5e64549076175f8177f0e0ce0a427ec2d2f0`
- Commit message: `Keep Drone Construction proof sources out of adapter compilation`
- Latest known adapter project blob: `18aad87bf5f9809585e688f0f185cfc1d8e8019c`
- Related issue: [Experiences Issue #20](https://github.com/TrentBest/TheSingularityWorkshop.Experiences/issues/20)
- Workflow to diagnose: [Visualize build.yml](https://github.com/TrentBest/TheSingularityWorkshop.Visualize/blob/development/.github/workflows/build.yml)
- CiAi observer workflow: [Observe Release Candidate](https://github.com/TrentBest/TheSingularityWorkshop.CiAi/blob/master/.github/workflows/observe-release-candidate.yml)

### Evidence already observed

1. Visualize workflow run [40119753401](https://github.com/TrentBest/TheSingularityWorkshop.Visualize/actions/runs/40119753401) failed before GitHub created any jobs. The Actions jobs endpoint returned `total_count: 0` and `jobs: []`. Other nearby attempts with this workflow filename also showed zero jobs. Treat this as a workflow discovery/validation/configuration problem until evidence says otherwise—not as a C# build failure.
2. The separate observer-driven GUI.SVG proof workflow run [40119754506](https://github.com/TrentBest/TheSingularityWorkshop.Visualize/actions/runs/40119754506) was reported green after a prior correction.
3. An earlier observer-proof run, [40118725036](https://github.com/TrentBest/TheSingularityWorkshop.Visualize/actions/runs/40118725036), did create a job and exposed C# compile errors because the adapter project's default SDK compile glob pulled nested `DroneConstructionProof/Program.cs` into the adapter build. Commit `b33a5e64549076175f8177f0e0ce0a427ec2d2f0` added `<Compile Remove="DroneConstructionProof/**/*.cs" />` alongside the existing exclusion for `Proof/**/*.cs`. Please verify the fix against fresh run evidence.
4. The Drone Construction proof has not yet been verified end-to-end. Do not describe it as green or complete without a successful run and artifact inspection.

## Requested CiAi assistance

Please inspect the exact candidate commit and provide an evidence-based diagnosis:

1. Use CiAi's release-candidate observation capability (or its closest supported equivalent) against repository `TrentBest/TheSingularityWorkshop.Visualize` and the exact 40-character SHA `b33a5e64549076175f8177f0e0ce0a427ec2d2f0`.
2. Explain why `.github/workflows/build.yml` is being recorded/displayed with zero jobs. Check workflow YAML parsing, event and branch filters, workflow-file discovery, GitHub Actions validation errors, and whether the workflow is being rejected before job creation. If the observer only includes pull-request-triggered runs, call out that limitation explicitly; do not interpret an empty result as proof of success.
3. Inspect the current workflow diff, especially the Drone Construction integration steps, checkout paths, environment variables, and indentation around the new steps. The workflow is large, so prefer a minimal targeted correction over a rewrite.
4. Check whether the existing `Observe Release Candidate` workflow can be dispatched for this target commit. If you have permission to dispatch it, use the workflow's documented inputs:
   - `repository`: `TrentBest/TheSingularityWorkshop.Visualize`
   - `commit_sha`: `b33a5e64549076175f8177f0e0ce0a427ec2d2f0`
   If you cannot dispatch it, state exactly what action the repository owner must take in GitHub Actions. Do not claim it ran unless a run ID and its result are available.
5. Once workflow-definition validation is repaired, get a fresh run that actually creates jobs, then resolve any build or proof errors one at a time.

## End-to-end proof contract

The intended proof is a small, deterministic sample—not a claim of population-scale performance.

- Source repository: `TrentBest/TheSingularityWorkshop.Experiences`, branch `development`.
- Real source: `ConstructionSimulation`, `ConstructionSnapshotObserver`, and the existing snapshot SVG renderer/view.
- Create a deterministic 4×2 construction simulation with three agents (IDs 101, 202, 303).
- Publish and retrieve an accepted snapshot through the observer; preserve run identity, tick, observer sequence, simulation-space identity, and stable agent IDs.
- Adapt the snapshot's agents to Renderer `RenderDrawCommand` objects and a `RenderFrame`.
- Run the existing Visualize `RendererFrameAdapter`, resource catalog, `Visualizer`, and `GuiSvgVisualizerBackend` pipeline to produce a real SVG.
- Assert that three authoritative agents produce three submitted draws, with entity IDs `101,202,303`.
- Upload the SVG and a plain-text provenance manifest as workflow artifacts.
- The intended manifest checks include:
  - `run=drone-construction-renderer-proof`
  - `observerSequence=1`
  - `authoritativeAgents=3`
  - `submittedDraws=3`
  - `entityIds=101,202,303`

Relevant new proof files in Visualize `development`:
- `integrations/RendererAdapter/DroneConstructionProof/Program.cs`
- `integrations/RendererAdapter/DroneConstructionProof/DroneConstructionFrameProof.csproj`
- `integrations/RendererAdapter/DroneConstructionProof/README.md`

The main workflow checks out Experiences under repository-root `ExperiencesSource` and invokes the proof project. Validate all relative paths from the actual process working directory; do not assume project-relative and repository-relative paths are interchangeable.

## Architectural and operational constraints

- Keep the simulation authoritative; presentation observes snapshots and must not alter simulation state.
- Keep core simulation code renderer/host independent. Do not add Unity, WPF, browser, or graphics dependencies to the simulation core.
- Use the existing snapshot SVG renderer where appropriate; do not create a competing renderer without a demonstrated need.
- Clearly distinguish implemented, tested, planned, and unverified behavior.
- Do not claim 100,000-agent capacity, starvation freedom, replay determinism, GPU rendering, hosted telemetry, or scale results unless separately implemented and measured.
- Operation Sith: final branch set should be `master` + `development`. Preserve/reconcile work before any branch cleanup. Do not delete branches.
- Do not merge pull requests or publish NuGet packages. Publishing requires explicit owner approval.
- Prefer a small, auditable fix and provide commit SHA, workflow run URL/ID, job-level results, artifact evidence, and any remaining blockers.

## What to report back

Please return:
1. The exact root cause of the zero-job workflow result, with evidence.
2. Whether CiAi's observer actually ran on the specified commit; if so, its run ID and findings.
3. The minimal fix, if one is warranted, and the commit SHA.
4. The first fresh workflow run with jobs, plus build/test/proof outcomes.
5. Whether the SVG and manifest artifacts exist and pass the stated assertions.
6. Remaining blockers and a revised readiness estimate. Current working estimate before this diagnosis: **60%** toward the intended showcase; this is not a test pass rate or release approval.


## New finding from direct inspection of current `development` workflow

A fresh read of `.github/workflows/build.yml` on `development` shows the workflow is not merely failing at runtime: the file content itself is visibly corrupted/interleaved.

Evidence from the current file:
- The workflow has 1,542 lines, although the valid-looking opening structure starts with one top-level `name`, `on`, `permissions`, and `jobs`.
- At lines 480–501, the first `publish_nuget` job appears to finish normally.
- Line 502 then contains the stray fragment `"$manifest"`; line 503 contains merged text `grep -q '^observerSequence=1        shell: bash`, immediately followed by another step's `run: |` at line 504. This is invalid YAML/step structure and is a concrete cause candidate for the zero-job workflow failure.
- The file also repeats top-level job identifiers: `publish_nuget:` occurs at lines 480, 686, 892, 1098, 1304, and 1520. The same `build-and-test:`/step sequences appear to have been duplicated/interleaved. Duplicate job IDs and the broken line at 502 must be resolved before treating this as a C# issue.
- The malformed section interleaves the verification for the earlier `renderer-selected-frame.svg` proof with Drone Construction manifest checks. Do not patch only the visible line: inspect the entire file and recover a coherent workflow definition.

Recommended recovery:
1. Compare the current file with the last known-good workflow commit and identify the smallest clean base.
2. Reapply intended changes exactly once: GUI.SVG package proof, existing Visualize build/test, Renderer/Visualize proof, City Massing proof, and Drone Construction proof.
3. Ensure each top-level job ID appears once, each step is nested under the correct job, and there are no stray/interleaved fragments.
4. Run a YAML/workflow syntax validation if available, then push only to `development` and verify a fresh Actions run actually creates jobs.
5. Keep the `publish_nuget` gate disabled; do not publish or merge.

This is a stronger diagnosis than the earlier hypothesis about workflow discovery/events. It is based on the live file content, but a fresh successful run is still required to verify the repair.


## Additional recovery finding (2026-10-25)

A fresh branch comparison reports `development` and `master` as diverged: `development` is 383 commits ahead and 6 behind, with `94ccec89da19c4d8b2ba2cc923cb684d43a22d20` as the merge base. The comparison reports `.github/workflows/build.yml` as added on development; fetching that path from `master` returns 404. Therefore, do **not** restore the workflow by copying from `master`: that file is absent there, and the branches have substantial independent history.

The GitHub commit endpoint reliably returns the specific diff for commit `8f88e9f58832ade1535e9a33f4e5a07d2e4cea93`, showing a local repair of the Drone Construction verification block. However, a historical `fetch_file` request returned the same current blob rather than trustworthy historical content, so it is not a safe way to recover a prior version. Treat commit diffs as evidence of individual edits, not as a complete recoverable workflow snapshot.

**Recovery guidance:** preserve all intended jobs/proofs in `development`; reconstruct a single coherent workflow from the current workflow plus verified commit diffs, or have the CiAi workflow-repair process do so. Keep a pre-edit copy/commit before replacement, validate YAML and unique job IDs, then confirm a new Actions run creates jobs. Do not merge branches, delete branches, or publish packages.


## Correction from a new live read (2026-10-25 01:52Z)

The current live `.github/workflows/build.yml` content has changed relative to the earlier 1,542-line corruption snapshot recorded above. A full fetch now returns blob SHA `995d0018bf4632c367c3a622a66b310366f03ad3`, **474 lines** (including the trailing empty split entry), with exactly one `publish_nuget` job at line 452 and no duplicate job IDs detected. The previously reported six duplicated `publish_nuget` blocks and the stray merged manifest/renderer fragment are **not present in this fetched version**. Do not repeat those earlier findings as current-state facts.

The current workflow has a coherent-looking top-level structure (`name`, `on`, `permissions`, `jobs`) and three job IDs: `gui-svg-package-proof`, `build-and-test`, and the manually gated `publish_nuget`. The Renderer-selected-frame verification is again a distinct step at lines 268–283, and the Drone Construction verification should be checked in the same current blob before any further edit. This observation alone does **not** establish that GitHub accepts the workflow or that jobs run.

**Next action:** determine the commit that introduced blob `995d0018...` and inspect recent Actions evidence for that exact commit. If a fresh run now creates jobs, stop treating YAML corruption as the current blocker and diagnose the actual job/step failures instead. If no fresh run exists, request/trigger validation through the available authorized workflow mechanism; do not claim CiAi ran unless its run ID and results are returned. Do not rewrite the workflow just to match the stale corruption report. Preserve the publish gate and all branch/merge restrictions.
## Fresh Actions evidence and targeted compile-glob fix (2026-10-25 02:00Z)

The branch advanced after the earlier correction. Current development head at the time of this note was 30bbf575db96b9b910e7866870b7db9947280ce0 (Restore Drone Construction proof as an isolated CI job). The latest workflow inventory now definitively shows that .github/workflows/build.yml is parsing and executing; the earlier zero-job failure is no longer the active blocker.

### Fresh runs

- [Build/test run 40152366666](https://github.com/TrentBest/TheSingularityWorkshop.Visualize/actions/runs/40152366666), commit c5eaf98514a1df474e7e2d6a22dca1239208aaf8: success. Jobs gui-svg-package-proof and build-and-test passed; NuGet publishing was skipped.
- [Observer presentation run 40152560240](https://github.com/TrentBest/TheSingularityWorkshop.Visualize/actions/runs/40152560240), commit 30bbf575db96b9b910e7866870b7db9947280ce0: success. Observer-driven presentation job passed and uploaded visualize-observer-driven-presentation-proof.
- [Latest build/test run 40152560269](https://github.com/TrentBest/TheSingularityWorkshop.Visualize/actions/runs/40152560269), commit 30bbf575db96b9b910e7866870b7db9947280ce0: overall failure, but gui-svg-package-proof and build-and-test both passed. Only the new drone-construction-frame-proof job failed. publish_nuget was skipped. Existing artifacts were uploaded for the passing work, including visualize-nuget-package, visualize-test-results, renderer-to-visualize-frame-proof, visualize-city-massing-proof, and visualize-extremus-crowd-proof.

### Exact failure evidence and fix

The failing job log reports CS0246 for FactAttribute / Fact across ExperiencesSource/tests/DroneConstruction.Tests/ConstructionSimulationTests.cs, with the compiler attributing those files to the root TheSingularityWorkshop.Visualize.csproj. The root project excludes tests/**/*.cs, samples/**/*.cs, and integrations/**/*.cs, but the Experiences checkout lives in a root-level ExperiencesSource/ directory, outside those exclusions. Consequently, the root SDK default compile glob accidentally compiles another repository's test sources into the Visualize package project.

A minimal targeted fix was committed on development:
- Commit: 9e8bc1df597919b213b85059754831d3242596c0
- Message: Exclude checked-out Experiences sources from Visualize compilation
- File: TheSingularityWorkshop.Visualize.csproj
- Change: add <Compile Remove="ExperiencesSource/**/*.cs" /> alongside the existing compile exclusions.

This excludes only the external checkout from the root package project's compile glob; the Drone Construction proof still references the specific Experiences project directly through its own ProjectReference. It does not add an xUnit dependency to Visualize or change simulation code.

### Next validation requested

1. Inspect commit 9e8bc1df597919b213b85059754831d3242596c0 and confirm the compile exclusion is narrowly scoped.
2. Verify the fresh build.yml run triggered by that commit. Required outcomes: build-and-test and gui-svg-package-proof stay green, drone-construction-frame-proof builds and runs, manifest assertions pass, and both SVG and manifest are uploaded as drone-construction-frame-proof.
3. Inspect the artifact manifest itself to confirm observerSequence=1, authoritativeAgents=3, submittedDraws=3, and entityIds=101,202,303. The current workflow's shell assertions verify the run ID, space kind/id, draw count, IDs, and backend, but do not yet assert observerSequence or authoritativeAgents; consider adding those assertions after confirming the proof output.
4. Do not state that CiAi's observer ran unless its run ID/results are produced. We have direct GitHub Actions evidence for the runs above, but no confirmed CiAi observer run in the available evidence.
5. Keep publishing skipped; no merge or branch deletion.
