# GPU System Detection and Hardware Inventory

**Status:** The API-neutral discovery contract is implemented in `src/GPU`; an isolated Vulkan physical-device enumeration provider now exists in `src/GPU/Vulkan`. Device initialization, surface presentation, and selection are not yet implemented.

## Purpose

Visualize needs to recognize the machine it is running on before choosing how to execute work. That means more than asking whether an NVIDIA, AMD, or Intel GPU exists. We need to know which devices are visible to each API, which one can initialize, what capabilities and limits it exposes, which driver/runtime is in use, and whether it is physical hardware or a software adapter.

The detector is an inventory and evidence service. It does not select a backend by vendor name, create GPU resources, or replace the operating-system/vendor driver.

## Layered detection

### 1. Host and system profile

Collect, where available: OS family/version/build, process architecture/bitness, .NET/runtime version, host kind and presentation constraints (WPF/native desktop, browser, headless), display/surface information relevant to the active host, native runtime/binding prerequisites, and CPU/system-memory facts as diagnostic context—not as a proxy for GPU capability.

Do not make hardware/API decisions from OS name alone.

### 2. API adapter enumeration

Each available backend enumerates its own adapters and returns a backend-specific report. Vulkan, D3D12, and WebGPU may expose different device sets and different levels of detail.

Capture fields when exposed: API/backend and adapter name; raw vendor ID and device ID; driver name/version and API/runtime version; device type (discrete, integrated, virtual, CPU/software, unknown); dedicated/shared memory heaps, budgets and limits; supported queues, formats, extensions/features, compute, timestamps, synchronization and readback; initialization outcome and structured failure reason; stable identity where available. Enumeration ordinal is session-local only.

Do not infer model, feature set, driver quality, performance, or physical status from vendor ID or marketing name. Preserve unavailable fields as unknown.

### 3. Normalized profile plus raw details

Emit a serializable System Profile containing host facts and zero or more Adapter Reports. Keep normalized common fields for discovery and policy while preserving backend-specific detail. Distinguish these evidence states:

- adapter discovered;
- backend successfully initialized the adapter;
- device successfully created;
- required workload capabilities verified;
- physical-hardware performance evidence collected.

Discovery is not initialization; initialization is not workload proof; workload proof is not a benchmark.

## Selection policy

Selection is separate. It consumes host/surface compatibility, all adapter reports, required/optional workload features, memory/format/queue limits, explicit application/user policy (including power/performance intent), and initialization/runtime health.

Return a decision record: selected adapter/backend, rejected alternatives and reasons, capability match, fallback history, and degraded behavior. Support multiple GPUs. Enumeration order or primary-display status must not silently determine selection.

Vendor is a diagnostic and possible extension key, not the selection algorithm. Enable vendor-specific optimization only after checking the actual feature/extension and validating output; optional optimizations retain a correct fallback.

## Ownership and execution stack

`Vendor kernel driver + OS graphics runtime → native API adapter → Visualize.GPU → Renderer / Visualize → Foundry and Workshop ecosystem`

The vendor driver and OS runtime remain the hardware boundary for device initialization, command execution, scheduling, and device memory management. Our code owns the documented API layer above it: system/device inventory; backend/device policy; resource lifetime, residency and revision-aware cache; array packing/transfers; command recording/submission and synchronization; shaders, compute, rendering and readback; instrumentation, profiling, benchmarks, fallback; and integration with Renderer, Visualize, Foundry and the wider ecosystem.

We can use native interop, unsafe code, specialized kernels, and vendor extensions when documented, supportable, and measurably valuable. Writing a kernel driver is a different project with OS signing, security, hardware documentation, and maintenance burdens; it is not required to get close to the GPU through Vulkan/D3D12/CUDA-class interfaces.

## Refresh and lifecycle

Refresh on adapter arrival/removal, device loss, driver/runtime changes, host surface changes, and explicit rescan. A disappeared/unusable device invalidates its active resources and triggers a visible recovery/fallback decision. Never retain stale adapter handles as valid.

