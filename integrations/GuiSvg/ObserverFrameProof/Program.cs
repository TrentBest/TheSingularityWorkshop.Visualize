using System.Numerics;
using System.Text.Json;
using TheSingularityWorkshop.Visualize;
using TheSingularityWorkshop.Visualize.GuiSvg;
using TheSingularityWorkshop.Renderer;
using TheSingularityWorkshop.Renderer.VisualizeAdapter;

var outputDirectory = args.Length > 0 ? Path.GetFullPath(args[0]) : Path.GetFullPath("artifacts/observer-frame");
Directory.CreateDirectory(outputDirectory);

var horizons = new[]
{
    new EventHorizon("Interactive", 5, 60),
    new EventHorizon("Near", 20, 30),
    new EventHorizon("Distant", 100, 1)
};
var subjects = new[]
{
    new Subject(501, "Survey drone", new Vector3(2, 0, 0), new Vector4(0.25f, 0.9f, 0.95f, 1)),
    new Subject(502, "Cargo lifter", new Vector3(10, 0, 0), new Vector4(1f, 0.72f, 0.32f, 1)),
    new Subject(503, "Landing beacon", new Vector3(30, 0, 0), new Vector4(0.5f, 0.75f, 1f, 1)),
    new Subject(504, "Relay mast", new Vector3(80, 0, 0), new Vector4(0.72f, 0.55f, 1f, 1))
};
var catalog = new VisualizerResourceCatalog();
var detailedGeometry = new VisualizerResourceId(1);
var distantGeometry = new VisualizerResourceId(2);
catalog.SetGeometry(detailedGeometry, MarkerMesh(detailed: true));
catalog.SetGeometry(distantGeometry, MarkerMesh(detailed: false));
for (var i = 0; i < subjects.Length; i++)
    catalog.SetMaterial(new VisualizerResourceId((ulong)(10 + i)), new VisualizerMaterial(1, subjects[i].Color));

var first = await RenderObserver(0, "observer-x-000.svg");
var second = await RenderObserver(50, "observer-x-050.svg");
if (first.Rows.Select(row => row.Representation).SequenceEqual(second.Rows.Select(row => row.Representation)))
    throw new InvalidOperationException("Moving the observer did not change any representation decisions.");
if (first.CommandCount != subjects.Length || second.CommandCount != subjects.Length)
    throw new InvalidOperationException("Expected one stable-identity draw command per subject in both frames.");

