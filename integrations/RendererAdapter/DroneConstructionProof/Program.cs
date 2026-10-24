using System.Numerics;
using TheSingularityWorkshop.Experiences.DroneConstruction;
using TheSingularityWorkshop.Renderer;
using TheSingularityWorkshop.Renderer.VisualizeAdapter;
using TheSingularityWorkshop.Visualize;
using TheSingularityWorkshop.Visualize.GuiSvg;

var output = args.Length > 0
    ? Path.GetFullPath(args[0])
    : Path.GetFullPath("artifacts/drone-construction-frame.svg");
Directory.CreateDirectory(Path.GetDirectoryName(output)!);

const string spaceId = "construction-proof-grid";
const string runId = "drone-construction-renderer-proof";
var simulation = new ConstructionSimulation(
    4,
    2,
    (_, _) => 0,
    [
        new ConstructionAgent(101, new GridCell(0, 0)),
        new ConstructionAgent(202, new GridCell(1, 0)),
        new ConstructionAgent(303, new GridCell(2, 1))
    ],
    [],
    spaceId);
var observer = new ConstructionSnapshotObserver(runId);
var snapshot = simulation.Snapshot();
if (!observer.TryPublish(snapshot))
    throw new InvalidOperationException("The committed construction snapshot was not accepted.");
if (!observer.TryGetLatest(out var accepted, out var sequence) || accepted is null)
    throw new InvalidOperationException("The observer did not expose its committed snapshot.");

var commands = new List<RenderDrawCommand>();
var geometryId = new RenderResourceId(1);
var materialId = new RenderResourceId(2);
foreach (var agent in accepted.Agents
    .Where(agent => agent.Location.SpaceKind == ConstructionSpaceKind.SimulationGrid
        && StringComparer.Ordinal.Equals(agent.Location.SpaceId, spaceId))
    .OrderBy(agent => agent.AgentId))
{
    var transform = Matrix4x4.CreateScale(0.18f)
        * Matrix4x4.CreateTranslation(agent.Position.X * 0.45f - 0.5f, 0.35f - agent.Position.Y * 0.45f, 0);
    commands.Add(new RenderDrawCommand(
        geometryId,
        materialId,
        transform,
        indexCount: 3,
        firstIndex: 0,
        baseVertex: 0,
        RendererRepresentation.Interactive,
        entity: new RenderEntityId(agent.AgentId)));
}

if (commands.Count != 3)
    throw new InvalidOperationException($"Expected three in-space agents, found {commands.Count}.");
var expectedIds = new ulong[] { 101, 202, 303 };
if (!commands.Select(command => command.Entity.Value).SequenceEqual(expectedIds))
    throw new InvalidOperationException("Stable construction agent IDs were not preserved in draw-command order.");

var frame = new RenderFrame(
    new RenderView(Vector3.Zero, Matrix4x4.Identity, Matrix4x4.Identity),
    commands);
var visualizerFrame = new RendererFrameAdapter().Adapt(frame);
var catalog = new VisualizerResourceCatalog();
var vertices = new[]
{
    new VisualizerVertex(new Vector3(-0.08f, -0.07f, 0), Vector3.UnitZ, Vector2.Zero, Vector4.One),
    new VisualizerVertex(new Vector3(0, 0.09f, 0), Vector3.UnitZ, Vector2.Zero, Vector4.One),
    new VisualizerVertex(new Vector3(0.08f, -0.07f, 0), Vector3.UnitZ, Vector2.Zero, Vector4.One)
};
catalog.SetGeometry(new VisualizerResourceId(geometryId.Value),
    new VisualizerGeometry(1, vertices, new uint[] { 0, 1, 2 }));
catalog.SetMaterial(new VisualizerResourceId(materialId.Value),
    new VisualizerMaterial(1, new Vector4(0.29f, 0.84f, 0.65f, 1)));

await using var visualizer = new Visualizer(new GuiSvgVisualizerBackend(output), catalog);
var result = await visualizer.RenderAsync(visualizerFrame);
if (result.SubmittedDrawCount != 3)
    throw new InvalidOperationException($"Expected three submitted draws, found {result.SubmittedDrawCount}.");

var manifestPath = Path.ChangeExtension(output, ".manifest.txt");
await File.WriteAllTextAsync(manifestPath,
    $"run={observer.RunId}{Environment.NewLine}" +
    $"tick={accepted.Tick}{Environment.NewLine}" +
    $"observerSequence={sequence}{Environment.NewLine}" +
    $"spaceKind={ConstructionSpaceKind.SimulationGrid}{Environment.NewLine}" +
    $"spaceId={spaceId}{Environment.NewLine}" +
    $"authoritativeAgents={accepted.Agents.Count}{Environment.NewLine}" +
    $"submittedDraws={result.SubmittedDrawCount}{Environment.NewLine}" +
    $"entityIds={string.Join(",", commands.Select(command => command.Entity.Value))}{Environment.NewLine}" +
    "backend=CPU/SVG" + Environment.NewLine);

Console.WriteLine($"Construction snapshot -> RenderFrame -> RendererFrameAdapter -> Visualize -> GUI.SVG: {result.SubmittedDrawCount} draws.");
Console.WriteLine($"run={observer.RunId}; tick={accepted.Tick}; sequence={sequence}; entityIds={string.Join(",", expectedIds)}");
Console.WriteLine($"SVG: {output}");
Console.WriteLine($"Manifest: {manifestPath}");
