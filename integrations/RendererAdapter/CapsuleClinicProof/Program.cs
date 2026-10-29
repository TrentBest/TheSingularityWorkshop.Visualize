using System.Numerics;
using System.Text.Json;
using TheSingularityWorkshop.Experiences.CapsuleClinic;
using TheSingularityWorkshop.Renderer;
using TheSingularityWorkshop.Renderer.VisualizeAdapter;
using TheSingularityWorkshop.Visualize;
using TheSingularityWorkshop.Visualize.GuiSvg;

var output = args.Length > 0
    ? Path.GetFullPath(args[0])
    : Path.GetFullPath("artifacts/capsule-clinic-renderer-frame.svg");
Directory.CreateDirectory(Path.GetDirectoryName(output)!);

using var session = new CapsuleClinicLocalSession(
    "capsule-clinic-renderer-proof", "player", 4101, "practice-bot", 9202);
var snapshot = session.CaptureSnapshot();

const float cellSize = 0.035f;
var geometryId = new RenderResourceId(1);
var emptyMaterialId = new RenderResourceId(2);
var redMaterialId = new RenderResourceId(3);
var blueMaterialId = new RenderResourceId(4);
var yellowMaterialId = new RenderResourceId(5);
var purpleMaterialId = new RenderResourceId(6);
var catalog = new VisualizerResourceCatalog();
var vertices = new[]
{
    new VisualizerVertex(new Vector3(-0.5f, -0.5f, 0), Vector3.UnitZ, Vector2.Zero, Vector4.One),
    new VisualizerVertex(new Vector3(0.5f, -0.5f, 0), Vector3.UnitZ, Vector2.UnitX, Vector4.One),
    new VisualizerVertex(new Vector3(0.5f, 0.5f, 0), Vector3.UnitZ, Vector2.One, Vector4.One),
    new VisualizerVertex(new Vector3(-0.5f, 0.5f, 0), Vector3.UnitZ, Vector2.UnitY, Vector4.One)
};
catalog.SetGeometry(new VisualizerResourceId(geometryId.Value),
    new VisualizerGeometry(1, vertices, [0, 1, 2, 0, 2, 3]));
catalog.SetMaterial(new VisualizerResourceId(emptyMaterialId.Value),
    new VisualizerMaterial(1, new Vector4(0.08f, 0.13f, 0.18f, 1)));
catalog.SetMaterial(new VisualizerResourceId(redMaterialId.Value),
    new VisualizerMaterial(1, new Vector4(1f, 0.24f, 0.34f, 1)));
catalog.SetMaterial(new VisualizerResourceId(blueMaterialId.Value),
    new VisualizerMaterial(1, new Vector4(0.22f, 0.65f, 1f, 1)));
catalog.SetMaterial(new VisualizerResourceId(yellowMaterialId.Value),
    new VisualizerMaterial(1, new Vector4(1f, 0.78f, 0.2f, 1)));
catalog.SetMaterial(new VisualizerResourceId(purpleMaterialId.Value),
    new VisualizerMaterial(1, new Vector4(0.72f, 0.48f, 1f, 1)));

RenderResourceId MaterialFor(CapsuleColor color) => color switch
{
    CapsuleColor.Red => redMaterialId,
    CapsuleColor.Blue => blueMaterialId,
    CapsuleColor.Yellow => yellowMaterialId,
    CapsuleColor.Purple => purpleMaterialId,
    _ => throw new ArgumentOutOfRangeException(nameof(color))
};

