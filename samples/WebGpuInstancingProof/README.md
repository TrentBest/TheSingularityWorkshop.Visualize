# WebGPU Instancing Proof

This is the first **visible GPU workload** for Visualize: a single procedural triangle drawn many times from one contiguous GPU storage buffer. It is deliberately a standalone browser proof so it can be run and judged before a .NET backend is selected.

## Run it

1. Use a current browser with WebGPU enabled and a supported GPU.
2. From the repository root, run a local static server:

   ```powershell
   py -m http.server 8080
   ```

3. Open [http://localhost:8080/samples/WebGpuInstancingProof/](http://localhost:8080/samples/WebGpuInstancingProof/) in the browser.

A secure context is required; `localhost` qualifies. If WebGPU is unavailable, the sample reports that rather than silently falling back to a CPU renderer.

## What you can see

- 1,000–50,000 instances, each submitted through one instanced draw call.
- An observer offset that changes the view in the vertex shader.
- A near cohort updated every frame and a far cohort refreshed at approximately 5 Hz.
- GPU buffer upload accounting and a rough frame-rate display.
- Window resizing and a population control.

## What this proves — and what it does not

**Proves:** a real WebGPU device can render a large population from a GPU-resident instance buffer with distinct near/far update cadence.

**Does not yet prove:** integration with `IVisualizerBackend`, the .NET resource catalog, Renderer Event Horizon buckets, FSM_API/FSM_COS scheduling, compute-to-render, or production performance. The displayed FPS is a browser-side estimate, not a benchmark. Upload totals count bytes submitted by the sample, not bytes transferred over the physical bus.

This prototype is intentionally not a backend decision. The next step is to connect the proven workload to Visualize's backend-neutral contracts and then integrate Renderer/FSM scheduling without moving renderer ownership into the wrong layer.
