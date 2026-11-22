using System.Numerics;
using System.Text.Json;
using TheSingularityWorkshop.Renderer;
using TheSingularityWorkshop.Renderer.VisualizeAdapter;
using TheSingularityWorkshop.Visualize;
using TheSingularityWorkshop.Visualize.GuiSvg;

// One canonical Renderer frame feeds all three observable representations:
// documentation SVG, AI-inspectable Renderer structure, and Visualize backend output.
var output = args.Length > 0
    ? Path.GetFullPath(args[0])
    : Path.GetFullPath("samples/PrimitiveGallery/generated/10-renderer-selected-frame.svg");
var outputDirectory = Path.GetDirectoryName(output)!;
Directory.CreateDirectory(outputDirectory);
var rendererJsonPath = Path.ChangeExtension(output, ".renderer.json");
var visualizeSvgPath = Path.ChangeExtension(output, ".visualize.svg");

var catalog = new VisualizerResourceCatalog();
var commands = new List<RenderDrawCommand>();
var rules = new[]
{
    (Horizon: new EventHorizon("Interactive", 1, 10), Representation: RepresentationTarget.Interactive),
    (Horizon: new EventHorizon("Near", 5, 8), Representation: RepresentationTarget.Near),
    (Horizon: new EventHorizon("Distant", 15, 4), Representation: RepresentationTarget.Distant),
    (Horizon: new EventHorizon("Aggregate", 30, 2), Representation: RepresentationTarget.Aggregate),
    (Horizon: new EventHorizon("Semantic", 100, 1), Representation: RepresentationTarget.Semantic)
};
var horizons = rules.Select(rule => rule.Horizon).ToArray();
var observerDistances = new[] { 0.5, 2.5, 8, 20, 45 };

for (var index = 0; index < observerDistances.Length; index++)
{
    var distance = observerDistances[index];
    var horizon = ObserverDistance.Select(distance, horizons);
    var desired = rules.First(rule => ReferenceEquals(rule.Horizon, horizon)).Representation;
    var context = new RendererComputationContext($"renderer-proof-{desired}", desired);
    using var machine = new RendererComputationMachine(context);
    var selected = machine.Advance();

    var geometryId = new RenderResourceId((ulong)(100 + index));
    var materialId = new RenderResourceId((ulong)(200 + index));
    var entityId = new RenderEntityId((ulong)(300 + index));
    var shape = GeometryFor(selected);
    catalog.SetGeometry(new VisualizerResourceId(geometryId.Value), shape.Geometry);
    catalog.SetMaterial(new VisualizerResourceId(materialId.Value), new VisualizerMaterial(1, shape.Color));

    var x = -0.40f + index * 0.20f;
    var scale = selected switch
    {
        RepresentationTarget.Semantic => 0.38f,
        RepresentationTarget.Aggregate => 0.62f,
        RepresentationTarget.Distant => 0.78f,
        RepresentationTarget.Near => 0.95f,
        RepresentationTarget.Interactive => 1.12f,
        _ => throw new ArgumentOutOfRangeException(nameof(selected))
    };

    commands.Add(new RenderDrawCommand(
        geometryId,
        materialId,
        Matrix4x4.CreateScale(scale) * Matrix4x4.CreateTranslation(x, 0, 0),
        shape.Geometry.Indices.Count,
        firstIndex: 0,
        baseVertex: 0,
        Enum.Parse<RendererRepresentation>(selected.ToString()),
        entity: entityId));
    Console.WriteLine($"distance {distance,5:0.0} -> horizon {horizon.Name,-11} -> FSM {selected,-11} -> entity {entityId.Value}");
}

var renderFrame = new RenderFrame(
    new RenderView(Vector3.Zero, Matrix4x4.Identity, Matrix4x4.Identity),
    commands);
var adapter = new RendererFrameAdapter();
var visualizerFrame = adapter.Adapt(renderFrame);

// Assert that the adapter preserves the semantic identity, resources, selected
// representation, and command order before any backend is allowed to consume it.
if (visualizerFrame.Commands.Count != renderFrame.Commands.Count)
    throw new InvalidOperationException("Renderer-to-Visualize adaptation changed the command count.");

for (var index = 0; index < renderFrame.Commands.Count; index++)
{
    var source = renderFrame.Commands[index];
    var adapted = visualizerFrame.Commands[index];
    if (adapted.Entity.Value != source.Entity.Value ||
        adapted.Geometry.Value != source.Geometry.Value ||
        adapted.Material.Value != source.Material.Value ||
        adapted.Representation.ToString() != source.Representation.ToString())
    {
        throw new InvalidOperationException($"Renderer-to-Visualize adaptation changed command identity or meaning at index {index}.");
    }
}

// The Renderer artifact is deliberately explicit rather than relying on implicit
// System.Numerics serialization. It is intended to be inspectable by tools and AI.
var rendererDocument = new
{
    schema = "workshop.renderer-frame.v1",
    source = "TheSingularityWorkshop.Renderer",
    commandCount = renderFrame.Commands.Count,
    view = new
    {
        observerPosition = new[] { renderFrame.View.ObserverPosition.X, renderFrame.View.ObserverPosition.Y, renderFrame.View.ObserverPosition.Z },
        viewMatrix = MatrixValues(renderFrame.View.View),
        projectionMatrix = MatrixValues(renderFrame.View.Projection)
    },
    commands = renderFrame.Commands.Select((command, index) => new
    {
        order = index,
        entityId = command.Entity.Value,
        geometryResourceId = command.Geometry.Value,
        materialResourceId = command.Material.Value,
        representation = command.Representation.ToString(),
        worldTransform = MatrixValues(command.WorldTransform),
        command.IndexCount,
        command.FirstIndex,
        command.BaseVertex,
        command.InstanceCount,
        command.FirstInstance
    }).ToArray()
};
await File.WriteAllTextAsync(rendererJsonPath,
    JsonSerializer.Serialize(rendererDocument, new JsonSerializerOptions { WriteIndented = true }));

