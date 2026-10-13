using Xunit;
using TheSingularityWorkshop.Visualize;
using System.Numerics;
using TheSingularityWorkshop.Visualize.GuiSvg;

namespace TheSingularityWorkshop.Visualize.GuiSvg.Tests;

public sealed class GuiSvgVisualizerBackendTests
{
    [Fact]
    public async Task Visualizer_frame_is_serialized_through_gui_svg_and_animation_is_explicit()
    {
        var output = Path.Combine(Path.GetTempPath(), $"visualize-gui-svg-{Guid.NewGuid():N}.svg");
        try
        {
            var catalog = new VisualizerResourceCatalog();
            var pointId = new VisualizerResourceId(1);
            var lineId = new VisualizerResourceId(2);
            var lineMaterialId = new VisualizerResourceId(3);
            var pointMaterialIds = new[]
            {
                new VisualizerResourceId(4), new VisualizerResourceId(5),
                new VisualizerResourceId(6), new VisualizerResourceId(7)
            };
            catalog.SetGeometry(pointId, new VisualizerGeometry(1,
            [
                new VisualizerVertex(new Vector3(-0.012f, 0, 0), Vector3.UnitZ, Vector2.Zero, Vector4.One),
                new VisualizerVertex(new Vector3(0, 0.018f, 0), Vector3.UnitZ, Vector2.Zero, Vector4.One),
                new VisualizerVertex(new Vector3(0.012f, 0, 0), Vector3.UnitZ, Vector2.Zero, Vector4.One),
                new VisualizerVertex(new Vector3(0, -0.018f, 0), Vector3.UnitZ, Vector2.Zero, Vector4.One)
            ], [0, 1, 2, 0, 2, 3]));
            catalog.SetGeometry(lineId, new VisualizerGeometry(1,
            [
                new VisualizerVertex(new Vector3(-0.25f, -0.0025f, 0), Vector3.UnitZ, Vector2.Zero, Vector4.One),
                new VisualizerVertex(new Vector3(-0.25f, 0.0025f, 0), Vector3.UnitZ, Vector2.Zero, Vector4.One),
                new VisualizerVertex(new Vector3(0.25f, 0.0025f, 0), Vector3.UnitZ, Vector2.Zero, Vector4.One),
                new VisualizerVertex(new Vector3(0.25f, -0.0025f, 0), Vector3.UnitZ, Vector2.Zero, Vector4.One)
            ], [0, 1, 2, 0, 2, 3]));
            catalog.SetMaterial(lineMaterialId, new VisualizerMaterial(1, new Vector4(0.35f, 0.80f, 0.92f, 1)));
            catalog.SetMaterial(pointMaterialIds[0], new VisualizerMaterial(1, new Vector4(0.49f, 0.91f, 0.75f, 1)));
            catalog.SetMaterial(pointMaterialIds[1], new VisualizerMaterial(1, new Vector4(1.00f, 0.75f, 0.38f, 1)));
            catalog.SetMaterial(pointMaterialIds[2], new VisualizerMaterial(1, new Vector4(0.50f, 0.72f, 1.00f, 1)));
            catalog.SetMaterial(pointMaterialIds[3], new VisualizerMaterial(1, new Vector4(0.72f, 0.57f, 1.00f, 1)));
            var points = new[]
            {
                new Vector2(-0.32f, -0.18f), new Vector2(0.28f, -0.22f),
                new Vector2(0.36f, 0.12f), new Vector2(-0.18f, 0.24f)
            };
            var commands = new List<VisualizerDrawCommand>();
            for (var i = 0; i < points.Length; i++)
            {
                var point = points[i];
                commands.Add(new VisualizerDrawCommand(
                    pointId, pointMaterialIds[i], Matrix4x4.CreateTranslation(point.X, point.Y, 0),
                    6, 0, 0, VisualizerRepresentation.Near));
            }
            for (var i = 0; i < points.Length; i++)
            {
                var a = points[i];
                var b = points[(i + 1) % points.Length];
                var delta = b - a;
                var transform = Matrix4x4.CreateScale(delta.Length() * 2, 1, 1) *
                    Matrix4x4.CreateRotationZ(MathF.Atan2(delta.Y, delta.X)) *
                    Matrix4x4.CreateTranslation((a.X + b.X) / 2, (a.Y + b.Y) / 2, 0);
                commands.Add(new VisualizerDrawCommand(
                    lineId, lineMaterialId, transform, 6, 0, 0, VisualizerRepresentation.Near));
            }
            var frame = new VisualizerFrame(
                new VisualizerView(Vector3.Zero, Matrix4x4.Identity, Matrix4x4.Identity),
                commands);

            await using var visualizer = new Visualizer(new GuiSvgVisualizerBackend(output, animateConnectedPoints: true), catalog);
            var result = await visualizer.RenderAsync(frame);

            var svg = await File.ReadAllTextAsync(output);
            Assert.Equal(8, result.SubmittedDrawCount);
            Assert.Contains("<path", svg);
            Assert.Contains("fill=\"#59CCEB\"", svg);
            Assert.Contains("fill=\"#7DE8BF\"", svg);
            Assert.Contains("<animate", svg);
            Assert.Contains("<animateMotion", svg);
        }
        finally
        {
            if (File.Exists(output)) File.Delete(output);
        }
    }
}
