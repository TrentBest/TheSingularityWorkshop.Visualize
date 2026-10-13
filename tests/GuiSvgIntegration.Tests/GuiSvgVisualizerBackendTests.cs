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
            var geometryId = new VisualizerResourceId(1);
            var materialId = new VisualizerResourceId(2);
            catalog.SetGeometry(geometryId, new VisualizerGeometry(1,
            [
                new VisualizerVertex(new Vector3(-0.25f, -0.2f, 0), Vector3.UnitZ, Vector2.Zero, Vector4.One),
                new VisualizerVertex(new Vector3(0.25f, -0.2f, 0), Vector3.UnitZ, Vector2.Zero, Vector4.One),
                new VisualizerVertex(new Vector3(0, 0.25f, 0), Vector3.UnitZ, Vector2.Zero, Vector4.One)
            ], [0, 1, 2]));
            catalog.SetMaterial(materialId, new VisualizerMaterial(1, new Vector4(0.25f, 0.75f, 0.9f, 1)));
            var command = new VisualizerDrawCommand(
                geometryId, materialId, Matrix4x4.Identity, 3, 0, 0, VisualizerRepresentation.Near);
            var frame = new VisualizerFrame(
                new VisualizerView(Vector3.Zero, Matrix4x4.Identity, Matrix4x4.Identity),
                [command]);

            await using var visualizer = new Visualizer(new GuiSvgVisualizerBackend(output, animateConnectedPoints: true), catalog);
            var result = await visualizer.RenderAsync(frame);

            var svg = await File.ReadAllTextAsync(output);
            Assert.Equal(1, result.SubmittedDrawCount);
            Assert.Contains("<path", svg);
            Assert.Contains("fill=\"#40BFE6\"", svg);
            Assert.Contains("<animate", svg);
            Assert.Contains("<animateMotion", svg);
        }
        finally
        {
            if (File.Exists(output)) File.Delete(output);
        }
    }
}
