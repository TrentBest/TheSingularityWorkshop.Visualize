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
    private readonly string _outputPath;
    private readonly bool _animateConnectedPoints;
    private readonly SvgGuiRenderer _renderer = new();
    private long _frame;

    /// <summary>Creates an SVG backend that writes each submitted frame to a file.</summary>
    /// <param name="outputPath">Destination path for the emitted SVG document.</param>
    /// <param name="animateConnectedPoints">Whether to add the optional SVG-only route animation.</param>
    public GuiSvgVisualizerBackend(string outputPath, bool animateConnectedPoints = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);
        _outputPath = outputPath;
        _animateConnectedPoints = animateConnectedPoints;
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
            .Text("Visualizer frame manifested through GUI.SVG");
        scene.Child("svg-rect", "background", background => background
            .Property("x", "0").Property("y", "0")
            .Property("width", "100").Property("height", "56.25")
            .Property("fill", "#0b1725"));
        for (var x = 10; x < 100; x += 10)
            scene.Child("svg-path", $"grid-x-{x}", line => line
                .Property("d", $"M{x} 0V56.25").Property("fill", "none")
                .Property("stroke", "#203447").Property("stroke-width", "0.2"));
        for (var y = 5.625; y < 56.25; y += 5.625)
        {
            var gridY = y.ToString(CultureInfo.InvariantCulture);
            scene.Child("svg-path", $"grid-y-{gridY}", line => line
                .Property("d", $"M0 {gridY}H100").Property("fill", "none")
                .Property("stroke", "#203447").Property("stroke-width", "0.2"));
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
                    var a = Vector3.Transform(geometry.Vertices[(int)geometry.Indices[index + command.BaseVertex]].Position, transform);
                    var b = Vector3.Transform(geometry.Vertices[(int)geometry.Indices[index + 1 + command.BaseVertex]].Position, transform);
                    var c = Vector3.Transform(geometry.Vertices[(int)geometry.Indices[index + 2 + command.BaseVertex]].Position, transform);
                    var path = $"M{X(a.X)} {Y(a.Y)}L{X(b.X)} {Y(b.Y)}L{X(c.X)} {Y(c.Y)}Z";
                    var fill = Color(tint);
                    scene.Child("svg-path", $"draw-{drawIndex++}", node => node
                        .Property("d", path)
                        .Property("fill", fill));
                }
            }
        }

        if (_animateConnectedPoints)
        {
            const string route = "M18 46.125L78 50.125L82 16.125L32 4.125L18 46.125";
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
            AddMotionMarker(scene, "marker-leading", "#ffcf86", "5.2", "0s", route);
            AddMotionMarker(scene, "marker-middle", "#7ee0bd", "3.2", "-0.35s", route);
            AddMotionMarker(scene, "marker-trailing", "#8fcaff", "2.1", "-0.7s", route);
        }

        var svg = _renderer.Render(scene, width: 320, height: 180);
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(_outputPath))!);
        File.WriteAllText(_outputPath, svg);
        stopwatch.Stop();

        return ValueTask.FromResult(
            new VisualizerFrameResult(++_frame, frame.Commands.Count, stopwatch.Elapsed, wasPresented: false));
    }

    /// <summary>Releases backend resources. The SVG proof backend owns no persistent native resources.</summary>
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    private static void AddMotionMarker(GuiBuilder scene, string id, string fill, string radius, string begin, string route)
    {
        scene.Child("svg-circle", id, marker => marker
            .Property("cx", "18")
            .Property("cy", "46.125")
            .Property("r", radius)
            .Property("fill", fill)
            .Property("opacity", "0.9")
            .Property("stroke", "#fff1ce")
            .Property("stroke-width", "0.35")
            .Child("svg-animate-motion", id + "-motion", motion => motion
                .Property("path", route)
                .Property("begin", begin)
                .Property("dur", "4s")
                .Property("repeatCount", "indefinite")));
    }

    private static double X(float x) => 50 + x * 100;
    private static double Y(float y) => 28.125 - y * 100;

    private static string Color(Vector4 color)
    {
        static int Channel(float value) => (int)Math.Clamp(MathF.Round(value * 255), 0, 255);
        return $"#{Channel(color.X):X2}{Channel(color.Y):X2}{Channel(color.Z):X2}";
    }
}
