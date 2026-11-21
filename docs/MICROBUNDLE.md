# Visualize MicroBundle

## Capability and composition boundary

Visualize is the Workshop capability that manifests presentation work selected by Renderer. It is intended to be resolved and composed by FSM_COS as a **MicroBundle**, not consumed by hosts as a NuGet capability package.

The .NET project in this repository is the source/build representation of the capability. Its source project reference to Renderer exists only for compiling and testing the narrow adapter. It is not the runtime delivery or composition mechanism.

FSM_COS owns manifest interpretation, dependency closure, validation, load order, configuration, and runtime composition. MicroBundleRepository (or another host-supplied catalog) owns artifact lookup and retrieval. Visualize owns its capability implementation, presentation contracts, resource validation, backend execution, and diagnostics. AnyApp or another host owns its native/browser surface and lifecycle.

## Proposed canonical identity

The proposed stable ontology identity is:

```text
TheSingularityWorkshop.Visualize
```

This is a semantic capability identity, not a repository URL, NuGet package coordinate, filesystem path, or repository-local numeric handle. Confirm/register this identity with the MicroBundleDomain and repository workflow before producing a durable artifact.

A portable artifact reference is:

```text
(ontology string, version, SHA-256)
```

- **Ontology string** identifies the Visualize capability.
- **Version** identifies the declared capability artifact version.
- **SHA-256** identifies the exact artifact bytes and allows the resolver to verify integrity before loading.

A repository may map that portable reference to an opaque non-zero numeric handle for local blob lookup. Such a handle is repository-local only and must never be treated as the portable semantic identity.

## Responsibility flow

```text
Experience / authoritative state
          |
          v
Renderer MicroBundle
  observer-relative representation
          |
          v
Renderer → Visualize adapter
  contract translation; no policy rewrite
          |
          v
Visualize MicroBundle
  resources, backend execution, diagnostics
          |
          v
Selected backend + host surface
          |
          v
Pixels / presentation
```

Renderer owns what representation and computational detail are required. The adapter preserves the Renderer command meaning while translating to Visualize's backend-neutral frame contracts. Visualize owns how that presentation work is manifested by a selected backend. The host owns its window/canvas, resize, DPI, focus, input routing, and shutdown lifecycle.

## Build-time source references are not runtime composition

The Renderer `ProjectReference` in `integrations/RendererAdapter` is intentionally limited to build and test validation. It allows the adapter to compile against the current Renderer source API and lets integration tests exercise the translation boundary.

It does not:

- locate a portable MicroBundle artifact;
- verify an artifact digest;
- resolve a RuntimeManifest;
- establish a dependency closure;
- load or arbitrate a MicroBundle through FSM_COS;
- create a RuntimeAssembly;
- prove that AnyApp or another host can execute the composed capability.

The Visualize project is non-packable as a NuGet capability. CI must not pack or publish it to NuGet. Ordinary NuGet dependencies remain acceptable for unrelated implementation dependencies, but they do not replace the MicroBundle artifact and composition contract.

## Evidence levels

| Evidence | What it establishes | What it does not establish |
| --- | --- | --- |
| Source build and contract tests | Visualize source and contracts compile and behave as tested | Runtime MicroBundle loading |
| Renderer adapter proof | Renderer commands cross the adapter with tested meaning/identity preservation | FSM_COS composition |
| CPU/SVG backend proof | A frame is validated and manifested as SVG on the CPU | GPU execution or a native interactive surface |
| Browser/WebGPU experiment | The browser experiment exercises its own WebGPU path | Execution of the C# Visualize assembly unless a real bridge is demonstrated |
| FSM_COS integration test | The actual artifact is resolved, verified, loaded, and composed | Hardware GPU correctness |
| Hardware-backed host proof | The selected backend executes on identified hardware and presents through the host | Unmeasured scale/performance claims |

## Runtime integration still required

The next implementation milestone is tracked in [issue #49](https://github.com/TrentBest/TheSingularityWorkshop.Visualize/issues/49). It must coordinate with MicroBundleDomain, MicroBundleRepository, FSM_COS, Renderer, and AnyApp rather than duplicating their responsibilities.

The proof must use actual artifacts and a catalog/repository adapter, then exercise the FSM_COS composition lifecycle. Missing artifacts, incompatible versions, digest mismatches, missing dependencies, duplicate identities, and failed loads must reject composition before a host receives the result.

This document defines the intended Visualize capability boundary and a proposed portable identity. It does not claim that the durable artifact publisher, repository resolver, FSM_COS loader, or end-to-end host composition is already implemented.