## Acceptance tests

- Host profile reports known OS/runtime/process facts without assuming a GPU.
- Enumerate zero, one, and multiple adapters; preserve every returned device.
- Preserve vendor/device IDs and raw backend-specific properties.
- Distinguish API-reported software/CPU devices from hardware where exposed.
- Record unavailable fields as unknown, never fabricated.
- Distinguish discovered, initialized, device-created, workload-verified, and benchmarked states.
- Record actionable initialization failure reasons.
- Test multi-GPU selection with explicit preference and capability constraints; enumeration order does not decide selection.
- Simulate device removal/loss and verify profile refresh, resource invalidation, and fallback diagnostics.
- Serialize profiles for support/benchmark artifacts without collecting unrelated personal/system data.

## Implementation boundary

Keep the initial System Detector API-neutral in Visualize.GPU. Backend adapters provide enumeration and raw capability reports. OS-specific probes live in optional diagnostic providers. Do not add WMI, SetupAPI, NVML, CUDA, Vulkan, DirectX, or browser dependencies to core merely to populate one field.

Implement the narrow vertical slice first: host profile + adapter enumeration contract + structured reports + selection explanation. Then implement enumeration in the first native adapter and verify it against actual hardware. Do not claim hardware detection from mocks or CI software adapters.


## Implemented API-neutral discovery slice

The core package now contains:

- `GpuHostInfo.CaptureCurrent()` for OS/runtime and process/OS architecture facts.
- `GpuAdapterInfo` and `GpuAdapterType` for backend-reported identity, raw vendor/device IDs, driver/API versions, memory facts, and optional feature names. Unknown values remain null or `Unknown`.
- `IGpuAdapterProvider` as the isolated enumeration seam for each API adapter.
- `GpuSystemDetector` to aggregate every configured provider, preserve multi-adapter results, and report a provider failure without discarding successful results from other providers.
- `GpuSystemDetectionResult` to expose the host snapshot, discovery timestamp, adapter reports, and failures.

Focused tests cover multiple providers/devices, raw identifier preservation, unknown values, software adapters, partial backend failure, provider identity validation, cancellation, and current host capture.

This slice is intentionally not a native hardware detector yet: no Vulkan, Direct3D, WebGPU, WMI, SetupAPI, NVML, or CUDA provider has been added. A report from a fake/test provider proves the contract behavior only. Real hardware recognition begins when a native provider is implemented and run on a machine with the relevant driver/runtime. Device initialization, workload verification, explicit selection policy, hot-plug refresh, and benchmarking remain subsequent evidence stages.


## First native enumeration provider

The optional `TheSingularityWorkshop.Visualize.GPU.Vulkan` project now contains `VulkanAdapterProvider`, pinned to Silk.NET.Vulkan 2.23.0. It creates a minimal Vulkan 1.0 instance, enumerates every physical device exposed by the installed loader/ICDs, and reports:

- API-reported device type, name, and raw vendor/device IDs;
- Vulkan API version and raw vendor-defined driver-version bits;
- total device-local and non-device-local memory heap sizes (reported in dedicated neutral fields, not misrepresented as dedicated VRAM or live shared-memory budgets);
- whether any queue family supports graphics, compute, and transfer.

The provider destroys its temporary instance after each enumeration and remains separate from the API-neutral GPU assembly. The adapter ID includes the enumeration ordinal and is therefore session-local, not a persistent hardware identity.

**Important limitations:** memory heap totals are not live budgets or free-memory estimates; Vulkan's driver-version integer is vendor-defined and is preserved as raw hexadecimal; queue flags are discovery facts, not proof of presentation compatibility. This provider does not create a logical device, initialize a workload, create a WPF surface, render pixels, select a GPU, or benchmark physical hardware. A Vulkan software implementation may also be enumerated.

The solution and contract-test project now reference this adapter project so normal CI builds compile it. Runtime enumeration still requires execution on a machine with a Vulkan loader and ICD installed.
