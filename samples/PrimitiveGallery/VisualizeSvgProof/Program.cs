using System.Diagnostics;
using System.Globalization;
using System.Numerics;
using System.Text;
using TheSingularityWorkshop.Visualize;

var output = args.Length > 0 ? Path.GetFullPath(args[0]) : Path.GetFullPath("generated");
Directory.CreateDirectory(output);
var examples = new[]
{
    BuildPointExample(Path.Combine(output, "01-one-point.svg"), 1, false),
    BuildPointExample(Path.Combine(output, "02-point-cloud-100.svg"), 100, true),
    BuildConnectedExample(Path.Combine(output, "03-four-connected-points.svg")),
    BuildLineExample(Path.Combine(output, "04-one-line.svg"), 1),
    BuildLineExample(Path.Combine(output, "05-many-lines-24.svg"), 24)
};

foreach (var example in examples)
{
    await using var visualizer = new Visualizer(new SvgProofBackend(example.Path, example.Title), example.Resources);
    var result = await visualizer.RenderAsync(example.Frame);
    Console.WriteLine($"{example.Title}: Visualizer submitted {result.SubmittedDrawCount} draw command(s) -> {example.Path}");
}

static Example BuildPointExample(string path, int count, bool instanced)
{
    var catalog = NewCatalog();
    var geometry = new VisualizerResourceId(1);
    var material = new VisualizerResourceId(2);
    var instanceId = new VisualizerResourceId(3);
    catalog.SetGeometry(geometry, PointMesh());
    catalog.SetMaterial(material, new VisualizerMaterial(1, Vector4.One));
    VisualizerDrawCommand command;
    if (instanced)
    {
        var instances = Enumerable.Range(0, count).Select(i =>
        {
            var x = -0.42f + (i % 10) * 0.093f;
            var y = 0.36f - (i / 10) * 0.073f;
            var tint = new Vector4(0.25f + (i % 5) * 0.14f, 0.78f, 0.95f - (i % 4) * 0.13f, 1);
            return new VisualizerInstance(Matrix4x4.CreateTranslation(x, y, 0), tint);
        }).ToArray();
        catalog.SetInstances(instanceId, new VisualizerInstanceBuffer(1, instances));
        command = new VisualizerDrawCommand(geometry, material, Matrix4x4.Identity, 6, 0, 0,
            VisualizerRepresentation.Near, instanceId, count, 0);
    }
    else
    {
        command = new VisualizerDrawCommand(geometry, material, Matrix4x4.Identity, 6, 0, 0, VisualizerRepresentation.Near);
    }
    return new Example(path, instanced ? $"Point cloud · {count} instances" : "One point",
        catalog, new VisualizerFrame(DefaultView(), [command]));
}

static Example BuildConnectedExample(string path)
{
    var catalog = NewCatalog();
    var pointId = new VisualizerResourceId(1);
    var lineId = new VisualizerResourceId(2);
    var materialId = new VisualizerResourceId(3);
    catalog.SetGeometry(pointId, PointMesh());
    catalog.SetGeometry(lineId, LineMesh());
    catalog.SetMaterial(materialId, new VisualizerMaterial(1, Vector4.One));
    var points = new[] { new Vector2(-0.3f, -0.2f), new Vector2(0.3f, -0.2f), new Vector2(0.3f, 0.2f), new Vector2(-0.3f, 0.2f) };
    var commands = new List<VisualizerDrawCommand>();
    foreach (var point in points)
        commands.Add(new VisualizerDrawCommand(pointId, materialId, Matrix4x4.CreateTranslation(point.X, point.Y, 0), 6, 0, 0, VisualizerRepresentation.Near));
    for (var i = 0; i < points.Length; i++)
    {
        var a = points[i]; var b = points[(i + 1) % points.Length]; var delta = b - a;
        var transform = Matrix4x4.CreateScale(delta.Length(), 1, 1) *
            Matrix4x4.CreateRotationZ(MathF.Atan2(delta.Y, delta.X)) *
            Matrix4x4.CreateTranslation((a.X + b.X) / 2, (a.Y + b.Y) / 2, 0);
        commands.Add(new VisualizerDrawCommand(lineId, materialId, transform, 6, 0, 0, VisualizerRepresentation.Near));
    }
    return new Example(path, "Four connected points", catalog, new VisualizerFrame(DefaultView(), commands));
}

static Example BuildLineExample(string path, int count)
{
    var catalog = NewCatalog();
    var geometry = new VisualizerResourceId(1);
    var material = new VisualizerResourceId(2);
    var instancesId = new VisualizerResourceId(3);
    catalog.SetGeometry(geometry, LineMesh());
    catalog.SetMaterial(material, new VisualizerMaterial(1, Vector4.One));
    VisualizerDrawCommand command;
    if (count == 1)
    {
        command = new VisualizerDrawCommand(geometry, material, Matrix4x4.Identity, 6, 0, 0, VisualizerRepresentation.Near);
    }
    else
    {
        var instances = Enumerable.Range(0, count).Select(i =>
        {
            var x = -0.42f + (i % 6) * 0.17f;
            var y = 0.32f - (i / 6) * 0.16f;
            var angle = (i % 6 - 2.5f) * 0.16f;
            return new VisualizerInstance(Matrix4x4.CreateRotationZ(angle) * Matrix4x4.CreateTranslation(x, y, 0),
                new Vector4(0.48f, 0.76f + (i % 3) * 0.08f, 1, 1));
        }).ToArray();
        catalog.SetInstances(instancesId, new VisualizerInstanceBuffer(1, instances));
        command = new VisualizerDrawCommand(geometry, material, Matrix4x4.Identity, 6, 0, 0,
            VisualizerRepresentation.Near, instancesId, count, 0);
    }
    return new Example(path, count == 1 ? "One line" : $"Many lines · {count} instances",
        catalog, new VisualizerFrame(DefaultView(), [command]));
}

