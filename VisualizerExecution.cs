using WorkshopFsmApi = FSM_API.FsmApi;
using TheSingularityWorkshop.FSM_API;

namespace TheSingularityWorkshop.Visualize;

/// <summary>
/// Owns Visualize execution contexts and FSM handles. The host advances this work by
/// updating <see cref="ProcessingGroup"/> through FSM_API; this type creates no scheduler.
/// </summary>
public sealed class VisualizerExecution : IDisposable
{
    private readonly object _sync = new();
    private readonly Visualizer _visualizer;
    private readonly HashSet<VisualizerExecutionSubmission> _submissions = [];
    private readonly string _definitionName;
    private bool _disposed;

    /// <summary>Creates an execution cohort in the named FSM processing group.</summary>
    public VisualizerExecution(Visualizer visualizer, string processingGroup = "Visualize")
    {
        ArgumentNullException.ThrowIfNull(visualizer);
        ArgumentException.ThrowIfNullOrWhiteSpace(processingGroup);

        _visualizer = visualizer;
        ProcessingGroup = processingGroup;
        _definitionName = $"Visualize.FrameSubmission.{Guid.NewGuid():N}";

        WorkshopFsmApi.Create.CreateProcessingGroup(ProcessingGroup);
        WorkshopFsmApi.Create.CreateFiniteStateMachine(_definitionName, processRate: -1, processingGroup: ProcessingGroup)
            .State("Queued", null, context => ((VisualizerExecutionContext)context).MarkReady(), null)
            .State("Preparing", context => ((VisualizerExecutionContext)context).BeginSubmission(), null, null)
            .State("Submitting", null, context => ((VisualizerExecutionContext)context).ObserveCompletion(), null)
            .State("Completed", null, null, null)
            .State("Failed", null, null, null)
            .State("Cancelled", null, null, null)
            .WithInitialState("Queued")
            .Transition("Queued", "Preparing", context => ((VisualizerExecutionContext)context).IsReady)
            .Transition("Preparing", "Submitting", _ => true)
            .Transition("Submitting", "Completed", context => ((VisualizerExecutionContext)context).IsCompleted)
            .Transition("Submitting", "Failed", context => ((VisualizerExecutionContext)context).IsFailed)
            .Transition("Submitting", "Cancelled", context => ((VisualizerExecutionContext)context).IsCancelled)
            .BuildDefinition();
    }

    /// <summary>The processing group the host must advance to progress Visualize work.</summary>
    public string ProcessingGroup { get; }

    /// <summary>Creates a context and FSM handle for one independently progressing frame.</summary>
    public VisualizerExecutionSubmission Submit(
        VisualizerFrame frame,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(frame);
        lock (_sync)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            var context = new VisualizerExecutionContext(_visualizer, frame, cancellationToken);
            var handle = WorkshopFsmApi.Create.CreateInstance(_definitionName, context, ProcessingGroup);
            var submission = new VisualizerExecutionSubmission(this, context, handle);
            _submissions.Add(submission);
            return submission;
        }
    }

    internal void Release(VisualizerExecutionSubmission submission)
    {
        lock (_sync)
        {
            if (!_submissions.Remove(submission))
                return;

            submission.Context.IsValid = false;
            WorkshopFsmApi.Interact.DestroyInstance(submission.Handle);
        }
    }

    /// <summary>Destroys all owned handles and this execution's FSM definition.</summary>
    public void Dispose()
    {
        lock (_sync)
        {
            if (_disposed)
                return;

            _disposed = true;
            foreach (var submission in _submissions.ToArray())
            {
                submission.Context.IsValid = false;
                WorkshopFsmApi.Interact.DestroyInstance(submission.Handle);
                submission.MarkReleased();
            }

            _submissions.Clear();
            WorkshopFsmApi.Interact.DestroyFiniteStateMachine(_definitionName, ProcessingGroup);
        }
    }
}

/// <summary>Observable state and result for one Visualize frame execution.</summary>
public sealed class VisualizerExecutionSubmission : IDisposable
{
    private readonly VisualizerExecution _owner;
    private bool _released;

    internal VisualizerExecutionSubmission(
        VisualizerExecution owner,
        VisualizerExecutionContext context,
        FSMHandle handle)
    {
        _owner = owner;
        Context = context;
        Handle = handle;
    }

    internal VisualizerExecutionContext Context { get; }
    internal FSMHandle Handle { get; }

    /// <summary>Gets the FSM state reached by this submission.</summary>
    public string State => Handle.CurrentState;

    /// <summary>Gets whether the submission reached a terminal state.</summary>
    public bool IsTerminal => State is "Completed" or "Failed" or "Cancelled";

    /// <summary>Gets the backend result after successful completion.</summary>
    public VisualizerFrameResult? Result => Context.Result;

    /// <summary>Gets the backend exception after failure.</summary>
    public Exception? Error => Context.Error;

    /// <summary>Explicitly releases this submission's FSM handle.</summary>
    public void Dispose()
    {
        if (_released)
            return;
        _owner.Release(this);
        _released = true;
    }

    internal void MarkReleased() => _released = true;
}

internal sealed class VisualizerExecutionContext : IStateContext
{
    private readonly Visualizer _visualizer;
    private readonly VisualizerFrame _frame;
    private readonly CancellationToken _cancellationToken;
    private Task<VisualizerFrameResult>? _submission;

    internal VisualizerExecutionContext(
        Visualizer visualizer,
        VisualizerFrame frame,
        CancellationToken cancellationToken)
    {
        _visualizer = visualizer;
        _frame = frame;
        _cancellationToken = cancellationToken;
        Name = $"VisualizerFrame:{Guid.NewGuid():N}";
        IsValid = true;
    }

    public string Name { get; set; }
    public bool IsValid { get; set; }
    internal bool IsReady { get; private set; }
    internal bool IsCompleted { get; private set; }
    internal bool IsFailed { get; private set; }
    internal bool IsCancelled { get; private set; }
    internal VisualizerFrameResult? Result { get; private set; }
    internal Exception? Error { get; private set; }

    internal void MarkReady() => IsReady = true;

    internal void BeginSubmission()
    {
        try
        {
            _submission = _visualizer.RenderAsync(_frame, _cancellationToken).AsTask();
        }
        catch (Exception exception)
        {
            _submission = Task.FromException<VisualizerFrameResult>(exception);
        }
    }

    internal void ObserveCompletion()
    {
        if (_submission is not { IsCompleted: true } submission)
            return;

        if (submission.IsCanceled)
        {
            IsCancelled = true;
            return;
        }

        if (submission.IsFaulted)
        {
            Error = submission.Exception?.GetBaseException();
            IsFailed = true;
            return;
        }

        Result = submission.Result;
        IsCompleted = true;
    }
}
