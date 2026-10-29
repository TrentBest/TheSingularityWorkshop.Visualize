using System.Text.Json;
using TheSingularityWorkshop.Experiences.CapsuleClinic;
using TheSingularityWorkshop.Renderer.VisualizeAdapter;
using TheSingularityWorkshop.Visualize;
using TheSingularityWorkshop.Experiences.CapsuleClinic.VisualizeIntegration;
using TheSingularityWorkshop.Visualize.GuiSvg;

var output = args.Length > 0
    ? Path.GetFullPath(args[0])
    : Path.GetFullPath("artifacts/capsule-clinic-renderer-frame.svg");
Directory.CreateDirectory(Path.GetDirectoryName(output)!);

using var session = new CapsuleClinicLocalSession(
    "capsule-clinic-renderer-proof", "player", 4101, "practice-bot", 9202);

// Exercise the real intent boundary so the frame contains both settled cells and a newly active capsule.
var drop = session.SubmitIntent(CapsuleClinicIntents.HardDrop);
if (!drop.IntentRecognized || !drop.Decision.Accepted)
    throw new InvalidOperationException($"Authoritative hard drop was rejected: {drop.Decision.Rejection}.");

var snapshot = drop.Snapshot;
var presentation = CapsuleClinicRenderFrameFactory.Create(snapshot);
var repeatPresentation = CapsuleClinicRenderFrameFactory.Create(snapshot);
var adaptedFrame = new RendererFrameAdapter().Adapt(presentation.Frame);
await using var visualizer = new Visualizer(
    new GuiSvgVisualizerBackend(output), presentation.ResourceCatalog);
var result = await visualizer.RenderAsync(adaptedFrame);

if (presentation.PaneCount != 4)
    throw new InvalidOperationException($"Expected four arena panes; got {presentation.PaneCount}.");
if (presentation.OccupiedCells <= 0)
    throw new InvalidOperationException("The hard-drop snapshot did not produce any settled board cells.");
if (presentation.ActiveSegments < 2)
    throw new InvalidOperationException($"Expected two active capsule segments; got {presentation.ActiveSegments}.");
if (!presentation.Frame.Commands.SequenceEqual(repeatPresentation.Frame.Commands))
    throw new InvalidOperationException("Repeated frame creation changed draw-command order or values.");
if (presentation.CommandCount != presentation.BaseCellCommands + presentation.ActiveSegments)
    throw new InvalidOperationException("The frame does not contain the expected stable board-cell and active-segment commands.");
if (result.SubmittedDrawCount != presentation.CommandCount)
    throw new InvalidOperationException($"Expected {presentation.CommandCount} submitted draws; got {result.SubmittedDrawCount}.");

var manifestPath = Path.ChangeExtension(output, ".manifest.json");
var manifest = new
{
    experience = "Capsule Clinic",
    backend = "CPU/SVG",
    authoritativeSnapshot = true,
    gpuSubmission = false,
    hardDropAccepted = drop.Decision.Accepted,
    boardWidth = presentation.BoardWidth,
    boardHeight = presentation.BoardHeight,
    paneCount = presentation.PaneCount,
    baseCellCommands = presentation.BaseCellCommands,
    occupiedCells = presentation.OccupiedCells,
    activeSegments = presentation.ActiveSegments,
    commandCount = presentation.CommandCount,
    submittedDrawCount = result.SubmittedDrawCount,
    deterministicCommandSequence = true,
    stableCommandOrdering = "pane order, then bottom-up row-major cells, then active capsule segments"
};
await File.WriteAllTextAsync(manifestPath, JsonSerializer.Serialize(manifest, new JsonSerializerOptions { WriteIndented = true }));
Console.WriteLine(JsonSerializer.Serialize(manifest, new JsonSerializerOptions { WriteIndented = true }));
Console.WriteLine($"SVG: {output}");
Console.WriteLine($"Manifest: {manifestPath}");
