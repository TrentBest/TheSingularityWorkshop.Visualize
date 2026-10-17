using System.Numerics;
using TheSingularityWorkshop.Visualize;
using TheSingularityWorkshop.Visualize.GuiSvg;\nusing TheSingularityWorkshop.Renderer;\nusing TheSingularityWorkshop.Renderer.VisualizeAdapter;

var outputDirectory = args.Length > 0 ? Path.GetFullPath(args[0]) : Path.GetFullPath("artifacts/city-massing");
Directory.CreateDirectory(outputDirectory);

var scene = CityMassing.Build();
var skins = new[]
{
    new CitySkin("noir", "#050a12", "#153447", "#00eaff", "#ff3fd2", "#d7eaff", "#9beeff", "#7d9cab"),
    new CitySkin("metro", "#101821", "#364a53", "#7f9ca4", "#d6a86e", "#d9e0d9", "#283944", "#f4bb68"),
    new CitySkin("harmony", "#edf4e9", "#bdd6c7", "#5d9e7c", "#d28b46", "#b7d9c0", "#2f6a56", "#1d8065")
};

foreach (var skin in skins)
{
    var catalog = new VisualizerResourceCatalog();
    var geometryId = new VisualizerResourceId(1);
    var materialIds = new Dictionary<string, VisualizerResourceId>
    {
        ["road"] = new(2), ["left"] = new(3), ["right"] = new(4),
        ["roof"] = new(5), ["window"] = new(6), ["crown"] = new(7)
    };
    catalog.SetGeometry(geometryId, new VisualizerGeometry(1, scene.Vertices, scene.Indices));
    foreach (var (slot, id) in materialIds)
        catalog.SetMaterial(id, new VisualizerMaterial(1, CityMassing.Color(skin.ColorFor(slot))));

    // This is the first city proof that asks Renderer for a representation decision per face.
    // Distances use the proof scene's normalized world units; these thresholds are fixture policy,
    // not production city-scale thresholds.
    var policy = new ObserverRepresentationPolicy(
    [
        new RepresentationHorizonRule(new EventHorizon("Interactive", 0.035, 10), RepresentationTarget.Interactive),
        new RepresentationHorizonRule(new EventHorizon("Near", 0.12, 8), RepresentationTarget.Near),
        new RepresentationHorizonRule(new EventHorizon("Distant", 0.30, 4), RepresentationTarget.Distant),
        new RepresentationHorizonRule(new EventHorizon("Aggregate", 0.65, 2), RepresentationTarget.Aggregate),
        new RepresentationHorizonRule(new EventHorizon("Semantic", 2.0, 1), RepresentationTarget.Semantic)
    ]);

    var observer = new Vector3(0, 0, 0.04f);
    var renderCommands = scene.Faces.Select(face =>
    {
        var center = FaceCenter(scene, face);
        var decision = policy.Select(Vector3.Distance(observer, center));
        return new RenderDrawCommand(
            new RenderResourceId(geometryId.Value),
            new RenderResourceId(materialIds[face.MaterialSlot].Value),
            Matrix4x4.Identity,
            face.IndexCount,
            face.FirstIndex,
            0,
            ToRendererRepresentation(decision.Representation));
    }).ToArray();

    var renderFrame = new RenderFrame(
        new RenderView(observer, Matrix4x4.Identity, Matrix4x4.Identity),
        renderCommands);
    var frame = new RendererFrameAdapter().Adapt(renderFrame);

    var output = Path.Combine(outputDirectory, $"singularity-city-{skin.Name}.svg");
    await using var visualizer = new Visualizer(
        new GuiSvgVisualizerBackend(
            output,
            backgroundColor: skin.Background,
            gridColor: skin.Grid,
            sceneLabel: $"Singularity City / {skin.Name.ToUpperInvariant()} / Renderer policy → Visualize → GUI.SVG",
            drawGrid: false),
        catalog);
    var result = await visualizer.RenderAsync(frame);
    var counts = renderCommands.GroupBy(command => command.Representation)
        .OrderBy(group => group.Key)
        .Select(group => $"{group.Key}={group.Count()}");
    Console.WriteLine($"{skin.Name}: {scene.BuildingCount} buildings, {result.SubmittedDrawCount} draw commands; {string.Join(", ", counts)} -> {output}");
}

static Vector3 FaceCenter(CityScene scene, CityFace face)
{
    var sum = Vector3.Zero;
    var count = 0;
    for (var i = face.FirstIndex; i < face.FirstIndex + face.IndexCount; i++)
    {
        var vertexIndex = scene.Indices[i];
        sum += scene.Vertices[(int)vertexIndex].Position;
        count++;
    }

    return count == 0 ? Vector3.Zero : sum / count;
}

static RendererRepresentation ToRendererRepresentation(RepresentationTarget target) => target switch
{
    RepresentationTarget.Semantic => RendererRepresentation.Semantic,
    RepresentationTarget.Aggregate => RendererRepresentation.Aggregate,
    RepresentationTarget.Distant => RendererRepresentation.Distant,
    RepresentationTarget.Near => RendererRepresentation.Near,
    RepresentationTarget.Interactive => RendererRepresentation.Interactive,
    _ => throw new ArgumentOutOfRangeException(nameof(target), target, "Unknown representation target.")
};

internal sealed record CityFace(int FirstIndex, int IndexCount, string MaterialSlot);
internal sealed record CityScene(IReadOnlyList<VisualizerVertex> Vertices, IReadOnlyList<uint> Indices, IReadOnlyList<CityFace> Faces, int BuildingCount);
internal sealed record CitySkin(string Name, string Background, string Grid, string Left, string Right, string Roof, string Window, string Road)
{
    public string ColorFor(string slot) => slot switch
    {
        "left" => Left, "right" => Right, "roof" => Roof, "window" => Window,
        "crown" => Roof, _ => Road
    };
}

