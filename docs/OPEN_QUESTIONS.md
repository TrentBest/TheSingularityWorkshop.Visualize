# Open Questions

These questions are deliberately recorded rather than silently decided by implementation convenience.

## Graphics backend and host

1. Which maintained .NET binding gives the best first native GPU path for AnyApp/WPF?
2. Should the first native backend be Direct3D 12 directly, Vulkan directly, or a cross-backend implementation?
3. What WPF surface integration provides the best lifecycle, resize/DPI, presentation latency, and device recovery?
4. What is the browser support baseline: WebGPU-only, WebGPU plus WebGL2 fallback, or an explicitly limited support statement?
5. Which shader language and compilation/reflection pipeline best serves native and browser backends?
6. How should backend-specific advanced features be exposed without turning the common API into a lowest-common-denominator?

## Resource model

7. What are the canonical geometry, index, material, texture, shader, and resource-revision contracts?
8. How do logical RenderResourceId values map to versioned CPU-side descriptions and backend device resources?
9. What are the rules for uploads, eviction, residency, device loss, and resource recreation?
10. What does RenderFrame need to communicate about transparency, ordering, render passes, and instancing?

## Gestures and pose data

11. What is the canonical skeleton/joint/pose representation?
12. Is a Gesture definition a sequence of pose references plus mathematical transitions, or does it require a richer procedural graph?
13. How are spatial anchors, equipment-specific attachments, and physical constraints represented?
14. How do root motion and world movement interact without letting rendered transforms become authoritative?
15. Which transition operations are sufficiently data-parallel for GPU evaluation?
16. What CPU reference evaluator and test vectors define correctness for future GPU implementations?
17. How is stable identity combined with context and time to produce bounded, believable variation?
18. How do interruption, injury, equipment changes, collision, and failure affect a running Gesture?
19. Should TheSingularityWorkshop.Gestures be the standalone package name? It is recommended but not yet created.

## Performance and release

20. What repeatable population, Gesture, and frame-time workloads become regression gates?
21. What quality tolerances are acceptable between CPU and GPU pose evaluation?
22. What target devices and performance floors are required for the first supported release?
23. Which capabilities should be first-class MicroBundles, and which are backend implementation details?
24. What license and dependency constraints apply to graphics bindings, shader compilers, and native runtimes?

## Current recommendation

Prove a native WPF GPU surface first, but keep host and backend contracts distinct. Define the minimal Gesture output needed by the first scene before locking a pose binary schema. Create a standalone Gestures package once its data model and CPU reference semantics are clear enough to test independently.
