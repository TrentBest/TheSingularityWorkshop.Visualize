using System.Diagnostics;
using System.Globalization;
using System.Numerics;
using System.Text;
using TheSingularityWorkshop.Visualize;

var outputDirectory = args.Length > 0 ? Path.GetFullPath(args[0]) : Path.Combine(AppContext.BaseDirectory, "generated");
Directory.CreateDirectory(outputDirectory);

var examples = new[]
{
    new Example("01-one-point", "One point", BuildPointFrame(1, cloud: false)),
    new Example("02-point-cloud-100", "Point cloud · 100 instances", BuildPointFrame(100, cloud: true)),
    new Example("03-four-connected-points", "Four connected points", BuildConnectedFrame()),
    new Example("04-one-line", "One line", BuildLineFrame(1)),
    new Example("05-many-lines-24", "Many lines · 24 instances", BuildLineFrame(24))
};

foreach (var example in examples)
{
    var backend = new SvgProofBackend(Path.Combine(outputDirectory, example.FileName + ".svg"), example.Title);
    var catalog = new VisualizerResourceCatalog();
    var pointGeometryId = new VisualizerResourceId(1);
    var lineGeometryId = new VisualizerResourceId(2);
    var materialId = new VisualizerResourceId(3);
    catalog.SetGeometry(pointGeometryId, PointMarker());
    catalog.SetGeometry(lineGeometryId, LineSegment());
    catalog.SetMaterial(materialId, new VisualizerMaterial(1, Vector4.One));
    if (example.FrameResources is not null)
    {
        foreach (var item in example.FrameResources.Geometry)
            catalog.SetGeometry(item.Key, item.Value);
        foreach (var item in example.FrameResources.Instances)
            catalog.SetInstances(item.Key, item.Value);
    }

    await using var visualizer = new Visualizer(backend, catalog);
    await visualizer.RenderAsync(example.Frame);
    Console.WriteLine($"Generated {Path.Combine(outputDirectory, example.FileName + ".svg")}");
}

static Example Make(string file, string title, VisualizerFrame frame, VisualizerResourceCatalog? catalog = null)
{
    // Resource catalog entries are included explicitly so each generated example goes through Visualizer.RenderAsync.
    return new Example(file, title, frame);
}

static VisualizerGeometry PointMarker()
{
    // A point is represented by a tiny diamond mesh: two indexed triangles, not an SVG-authored circle.
    return new VisualizerGeometry(1,
    [
        Vertex(-0.012f, 0), Vertex(0, 0.018f), Vertex(0.012f, 0), Vertex(0, -0.018f)
    ], [0, 1, 2, 0, 2, 3]);
}

static VisualizerGeometry LineSegment()
{
    // A line is represented by a thin rectangle made of two indexed triangles.
    return new VisualizerGeometry(1,
    [
        Vertex(-0.5f, -0.0025f), Vertex(-0.5f, 0.0025f),
        Vertex(0.5f, 0.0025f), Vertex(0.5f, -0.0025f)
    ], [0, 1, 2, 0, 2, 3]);
}

static VisualizerVertex Vertex(float x, float y) =>
    new(new Vector3(x, y, 0), Vector3.UnitZ, Vector2.Zero, Vector4.One);

static VisualizerFrame BuildPointFrame(int count, bool cloud)
{
    var geometryId = new VisualizerResourceId(1);
    var materialId = new VisualizerResourceId(3);
    var instancesId = new VisualizerResourceId(4);
    var instances = Enumerable.Range(0, count).Select(i =>
    {
        var x = cloud ? -0.42f + (i % 10) * 0.093f : 0;
        var y = cloud ? 0.36f - (i / 10) * 0.073f : 0;
        var tint = cloud
            ? new Vector4(0.25f + (i % 5) * 0.14f, 0.78f, 0.95f - (i % 4) * 0.13f, 1)
            : new Vector4(0.49f, 0.92f, 0.76f, 1);
        return new VisualizerInstance(Matrix4x4.CreateTranslation(x, y, 0), tint);
    }).ToArray();
    var catalog = new VisualizerResourceCatalog();
    catalog.SetGeometry(geometryId, PointMarker());
    catalog.SetMaterial(materialId, new VisualizerMaterial(1, Vector4.One));
    catalog.SetInstances(instancesId, new VisualizerInstanceBuffer(1, instances));
    var command = cloud
        ? new VisualizerDrawCommand(geometryId, materialId, Matrix4x4.Identity, 6, 0, 0,
            VisualizerRepresentation.Near, instancesId, count, 0)
        : new VisualizerDrawCommand(geometryId, materialId, Matrix4x4.Identity, 6, 0, 0,
            VisualizerRepresentation.Near);
    // A small self-contained resource bundle is carried alongside the frame for the proof runner.
    Frames.Resources[command.GetHashCode()] = (catalog, geometryId, instancesId, cloud);
    return new VisualizerFrame(new VisualizerView(Vector3.Zero, Matrix4x4.Identity, Matrix4x4.Identity), [command]);
}

