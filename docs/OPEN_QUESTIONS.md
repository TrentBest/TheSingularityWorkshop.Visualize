# Open Questions

These questions are deliberately recorded rather than silently decided by implementation convenience.

## Graphics backend and host

1. Which candidate best serves the array-first data model across AnyApp/WPF and browser hosting: Vulkan + WebGPU, a cross-backend implementation, or another justified pairing?
2. What WPF surface integration provides the best lifecycle, resize/DPI behavior, presentation latency, and device recovery for the selected native path?
3. What browser support baseline is required: WebGPU-only, WebGPU plus WebGL2 fallback, or an explicitly limited support statement?
4. Which shader language and compilation/reflection pipeline best serves native and browser backends without forcing identical binaries?
5. How should backend-specific advanced features be exposed without turning the common API into a lowest-common-denominator?
6. What evidence would justify selecting Direct3D 12 despite the current decision not to extend its isolated experiment by default?

## Array and resource model

7. What canonical contiguous layouts should represent geometry, indices, materials, textures, transforms, pose samples, visibility, and resource revisions?
8. Which data remains resident on the GPU, which is streamed, and how should updates avoid needless copies?
9. How should the Renderer-to-Visualizer adapter map Renderer RenderResourceId values to VisualizerResourceId and versioned CPU-side descriptions?
10. What are the rules for uploads, eviction, residency, device loss, and resource recreation?
11. What does RenderFrame need to communicate about transparency, ordering, render passes, indirect drawing, and instancing?
12. Which data belongs in storage/structured buffers, textures, packed integer layouts, or hybrid representations—and what workload proves the choice?

## Benchmark and GPU execution

13. What repeatable population, Gesture, upload, and frame-time workloads become regression gates?
14. How will CPU conversion/submission, allocations, transfer volume, GPU pass timings, synchronization stalls, memory, and frame-time percentiles be measured separately?
15. Which transition and pose operations are sufficiently data-parallel for GPU evaluation?
16. What quality tolerances are acceptable between CPU and GPU pose evaluation?
17. What target devices and performance floors are required for the first supported release?
18. What license and dependency constraints apply to graphics bindings, shader compilers, and native runtimes?

## Gestures and pose data

19. What is the canonical skeleton/joint/pose representation?
20. Is a Gesture definition a sequence of pose references plus mathematical transitions, or does it require a richer procedural graph?
21. How are spatial anchors, equipment-specific attachments, and physical constraints represented?
22. How do root motion and world movement interact without letting rendered transforms become authoritative?
23. What CPU reference evaluator and test vectors define correctness for future GPU implementations?
24. How is stable identity combined with context and time to produce bounded, believable variation?
25. How do interruption, injury, equipment changes, collision, and failure affect a running Gesture?
26. Should TheSingularityWorkshop.Gestures be the standalone package name? It is recommended but not yet created.

## Composition and release

27. Which capabilities should be first-class MicroBundles, and which are backend implementation details?

## Current recommendation

Choose the GPU execution model before advancing a specific API prototype. Evaluate contiguous arrays, bulk uploads, indexed/instanced rendering, and compute-to-render flow across native and browser constraints. Keep Visualize core contracts independent of host and graphics APIs. Define the minimal Gesture output needed by the first scene before locking a pose binary schema; create a standalone Gestures package once its data model and CPU reference semantics are clear enough to test independently.
