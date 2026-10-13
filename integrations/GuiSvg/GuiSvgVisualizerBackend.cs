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
            const string route = "M20 48.125L80 48.125L80 8.125L20 8.125Z";
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
            scene.Child("svg-circle", "moving-marker", marker => marker
                .Property("cx", "20")
                .Property("cy", "48.125")
                .Property("r", "1.4")
                .Property("fill", "#ffcf86")
                .Property("stroke", "#fff1ce")
                .Property("stroke-width", "0.45")
                .Child("svg-animate-motion", "marker-motion", motion => motion
                    .Property("path", route)
                    .Property("dur", "4s")
                    .Property("repeatCount", "indefinite")));
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

    private static double X(float x) => 50 + x * 100;
    private static double Y(float y) => 28.125 - y * 100;

    private static string Color(Vector4 color)
    {
        static int Channel(float value) => (int)Math.Clamp(MathF.Round(value * 255), 0, 255);
        return $"#{Channel(color.X):X2}{Channel(color.Y):X2}{Channel(color.Z):X2}";
    }
}
