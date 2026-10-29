using System.Globalization;
using System.Numerics;
using System.Text;
using System.Xml;

namespace TheSingularityWorkshop.Visualize;

/// <summary>
/// Produces a deterministic, static SVG artifact from a Visualize frame and its logical resources.
/// </summary>
/// <remarks>
/// This is a CPU-side artifact backend, not a GPU renderer or interactive host surface. It has no
/// dependency on GUI.Core or GUI.SVG. Triangle lists are projected through the frame's world, view,
/// and projection matrices; unsupported indexed primitive layouts fail explicitly.
/// </remarks>
public sealed class SvgVisualizerBackend : IVisualizerBackend
{
    private readonly Action<string> _svgSink;
    private readonly int _width;
    private readonly int _height;
    private readonly string _background;
    private long _frameNumber;
    private int _disposed;

    /// <summary>Creates a static SVG backend that sends each generated artifact to a caller-owned sink.</summary>
    public SvgVisualizerBackend(
        Action<string> svgSink,
        int width = 800,
        int height = 600,
        string background = "#ffffff")
    {
        ArgumentNullException.ThrowIfNull(svgSink);
        ArgumentException.ThrowIfNullOrWhiteSpace(background);
        VisualizerSvgDocument.ValidateColor(background, nameof(background));
        if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
        if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));
        _svgSink = svgSink;
        _width = width;
        _height = height;
        _background = background;
    }

    /// <inheritdoc />
    public VisualizerCapabilities Capabilities { get; } =
        new("Visualize static SVG artifact backend", "SVG/CPU", false, false, false, false);

    /// <inheritdoc />
    public ValueTask<VisualizerFrameResult> RenderAsync(
        VisualizerFrame frame,
        IVisualizerResourceProvider resources,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(frame);
        ArgumentNullException.ThrowIfNull(resources);
        ObjectDisposedException.ThrowIf(Volatile.Read(ref _disposed) != 0, this);
        cancellationToken.ThrowIfCancellationRequested();

        var timer = System.Diagnostics.Stopwatch.StartNew();
        var svg = BuildSvg(frame, resources, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        _svgSink(svg);
        timer.Stop();

        var number = Interlocked.Increment(ref _frameNumber) - 1;
        return ValueTask.FromResult(new VisualizerFrameResult(number, frame.Commands.Count, timer.Elapsed, wasPresented: false));
    }

    /// <summary>Builds the SVG text without writing it to the configured sink.</summary>
    public string BuildSvg(
        VisualizerFrame frame,
        IVisualizerResourceProvider resources,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(frame);
        ArgumentNullException.ThrowIfNull(resources);
        ObjectDisposedException.ThrowIf(Volatile.Read(ref _disposed) != 0, this);

        var primitives = new List<VisualizerSvgPrimitive>();
        foreach (var command in frame.Commands)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (command.IndexCount % 3 != 0)
                throw new NotSupportedException($"SVG backend supports triangle-list draws; command index count {command.IndexCount} is not divisible by three.");
            if (!resources.TryGetGeometry(command.Geometry, out var geometry) || geometry is null)
                throw new InvalidOperationException($"Missing geometry resource {command.Geometry.Value}.");
            if (!resources.TryGetMaterial(command.Material, out var material) || material is null)
                throw new InvalidOperationException($"Missing material resource {command.Material.Value}.");
            if (command.FirstIndex + (long)command.IndexCount > geometry.Indices.Count)
                throw new InvalidOperationException($"Draw command exceeds geometry index range for resource {command.Geometry.Value}.");

            VisualizerInstanceBuffer? instanceBuffer = null;
            if (command.Instances.IsValid &&
                (!resources.TryGetInstances(command.Instances, out instanceBuffer) || instanceBuffer is null))
                throw new InvalidOperationException($"Missing instance resource {command.Instances.Value}.");
            var instanceCount = command.Instances.IsValid ? command.InstanceCount : 1;
            if (instanceBuffer is not null && command.FirstInstance + (long)instanceCount > instanceBuffer.Instances.Count)
                throw new InvalidOperationException($"Draw command exceeds instance range for resource {command.Instances.Value}.");

            for (var instanceOffset = 0; instanceOffset < instanceCount; instanceOffset++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var instance = instanceBuffer is null
                    ? new VisualizerInstance(Matrix4x4.Identity, Vector4.One)
                    : instanceBuffer.Instances[command.FirstInstance + instanceOffset];
                var world = instance.WorldTransform * command.WorldTransform;
                for (var i = command.FirstIndex; i < command.FirstIndex + command.IndexCount; i += 3)
                {
                    var points = new (double X, double Y)[3];
                    var color = Vector4.Zero;
                    for (var corner = 0; corner < 3; corner++)
                    {
                        var vertexIndex = (long)geometry.Indices[i + corner] + command.BaseVertex;
                        if (vertexIndex < 0 || vertexIndex >= geometry.Vertices.Count)
                            throw new InvalidOperationException($"Effective vertex index {vertexIndex} is outside geometry resource {command.Geometry.Value}.");
                        var vertex = geometry.Vertices[(int)vertexIndex];
                        var clip = Vector4.Transform(new Vector4(vertex.Position, 1), world * frame.View.View * frame.View.Projection);
                        if (!float.IsFinite(clip.X) || !float.IsFinite(clip.Y) || !float.IsFinite(clip.W) || Math.Abs(clip.W) < 1e-7f)
                            throw new InvalidOperationException("A triangle vertex produced invalid clip coordinates.");
                        var ndcX = clip.X / clip.W;
                        var ndcY = clip.Y / clip.W;
                        if (!float.IsFinite(ndcX) || !float.IsFinite(ndcY))
                            throw new InvalidOperationException("A triangle vertex produced non-finite normalized coordinates.");
                        points[corner] = ((ndcX + 1f) * _width * 0.5f, (1f - ndcY) * _height * 0.5f);
                        color += new Vector4(
                            material.BaseColor.X * vertex.Color.X,
                            material.BaseColor.Y * vertex.Color.Y,
                            material.BaseColor.Z * vertex.Color.Z,
                            material.BaseColor.W * vertex.Color.W) / 3f;
                    }
                    color = new Vector4(color.X * instance.Color.X, color.Y * instance.Color.Y, color.Z * instance.Color.Z, color.W * instance.Color.W);
                    if (!float.IsFinite(color.W) || color.W < 0f || color.W > 1f)
                        throw new InvalidOperationException("Computed triangle alpha must be finite and within the inclusive range [0, 1].");
                    var attributes = new SortedDictionary<string, string>(StringComparer.Ordinal)
                    {
                        ["data-representation"] = command.Representation.ToString().ToLowerInvariant()
                    };
                    if (color.W < 1f)
                        attributes["fill-opacity"] = color.W.ToString("0.###", CultureInfo.InvariantCulture);
                    if (command.Entity.IsValid)
                        attributes["data-entity-id"] = command.Entity.Value.ToString(CultureInfo.InvariantCulture);

                    primitives.Add(new VisualizerSvgPolygon(
                        $"visualize-triangle-{primitives.Count.ToString(CultureInfo.InvariantCulture)}",
                        points,
                        ToHex(color),
                        attributes));
                }
            }
        }

        cancellationToken.ThrowIfCancellationRequested();
        return new VisualizerSvgDocument(_width, _height, primitives, _background, role: "img").ToSvg();
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        Interlocked.Exchange(ref _disposed, 1);
        return ValueTask.CompletedTask;
    }

    private static string ToHex(Vector4 color)
    {
        static int Channel(float value) => (int)MathF.Round(Math.Clamp(LinearToSrgb(Math.Clamp(value, 0f, 1f)), 0f, 1f) * 255f);
        return FormattableString.Invariant($"#{Channel(color.X):X2}{Channel(color.Y):X2}{Channel(color.Z):X2}");
    }

    private static float LinearToSrgb(float value) =>
        value <= 0.0031308f ? 12.92f * value : 1.055f * MathF.Pow(value, 1f / 2.4f) - 0.055f;
}
