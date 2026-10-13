using System.Numerics;
using TheSingularityWorkshop.Visualize;
using TheSingularityWorkshop.Visualize.GuiSvg;

var output = args.Length > 0
    ? Path.GetFullPath(args[0])
    : Path.GetFullPath("samples/PrimitiveGallery/generated/07-gui-svg-connected-points.svg");
var catalog = new VisualizerResourceCatalog();
var geometryId = new VisualizerResourceId(1);
var materialId = new VisualizerResourceId(2);
catalog.SetGeometry(geometryId, new VisualizerGeometry(1,
[
    new VisualizerVertex(new Vector3(-0.32f, -0.18f, 0), Vector3.UnitZ, Vector2.Zero, Vector4.One),
    new VisualizerVertex(new Vector3(0.28f, -0.22f, 0), Vector3.UnitZ, Vector2.Zero, Vector4.One),
    new VisualizerVertex(new Vector3(0.36f, 0.12f, 0), Vector3.UnitZ, Vector2.Zero, Vector4.One),
    new VisualizerVertex(new Vector3(-0.18f, 0.24f, 0), Vector3.UnitZ, Vector2.Zero, Vector4.One)
], [0, 1, 2, 0, 2, 3]));
catalog.SetMaterial(materialId, new VisualizerMaterial(1, new Vector4(0.22f, 0.72f, 0.92f, 1)));
var command = new VisualizerDrawCommand(
    geometryId, materialId, Matrix4x4.Identity, 6, 0, 0, VisualizerRepresentation.Near);
var frame = new VisualizerFrame(
    new VisualizerView(Vector3.Zero, Matrix4x4.Identity, Matrix4x4.Identity),
    [command]);

await using var visualizer = new Visualizer(
    new GuiSvgVisualizerBackend(output, animateConnectedPoints: true),
    catalog);
var result = await visualizer.RenderAsync(frame);
Console.WriteLine($"GUI.SVG integration: {result.SubmittedDrawCount} draw command(s) -> {output}");
