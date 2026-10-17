using System.Diagnostics;
using System.Globalization;
using System.Numerics;
using TheSingularityWorkshop.Visualize;
using TheSingularityWorkshop.Workshop.Gui;
using TheSingularityWorkshop.Workshop.Gui.Svg;

namespace TheSingularityWorkshop.Visualize.GuiSvg;

/// <summary>
/// Visualizer backend that manifests submitted indexed geometry through the reusable GUI.SVG adapter.
/// </summary>
/// <remarks>
/// This is a CPU-side SVG proof/integration backend, not a GPU renderer. SVG animation nodes
/// are presentation instructions; they do not execute FSM_API or advance authoritative state.
/// </remarks>
public sealed class GuiSvgVisualizerBackend : IVisualizerBackend
{
    private readonly string? _outputPath;
    private readonly Action<string>? _svgSink;
    private readonly bool _animateConnectedPoints;
    private readonly bool _projectWithCamera;
    private readonly string _backgroundColor;
    private readonly string _gridColor;
    private readonly string _sceneLabel;
    private readonly bool _drawGrid;
    private readonly SvgGuiRenderer _renderer = new();
    private long _frame;

    /// <summary>Creates an SVG backend that writes each submitted frame to a file.</summary>
    /// <param name="outputPath">Destination path for the emitted SVG document.</param>
    /// <param name="animateConnectedPoints">Whether to add the optional SVG-only route animation.</param>
    /// <param name="projectWithCamera">Whether to use the frame view and projection matrices for CPU-side perspective projection.</param>
    /// <param name="backgroundColor">SVG canvas background colour.</param>
    /// <param name="gridColor">SVG guide-grid colour.</param>
    /// <param name="sceneLabel">Accessible scene label written into the GUI scene tree.</param>
    /// <param name="drawGrid">Whether to draw the backend guide grid.</param>
    public GuiSvgVisualizerBackend(
        string outputPath,
        bool animateConnectedPoints = false,
        bool projectWithCamera = false,
        string backgroundColor = "#0b1725",
        string gridColor = "#203447",
        string sceneLabel = "Visualizer frame manifested through GUI.SVG",
        bool drawGrid = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(backgroundColor);
        ArgumentException.ThrowIfNullOrWhiteSpace(gridColor);
        ArgumentException.ThrowIfNullOrWhiteSpace(sceneLabel);
        _outputPath = outputPath;
        _svgSink = null;
        _animateConnectedPoints = animateConnectedPoints;
        _projectWithCamera = projectWithCamera;
        _backgroundColor = backgroundColor;
        _gridColor = gridColor;
        _sceneLabel = sceneLabel;
        _drawGrid = drawGrid;
    }

    /// <summary>Creates an SVG backend that sends each submitted frame to an in-memory sink.</summary>
    /// <param name="svgSink">Receives the complete serialized SVG document for the submitted frame.</param>
    /// <param name="animateConnectedPoints">Whether to add the optional SVG-only route animation.</param>
    /// <param name="projectWithCamera">Whether to use the frame view and projection matrices for CPU-side perspective projection.</param>
    /// <param name="backgroundColor">SVG canvas background colour.</param>
    /// <param name="gridColor">SVG guide-grid colour.</param>
    /// <param name="sceneLabel">Accessible scene label written into the GUI scene tree.</param>
    /// <param name="drawGrid">Whether to draw the backend guide grid.</param>
    /// <remarks>
    /// Use this overload when the host owns the output surface, such as a browser component that
    /// binds the resulting SVG to its UI. The sink is invoked synchronously on the rendering call.
    /// This remains a CPU-side SVG manifestation path, not a GPU renderer.
    /// </remarks>
    public GuiSvgVisualizerBackend(
        Action<string> svgSink,
        bool animateConnectedPoints = false,
        bool projectWithCamera = false,
        string backgroundColor = "#0b1725",
        string gridColor = "#203447",
        string sceneLabel = "Visualizer frame manifested through GUI.SVG",
        bool drawGrid = true)
    {
        ArgumentNullException.ThrowIfNull(svgSink);
        ArgumentException.ThrowIfNullOrWhiteSpace(backgroundColor);
        ArgumentException.ThrowIfNullOrWhiteSpace(gridColor);
        ArgumentException.ThrowIfNullOrWhiteSpace(sceneLabel);
        _outputPath = null;
        _svgSink = svgSink;
        _animateConnectedPoints = animateConnectedPoints;
        _projectWithCamera = projectWithCamera;
        _backgroundColor = backgroundColor;
        _gridColor = gridColor;
        _sceneLabel = sceneLabel;
        _drawGrid = drawGrid;
    }

