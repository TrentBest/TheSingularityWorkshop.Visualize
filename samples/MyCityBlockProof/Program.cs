using System.Numerics;
using System.Text.Json;
using TheSingularityWorkshop.Visualize;

var output = args.Length > 0
    ? Path.GetFullPath(args[0])
    : Path.GetFullPath("artifacts/mycity-block");
Directory.CreateDirectory(output);

var geometryId = new VisualizerResourceId(1);
var districtMaterial = new VisualizerResourceId(2);
var parcelMaterial = new VisualizerResourceId(3);
var buildingMaterial = new VisualizerResourceId(4);
var roofMaterial = new VisualizerResourceId(5);

var mesh = new CityMeshBuilder();
var districtRanges = new Dictionary<ulong, (int First, int Count)>
{
    [101] = mesh.Rectangle(-0.94f, -0.72f, -0.04f, 0.72f),
    [102] = mesh.Rectangle(0.04f, -0.72f, 0.94f, 0.72f)
};
var parcelRanges = new Dictionary<ulong, (int First, int Count)>
{
    [201] = mesh.Rectangle(-0.86f, -0.62f, -0.48f, -0.12f),
    [202] = mesh.Rectangle(-0.42f, -0.62f, -0.08f, -0.12f),
    [203] = mesh.Rectangle(-0.86f, 0.10f, -0.48f, 0.62f),
    [204] = mesh.Rectangle(-0.42f, 0.10f, -0.08f, 0.62f),
    [205] = mesh.Rectangle(0.08f, -0.62f, 0.42f, -0.12f),
    [206] = mesh.Rectangle(0.48f, -0.62f, 0.86f, -0.12f),
    [207] = mesh.Rectangle(0.08f, 0.10f, 0.42f, 0.62f),
    [208] = mesh.Rectangle(0.48f, 0.10f, 0.86f, 0.62f)
};
var buildingRanges = new Dictionary<ulong, ((int First, int Count) Body, (int First, int Count) Roof)>
{
    [301] = (mesh.Rectangle(-0.80f, -0.48f, -0.54f, -0.20f), mesh.Triangle(-0.82f, -0.48f, -0.67f, -0.08f, -0.52f, -0.48f)),
    [302] = (mesh.Rectangle(-0.34f, 0.22f, -0.16f, 0.52f), mesh.Triangle(-0.36f, 0.22f, -0.25f, 0.64f, -0.14f, 0.22f)),
    [303] = (mesh.Rectangle(0.55f, -0.48f, 0.79f, -0.18f), mesh.Triangle(0.53f, -0.48f, 0.67f, -0.06f, 0.81f, -0.48f)),
    [304] = (mesh.Rectangle(0.16f, 0.22f, 0.34f, 0.50f), mesh.Triangle(0.14f, 0.22f, 0.25f, 0.62f, 0.36f, 0.22f))
};

var resources = new VisualizerResourceCatalog();
resources.SetGeometry(geometryId, mesh.Build());
resources.SetMaterial(districtMaterial, new VisualizerMaterial(1, new Vector4(0.18f, 0.32f, 0.40f, 1)));
resources.SetMaterial(parcelMaterial, new VisualizerMaterial(1, new Vector4(0.34f, 0.48f, 0.48f, 1)));
resources.SetMaterial(buildingMaterial, new VisualizerMaterial(1, new Vector4(0.82f, 0.62f, 0.36f, 1)));
resources.SetMaterial(roofMaterial, new VisualizerMaterial(1, new Vector4(0.92f, 0.76f, 0.48f, 1)));

var sceneIdentity = "workshop://mycity/block/alpha";
var near = CreateSnapshot(1, includeRoofs: true);
var distant = CreateSnapshot(2, includeRoofs: false);
var view = new VisualizerView(Vector3.Zero, Matrix4x4.Identity, Matrix4x4.Identity);
var nearFrame = near.CreateFrame(view);
var distantFrame = distant.CreateFrame(view);

var backend = new SvgVisualizerBackend(_ => { }, width: 1200, height: 760, background: "#101923");
var nearSvg = backend.BuildSvg(nearFrame, resources);
var repeatedNearSvg = backend.BuildSvg(near.CreateFrame(view), resources);
var distantSvg = backend.BuildSvg(distantFrame, resources);

if (!StringComparer.Ordinal.Equals(nearSvg, repeatedNearSvg))
    throw new InvalidOperationException("Identical city-block input did not produce deterministic SVG output.");
if (StringComparer.Ordinal.Equals(nearSvg, distantSvg))
    throw new InvalidOperationException("The representation change did not change the manifested output.");
if (near.SceneIdentity != distant.SceneIdentity || near.Revision != 1 || distant.Revision != 2)
    throw new InvalidOperationException("City-block identity/revision continuity failed.");