static VisualizerFrame BuildConnectedFrame()
{
    var commands = new List<VisualizerDrawCommand>();
    var geometryId = new VisualizerResourceId(1);
    var lineId = new VisualizerResourceId(2);
    var materialId = new VisualizerResourceId(3);
    var points = new[] { new Vector2(-0.3f, -0.2f), new Vector2(0.3f, -0.2f), new Vector2(0.3f, 0.2f), new Vector2(-0.3f, 0.2f) };
    var catalog = new VisualizerResourceCatalog();
    catalog.SetGeometry(geometryId, PointMarker());
    catalog.SetGeometry(lineId, LineSegment());
    catalog.SetMaterial(materialId, new VisualizerMaterial(1, Vector4.One));
    var ids = new[] { geometryId, lineId };
    var frameCommands = new List<VisualizerDrawCommand>();
    foreach (var p in points)
        frameCommands.Add(new VisualizerDrawCommand(geometryId, materialId, Matrix4x4.CreateTranslation(p.X, p.Y, 0), 6, 0, 0, VisualizerRepresentation.Near));
    for (var i = 0; i < points.Length; i++)
    {
        var a = points[i]; var b = points[(i + 1) % points.Length];
        var delta = b - a;
        var transform = Matrix4x4.CreateScale(delta.Length(), 1, 1) *
                        Matrix4x4.CreateRotationZ(MathF.Atan2(delta.Y, delta.X)) *
                        Matrix4x4.CreateTranslation((a.X + b.X) / 2, (a.Y + b.Y) / 2, 0);
        frameCommands.Add(new VisualizerDrawCommand(lineId, materialId, transform, 6, 0, 0, VisualizerRepresentation.Near));
    }
    Frames.Resources[frameCommands[0].GetHashCode()] = (catalog, geometryId, default, false);
    return new VisualizerFrame(new VisualizerView(Vector3.Zero, Matrix4x4.Identity, Matrix4x4.Identity), frameCommands);
}

static VisualizerFrame BuildLineFrame(int count)
{
    var geometryId = new VisualizerResourceId(2);
    var materialId = new VisualizerResourceId(3);
    var instancesId = new VisualizerResourceId(4);
    var catalog = new VisualizerResourceCatalog();
    catalog.SetGeometry(geometryId, LineSegment());
    catalog.SetMaterial(materialId, new VisualizerMaterial(1, Vector4.One));
    var instances = Enumerable.Range(0, count).Select(i =>
    {
        var x = count == 1 ? 0 : -0.42f + (i % 6) * 0.17f;
        var y = count == 1 ? 0 : 0.32f - (i / 6) * 0.16f;
        var angle = count == 1 ? 0 : (i % 6 - 2.5f) * 0.16f;
        return new VisualizerInstance(Matrix4x4.CreateRotationZ(angle) * Matrix4x4.CreateTranslation(x, y, 0),
            new Vector4(0.48f, 0.76f + (i % 3) * 0.08f, 1, 1));
    }).ToArray();
    catalog.SetInstances(instancesId, new VisualizerInstanceBuffer(1, instances));
    var command = count == 1
        ? new VisualizerDrawCommand(geometryId, materialId, Matrix4x4.Identity, 6, 0, 0, VisualizerRepresentation.Near)
        : new VisualizerDrawCommand(geometryId, materialId, Matrix4x4.Identity, 6, 0, 0, VisualizerRepresentation.Near, instancesId, count, 0);
    Frames.Resources[command.GetHashCode()] = (catalog, geometryId, instancesId, count > 1);
    return new VisualizerFrame(new VisualizerView(Vector3.Zero, Matrix4x4.Identity, Matrix4x4.Identity), [command]);
}