internal sealed class CityMassing
{
    private readonly List<VisualizerVertex> _vertices = [];
    private readonly List<uint> _indices = [];
    private readonly List<CityFace> _faces = [];
    private int _buildings;

    public IReadOnlyList<VisualizerVertex> Vertices => _vertices;
    public IReadOnlyList<uint> Indices => _indices;
    public IReadOnlyList<CityFace> Faces => _faces;
    public int BuildingCount => _buildings;

    public static CityScene Build()
    {
        var city = new CityMassing();
        city.AddRoadNetwork();
        city.AddBuildings();
        return new CityScene(city.Vertices, city.Indices, city.Faces, city.BuildingCount);
    }

    public static Vector4 Color(string hex)
    {
        var value = hex.TrimStart('#');
        return new Vector4(
            Convert.ToInt32(value[..2], 16) / 255f,
            Convert.ToInt32(value.Substring(2, 2), 16) / 255f,
            Convert.ToInt32(value.Substring(4, 2), 16) / 255f,
            1f);
    }

    private void AddRoadNetwork()
    {
        for (var lane = -14; lane <= 14; lane += 2)
        {
            var offset = lane * 1.45f;
            AddStrip(50 + offset, 3, 50 + offset - 42, 53, .075f, "road");
            AddStrip(50 + offset, 3, 50 + offset + 42, 53, .075f, "road");
        }
    }

    private void AddBuildings()
    {
        for (var row = 0; row < 18; row++)
        for (var col = 0; col < 22; col++)
        {
            var hash = (col * 73 + row * 151 + col * row * 17) % 101;
            if (hash < 13) continue;
            var count = 1 + (hash % 4 == 0 ? 2 : hash % 3 == 0 ? 1 : 0);
            for (var local = 0; local < count; local++)
            {
                var localHash = (col * 97 + row * 61 + local * 43 + col * row * 7) % 997;
                var x = 50 + (col - row) * 1.45f + (local - 1) * .4f;
                var y = 4 + (col + row) * 1.22f + (local % 2) * .18f;
                var floors = 3 + localHash % 58;
                var landmark = (col == 10 && row == 8 && local == 0) ||
                    (col == 14 && row == 5 && local == 0) ||
                    (col == 5 && row == 13 && local == 0);
                if (landmark) floors = 75 + localHash % 45;
                var height = floors * .035f;
                const float width = .52f;
                const float depth = .28f;
                var roof = new[]
                {
                    new Vector2(x - width, y - height),
                    new Vector2(x, y - height - depth),
                    new Vector2(x + width, y - height),
                    new Vector2(x, y - height + depth)
                };
                var left = new[]
                {
                    new Vector2(x - width, y - height), new Vector2(x, y - height + depth),
                    new Vector2(x, y + depth), new Vector2(x - width, y)
                };
                var right = new[]
                {
                    new Vector2(x + width, y - height), new Vector2(x, y - height + depth),
                    new Vector2(x, y + depth), new Vector2(x + width, y)
                };

                AddQuad(left, "left");
                AddQuad(right, "right");
                AddQuad(roof, "roof");
                AddStrip(x - width * .48f, y - height + .12f, x - width * .48f, y - .05f, .035f, "window");
                AddStrip(x + width * .45f, y - height + .12f, x + width * .45f, y - .05f, .035f, "window");
                if (landmark)
                {
                    AddTriangle(
                        new Vector2(x - width * .45f, y - height),
                        new Vector2(x, y - height - depth - .42f),
                        new Vector2(x + width * .45f, y - height),
                        "crown");
                }
                _buildings++;
            }
        }
    }

    private void AddStrip(float x1, float y1, float x2, float y2, float width, string material)
    {
        var dx = x2 - x1;
        var dy = y2 - y1;
        var length = MathF.Sqrt(dx * dx + dy * dy);
        if (length < 1e-5f) return;
        var px = -dy / length * width;
        var py = dx / length * width;
        AddQuad(
        [
            new Vector2(x1 + px, y1 + py), new Vector2(x2 + px, y2 + py),
            new Vector2(x2 - px, y2 - py), new Vector2(x1 - px, y1 - py)
        ], material);
    }

    private void AddQuad(IReadOnlyList<Vector2> points, string material)
    {
        var firstIndex = _indices.Count;
        var firstVertex = (uint)_vertices.Count;
        foreach (var point in points)
            _vertices.Add(new VisualizerVertex(ToWorld(point), Vector3.UnitZ, Vector2.Zero, Vector4.One));
        _indices.Add(firstVertex);
        _indices.Add(firstVertex + 1);
        _indices.Add(firstVertex + 2);
        _indices.Add(firstVertex);
        _indices.Add(firstVertex + 2);
        _indices.Add(firstVertex + 3);
        _faces.Add(new CityFace(firstIndex, 6, material));
    }

    private void AddTriangle(Vector2 a, Vector2 b, Vector2 c, string material)
    {
        var firstIndex = _indices.Count;
        var firstVertex = (uint)_vertices.Count;
        foreach (var point in new[] { a, b, c })
            _vertices.Add(new VisualizerVertex(ToWorld(point), Vector3.UnitZ, Vector2.Zero, Vector4.One));
        _indices.Add(firstVertex);
        _indices.Add(firstVertex + 1);
        _indices.Add(firstVertex + 2);
        _faces.Add(new CityFace(firstIndex, 3, material));
    }

    private static Vector3 ToWorld(Vector2 svgPoint) =>
        new((svgPoint.X - 50) / 100f, (28.125f - svgPoint.Y) / 100f, 0);
}
