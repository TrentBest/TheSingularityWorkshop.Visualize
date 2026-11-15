namespace TheSingularityWorkshop.Visualize;

/// <summary>
/// Identifies one immutable, complete world-scene snapshot independently of its observer view.
/// </summary>
/// <remarks>
/// Replacing a snapshot with a higher revision for the same scene identity is the update contract.
/// This first version intentionally does not define patch delivery, object deletion messages, or
/// cross-process persistence. Each snapshot is complete: objects omitted from the next revision
/// are absent from that revision.
/// </remarks>
public sealed class VisualizerSceneSnapshot
{
    private readonly VisualizerSceneObject[] _objects;

    /// <summary>Creates a complete scene snapshot with deterministic object ordering.</summary>
    public VisualizerSceneSnapshot(
        string sceneIdentity,
        ulong revision,
        IEnumerable<VisualizerSceneObject> objects)
    {
        if (string.IsNullOrWhiteSpace(sceneIdentity))
            throw new ArgumentException("A non-empty stable scene identity is required.", nameof(sceneIdentity));
        if (revision == 0)
            throw new ArgumentOutOfRangeException(nameof(revision), "Scene revisions start at one.");
        ArgumentNullException.ThrowIfNull(objects);

        var snapshot = objects.ToArray();
        if (snapshot.Any(item => item is null))
            throw new ArgumentException("Scene objects cannot contain null entries.", nameof(objects));
        if (snapshot.Select(item => item.Entity).Distinct().Count() != snapshot.Length)
            throw new ArgumentException("Every scene object must have a unique stable entity identity.", nameof(objects));

        Array.Sort(snapshot, static (left, right) =>
        {
            var layer = left.Layer.CompareTo(right.Layer);
            return layer != 0 ? layer : left.Entity.Value.CompareTo(right.Entity.Value);
        });

        SceneIdentity = sceneIdentity;
        Revision = revision;
        _objects = snapshot;
        Objects = Array.AsReadOnly(_objects);
    }

    /// <summary>Gets the stable identity shared by every revision of this scene.</summary>
    public string SceneIdentity { get; }

    /// <summary>Gets the monotonically increasing revision supplied by the scene owner.</summary>
    public ulong Revision { get; }

    /// <summary>Gets the complete immutable object set, ordered by layer then entity identity.</summary>
    public IReadOnlyList<VisualizerSceneObject> Objects { get; }

    /// <summary>
    /// Creates an observer-specific frame without changing world-space object transforms or scene revision.
    /// Hidden objects and objects outside <paramref name="visibleLayers"/> are omitted.
    /// </summary>
    public VisualizerFrame CreateFrame(
        VisualizerView view,
        IReadOnlySet<int>? visibleLayers = null)
    {
        var commands = new List<VisualizerDrawCommand>();
        foreach (var item in _objects)
        {
            if (!item.IsVisible || (visibleLayers is not null && !visibleLayers.Contains(item.Layer)))
                continue;

            commands.AddRange(item.Commands);
        }

        return new VisualizerFrame(view, commands);
    }
}

/// <summary>One persistent scene object and its ordered manifestation commands.</summary>
/// <remarks>
/// Entity identity belongs to the object; draw commands are disposable manifestation details and may
/// change in number or topology between complete scene revisions.
/// </remarks>
public sealed class VisualizerSceneObject
{
    private readonly VisualizerDrawCommand[] _commands;

    /// <summary>Creates a scene object with stable identity, layer, visibility, and draw commands.</summary>
    public VisualizerSceneObject(
        VisualizerEntityId entity,
        int layer,
        bool isVisible,
        IEnumerable<VisualizerDrawCommand> commands)
    {
        if (!entity.IsValid)
            throw new ArgumentException("A valid persistent entity identity is required.", nameof(entity));
        if (layer < 0)
            throw new ArgumentOutOfRangeException(nameof(layer));
        ArgumentNullException.ThrowIfNull(commands);

        _commands = commands.ToArray();
        if (_commands.Length == 0)
            throw new ArgumentException("A scene object must provide at least one manifestation command.", nameof(commands));
        if (_commands.Any(command => command.Entity != entity))
            throw new ArgumentException("Every manifestation command must carry its owning scene object's entity identity.", nameof(commands));

        Entity = entity;
        Layer = layer;
        IsVisible = isVisible;
        Commands = Array.AsReadOnly(_commands);
    }

    /// <summary>Gets the persistent identity of this object, independent of its draw commands.</summary>
    public VisualizerEntityId Entity { get; }

    /// <summary>Gets the non-negative compositing layer; lower layers are emitted first.</summary>
    public int Layer { get; }

    /// <summary>Gets whether this object participates in generated frames.</summary>
    public bool IsVisible { get; }

    /// <summary>Gets the ordered manifestation commands for this object.</summary>
    public IReadOnlyList<VisualizerDrawCommand> Commands { get; }
}
