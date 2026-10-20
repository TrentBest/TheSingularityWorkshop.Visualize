# GPU Backend Architecture — Common Work, Native Capabilities

**Status:** Architecture direction; implementation and device behavior remain unverified.

## 1. The rule

Visualize must not care which GPU vendor or model is installed. It describes rendering and compute work in backend-neutral terms, asks the active backend what it can do, and selects an implementation compatible with the current host. The selected backend and the platform's own graphics stack deal with the actual device.

Do not build Visualize around Vulkan, WebGPU, Direct3D, a browser brand, or a GPU vendor. Those are execution paths, not the architecture.

The commonality is the workload contract and resource meaning. The backend-specific layer retains full access to its API's useful features. **Common core does not mean lowest-common-denominator GPU.**

## 2. Three distinct decisions

1. **Host compatibility:** What kind of application is running, and which presentation mechanisms can that host legally expose? A WPF desktop host and a browser page have different surface and lifecycle rules.
2. **Backend availability:** Which implementations are installed or available for this host, and can they initialize successfully?
3. **Capability fit:** Which requested workload features does each initialized device/backend support, within relevant limits?

Only after these checks should the runtime select an execution path. GPU vendor identity is diagnostic metadata, not the primary selection rule. Do not hard-code “NVIDIA means backend X” or assume a browser brand reveals its underlying GPU/API.

For example:
- A native host may select a Vulkan backend when Vulkan is available and suitable. Another native backend can be added when evidence justifies it.
- A browser host requests browser WebGPU when available and supported. The browser and its implementation choose and manage the underlying platform integration; Visualize must not try to bypass browser security or infer a specific vendor API.
- If a preferred backend cannot initialize, the host may try another compatible backend according to an explicit policy. It must report the selected backend and why fallbacks occurred.
- If no compatible GPU path exists, the host can choose an explicitly supported non-GPU path or fail with a diagnostic. A CPU/SVG result must never be reported as GPU execution.

## 3. The common contract

The backend-neutral Visualize contract should express the meaning of work, not API objects. Its responsibilities include:
- frames, views, draw work, resource IDs, materials, and transform semantics;
- contiguous vertex/index/instance data and stable revisions;
- resource creation/update/removal intent;
- portable baseline render/compute requests where semantics can be preserved;
- required versus optional features;
- diagnostics and resource lifetime intent.

Vulkan handles, WebGPU objects, D3D resources, browser objects, HWNDs, and device pointers stay outside core and Renderer contracts. The backend owns its device resources and translates logical IDs/revisions into API resources.

A backend may keep GPU-resident derived copies of authoritative CPU-side data. Revision changes drive the minimum correct update; unchanged revisions should not cause needless recreation or upload. This is a target to test, not a current zero-copy claim.

## 4. Baseline plus explicit extensions

Avoid two failure modes:
- **Lowest-common-denominator trap:** stripping useful backend features until every API looks identical.
- **Leaky abstraction trap:** letting API-specific types infect the core and every consumer.

Use a small common baseline plus explicit capability discovery and optional extension interfaces/requests. A backend reports supported features, limits, formats, compute support, instancing, synchronization/readback abilities, and relevant constraints. Feature availability must be queried from the actual initialized device/backend, not guessed from OS, browser name, or vendor.

A request declares which features are required and which are optional. If a required feature is missing, selection fails or chooses a compatible backend; it must not silently substitute behavior with different correctness or performance. Optional enhancements may use an explicit alternate path, with that choice visible in diagnostics and benchmarks.

Keep escape hatches narrow and deliberately backend-specific. They belong in an adapter/extension boundary and must not become required for the shared workload. Record when an extension makes a workload non-portable.

## 5. Host and backend boundaries

- **Visualize core:** backend-neutral work, resource semantics, requirements, and result/diagnostic contracts.
- **Native backend:** owns API device selection, queues, buffers, pipelines, command submission, synchronization, readback, and native resource disposal.
- **Host adapter:** owns the presentation surface and host lifecycle. AnyApp/WPF must not be forced to use browser APIs; Visualize core must not own WPF controls or window handles.
- **Browser backend/host adapter:** uses browser-provided WebGPU and browser lifecycle/security rules. Do not add browser APIs to the .NET core or assume native Vulkan can run inside a page.
- **Renderer:** decides observer-relative representations, horizons, visibility, and detail cadence. It submits the resulting work; it does not choose a GPU vendor.
- **Driver/runtime:** supplied by the platform ecosystem. Workshop code implements the renderer and adapters, not the hardware driver.

## 6. Resource and lifecycle behavior

Every backend must define and test:
- adapter/device identity and capability report;
- device loss, initialization failure, and fallback behavior;
- surface creation, resize, presentation, and shutdown where applicable;
- resource revision handling, ownership, disposal, and recreation after device loss;
- synchronization and readback semantics;
- unsupported required feature diagnostics.

A host surface is not the same thing as an offscreen target. Offscreen pixel readback can establish rendering correctness, but it does not prove WPF/browser presentation integration. Both gates are necessary for a production path.

## 7. Evaluation sequence

Use the same logical workload and assertions across backend implementations:

1. Verify host/backend compatibility and report the selected adapter/device/runtime.
2. Render a deterministic indexed triangle into a GPU-owned target and assert known pixels after GPU-to-CPU readback.
3. Exercise array uploads, indexed and instanced drawing, revisions, and resource reuse.
4. Verify compute-produced data consumed by rendering where the backend advertises the required capability.
5. Integrate the actual host surface and test resize, presentation, shutdown, device failure, and resource recreation.
6. Replay the same observer-driven workload and correctness assertions.
7. Benchmark equivalent visible work on named hardware, recording raw measurements, feature choices, driver/runtime, source revision, and repeatable commands.

Do not compare backends using workloads that silently differ in visual quality, enabled features, or output correctness. Separate software-adapter correctness evidence from physical-GPU performance evidence.

## 8. Current status and non-claims

This document defines an architecture direction. It does not mean capability negotiation, fallback selection, device-loss recovery, Vulkan rendering, browser WebGPU rendering, or GPU readback has been implemented. The dated [candidate comparison](GPU_BACKEND_CANDIDATE_COMPARISON.md) is research for selecting a first disposable experiment—not a permanent API choice. The [evaluation gate](GPU_BACKEND_EVALUATION.md) defines evidence required before claiming correctness or performance.
