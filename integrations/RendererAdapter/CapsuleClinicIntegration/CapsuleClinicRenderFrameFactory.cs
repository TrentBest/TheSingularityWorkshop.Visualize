using System.Numerics;
using TheSingularityWorkshop.Experiences.CapsuleClinic;
using TheSingularityWorkshop.Renderer;
using TheSingularityWorkshop.Visualize;

namespace TheSingularityWorkshop.Visualize.CapsuleClinic;

/// <summary>Creates a deterministic Renderer frame and its resource catalog from an authoritative Capsule Clinic snapshot.</summary>
/// <remarks>This is presentation mapping only. It does not advance game rules, mutate the snapshot, or authorize player intents.</remarks>
public static class CapsuleClinicRenderFrameFactory
{
    private const float CellSize = 0.035f;

    public static CapsuleClinicRenderFrame Create(CapsuleArenaSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        var geometryId = new RenderResourceId(1);
        var emptyMaterialId = new RenderResourceId(2);
        var redMaterialId = new RenderResourceId(3);
        var blueMaterialId = new RenderResourceId(4);
        var yellowMaterialId = new RenderResourceId(5);
        var purpleMaterialId = new RenderResourceId(6);
        var catalog = new VisualizerResourceCatalog();
        var vertices = new[]
        {
            new VisualizerVertex(new Vector3(-0.5f, -0.5f, 0), Vector3.UnitZ, Vector2.Zero, Vector4.One),
            new VisualizerVertex(new Vector3(0.5f, -0.5f, 0), Vector3.UnitZ, Vector2.UnitX, Vector4.One),
            new VisualizerVertex(new Vector3(0.5f, 0.5f, 0), Vector3.UnitZ, Vector2.One, Vector4.One),
            new VisualizerVertex(new Vector3(-0.5f, 0.5f, 0), Vector3.UnitZ, Vector2.UnitY, Vector4.One)
        };
        catalog.SetGeometry(new VisualizerResourceId(geometryId.Value),
            new VisualizerGeometry(1, vertices, [0, 1, 2, 0, 2, 3]));
        catalog.SetMaterial(new VisualizerResourceId(emptyMaterialId.Value),
            new VisualizerMaterial(1, new Vector4(0.08f, 0.13f, 0.18f, 1)));
        catalog.SetMaterial(new VisualizerResourceId(redMaterialId.Value),
            new VisualizerMaterial(1, new Vector4(1f, 0.24f, 0.34f, 1)));
        catalog.SetMaterial(new VisualizerResourceId(blueMaterialId.Value),
            new VisualizerMaterial(1, new Vector4(0.22f, 0.65f, 1f, 1)));
        catalog.SetMaterial(new VisualizerResourceId(yellowMaterialId.Value),
            new VisualizerMaterial(1, new Vector4(1f, 0.78f, 0.2f, 1)));
        catalog.SetMaterial(new VisualizerResourceId(purpleMaterialId.Value),
            new VisualizerMaterial(1, new Vector4(0.72f, 0.48f, 1f, 1)));

        RenderResourceId MaterialFor(CapsuleColor color) => color switch
        {
            CapsuleColor.Red => redMaterialId,
            CapsuleColor.Blue => blueMaterialId,
            CapsuleColor.Yellow => yellowMaterialId,
            CapsuleColor.Purple => purpleMaterialId,
            _ => throw new ArgumentOutOfRangeException(nameof(color))
        };

        var commands = new List<RenderDrawCommand>();
        var occupiedCells = 0;
        var activeSegments = 0;
        var entityId = 1UL;
        for (var paneIndex = 0; paneIndex < snapshot.Panes.Count; paneIndex++)
        {
            var pane = snapshot.Panes[paneIndex];
            var (centerX, centerY) = pane.Pane switch
            {
                ArenaPane.LocalUpperLeft => (-0.48f, 0.30f),
                ArenaPane.ComparisonUpperRight => (0.48f, 0.30f),
                ArenaPane.ComparisonLowerLeft => (-0.48f, -0.30f),
                ArenaPane.ComparisonLowerRight => (0.48f, -0.30f),
                _ => throw new ArgumentOutOfRangeException(nameof(snapshot), "Unknown arena pane.")
            };

            for (var y = 0; y < snapshot.BoardHeight; y++)
            for (var x = 0; x < snapshot.BoardWidth; x++)
            {
                var cell = pane.Player?.Board[y * snapshot.BoardWidth + x];
                var materialId = cell is { } settled ? MaterialFor(settled.Color) : emptyMaterialId;
                if (cell.HasValue) occupiedCells++;
                var transform = Matrix4x4.CreateScale(CellSize)
                    * Matrix4x4.CreateTranslation(
                        centerX + (x - (snapshot.BoardWidth - 1) / 2f) * CellSize,
                        centerY + (y - (snapshot.BoardHeight - 1) / 2f) * CellSize,
                        0);
                commands.Add(new RenderDrawCommand(
                    geometryId, materialId, transform, 6, 0, 0,
                    RendererRepresentation.Interactive, entity: new RenderEntityId(entityId++)));
            }

            if (pane.Player?.Active is not { } active)
                continue;

            var pivotX = active.PivotX;
            var pivotY = active.PivotY;
            var partnerX = pivotX + (active.Orientation == CapsuleOrientation.Horizontal ? 1 : 0);
            var partnerY = pivotY + (active.Orientation == CapsuleOrientation.Vertical ? 1 : 0);
            foreach (var segment in new[]
            {
                (X: pivotX, Y: pivotY, Color: active.Pair.PivotColor),
                (X: partnerX, Y: partnerY, Color: active.Pair.PartnerColor)
            })
            {
                if ((uint)segment.X >= (uint)snapshot.BoardWidth ||
                    (uint)segment.Y >= (uint)snapshot.BoardHeight)
                    continue;
                var transform = Matrix4x4.CreateScale(CellSize * 0.88f)
                    * Matrix4x4.CreateTranslation(
                        centerX + (segment.X - (snapshot.BoardWidth - 1) / 2f) * CellSize,
                        centerY + (segment.Y - (snapshot.BoardHeight - 1) / 2f) * CellSize,
                        0.001f);
                commands.Add(new RenderDrawCommand(
                    geometryId, MaterialFor(segment.Color), transform, 6, 0, 0,
                    RendererRepresentation.Interactive, entity: new RenderEntityId(entityId++)));
                activeSegments++;
            }
        }

        var frame = new RenderFrame(
            new RenderView(Vector3.Zero, Matrix4x4.Identity, Matrix4x4.Identity),
            commands);
        return new CapsuleClinicRenderFrame(frame, catalog, snapshot.Panes.Count,
            snapshot.BoardWidth, snapshot.BoardHeight, occupiedCells, activeSegments);
    }
}

/// <summary>The frame and presentation evidence derived from one authoritative snapshot.</summary>
public sealed record CapsuleClinicRenderFrame(
    RenderFrame Frame,
    VisualizerResourceCatalog ResourceCatalog,
    int PaneCount,
    int BoardWidth,
    int BoardHeight,
    int OccupiedCells,
    int ActiveSegments)
{
    public int BaseCellCommands => PaneCount * BoardWidth * BoardHeight;
    public int CommandCount => Frame.Commands.Count;
}
