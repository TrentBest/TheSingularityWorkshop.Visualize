using System.Numerics;
using TheSingularityWorkshop.Visualize;
using TheSingularityWorkshop.Visualize.GuiSvg;

// This proof deliberately uses the frame's world, view, and perspective transforms.
// It is a CPU SVG projection of real Visualizer mesh resources—not a GPU renderer.
var output = args.Length > 0
    ? Path.GetFullPath(args[0])
    : Path.GetFullPath("samples/PrimitiveGallery/generated/08-gui-svg-perspective-mesh.svg");

var catalog = new VisualizerResourceCatalog();
var geometryId = new VisualizerResourceId(1);
var mesh = Octahedron();
catalog.SetGeometry(geometryId, mesh);

var palette = new[]
{
    new Vector4(0.18f, 0.78f, 0.76f, 1f),
    new Vector4(0.96f, 0.65f, 0.28f, 1f),
    new Vector4(0.38f, 0.61f, 0.96f, 1f),
    new Vector4(0.72f, 0.45f, 0.92f, 1f)
};
for (var i = 0; i < palette.Length; i++)
    catalog.SetMaterial(new VisualizerResourceId((ulong)(i + 2)), new VisualizerMaterial(1, palette[i]));

var world = Matrix4x4.CreateRotationY(0.52f) * Matrix4x4.CreateRotationX(-0.24f);
var cameraPosition = new Vector3(2.8f, 2.2f, 4.4f);
var view = Matrix4x4.CreateLookAt(cameraPosition, Vector3.Zero, Vector3.UnitY);
var projection = Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 3f, 16f / 9f, 0.1f, 100f);

// Painter ordering is sufficient for this convex, opaque proof mesh: far faces first.
var faceOrder = Enumerable.Range(0, mesh.Indices.Count / 3)
    .Select(face =>
    {
        var a = mesh.Vertices[(int)mesh.Indices[face * 3]].Position;
        var b = mesh.Vertices[(int)mesh.Indices[face * 3 + 1]].Position;
        var c = mesh.Vertices[(int)mesh.Indices[face * 3 + 2]].Position;
        var center = (a + b + c) / 3f;
        return (Face: face, Depth: Vector3.Transform(center, world * view).Z);
    })
    .OrderBy(item => item.Depth)
    .ToArray();

var commands = faceOrder.Select(item => new VisualizerDrawCommand(
    geometryId,
    new VisualizerResourceId((ulong)(2 + item.Face % palette.Length)),
    world,
    3,
    item.Face * 3,
    0,
    VisualizerRepresentation.Near)).ToArray();

var frame = new VisualizerFrame(
    new VisualizerView(cameraPosition, view, projection),
    commands);

await using var visualizer = new Visualizer(
    new GuiSvgVisualizerBackend(output, projectWithCamera: true),
    catalog);
var result = await visualizer.RenderAsync(frame);
Console.WriteLine($"Perspective mesh proof: {result.SubmittedDrawCount} triangle draw(s) -> {output}");

static VisualizerGeometry Octahedron() => new(1,
[
    Vertex(0, 1, 0), Vertex(1, 0, 0), Vertex(0, 0, 1),
    Vertex(-1, 0, 0), Vertex(0, 0, -1), Vertex(0, -1, 0)
],
[
    0, 1, 2,
    0, 2, 3,
    0, 3, 4,
    0, 4, 1,
    5, 2, 1,
    5, 3, 2,
    5, 4, 3,
    5, 1, 4
]);

static VisualizerVertex Vertex(float x, float y, float z) =>
    new(new Vector3(x, y, z), Vector3.Normalize(new Vector3(x, y, z)), Vector2.Zero, Vector4.One);
