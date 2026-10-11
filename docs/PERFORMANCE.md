# Performance and Benchmark Rules

## The claim we are trying to prove

The objective is not to win a synthetic draw-call contest. It is to make a large, living, individually distinguishable world convincing while spending computation only where an observer can benefit.

Renderer's Event Horizons reduce unnecessary representation and update work before it reaches Visualizer. Visualizer must then execute the remaining work efficiently on the GPU.

**A GPU label is not a performance result.**

## Required measurements

Every published benchmark should record:
- CPU model, core count, memory, OS, GPU model and memory, driver, API/backend, and runtime version;
- scene definition and stable workload seed;
- resolution, quality settings, visible and total population;
- number of draw calls, dispatches, instances, vertices, and triangles where applicable;
- CPU frame/submission time;
- GPU execution time by pass where supported;
- frame-time distribution (median, 95th, 99th percentile), not average FPS alone;
- allocation rate, garbage collection, resource churn, and memory use;
- upload/download volume and synchronization stalls;
- cold-start and shader/pipeline compilation costs;
- input-to-presentation latency where measurable;
- power behavior when comparing laptop/mobile devices;
- error handling and device-loss recovery.

## Compare equivalent work

- Use the same scene, resolution, visual quality, and workload seed.
- Separate CPU simulation cost from GPU presentation cost.
- Include transfer and synchronization cost in the end-to-end result.
- Warm up pipelines before steady-state measurements, but report cold-start costs separately.
- Run enough repetitions to expose variance and stutter.
- Keep raw results and commands/scripts with the benchmark.
- Do not compare one backend at reduced fidelity against another at full fidelity without stating the difference.

## Gesture workloads

A useful early workload is a squad or crowd performing one shared procedural Gesture with bounded per-entity variation. Measure:
- number of active entities;
- number of unique Gesture definitions;
- pose representation and bones/joints per entity;
- update frequency per Event Horizon;
- CPU reference evaluation cost;
- upload bandwidth;
- GPU evaluation and draw cost;
- visual divergence from reference output;
- time spent waiting for synchronization.

The goal is not necessarily to move every Gesture operation onto the GPU. The goal is to choose the split that delivers correct, believable motion with the lowest measured total cost.

## Performance claims

A benchmark is not reproducible without a named device, driver/backend, workload, resolution, settings, and source revision. Report actual numbers and conditions. Do not extrapolate a small triangle test to thousands of soldiers or a large spacecraft.