    /// <summary>Describes the SVG proof backend's presentation capabilities.</summary>
    public VisualizerCapabilities Capabilities { get; } =
        new("GUI.SVG Visualizer backend", "SVG", false, true, false, false);

    /// <summary>Manifests a Visualizer frame through the GUI.SVG adapter.</summary>
    public ValueTask<VisualizerFrameResult> RenderAsync(
        VisualizerFrame frame,
        IVisualizerResourceProvider resources,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(frame);
        ArgumentNullException.ThrowIfNull(resources);
        cancellationToken.ThrowIfCancellationRequested();

        var stopwatch = Stopwatch.StartNew();
        var scene = GuiBuilder.Create("svg-scene", $"visualize-frame-{_frame + 1}")
            .Text(_sceneLabel);
        scene.Child("svg-rect", "background", background => background
            .Property("x", "0").Property("y", "0")
            .Property("width", "100").Property("height", "56.25")
            .Property("fill", _backgroundColor));
        if (_drawGrid)
        {
            for (var x = 10; x < 100; x += 10)
                scene.Child("svg-path", $"grid-x-{x}", line => line
                    .Property("d", $"M{x} 0V56.25").Property("fill", "none")
                    .Property("stroke", _gridColor).Property("stroke-width", "0.2"));
            for (var y = 5.625; y < 56.25; y += 5.625)
            {
                var gridY = y.ToString(CultureInfo.InvariantCulture);
                scene.Child("svg-path", $"grid-y-{gridY}", line => line
                    .Property("d", $"M0 {gridY}H100").Property("fill", "none")
                    .Property("stroke", _gridColor).Property("stroke-width", "0.2"));
            }
        }

        var drawIndex = 0;
        foreach (var command in frame.Commands)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!resources.TryGetGeometry(command.Geometry, out var geometry) || geometry is null ||
                !resources.TryGetMaterial(command.Material, out var material) || material is null)
                throw new InvalidOperationException("Visualizer submitted unresolved geometry or material resources.");

            VisualizerInstanceBuffer? instances = null;
            if (command.Instances.IsValid && !resources.TryGetInstances(command.Instances, out instances))
                throw new InvalidOperationException("Visualizer submitted an unresolved instance buffer.");