static VisualizerResourceCatalog NewCatalog() => new();
static VisualizerView DefaultView() => new(Vector3.Zero, Matrix4x4.Identity, Matrix4x4.Identity);

static VisualizerGeometry PointMesh() => new(1,
[
    Vertex(-0.012f, 0), Vertex(0, 0.018f), Vertex(0.012f, 0), Vertex(0, -0.018f)
], [0, 1, 2, 0, 2, 3]);

static VisualizerGeometry LineMesh() => new(1,
[
    Vertex(-0.5f, -0.0025f), Vertex(-0.5f, 0.0025f),
    Vertex(0.5f, 0.0025f), Vertex(0.5f, -0.0025f)
], [0, 1, 2, 0, 2, 3]);

static VisualizerVertex Vertex(float x, float y) =>
    new(new Vector3(x, y, 0), Vector3.UnitZ, Vector2.Zero, Vector4.One);

sealed record Example(string Path, string Title, VisualizerResourceCatalog Resources, VisualizerFrame Frame);

sealed class SvgProofBackend(string path, string title) : IVisualizerBackend
{
    private long _frame;
    public VisualizerCapabilities Capabilities { get; } = new("SVG proof backend", "SVG", false, true, false, false);

    public ValueTask<VisualizerFrameResult> RenderAsync(VisualizerFrame frame, IVisualizerResourceProvider resources, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var stopwatch = Stopwatch.StartNew();
        var sb = new StringBuilder();
        sb.Append("<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 320 180\" role=\"img\" aria-label=\"")
            .Append(Escape(title)).Append("\"><rect width=\"320\" height=\"180\" rx=\"8\" fill=\"#0b1725\"/>");
        for (var x = 20; x < 320; x += 20) sb.Append("<path d=\"M").Append(x).Append(" 0V180\" stroke=\"#203447\" stroke-width=\".5\"/>");
        for (var y = 20; y < 180; y += 20) sb.Append("<path d=\"M0 ").Append(y).Append("H320\" stroke=\"#203447\" stroke-width=\".5\"/>");
        foreach (var command in frame.Commands)
        {
            if (!resources.TryGetGeometry(command.Geometry, out var geometry) || geometry is null ||
                !resources.TryGetMaterial(command.Material, out var material) || material is null)
                throw new InvalidOperationException("Visualizer submitted unresolved resources.");
            VisualizerInstanceBuffer? buffer = null;
            if (command.Instances.IsValid && !resources.TryGetInstances(command.Instances, out buffer))
                throw new InvalidOperationException("Visualizer submitted an unresolved instance buffer.");
            var instanceCount = command.Instances.IsValid ? command.InstanceCount : 1;
            for (var i = 0; i < instanceCount; i++)
            {
                var transform = command.WorldTransform;
                var tint = material!.BaseColor;
                if (buffer is not null)
                {
                    var instance = buffer.Instances[command.FirstInstance + i];
                    transform *= instance.WorldTransform;
                    tint *= instance.Color;
                }
                for (var index = command.FirstIndex; index < command.FirstIndex + command.IndexCount; index += 3)
                {
                    var a = Vector3.Transform(geometry!.Vertices[(int)geometry.Indices[index + command.BaseVertex]].Position, transform);
                    var b = Vector3.Transform(geometry.Vertices[(int)geometry.Indices[index + 1 + command.BaseVertex]].Position, transform);
                    var c = Vector3.Transform(geometry.Vertices[(int)geometry.Indices[index + 2 + command.BaseVertex]].Position, transform);
                    sb.Append("<path d=\"M").Append(Px(a.X)).Append(' ').Append(Py(a.Y)).Append("L")
                        .Append(Px(b.X)).Append(' ').Append(Py(b.Y)).Append('L').Append(Px(c.X)).Append(' ')
                        .Append(Py(c.Y)).Append("Z\" fill=\"").Append(Color(tint)).Append("\"/>");
                }
            }
        }
        sb.Append("</svg>");
        File.WriteAllText(path, sb.ToString());
        stopwatch.Stop();
        return ValueTask.FromResult(new VisualizerFrameResult(++_frame, frame.Commands.Count, stopwatch.Elapsed, wasPresented: false));
    }

    private static string Px(float x) => (160 + x * 320).ToString("0.###", CultureInfo.InvariantCulture);
    private static string Py(float y) => (90 - y * 320).ToString("0.###", CultureInfo.InvariantCulture);
    private static string Color(Vector4 c)
    {
        static int C(float x) => (int)Math.Clamp(MathF.Round(x * 255), 0, 255);
        return $"#{C(c.X):X2}{C(c.Y):X2}{C(c.Z):X2}";
    }
    private static string Escape(string s) => s.Replace("&", "&amp;").Replace("\"", "&quot;").Replace("<", "&lt;").Replace(">", "&gt;");
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