var report = new
{
    evidence = "Renderer policy + RenderFrame -> RendererFrameAdapter -> Visualizer -> GUI.SVG",
    backend = "CPU/SVG",
    gpuSubmission = false,
    subjects = subjects.Length,
    frames = new[] { first, second },
    boundary = "This proves observer-driven representation selection reaches generated Visualize/GUI.SVG output. It does not measure GPU work, timing, allocations, or production renderer scheduling."
};
var reportPath = Path.Combine(outputDirectory, "observer-frame-report.json");
await File.WriteAllTextAsync(reportPath, JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
Console.WriteLine($"Observer x=0: {first.CommandCount} commands, {first.IndexCount} selected indices -> {first.Output}");
Console.WriteLine($"Observer x=50: {second.CommandCount} commands, {second.IndexCount} selected indices -> {second.Output}");
Console.WriteLine($"Report -> {reportPath}");

async Task<FrameReport> RenderObserver(float observerX, string fileName)
{
    var commands = new List<RenderDrawCommand>();
    var rows = new List<SubjectReport>();
    for (var i = 0; i < subjects.Length; i++)
    {
        var subject = subjects[i];
        var distance = Vector3.Distance(new Vector3(observerX, 0, 0), subject.Position);
        var horizon = ObserverDistance.Select(distance, horizons);
        var representation = horizon.Name switch
        {
            "Interactive" => RendererRepresentation.Interactive,
            "Near" => RendererRepresentation.Near,
            "Distant" => RendererRepresentation.Distant,
            _ => throw new InvalidOperationException($"Unexpected horizon '{horizon.Name}'.")
        };
        var isDetailed = representation is RendererRepresentation.Interactive or RendererRepresentation.Near;
        var indexCount = isDetailed ? 24 : 6;
        var normalizedX = (subject.Position.X - 40f) / 100f;
        var transform = Matrix4x4.CreateScale(isDetailed ? 1f : 0.75f) *
            Matrix4x4.CreateTranslation(normalizedX, subject.Position.Y, 0);
        commands.Add(new RenderDrawCommand(
            new RenderResourceId((isDetailed ? detailedGeometry : distantGeometry).Value),
            new RenderResourceId((ulong)(10 + i)),
            transform,
            indexCount, 0, 0, representation,
            entity: new RenderEntityId(subject.Id)));
        rows.Add(new SubjectReport(subject.Name, subject.Id, Math.Round(distance, 2), horizon.Name, representation.ToString(), indexCount));
    }

    var frame = new RenderFrame(
        new RenderView(new Vector3(observerX, 0, 0), Matrix4x4.Identity, Matrix4x4.Identity),
        commands);
    var visualizerFrame = new RendererFrameAdapter().Adapt(frame);
    if (visualizerFrame.Commands.Count != frame.Commands.Count)
        throw new InvalidOperationException("RendererFrameAdapter changed the frame command count.");
    for (var i = 0; i < frame.Commands.Count; i++)
    {
        if (frame.Commands[i].Entity.Value != visualizerFrame.Commands[i].Entity.Value ||
            frame.Commands[i].Representation.ToString() != visualizerFrame.Commands[i].Representation.ToString())
            throw new InvalidOperationException($"RendererFrameAdapter changed identity or representation at command {i}.");
    }

    var output = Path.Combine(outputDirectory, fileName);
    await using var visualizer = new Visualizer(
        new GuiSvgVisualizerBackend(
            output,
            backgroundColor: "#08111d",
            gridColor: "#1a3445",
            sceneLabel: $"Observer-driven frame / X={observerX:0} / Renderer -> Visualize -> GUI.SVG",
            drawGrid: true),
        catalog);
    var result = await visualizer.RenderAsync(visualizerFrame);
    if (result.SubmittedDrawCount != subjects.Length)
        throw new InvalidOperationException($"Expected {subjects.Length} SVG draw submissions; got {result.SubmittedDrawCount}.");
    return new FrameReport(observerX, output, result.SubmittedDrawCount, rows.Sum(row => row.SelectedIndices), rows);
}

static VisualizerGeometry MarkerMesh(bool detailed)
{
    if (!detailed)
        return new VisualizerGeometry(1,
        [
            Vertex(0, 0.026f), Vertex(0.026f, 0), Vertex(0, -0.026f), Vertex(-0.026f, 0)
        ], [0, 1, 2, 0, 2, 3]);

    var vertices = new List<VisualizerVertex> { Vertex(0, 0) };
    const int segments = 8;
    for (var i = 0; i < segments; i++)
    {
        var angle = i * MathF.Tau / segments;
        vertices.Add(Vertex(MathF.Cos(angle) * 0.026f, MathF.Sin(angle) * 0.026f));
    }
    var indices = new List<uint>();
    for (uint i = 1; i <= segments; i++)
    {
        indices.Add(0);
        indices.Add(i);
        indices.Add(i == segments ? 1u : i + 1);
    }
    return new VisualizerGeometry(1, vertices, indices);
}

static VisualizerVertex Vertex(float x, float y) =>
    new(new Vector3(x, y, 0), Vector3.UnitZ, Vector2.Zero, Vector4.One);

sealed record Subject(ulong Id, string Name, Vector3 Position, Vector4 Color);
sealed record SubjectReport(string Name, ulong EntityId, double Distance, string Horizon, string Representation, int SelectedIndices);
sealed record FrameReport(float ObserverX, string Output, int CommandCount, int IndexCount, IReadOnlyList<SubjectReport> Rows);
