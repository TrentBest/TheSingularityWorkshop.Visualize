using System.Numerics;
using TheSingularityWorkshop.Renderer;
using TheSingularityWorkshop.Renderer.VisualizeAdapter;
using TheSingularityWorkshop.Visualize;
using TheSingularityWorkshop.Visualize.GuiSvg;

// This is an executable integration proof, not a production scene system.
// Renderer FSM transitions select representations; this sample maps those states to
// simple test geometry, adapts the resulting RenderFrame, and submits it to GUI.SVG.
var output = args.Length > 0
    ? Path.GetFullPath(args[0])
    : Path.GetFullPath("samples/PrimitiveGallery/generated/08-renderer-selected-frame.svg");

var catalog = new VisualizerResourceCatalog();
var commands = new List<RenderDrawCommand>();
var horizons = new[]
{
    new EventHorizon(nameof(RepresentationTarget.Interactive), 1, 10),
    new EventHorizon(nameof(RepresentationTarget.Near), 5, 8),
    new EventHorizon(nameof(RepresentationTarget.Distant), 15, 4),
    new EventHorizon(nameof(RepresentationTarget.Aggregate), 30, 2),
    new EventHorizon(nameof(RepresentationTarget.Semantic), 100, 1)
};
var observerDistances = new[] { 0.5, 2.5, 8, 20, 45 };

for (var index = 0; index < observerDistances.Length; index++)
{
    var distance = observerDistances[index];
    var horizon = ObserverDistance.Select(distance, horizons);
    var desired = Enum.Parse<RepresentationTarget>(horizon.Name);
    var context = new RendererComputationContext($"renderer-proof-{desired}", desired);
    using var machine = new RendererComputationMachine(context);
    var selected = machine.Advance();

    var geometryId = new RenderResourceId((ulong)(100 + index));
    var materialId = new RenderResourceId((ulong)(200 + index));
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
        Enum.Parse<RendererRepresentation>(selected.ToString())));
    Console.WriteLine($"distance {distance,5:0.0} -> horizon {horizon.Name,-11} -> FSM {selected,-11} -> geometry {geometryId.Value}");
}

var renderFrame = new RenderFrame(
    new RenderView(Vector3.Zero, Matrix4x4.Identity, Matrix4x4.Identity),
    commands);
var adapter = new RendererFrameAdapter();
var visualizerFrame = adapter.Adapt(renderFrame);

await using var visualizer = new Visualizer(
    new GuiSvgVisualizerBackend(output),
    catalog);
var result = await visualizer.RenderAsync(visualizerFrame);
Console.WriteLine($"Renderer -> RendererFrameAdapter -> Visualizer -> GUI.SVG: {result.SubmittedDrawCount} draw command(s) -> {output}");

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