var commands = new List<RenderDrawCommand>();
var occupiedCells = 0;
var activeSegments = 0;
var entityId = 1UL;
for (var paneIndex = 0; paneIndex < snapshot.Panes.Count; paneIndex++)
{
    var pane = snapshot.Panes[paneIndex];
    var (centerX, centerY) = pane.Pane switch
    {
        ArenaPane.LocalUpperLeft => (-0.48f, 0.30f),
        ArenaPane.ComparisonUpperRight => (0.48f, 0.30f),
        ArenaPane.ComparisonLowerLeft => (-0.48f, -0.30f),
        ArenaPane.ComparisonLowerRight => (0.48f, -0.30f),
        _ => throw new ArgumentOutOfRangeException(nameof(snapshot), "Unknown arena pane.")
    };

    for (var y = 0; y < snapshot.BoardHeight; y++)
    for (var x = 0; x < snapshot.BoardWidth; x++)
    {
        var cell = pane.Player?.Board[y * snapshot.BoardWidth + x];
        var materialId = cell is { } settled ? MaterialFor(settled.Color) : emptyMaterialId;
        if (cell.HasValue) occupiedCells++;
        var transform = Matrix4x4.CreateScale(cellSize)
            * Matrix4x4.CreateTranslation(
                centerX + (x - (snapshot.BoardWidth - 1) / 2f) * cellSize,
                centerY + (y - (snapshot.BoardHeight - 1) / 2f) * cellSize,
                0);
        commands.Add(new RenderDrawCommand(
            geometryId, materialId, transform, 6, 0, 0,
            RendererRepresentation.Interactive, entity: new RenderEntityId(entityId++)));
    }

    if (pane.Player?.Active is { } active)
    {
        var pivot = active.Pivot;
        var partnerX = pivot.X + (active.Orientation == CapsuleOrientation.Horizontal ? 1 : 0);
        var partnerY = pivot.Y + (active.Orientation == CapsuleOrientation.Vertical ? 1 : 0);
        foreach (var segment in new[]
        {
            (X: pivot.X, Y: pivot.Y, Color: active.Pair.PivotColor),
            (X: partnerX, Y: partnerY, Color: active.Pair.PartnerColor)
        })
        {
            if ((uint)segment.X >= (uint)snapshot.BoardWidth ||
                (uint)segment.Y >= (uint)snapshot.BoardHeight)
                continue;
            var transform = Matrix4x4.CreateScale(cellSize * 0.88f)
                * Matrix4x4.CreateTranslation(
                    centerX + (segment.X - (snapshot.BoardWidth - 1) / 2f) * cellSize,
                    centerY + (segment.Y - (snapshot.BoardHeight - 1) / 2f) * cellSize,
                    0.001f);
            commands.Add(new RenderDrawCommand(
                geometryId, MaterialFor(segment.Color), transform, 6, 0, 0,
                RendererRepresentation.Interactive, entity: new RenderEntityId(entityId++)));
            activeSegments++;
        }
    }
}

var frame = new RenderFrame(
    new RenderView(Vector3.Zero, Matrix4x4.Identity, Matrix4x4.Identity),
    commands);
var adaptedFrame = new RendererFrameAdapter().Adapt(frame);
await using var visualizer = new Visualizer(new GuiSvgVisualizerBackend(output), catalog);
var result = await visualizer.RenderAsync(adaptedFrame);
var expectedBaseCells = snapshot.Panes.Count * snapshot.BoardWidth * snapshot.BoardHeight;
if (commands.Count != expectedBaseCells + activeSegments)
    throw new InvalidOperationException("The frame does not contain the expected stable board-cell and active-segment commands.");
if (result.SubmittedDrawCount != commands.Count)
    throw new InvalidOperationException($"Expected {commands.Count} submitted draws; got {result.SubmittedDrawCount}.");

var manifestPath = Path.ChangeExtension(output, ".manifest.json");
var manifest = new
{
    experience = "Capsule Clinic",
    backend = "CPU/SVG",
    authoritativeSnapshot = true,
    gpuSubmission = false,
    boardWidth = snapshot.BoardWidth,
    boardHeight = snapshot.BoardHeight,
    paneCount = snapshot.Panes.Count,
    baseCellCommands = expectedBaseCells,
    occupiedCells,
    activeSegments,
    commandCount = commands.Count,
    submittedDrawCount = result.SubmittedDrawCount,
    stableCommandOrdering = "pane order, then bottom-up row-major cells, then active capsule segments"
};
await File.WriteAllTextAsync(manifestPath, JsonSerializer.Serialize(manifest, new JsonSerializerOptions { WriteIndented = true }));
Console.WriteLine(JsonSerializer.Serialize(manifest, new JsonSerializerOptions { WriteIndented = true }));
Console.WriteLine($"SVG: {output}");
Console.WriteLine($"Manifest: {manifestPath}");