sealed record Example(string FileName, string Title, VisualizerFrame Frame)
{
    public (Dictionary<VisualizerResourceId, VisualizerGeometry> Geometry, Dictionary<VisualizerResourceId, VisualizerInstanceBuffer> Instances)? FrameResources { get; init; }
}

static class Frames
{
    public static readonly Dictionary<int, (VisualizerResourceCatalog Catalog, VisualizerResourceId Geometry, VisualizerResourceId Instances, bool Instanced)> Resources = new();
}

sealed class SvgProofBackend(string path, string title) : IVisualizerBackend
{
    private long _frame;
    public VisualizerCapabilities Capabilities { get; } = new("SVG proof backend", "SVG", false, true, false, false);

    public ValueTask<VisualizerFrameResult> RenderAsync(VisualizerFrame frame, IVisualizerResourceProvider resources, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var stopwatch = Stopwatch.StartNew();
        var sb = new StringBuilder();
        sb.AppendLine("<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 320 180\" role=\"img\" aria-label=\"" + Escape(title) + "\">");
        sb.AppendLine("<rect width=\"320\" height=\"180\" rx=\"8\" fill=\"#0b1725\"/>");
        for (var x = 20; x < 320; x += 20) sb.Append("<path d=\"M").Append(x).Append(" 0V180\" stroke=\"#203447\" stroke-width=\".5\"/>");
        for (var y = 20; y < 180; y += 20) sb.Append("<path d=\"M0 ").Append(y).Append("H320\" stroke=\"#203447\" stroke-width=\".5\"/>");
        foreach (var command in frame.Commands)
        {
            if (!resources.TryGetGeometry(command.Geometry, out var geometry) || geometry is null ||
                !resources.TryGetMaterial(command.Material, out var material) || material is null)
                throw new InvalidOperationException("Visualizer submitted a frame with unresolved resources.");
            var count = command.Instances.IsValid && resources.TryGetInstances(command.Instances, out var buffer) && buffer is not null
                ? command.InstanceCount : 1;
            for (var i = 0; i < count; i++)
            {
                var transform = command.WorldTransform;
                var tint = material.BaseColor;
                if (command.Instances.IsValid && buffer is not null)
                {
                    var instance = buffer.Instances[command.FirstInstance + i];
                    transform = transform * instance.WorldTransform;
                    tint *= instance.Color;
                }
                var color = Color(tint);
                for (var index = command.FirstIndex; index < command.FirstIndex + command.IndexCount; index += 3)
                {
                    var a = geometry.Vertices[(int)geometry.Indices[index + command.BaseVertex]].Position;
                    var b = geometry.Vertices[(int)geometry.Indices[index + 1 + command.BaseVertex]].Position;
                    var c = geometry.Vertices[(int)geometry.Indices[index + 2 + command.BaseVertex]].Position;
                    a = Vector3.Transform(a, transform); b = Vector3.Transform(b, transform); c = Vector3.Transform(c, transform);
                    sb.Append("<path d=\"M").Append(Px(a.X)).Append(' ').Append(Py(a.Y)).Append("L")
                      .Append(Px(b.X)).Append(' ').Append(Py(b.Y)).Append('L').Append(Px(c.X)).Append(' ')
                      .Append(Py(c.Y)).Append("Z\" fill=\"").Append(color).Append("\"/>");
                }
            }
        }
        sb.AppendLine("</svg>");
        File.WriteAllText(path, sb.ToString());
        stopwatch.Stop();
        return ValueTask.FromResult(new VisualizerFrameResult(++_frame, frame.Commands.Count, stopwatch.Elapsed, wasPresented: true));
    }

    private static string Px(float x) => (160 + x * 320).ToString("0.###", CultureInfo.InvariantCulture);
    private static string Py(float y) => (90 - y * 320).ToString("0.###", CultureInfo.InvariantCulture);
    private static string Color(Vector4 color)
    {
        static int C(float x) => (int)Math.Clamp(MathF.Round(x * 255), 0, 255);
        return $"#{C(color.X):X2}{C(color.Y):X2}{C(color.Z):X2}";
    }
    private static string Escape(string value) => value.Replace("&", "&amp;").Replace("\"", "&quot;").Replace("<", "&lt;").Replace(">", "&gt;");
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