// View 1: documentation-facing SVG emitted by the existing GUI.SVG integration.
await using (var documentationVisualizer = new Visualizer(new GuiSvgVisualizerBackend(output), catalog))
{
    var documentationResult = await documentationVisualizer.RenderAsync(visualizerFrame);
    if (documentationResult.SubmittedDrawCount != renderFrame.Commands.Count)
        throw new InvalidOperationException(
            $"GUI.SVG submitted {documentationResult.SubmittedDrawCount} of {renderFrame.Commands.Count} Renderer commands.");
    Console.WriteLine($"Documentation SVG (GUI.SVG): {documentationResult.SubmittedDrawCount} commands -> {output}");
}

// View 2: the actual Visualize-owned CPU/SVG backend consumes the exact same adapted frame.
await using (var visualizeBackend = new SvgVisualizerBackend(svg => File.WriteAllText(visualizeSvgPath, svg)))
await using (var visualizer = new Visualizer(visualizeBackend, catalog))
{
    var visualizeResult = await visualizer.RenderAsync(visualizerFrame);
    if (visualizeResult.SubmittedDrawCount != renderFrame.Commands.Count)
        throw new InvalidOperationException(
            $"Visualize submitted {visualizeResult.SubmittedDrawCount} of {renderFrame.Commands.Count} Renderer commands.");
    Console.WriteLine($"Visualization output (Visualize SVG/CPU): {visualizeResult.SubmittedDrawCount} commands -> {visualizeSvgPath}");
}

Console.WriteLine($"Renderer structure (AI-inspectable JSON): {rendererJsonPath}");
Console.WriteLine("Tri-view identity/order/representation checks: PASS");

static float[] MatrixValues(Matrix4x4 value) =>
[
    value.M11, value.M12, value.M13, value.M14,
    value.M21, value.M22, value.M23, value.M24,
    value.M31, value.M32, value.M33, value.M34,
    value.M41, value.M42, value.M43, value.M44
];

static (VisualizerGeometry Geometry, Vector4 Color) GeometryFor(RepresentationTarget target)
{
    var (points, color) = target switch
    {
        RepresentationTarget.Semantic => (
            new[] { new Vector3(-0.07f, -0.07f, 0), new Vector3(0, 0.07f, 0), new Vector3(0.07f, -0.07f, 0) },
            new Vector4(0.49f, 0.91f, 0.75f, 1)),
        RepresentationTarget.Aggregate => (
            new[] { new Vector3(-0.09f, -0.07f, 0), new Vector3(-0.09f, 0.07f, 0), new Vector3(0.09f, 0.07f, 0), new Vector3(0.09f, -0.07f, 0) },
            new Vector4(1.00f, 0.75f, 0.38f, 1)),
        RepresentationTarget.Distant => (
            new[] { new Vector3(0, 0.10f, 0), new Vector3(0.09f, 0, 0), new Vector3(0, -0.10f, 0), new Vector3(-0.09f, 0, 0) },
            new Vector4(0.50f, 0.72f, 1.00f, 1)),
        RepresentationTarget.Near => (
            new[] { new Vector3(-0.09f, -0.07f, 0), new Vector3(0, 0.09f, 0), new Vector3(0.09f, -0.07f, 0), new Vector3(0, -0.02f, 0) },
            new Vector4(0.72f, 0.57f, 1.00f, 1)),
        RepresentationTarget.Interactive => (
            new[] { new Vector3(0, 0.12f, 0), new Vector3(0.035f, 0.035f, 0), new Vector3(0.12f, 0, 0), new Vector3(0.035f, -0.035f, 0), new Vector3(0, -0.12f, 0), new Vector3(-0.035f, -0.035f, 0), new Vector3(-0.12f, 0, 0), new Vector3(-0.035f, 0.035f, 0) },
            new Vector4(1.00f, 0.45f, 0.40f, 1)),
        _ => throw new ArgumentOutOfRangeException(nameof(target))
    };

    uint[] indices = target switch
    {
        RepresentationTarget.Semantic => [0, 1, 2],
        RepresentationTarget.Aggregate => [0, 1, 2, 0, 2, 3],
        RepresentationTarget.Distant => [0, 1, 2, 0, 2, 3],
        RepresentationTarget.Near => [0, 1, 2, 0, 2, 3],
        RepresentationTarget.Interactive => [0, 1, 2, 0, 2, 3, 0, 3, 4, 0, 4, 5, 0, 5, 6, 0, 6, 7],
        _ => throw new ArgumentOutOfRangeException(nameof(target))
    };

    var vertices = points.Select(point =>
        new VisualizerVertex(point, Vector3.UnitZ, Vector2.Zero, Vector4.One)).ToArray();
    return (new VisualizerGeometry(1, vertices, indices), color);
}