var expectedIds = near.Objects.Select(item => item.Entity.Value).Order().ToArray();
foreach (var entityId in expectedIds)
{
    var marker = $"data-entity-id=\"{entityId}\"";
    if (!nearSvg.Contains(marker, StringComparison.Ordinal) || !distantSvg.Contains(marker, StringComparison.Ordinal))
        throw new InvalidOperationException($"Stable semantic entity {entityId} was lost across revisions.");
}

var nearPath = Path.Combine(output, "mycity-block.svg");
var distantPath = Path.Combine(output, "mycity-block-distant.svg");
var manifestPath = Path.Combine(output, "mycity-block-manifest.json");
await File.WriteAllTextAsync(nearPath, nearSvg);
await File.WriteAllTextAsync(distantPath, distantSvg);

var manifest = new
{
    schemaVersion = 1,
    fixture = "mycity-block",
    fixtureKind = "domain-neutral-presentation-proof",
    sceneIdentity,
    revisions = new[] { near.Revision, distant.Revision },
    districtIds = new ulong[] { 101, 102 },
    parcelIds = new ulong[] { 201, 202, 203, 204, 205, 206, 207, 208 },
    buildingIds = new ulong[] { 301, 302, 303, 304 },
    stableEntityIds = expectedIds,
    nearCommandCount = nearFrame.Commands.Count,
    distantCommandCount = distantFrame.Commands.Count,
    deterministicRepeat = true,
    representationChangePreservesIdentity = true,
    backend = "SvgVisualizerBackend",
    execution = "CPU/static SVG",
    gpuExecution = false,
    artifacts = new[] { Path.GetFileName(nearPath), Path.GetFileName(distantPath) }
};
await File.WriteAllTextAsync(manifestPath, JsonSerializer.Serialize(manifest, new JsonSerializerOptions { WriteIndented = true }) + Environment.NewLine);

Console.WriteLine($"Generated {nearPath}");
Console.WriteLine($"Generated {distantPath}");
Console.WriteLine($"Generated {manifestPath}");
Console.WriteLine($"Stable entities: {expectedIds.Length}; near draws: {nearFrame.Commands.Count}; distant draws: {distantFrame.Commands.Count}");

VisualizerSceneSnapshot CreateSnapshot(ulong revision, bool includeRoofs)
{
    var objects = new List<VisualizerSceneObject>();
    foreach (var (id, range) in districtRanges.OrderBy(item => item.Key))
        objects.Add(SceneObject(id, 0, [Draw(range, districtMaterial, id, VisualizerRepresentation.Aggregate)]));
    foreach (var (id, range) in parcelRanges.OrderBy(item => item.Key))
        objects.Add(SceneObject(id, 1, [Draw(range, parcelMaterial, id, VisualizerRepresentation.Distant)]));
    foreach (var (id, ranges) in buildingRanges.OrderBy(item => item.Key))
    {
        var commands = new List<VisualizerDrawCommand>
        {
            Draw(ranges.Body, buildingMaterial, id, includeRoofs ? VisualizerRepresentation.Near : VisualizerRepresentation.Distant)
        };
        if (includeRoofs)
            commands.Add(Draw(ranges.Roof, roofMaterial, id, VisualizerRepresentation.Near));
        objects.Add(SceneObject(id, 2, commands));
    }

    return new VisualizerSceneSnapshot(sceneIdentity, revision, objects);
}

VisualizerSceneObject SceneObject(ulong id, int layer, IEnumerable<VisualizerDrawCommand> commands) =>
    new(new VisualizerEntityId(id), layer, true, commands);

VisualizerDrawCommand Draw(
    (int First, int Count) range,
    VisualizerResourceId material,
    ulong entity,
    VisualizerRepresentation representation) =>
    new(geometryId, material, Matrix4x4.Identity, range.Count, range.First, 0,
        representation, entity: new VisualizerEntityId(entity));

sealed class CityMeshBuilder
{
    private readonly List<VisualizerVertex> _vertices = [];
    private readonly List<uint> _indices = [];

    public (int First, int Count) Rectangle(float left, float bottom, float right, float top)
    {
        var first = _indices.Count;
        var a = Add(left, bottom);
        var b = Add(right, bottom);
        var c = Add(right, top);
        var d = Add(left, top);
        Triangle(a, b, c);
        Triangle(a, c, d);
        return (first, _indices.Count - first);
    }

    public (int First, int Count) Triangle(float ax, float ay, float bx, float by, float cx, float cy)
    {
        var first = _indices.Count;
        Triangle(Add(ax, ay), Add(bx, by), Add(cx, cy));
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
        _indices.Add((uint)a);
        _indices.Add((uint)b);
        _indices.Add((uint)c);
    }
}
