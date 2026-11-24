using System.Text.Json;
using TheSingularityWorkshop.Renderer;

const double targetFrameRate = 60;

var definitions = new[]
{
    new HorizonDefinition("Contact", 1, 60, RepresentationTarget.Interactive, [0.25, 0.95, 0.95], 3),
    new HorizonDefinition("1 m", 2, 60, RepresentationTarget.Near, [0.36, 0.88, 0.91], 5),
    new HorizonDefinition("2 m", 4, 30, RepresentationTarget.Near, [0.42, 0.82, 0.91], 8),
    new HorizonDefinition("4 m", 8, 15, RepresentationTarget.Distant, [0.49, 0.72, 0.96], 13),
    new HorizonDefinition("16 m", 16, 7.5, RepresentationTarget.Distant, [0.59, 0.63, 0.98], 24),
    new HorizonDefinition("32 m", 32, 3.75, RepresentationTarget.Aggregate, [0.68, 0.56, 0.96], 40),
    new HorizonDefinition("64 m", 64, 1.875, RepresentationTarget.Aggregate, [0.76, 0.51, 0.90], 64),
    new HorizonDefinition("128 m+", 128, 0.9375, RepresentationTarget.Semantic, [0.86, 0.55, 0.78], 100)
};

var horizons = definitions
    .Select(definition => new EventHorizon(
        definition.Name,
        definition.MaximumDistance,
        definition.UpdatesPerSecond))
    .ToArray();

var rules = horizons
    .Select((horizon, index) => new RepresentationHorizonRule(
        horizon,
        definitions[index].Representation))
    .ToArray();

var policy = new ObserverRepresentationPolicy(rules);

int FrameInterval(double updatesPerSecond) =>
    Math.Max(1, (int)Math.Round(targetFrameRate / updatesPerSecond, MidpointRounding.AwayFromZero));

var distances = new[] { 0.5, 1.5, 3d, 6d, 12d, 24d, 48d, 96d };

var probes = distances
    .Select(distance =>
    {
        var decision = policy.Select(distance);
        return new
        {
            distance,
            horizon = decision.HorizonName,
            representation = decision.Representation.ToString(),
            updatesPerSecond = decision.UpdatesPerSecond,
            frameInterval = FrameInterval(decision.UpdatesPerSecond)
        };
    })
    .ToArray();

var matrixOrdinal = 0;
var selectionMatrix = distances
    .SelectMany(distance => Enum.GetValues<RepresentationTarget>().Select(minimumRepresentation =>
    {
        var decision = policy.Select(distance, minimumRepresentation);
        var horizonId = Array.FindIndex(horizons, horizon => ReferenceEquals(horizon, decision.Horizon));
        var fsmState = AdvanceRendererState($"RendererPolicyManifest.Decision.{matrixOrdinal++}", decision.Representation);
        if (fsmState != decision.Representation)
            throw new InvalidOperationException("Renderer FSM state did not accept its selected representation.");
        return new
        {
            distance,
            minimumRepresentation = minimumRepresentation.ToString(),
            horizonId,
            horizon = decision.HorizonName,
            representation = decision.Representation.ToString(),
            fsmState = fsmState.ToString(),
            updatesPerSecond = decision.UpdatesPerSecond,
            frameInterval = FrameInterval(decision.UpdatesPerSecond)
        };
    }))
    .ToArray();

var focusDistance = 64d;
var focusDecision = policy.Select(focusDistance, RepresentationTarget.Interactive);

var manifest = new
{
    schemaVersion = 1,
    generatedBy = "TheSingularityWorkshop.Renderer.ObserverRepresentationPolicy",
    executionBoundary = "Renderer policy is evaluated by this .NET generator; the browser consumes this deterministic manifest. Renderer policy and FSM state transitions are evaluated by this .NET generator; the browser consumes the deterministic manifest and does not run the .NET policy or claim a GPU benchmark.",
    targetFrameRate,
    horizons = definitions.Select((definition, index) =>
    {
        var fsmState = AdvanceRendererState($"RendererPolicyManifest.Horizon.{index}", definition.Representation);
        return new
        {
            id = index,
            name = definition.Name,
            maxDistance = definition.MaximumDistance,
            updatesPerSecond = definition.UpdatesPerSecond,
            frameInterval = FrameInterval(definition.UpdatesPerSecond),
            representation = definition.Representation.ToString(),
            fsmState = fsmState.ToString(),
            color = definition.Color,
            depth = definition.Depth
        };
    }).ToArray(),
    selectionProbes = probes,
    selectionMatrix,
    attentionOverride = new
    {
        distance = focusDistance,
        minimumRepresentation = RepresentationTarget.Interactive.ToString(),
        horizon = focusDecision.HorizonName,
        representation = focusDecision.Representation.ToString(),
        fsmState = AdvanceRendererState("RendererPolicyManifest.AttentionOverride", focusDecision.Representation).ToString(),
        updatesPerSecond = focusDecision.UpdatesPerSecond,
        frameInterval = FrameInterval(focusDecision.UpdatesPerSecond)
    }
};

var outputPath = args.Length > 0
    ? Path.GetFullPath(args[0])
    : Path.GetFullPath("renderer-policy.json");

Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
await File.WriteAllTextAsync(outputPath, JsonSerializer.Serialize(
    manifest,
    new JsonSerializerOptions { WriteIndented = true }) + Environment.NewLine);

Console.WriteLine($"Renderer policy manifest written: {outputPath}");
Console.WriteLine($"Horizon rules: {horizons.Length}; selection decisions: {selectionMatrix.Length}; FSM transitions verified; attention override: {focusDecision.HorizonName}/{focusDecision.Representation}");

static RepresentationTarget AdvanceRendererState(string name, RepresentationTarget desired)
{
    var context = new RendererComputationContext(name, desired);
    using var machine = new RendererComputationMachine(context);
    return machine.Advance();
}

internal sealed record HorizonDefinition(
    string Name,
    double MaximumDistance,
    double UpdatesPerSecond,
    RepresentationTarget Representation,
    double[] Color,
    double Depth);
