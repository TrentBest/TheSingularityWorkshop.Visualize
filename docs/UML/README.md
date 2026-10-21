# UML source documentation

This folder records source-backed UML for individual C# files. Each page must be checked against the named file at the same repository revision; diagrams describe current implementation, not intended future behavior.

## Coverage

| Source file | UML page | Coverage |
|---|---|---|
| `Visualizer.cs` | [Visualizer.cs.md](Visualizer.cs.md) · [VisualizerUML.md](VisualizerUML.md) | Documented |
| `VisualizerFrame.cs` | [VisualizerFrame.cs.md](VisualizerFrame.cs.md) | Documented |
| `src/GPU/VisualizerFrameResolver.cs` | [VisualizerFrameResolver.cs.md](VisualizerFrameResolver.cs.md) · [VisualizerFrameResolverUML.md](VisualizerFrameResolverUML.md) | Documented |
| `IVisualizerBackend.cs` | [IVisualizerBackend.cs.md](IVisualizerBackend.cs.md) | Documented |
| Other production, sample, and test `.cs` files | Not yet documented | Pending |

## Rules

- One page per `.cs` file; preserve the source path in the page title.
- Include a class/interface diagram, responsibilities, public contract, dependencies, and observed constraints.
- Distinguish executable behavior from architectural intent and from future work.
- Link evidence to the exact source path and tests where known; do not infer that a sample proves GPU output unless it uses a real GPU backend.
- Update this index as each file is audited. This is an initial slice, not a claim of complete UML coverage.
