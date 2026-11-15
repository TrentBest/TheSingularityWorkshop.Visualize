using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using TheSingularityWorkshop.Visualize;

var output = args.Length > 0 ? Path.GetFullPath(args[0]) : Path.GetFullPath("artifacts/environment-scene");
Directory.CreateDirectory(output);

var resources = new VisualizerResourceCatalog();
var geometryId = new VisualizerResourceId(1);
var mesh = new MeshBuilder();
var floor = mesh.Rectangle(-0.92f, -0.82f, 0.92f, -0.66f);
var platform = mesh.Rectangle(-0.62f, -0.66f, 0.12f, -0.54f);
var box = mesh.Rectangle(-0.48f, -0.54f, -0.12f, -0.08f);
var ball = mesh.Circle(0.42f, -0.30f, 0.18f, 12);
var head = mesh.Circle(0.58f, 0.25f, 0.075f, 10);
var torso = mesh.Rectangle(0.535f, 0.02f, 0.625f, 0.20f);
var leftArm = mesh.Rectangle(0.43f, 0.08f, 0.54f, 0.12f);
var rightArm = mesh.Rectangle(0.62f, 0.08f, 0.73f, 0.12f);
var leftLeg = mesh.Rectangle(0.54f, -0.12f, 0.58f, 0.02f);
var rightLeg = mesh.Rectangle(0.59f, -0.12f, 0.63f, 0.02f);
resources.SetGeometry(geometryId, mesh.Build());

var materials = new[]
{
    Material(2, new Vector4(0.22f, 0.29f, 0.36f, 1)),
    Material(3, new Vector4(0.34f, 0.55f, 0.62f, 1)),
    Material(4, new Vector4(0.74f, 0.48f, 0.25f, 1)),
    Material(5, new Vector4(0.22f, 0.68f, 0.82f, 1)),
    Material(6, new Vector4(0.92f, 0.72f, 0.38f, 1)),
    Material(7, new Vector4(0.43f, 0.78f, 0.62f, 1))
};
foreach (var material in materials)
    resources.SetMaterial(new VisualizerResourceId(material.Id), material.Value);

var floorCommand = Command(floor, 2, 100);
var platformCommand = Command(platform, 3, 200);
var boxCommand = Command(box, 4, 300);
var ballCommand = Command(ball, 5, 400);
var articulatedCommands = new[]
{
    Command(head, 6, 500), Command(torso, 7, 500),
    Command(leftArm, 7, 500), Command(rightArm, 7, 500),
    Command(leftLeg, 7, 500), Command(rightLeg, 7, 500)
};

var sceneIdentity = "workshop://visualize/environment-fixture/v1";
var initial = new VisualizerSceneSnapshot(sceneIdentity, 1,
[
    new VisualizerSceneObject(new VisualizerEntityId(100), 0, true, [floorCommand]),
    new VisualizerSceneObject(new VisualizerEntityId(200), 1, true, [platformCommand]),
    new VisualizerSceneObject(new VisualizerEntityId(300), 2, true, [boxCommand]),
    new VisualizerSceneObject(new VisualizerEntityId(400), 2, true, [ballCommand]),
    new VisualizerSceneObject(new VisualizerEntityId(500), 3, true, articulatedCommands)
]);
var moved = new VisualizerSceneSnapshot(sceneIdentity, 2,
[
    new VisualizerSceneObject(new VisualizerEntityId(100), 0, true, [floorCommand]),
    new VisualizerSceneObject(new VisualizerEntityId(200), 1, true, [platformCommand]),
    new VisualizerSceneObject(new VisualizerEntityId(300), 2, true, [boxCommand]),
    new VisualizerSceneObject(new VisualizerEntityId(400), 2, true, [Command(ball, 5, 400, Matrix4x4.CreateTranslation(0.16f, 0.08f, 0))]),
    new VisualizerSceneObject(new VisualizerEntityId(500), 3, true, articulatedCommands)
]);

var backend = new SvgVisualizerBackend(_ => { }, 960, 540, "#101923");
var initialSvg = backend.BuildSvg(initial.CreateFrame(DefaultView()), resources);
var repeatedSvg = backend.BuildSvg(initial.CreateFrame(DefaultView()), resources);
var movedSvg = backend.BuildSvg(moved.CreateFrame(DefaultView()), resources);
if (!StringComparer.Ordinal.Equals(initialSvg, repeatedSvg))
    throw new InvalidOperationException("Identical scene snapshots did not produce identical SVG output.");
