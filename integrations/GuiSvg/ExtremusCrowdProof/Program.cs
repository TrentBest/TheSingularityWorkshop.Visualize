using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using TheSingularityWorkshop.Visualize;
using TheSingularityWorkshop.Visualize.GuiSvg;

var outputDirectory = args.Length > 0
    ? Path.GetFullPath(args[0])
    : Path.GetFullPath("artifacts/extremus-crowd");
Directory.CreateDirectory(outputDirectory);

const ulong geometryRevision = 3;
const ulong materialRevision = 2;
const ulong instanceRevision = 7;
const ulong geometryIdValue = 1;
const ulong materialIdValue = 2;
const ulong instancesIdValue = 3;
const int columns = 6;
const int rows = 4;
const int populationCount = columns * rows;

var catalog = new VisualizerResourceCatalog();
var geometryId = new VisualizerResourceId(geometryIdValue);
var materialId = new VisualizerResourceId(materialIdValue);
var instancesId = new VisualizerResourceId(instancesIdValue);

catalog.SetGeometry(geometryId, new VisualizerGeometry(geometryRevision,
[
    new VisualizerVertex(new Vector3(-0.035f, -0.045f, 0), Vector3.UnitZ, Vector2.Zero, Vector4.One),
    new VisualizerVertex(new Vector3(0.035f, -0.045f, 0), Vector3.UnitZ, Vector2.Zero, Vector4.One),
    new VisualizerVertex(new Vector3(0, 0.055f, 0), Vector3.UnitZ, Vector2.Zero, Vector4.One)
], [0, 1, 2]));
catalog.SetMaterial(materialId, new VisualizerMaterial(materialRevision, Vector4.One));

var records = new List<CrowdRecord>(populationCount);
var instances = new List<VisualizerInstance>(populationCount);
for (var row = 0; row < rows; row++)
{
    for (var column = 0; column < columns; column++)
    {
        var ordinal = row * columns + column;
        var stableId = $"extremus-fixture-{ordinal + 1:D3}";
        var x = -0.375f + column * 0.15f + (row % 2 == 0 ? 0.025f : -0.025f);
        var y = 0.18f - row * 0.12f;
        var scale = 0.82f + (ordinal % 4) * 0.08f;
        var heading = (ordinal % 7 - 3) * 0.055f;
        var tint = TintFor(ordinal);
        var transform = Matrix4x4.CreateScale(scale) *
            Matrix4x4.CreateRotationZ(heading) *
            Matrix4x4.CreateTranslation(x, y, 0);

        records.Add(new CrowdRecord(stableId, x, y, scale, heading));
        instances.Add(new VisualizerInstance(transform, tint));
    }
}

catalog.SetInstances(instancesId, new VisualizerInstanceBuffer(instanceRevision, instances));
var command = new VisualizerDrawCommand(
    geometryId,
    materialId,
    Matrix4x4.Identity,
    indexCount: 3,
    firstIndex: 0,
    baseVertex: 0,
    representation: VisualizerRepresentation.Distant,
    instances: instancesId,
    instanceCount: populationCount,
    firstInstance: 0);
var frame = new VisualizerFrame(
    new VisualizerView(Vector3.Zero, Matrix4x4.Identity, Matrix4x4.Identity),
    [command]);

// Two independent backend instances should produce byte-for-byte identical output for the same snapshot.
var svg = await RenderToStringAsync(frame, catalog);
var repeatedSvg = await RenderToStringAsync(frame, catalog);
if (!string.Equals(svg, repeatedSvg, StringComparison.Ordinal))
    throw new InvalidOperationException("Identical crowd inputs produced different SVG output.");

await VerifyMissingInstanceResourceIsRejectedAsync(frame, catalog, geometryId, materialId, instancesId);

