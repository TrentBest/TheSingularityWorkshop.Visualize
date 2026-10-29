using System.Text.Json;
using TheSingularityWorkshop.Experiences.CapsuleClinic;
using TheSingularityWorkshop.Renderer.VisualizeAdapter;
using TheSingularityWorkshop.Visualize;
using TheSingularityWorkshop.Visualize.CapsuleClinic;
using TheSingularityWorkshop.Visualize.GuiSvg;

var output = args.Length > 0
    ? Path.GetFullPath(args[0])
    : Path.GetFullPath("artifacts/capsule-clinic-renderer-frame.svg");
Directory.CreateDirectory(Path.GetDirectoryName(output)!);

using var session = new CapsuleClinicLocalSession(
    "capsule-clinic-renderer-proof", "player", 4101, "practice-bot", 9202);
var snapshot = session.CaptureSnapshot();
var presentation = CapsuleClinicRenderFrameFactory.Create(snapshot);
var adaptedFrame = new RendererFrameAdapter().Adapt(presentation.Frame);
await using var visualizer = new Visualizer(
    new GuiSvgVisualizerBackend(output), presentation.ResourceCatalog);
var result = await visualizer.RenderAsync(adaptedFrame);

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
    boardWidth = presentation.BoardWidth,
    boardHeight = presentation.BoardHeight,
    paneCount = presentation.PaneCount,
    baseCellCommands = presentation.BaseCellCommands,
    occupiedCells = presentation.OccupiedCells,
    activeSegments = presentation.ActiveSegments,
    commandCount = presentation.CommandCount,
    submittedDrawCount = result.SubmittedDrawCount,
    stableCommandOrdering = "pane order, then bottom-up row-major cells, then active capsule segments"
};
await File.WriteAllTextAsync(manifestPath, JsonSerializer.Serialize(manifest, new JsonSerializerOptions { WriteIndented = true }));
Console.WriteLine(JsonSerializer.Serialize(manifest, new JsonSerializerOptions { WriteIndented = true }));
Console.WriteLine($"SVG: {output}");
Console.WriteLine($"Manifest: {manifestPath}");