if (StringComparer.Ordinal.Equals(initialSvg, movedSvg))
    throw new InvalidOperationException("Moving the ball did not change the manifested scene.");
if (initial.SceneIdentity != moved.SceneIdentity || moved.Revision != 2)
    throw new InvalidOperationException("Scene identity/revision contract was not preserved.");

var initialPath = Path.Combine(output, "environment-scene.svg");
var movedPath = Path.Combine(output, "environment-scene-moved.svg");
File.WriteAllText(initialPath, initialSvg, new UTF8Encoding(false));
File.WriteAllText(movedPath, movedSvg, new UTF8Encoding(false));
var manifest = new
{
    schemaVersion = 1,
    fixture = "floor-platform-box-ball-articulated-placeholder",
    fixtureKind = "illustrative-not-physics-simulation",
    sceneIdentity,
    revisions = new[] { initial.Revision, moved.Revision },
    stableEntityIds = initial.Objects.Select(item => item.Entity.Value).ToArray(),
    objectCount = initial.Objects.Count,
    drawCommandCount = initial.Objects.Sum(item => item.Commands.Count),
    backend = "SvgVisualizerBackend",
    execution = "CPU/static SVG",
    gpuExecution = false,
    deterministicRepeat = true,
    movementChangesOutput = true,
    artifacts = new[]
    {
        new { file = Path.GetFileName(initialPath), sha256 = Hash(initialSvg) },
        new { file = Path.GetFileName(movedPath), sha256 = Hash(movedSvg) }
    }
};
var manifestPath = Path.Combine(output, "environment-scene-manifest.json");
File.WriteAllText(manifestPath, JsonSerializer.Serialize(manifest, new JsonSerializerOptions { WriteIndented = true }) + Environment.NewLine);
Console.WriteLine($"Generated {initialPath}");
Console.WriteLine($"Generated {movedPath}");
Console.WriteLine($"Generated {manifestPath}");

(ulong Id, VisualizerMaterial Value) Material(ulong id, Vector4 color) => (id, new VisualizerMaterial(1, color));
VisualizerDrawCommand Command((int First, int Count) range, ulong material, ulong entity, Matrix4x4? transform = null) =>
    new(geometryId, new VisualizerResourceId(material), transform ?? Matrix4x4.Identity,
        range.Count, range.First, 0, VisualizerRepresentation.Near, entity: new VisualizerEntityId(entity));
static VisualizerView DefaultView() => new(Vector3.Zero, Matrix4x4.Identity, Matrix4x4.Identity);
static string Hash(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();

sealed class MeshBuilder
{
    private readonly List<VisualizerVertex> _vertices = [];
    private readonly List<uint> _indices = [];

    public (int First, int Count) Rectangle(float left, float bottom, float right, float top)
    {
        var first = _indices.Count;
        var a = Add(left, bottom); var b = Add(right, bottom);
        var c = Add(right, top); var d = Add(left, top);
        Triangle(a, b, c); Triangle(a, c, d);
        return (first, _indices.Count - first);
    }

    public (int First, int Count) Circle(float x, float y, float radius, int segments)
    {
        var first = _indices.Count;
        var center = Add(x, y);
        var ring = new int[segments];
        for (var i = 0; i < segments; i++)
        {
            var angle = 2 * MathF.PI * i / segments;
            ring[i] = Add(x + MathF.Cos(angle) * radius, y + MathF.Sin(angle) * radius);
        }
        for (var i = 0; i < segments; i++)
            Triangle(center, ring[i], ring[(i + 1) % segments]);
        return (first, _indices.Count - first);
    }

    public VisualizerGeometry Build() => new(1, _vertices, _indices);

    private int Add(float x, float y)
    {
        _vertices.Add(new VisualizerVertex(new Vector3(x, y, 0), Vector3.UnitZ, Vector2.Zero, Vector4.One));
        return _vertices.Count - 1;
    }

    private void Triangle(int a, int b, int c)
    {
        _indices.Add((uint)a); _indices.Add((uint)b); _indices.Add((uint)c);
    }
}
