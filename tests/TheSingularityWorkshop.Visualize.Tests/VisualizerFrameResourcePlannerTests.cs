using System.Numerics;
using TheSingularityWorkshop.Visualize;
using TheSingularityWorkshop.Visualize.GPU;
using Xunit;

namespace TheSingularityWorkshop.Visualize.Tests;

public sealed class VisualizerFrameResourcePlannerTests
{
    [Fact]
    public void Plan_DeduplicatesResourcesAndPreservesDrawOrder()
    {
        var catalog = CreateCatalog();
        var frame = new VisualizerFrame(View(),
        [
            Command(geometry: 10),
            Command(geometry: 11),
            Command(geometry: 10, instanced: false)
        ]);

        var plan = VisualizerFrameResourcePlanner.Plan(frame, catalog);

        Assert.Equal(2, plan.Geometries.Count);
        Assert.Single(plan.Materials);
        Assert.Single(plan.Instances);
        Assert.Equal(3, plan.Draws.Count);

        Assert.Equal(new[] { 10UL, 11UL }, plan.Geometries.Select(x => x.Id.Value));
        Assert.Equal(new[] { 0, 1, 0 }, plan.Draws.Select(x => x.GeometryResourceIndex));
        Assert.All(plan.Draws, draw => Assert.Equal(0, draw.MaterialResourceIndex));
        Assert.Equal(new int?[] { 0, 0, null }, plan.Draws.Select(x => x.InstanceResourceIndex));
        Assert.Equal(new[] { 0, 1, 2 }, plan.Draws.Select(x => x.CommandIndex));
        Assert.Equal(new[] { 10UL, 11UL, 10UL }, plan.Draws.Select(x => x.Resolved.Command.Geometry.Value));
    }

    [Fact]
    public void Plan_RejectsAResourceRevisionChangingDuringFrameResolution()
    {
        var catalog = CreateCatalog();
        var provider = new ChangingGeometryProvider(catalog);
        var frame = new VisualizerFrame(View(), [Command(geometry: 10), Command(geometry: 10)]);

        var error = Assert.Throws<InvalidOperationException>(
            () => VisualizerFrameResourcePlanner.Plan(frame, provider));

        Assert.Contains("geometry resource 10", error.Message);
        Assert.Contains("revision 1", error.Message);
        Assert.Contains("revision 2", error.Message);
        Assert.Contains("coherent snapshot", error.Message);
    }

    private static VisualizerResourceCatalog CreateCatalog()
    {
        var catalog = new VisualizerResourceCatalog();
        var vertices = new[]
        {
            new VisualizerVertex(Vector3.Zero, Vector3.UnitZ, Vector2.Zero, Vector4.One),
            new VisualizerVertex(Vector3.UnitX, Vector3.UnitZ, Vector2.UnitX, Vector4.One),
            new VisualizerVertex(Vector3.UnitY, Vector3.UnitZ, Vector2.UnitY, Vector4.One)
        };
        catalog.SetGeometry(new VisualizerResourceId(10), new VisualizerGeometry(1, vertices, [0, 1, 2]));
        catalog.SetGeometry(new VisualizerResourceId(11), new VisualizerGeometry(1, vertices, [0, 1, 2]));
        catalog.SetMaterial(new VisualizerResourceId(20), new VisualizerMaterial(1, Vector4.One));
        catalog.SetInstances(new VisualizerResourceId(30), new VisualizerInstanceBuffer(1,
        [
            new VisualizerInstance(Matrix4x4.Identity, Vector4.One),
            new VisualizerInstance(Matrix4x4.CreateTranslation(1, 0, 0), Vector4.One)
        ]));
        return catalog;
    }

    private static VisualizerView View() => new(Vector3.Zero, Matrix4x4.Identity, Matrix4x4.Identity);

    private static VisualizerDrawCommand Command(ulong geometry, bool instanced = true) =>
        new(
            new VisualizerResourceId(geometry),
            new VisualizerResourceId(20),
            Matrix4x4.Identity,
            3,
            0,
            0,
            VisualizerRepresentation.Near,
            instanced ? new VisualizerResourceId(30) : default);

    private sealed class ChangingGeometryProvider(IVisualizerResourceProvider inner) : IVisualizerResourceProvider
    {
        private int _geometryReads;

        public bool TryGetGeometry(VisualizerResourceId id, out VisualizerGeometry? geometry)
        {
            if (id.Value == 10 && ++_geometryReads == 2)
            {
                geometry = new VisualizerGeometry(2,
                [
                    new VisualizerVertex(Vector3.Zero, Vector3.UnitZ, Vector2.Zero, Vector4.One),
                    new VisualizerVertex(Vector3.UnitX, Vector3.UnitZ, Vector2.UnitX, Vector4.One),
                    new VisualizerVertex(Vector3.UnitY, Vector3.UnitZ, Vector2.UnitY, Vector4.One)
                ], [0, 1, 2]);
                return true;
            }

            return inner.TryGetGeometry(id, out geometry);
        }

        public bool TryGetMaterial(VisualizerResourceId id, out VisualizerMaterial? material) =>
            inner.TryGetMaterial(id, out material);

        public bool TryGetInstances(VisualizerResourceId id, out VisualizerInstanceBuffer? instances) =>
            inner.TryGetInstances(id, out instances);
    }
}