            var instanceCount = command.Instances.IsValid ? command.InstanceCount : 1;
            for (var instanceIndex = 0; instanceIndex < instanceCount; instanceIndex++)
            {
                var transform = command.WorldTransform;
                var tint = material.BaseColor;
                if (instances is not null)
                {
                    var instance = instances.Instances[command.FirstInstance + instanceIndex];
                    transform *= instance.WorldTransform;
                    tint *= instance.Color;
                }

                for (var index = command.FirstIndex; index < command.FirstIndex + command.IndexCount; index += 3)
                {
                    var projection = _projectWithCamera
                        ? transform * frame.View.View * frame.View.Projection
                        : transform;
                    var a = Project(geometry.Vertices[(int)geometry.Indices[index + command.BaseVertex]].Position, projection, _projectWithCamera);
                    var b = Project(geometry.Vertices[(int)geometry.Indices[index + 1 + command.BaseVertex]].Position, projection, _projectWithCamera);
                    var c = Project(geometry.Vertices[(int)geometry.Indices[index + 2 + command.BaseVertex]].Position, projection, _projectWithCamera);
                    if (a is null || b is null || c is null)
                        continue;
                    var path = FormattableString.Invariant(
                        $"M{F(a.Value.X)} {F(a.Value.Y)}L{F(b.Value.X)} {F(b.Value.Y)}L{F(c.Value.X)} {F(c.Value.Y)}Z");
                    var fill = Color(tint);
                    scene.Child("svg-path", $"draw-{drawIndex++}", node => node
                        .Property("d", path)
                        .Property("fill", fill));
                }
            }
        }

        if (_animateConnectedPoints)
        {
            const string route = "M18 46.125L78 50.125L86 16.125L32 4.125L18 46.125";
            scene.Child("svg-path", "connected-route", path => path
                .Property("d", route)
                .Property("fill", "none")
                .Property("stroke", "#7ee0bd")
                .Property("stroke-width", "0.45")
                .Property("stroke-dasharray", "1.6 2.2")
                .Child("svg-animate", "route-dash-animation", animation => animation
                    .Property("attributeName", "stroke-dashoffset")
                    .Property("from", "0")
                    .Property("to", "-48")
                    .Property("dur", "4s")
                    .Property("repeatCount", "indefinite")));
            // Keep each point attached to its actual vertex. Earlier animateMotion markers
            // drifted across the closed path and read like objects falling into the shape.
            AddPulseMarker(scene, "vertex-a", "#ffcf86", "18", "46.125", "0s");
            AddPulseMarker(scene, "vertex-b", "#7ee0bd", "78", "50.125", "-0.7s");
            AddPulseMarker(scene, "vertex-c", "#8fcaff", "86", "16.125", "-1.4s");
            AddPulseMarker(scene, "vertex-d", "#c5a4ff", "32", "4.125", "-2.1s");
        }

        var svg = _renderer.Render(scene.Build(), width: 320, height: 180);
        if (_svgSink is not null)
        {
            _svgSink(svg);
        }
        else
        {
            var outputPath = _outputPath
                ?? throw new InvalidOperationException("The SVG backend has neither an output path nor an SVG sink.");
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outputPath))!);
            File.WriteAllText(outputPath, svg);
        }
        stopwatch.Stop();

        return ValueTask.FromResult(
            new VisualizerFrameResult(++_frame, frame.Commands.Count, stopwatch.Elapsed, wasPresented: false));
    }

    /// <summary>Releases backend resources. The SVG proof backend owns no persistent native resources.</summary>
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    private static void AddPulseMarker(GuiBuilder scene, string id, string fill, string x, string y, string begin)
    {
        scene.Child("svg-circle", id, marker => marker
            .Property("cx", x)
            .Property("cy", y)
            .Property("r", "1.35")
            .Property("fill", fill)
            .Property("opacity", "0.92")
            .Property("stroke", "#fff1ce")
            .Property("stroke-width", "0.35")
            .Child("svg-animate", id + "-pulse", animation => animation
                .Property("attributeName", "r")
                .Property("values", "1.35;2.15;1.35")
                .Property("keyTimes", "0;0.5;1")
                .Property("dur", "2.8s")
                .Property("begin", begin)
                .Property("repeatCount", "indefinite")));
    }

    private static Vector2? Project(Vector3 position, Matrix4x4 transform, bool cameraProjected)
    {
        var clip = Vector4.Transform(new Vector4(position, 1), transform);
        if (!float.IsFinite(clip.W) || MathF.Abs(clip.W) < 1e-6f)
            return null;

        var x = clip.X / clip.W;
        var y = clip.Y / clip.W;
        if (!float.IsFinite(x) || !float.IsFinite(y))
            return null;

        return cameraProjected
            ? new Vector2((float)(50 + x * 50), (float)(28.125 - y * 28.125))
            : new Vector2((float)X(x), (float)Y(y));
    }

    private static double X(float x) => 50 + x * 100;
    private static double Y(float y) => 28.125 - y * 100;
    private static string F(double value) => value.ToString("0.###", CultureInfo.InvariantCulture);

    private static string Color(Vector4 color)
    {
        static int Channel(float value) => (int)Math.Clamp(MathF.Round(value * 255), 0, 255);
        return $"#{Channel(color.X):X2}{Channel(color.Y):X2}{Channel(color.Z):X2}";
    }
}