var svgPath = Path.Combine(outputDirectory, "extremus-crowd.svg");
var manifestPath = Path.Combine(outputDirectory, "extremus-crowd-manifest.json");
await File.WriteAllTextAsync(svgPath, svg, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

var svgBytes = Encoding.UTF8.GetByteCount(svg);
var report = new
{
    schemaVersion = 1,
    proof = "deterministic-hand-authored-crowd-instance-cpu-svg",
    source = new
    {
        kind = "hand-authored-fixture",
        consumesGesturesBake = false,
        seed = 1729,
        algorithmVersion = "fixture-v1"
    },
    backend = new
    {
        name = "GUI.SVG Visualizer backend",
        execution = "CPU",
        hardwareGpuInstancing = false
    },
    counts = new
    {
        representedPopulation = populationCount,
        instanceRecords = instances.Count,
        drawCommands = frame.Commands.Count,
        submittedDrawCommands = 1,
        expectedSvgTrianglePaths = populationCount,
        geometryVertices = 3,
        geometryIndices = 3
    },
    resources = new
    {
        geometryId = geometryIdValue,
        geometryRevision,
        materialId = materialIdValue,
        materialRevision,
        instancesId = instancesIdValue,
        instanceRevision
    },
    repeatability = new
    {
        identicalInputProducedIdenticalSvg = true,
        missingInstanceResourceRejected = true
    },
    output = new
    {
        svgFile = Path.GetFileName(svgPath),
        svgBytes,
        svgSha256 = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(svg))).ToLowerInvariant()
    },
    records
};
var json = JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
await File.WriteAllTextAsync(manifestPath, json + Environment.NewLine, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

Console.WriteLine($"Crowd SVG: {svgPath}");
Console.WriteLine($"Manifest: {manifestPath}");
Console.WriteLine($"Population={populationCount}; instances={instances.Count}; drawCommands={frame.Commands.Count}; SVG bytes={svgBytes}");
Console.WriteLine("Repeatability and missing-instance-resource assertions passed.");

static Vector4 TintFor(int ordinal)
{
    var palette = new[]
    {
        new Vector4(0.35f, 0.82f, 0.96f, 1),
        new Vector4(0.48f, 0.91f, 0.72f, 1),
        new Vector4(1.00f, 0.69f, 0.39f, 1),
        new Vector4(0.76f, 0.62f, 1.00f, 1)
    };
    return palette[ordinal % palette.Length];
}

static async Task<string> RenderToStringAsync(VisualizerFrame frame, VisualizerResourceCatalog catalog)
{
    string? capturedSvg = null;
    var backend = new GuiSvgVisualizerBackend(
        svg => capturedSvg = svg,
        backgroundColor: "#08131f",
        gridColor: "#203447",
        sceneLabel: "Extremus deterministic crowd layout / CPU SVG proof",
        drawGrid: false);
    await using var visualizer = new Visualizer(backend, catalog);
    var result = await visualizer.RenderAsync(frame);
    if (result.SubmittedDrawCount != 1)
        throw new InvalidOperationException($"Expected one submitted instance-range command; got {result.SubmittedDrawCount}.");
    return capturedSvg ?? throw new InvalidOperationException("GUI.SVG backend did not emit output.");
}

static async Task VerifyMissingInstanceResourceIsRejectedAsync(
    VisualizerFrame sourceFrame,
    VisualizerResourceCatalog sourceCatalog,
    VisualizerResourceId geometryId,
    VisualizerResourceId materialId,
    VisualizerResourceId instancesId)
{
    var incompleteCatalog = new VisualizerResourceCatalog();
    if (!sourceCatalog.TryGetGeometry(geometryId, out var geometry) || geometry is null ||
        !sourceCatalog.TryGetMaterial(materialId, out var material) || material is null)
        throw new InvalidOperationException("Fixture setup failed to resolve base geometry/material.");
    incompleteCatalog.SetGeometry(geometryId, geometry);
    incompleteCatalog.SetMaterial(materialId, material);

    var source = sourceFrame.Commands[0];
    var missingResourceCommand = new VisualizerDrawCommand(
        source.Geometry, source.Material, source.WorldTransform, source.IndexCount,
        source.FirstIndex, source.BaseVertex, source.Representation, instancesId,
        source.InstanceCount, source.FirstInstance);
    var frame = new VisualizerFrame(sourceFrame.View, [missingResourceCommand]);
    var backend = new GuiSvgVisualizerBackend(_ => { }, drawGrid: false);
    await using var visualizer = new Visualizer(backend, incompleteCatalog);
    try
    {
        await visualizer.RenderAsync(frame);
    }
    catch (InvalidOperationException exception) when (exception.Message.Contains("missing instance resource", StringComparison.OrdinalIgnoreCase))
    {
        return;
    }

    throw new InvalidOperationException("Visualizer did not reject the missing instance resource.");
}

internal sealed record CrowdRecord(string StableId, float X, float Y, float Scale, float Heading);
