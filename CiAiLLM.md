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
