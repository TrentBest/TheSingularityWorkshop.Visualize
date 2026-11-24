using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using TheSingularityWorkshop.Visualize;

var output = args.Length > 0 ? Path.GetFullPath(args[0]) : Path.GetFullPath("artifacts/tactical-map");
Directory.CreateDirectory(output);

const string background = "#101923";
const string panel = "#192733";
const string grid = "#263845";
const string room = "#344B5A";
const string roomEdge = "#78909C";
const string accent = "#E7B968";
const string text = "#E8EEF1";
const string muted = "#9FB0B9";
var primitives = new List<VisualizerSvgPrimitive>
{
    new VisualizerSvgRectangle("map-panel", 24, 24, 1152, 712, panel),
    new VisualizerSvgText("map-title", 52, 62, "TACTICAL ENCOUNTER", accent, 18, "sans-serif", "bold"),
    new VisualizerSvgText("map-subtitle", 52, 86, "Illustrative vector scene · stable identities · editable source model", muted, 11)
};

// The grid is authored as ordinary SVG primitives. It is a visual aid, not a simulation grid.
for (var x = 72; x <= 1128; x += 32)
    primitives.Add(new VisualizerSvgRectangle($"grid-v-{x}", x, 112, 1, 576, grid));
for (var y = 112; y <= 688; y += 32)
    primitives.Add(new VisualizerSvgRectangle($"grid-h-{y}", 72, y, 1056, 1, grid));

// Stable encounter locations. These IDs are presentation identities supplied by the authoring model.
var locations = new[]
{
    new { Id = 1001UL, Name = "ENTRY", X = 104d, Y = 184d, W = 256d, H = 176d },
    new { Id = 1002UL, Name = "SECURITY", X = 488d, Y = 152d, W = 224d, H = 208d },
    new { Id = 1003UL, Name = "LABORATORY", X = 808d, Y = 184d, W = 256d, H = 176d },
    new { Id = 1004UL, Name = "SERVICE", X = 168d, Y = 456d, W = 256d, H = 160d },
    new { Id = 1005UL, Name = "CONTAINMENT", X = 552d, Y = 424d, W = 320d, H = 224d },
    new { Id = 1006UL, Name = "OBSERVATION", X = 936d, Y = 456d, W = 160d, H = 160d }
};
foreach (var location in locations)
{
    var metadata = new Dictionary<string, string> { ["data-entity-id"] = location.Id.ToString() };
    primitives.Add(new VisualizerSvgRectangle($"location-{location.Id}", location.X, location.Y,
        location.W, location.H, room, metadata));
    primitives.Add(new VisualizerSvgRectangle($"location-edge-{location.Id}", location.X, location.Y,
        location.W, 3, roomEdge));
    primitives.Add(new VisualizerSvgText($"location-label-{location.Id}", location.X + 14,
        location.Y + 26, location.Name, text, 12, "sans-serif", "bold"));
}

// Door openings and connectors are deliberately explicit authored geometry.
var doors = new[]
{
    (Id: 2001UL, X: 360d, Y: 256d, W: 128d, H: 12d),
    (Id: 2002UL, X: 712d, Y: 256d, W: 96d, H: 12d),
    (Id: 2003UL, X: 280d, Y: 360d, W: 12d, H: 96d),
    (Id: 2004UL, X: 664d, Y: 360d, W: 12d, H: 64d),
    (Id: 2005UL, X: 872d, Y: 520d, W: 64d, H: 12d)
};
foreach (var door in doors)
    primitives.Add(new VisualizerSvgRectangle($"door-{door.Id}", door.X, door.Y, door.W, door.H,
        accent, new Dictionary<string, string> { ["data-entity-id"] = door.Id.ToString() }));

// Two opposing teams and one objective marker. Concentric circles provide a clear token silhouette.
var tokens = new[]
{
    (Id: 3001UL, Label: "A1", X: 224d, Y: 288d, Color: "#55B8A6"),
    (Id: 3002UL, Label: "A2", X: 288d, Y: 288d, Color: "#55B8A6"),
    (Id: 3003UL, Label: "B1", X: 616d, Y: 264d, Color: "#D97972"),
    (Id: 3004UL, Label: "B2", X: 680d, Y: 296d, Color: "#D97972")
};
foreach (var token in tokens)
{
    var metadata = new Dictionary<string, string> { ["data-entity-id"] = token.Id.ToString() };
    primitives.Add(new VisualizerSvgCircle($"token-ring-{token.Id}", token.X, token.Y, 18, "#0B1118", metadata));
    primitives.Add(new VisualizerSvgCircle($"token-{token.Id}", token.X, token.Y, 14, token.Color, metadata));
    primitives.Add(new VisualizerSvgText($"token-label-{token.Id}", token.X, token.Y + 4,
        token.Label, "#101923", 10, "sans-serif", "bold", "middle"));
}
primitives.Add(new VisualizerSvgCircle("objective-ring", 712, 536, 22, "#0B1118",
    new Dictionary<string, string> { ["data-entity-id"] = "4001" }));
primitives.Add(new VisualizerSvgCircle("objective", 712, 536, 16, accent,
    new Dictionary<string, string> { ["data-entity-id"] = "4001" }));
primitives.Add(new VisualizerSvgText("objective-label", 712, 572, "OBJECTIVE", accent, 10,
    "sans-serif", "bold", "middle"));
primitives.Add(new VisualizerSvgText("legend", 52, 716,
    "TEAL  ·  TEAM A       CORAL  ·  TEAM B       GOLD  ·  OBJECTIVE / DOOR       GRID  ·  1 UNIT",
    muted, 10));

var document = new VisualizerSvgDocument(1200, 760, primitives, background, role: "img");
var svg = document.ToSvg();
var repeated = document.ToSvg();
if (!StringComparer.Ordinal.Equals(svg, repeated))
    throw new InvalidOperationException("Identical authored vector scene did not produce deterministic SVG output.");

var svgPath = Path.Combine(output, "tactical-map.svg");
var manifestPath = Path.Combine(output, "tactical-map-manifest.json");
await File.WriteAllTextAsync(svgPath, svg, new UTF8Encoding(false));
var manifest = new
{
    schemaVersion = 1,
    fixture = "tactical-map-authoring",
    fixtureKind = "illustrative-vector-scene-not-game-state",
    viewport = new { width = 1200, height = 760 },
    locationIds = locations.Select(item => item.Id).ToArray(),
    doorIds = doors.Select(item => item.Id).ToArray(),
    tokenIds = tokens.Select(item => item.Id).ToArray(),
    objectiveId = 4001UL,
    primitiveCount = primitives.Count,
    locationCount = locations.Length,
    doorCount = doors.Length,
    tokenCount = tokens.Length,
    backend = "VisualizerSvgDocument",
    execution = "CPU/static SVG",
    gpuExecution = false,
    deterministicRepeat = true,
    output = new
    {
        file = Path.GetFileName(svgPath),
        bytes = Encoding.UTF8.GetByteCount(svg),
        sha256 = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(svg))).ToLowerInvariant()
    }
};
await File.WriteAllTextAsync(manifestPath,
    JsonSerializer.Serialize(manifest, new JsonSerializerOptions { WriteIndented = true }) + Environment.NewLine);
Console.WriteLine($"Generated {svgPath}");
Console.WriteLine($"Generated {manifestPath}");
Console.WriteLine($"Locations: {locations.Length}; doors: {doors.Length}; tokens: {tokens.Length}; SVG primitives: {primitives.Count}");
