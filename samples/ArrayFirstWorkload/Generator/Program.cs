using System.Buffers.Binary;
using System.Globalization;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;

if (args.Length < 2 || args.Length > 3)
{
    Console.Error.WriteLine("Usage: dotnet run --project samples/ArrayFirstWorkload/Generator -- <instance-count> <output-directory> [seed]");
    return 2;
}

if (!int.TryParse(args[0], NumberStyles.None, CultureInfo.InvariantCulture, out var count) || count <= 0)
{
    Console.Error.WriteLine("Instance count must be a positive integer.");
    return 2;
}

var outputDirectory = Path.GetFullPath(args[1]);
var seed = 20261012u;
if (args.Length == 3)
{
    if (uint.TryParse(args[2], NumberStyles.None, CultureInfo.InvariantCulture, out var unsignedSeed))
        seed = unsignedSeed;
    else if (int.TryParse(args[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out var signedSeed))
        seed = unchecked((uint)signedSeed);
    else
    {
        Console.Error.WriteLine("Seed must be a 32-bit unsigned integer or a signed 32-bit integer bit pattern.");
        return 2;
    }
}

Directory.CreateDirectory(outputDirectory);
var vertices = new float[]
{
    0f, -0.5f, 0f,
    0.5f, 0.5f, 0f,
    -0.5f, 0.5f, 0f
};
var indices = new uint[] { 0, 1, 2 };
var vertexBytes = new byte[vertices.Length * sizeof(float)];
for (var i = 0; i < vertices.Length; i++)
    WriteSingle(vertexBytes, i * sizeof(float), vertices[i]);

var indexBytes = new byte[indices.Length * sizeof(uint)];
for (var i = 0; i < indices.Length; i++)
    BinaryPrimitives.WriteUInt32LittleEndian(indexBytes.AsSpan(i * sizeof(uint)), indices[i]);

// Logical instance layout: 16 little-endian float32 values for a row-major
// world transform, followed by 4 little-endian float32 color values (80 bytes).
const int instanceStride = 80;
var instanceBytes = new byte[checked(count * instanceStride)];
var random = new DeterministicRandom(seed);
var nearCount = Math.Max(1, (int)Math.Ceiling(count * 0.1));
for (var i = 0; i < count; i++)
{
    var offset = i * instanceStride;
    var x = random.NextFloat(-100f, 100f);
    var y = random.NextFloat(-60f, 60f);
    var z = i < nearCount ? random.NextFloat(-8f, -1f) : random.NextFloat(-250f, -20f);
    var scale = random.NextFloat(0.15f, 1.25f);

    // System.Numerics-style row-major matrix: translation in M41/M42/M43.
    var matrix = new float[]
    {
        scale, 0f, 0f, 0f,
        0f, scale, 0f, 0f,
        0f, 0f, scale, 0f,
        x, y, z, 1f
    };
    for (var component = 0; component < matrix.Length; component++)
        WriteSingle(instanceBytes, offset + component * sizeof(float), matrix[component]);

    var color = new[]
    {
        random.NextFloat(0.2f, 1f),
        random.NextFloat(0.2f, 1f),
        random.NextFloat(0.2f, 1f),
        1f
    };
    for (var component = 0; component < color.Length; component++)
        WriteSingle(instanceBytes, offset + 64 + component * sizeof(float), color[component]);
}

var vertexPath = Path.Combine(outputDirectory, "vertices.f32le");
var indexPath = Path.Combine(outputDirectory, "indices.u32le");
var instancePath = Path.Combine(outputDirectory, "instances.f32le");
await File.WriteAllBytesAsync(vertexPath, vertexBytes);
await File.WriteAllBytesAsync(indexPath, indexBytes);
await File.WriteAllBytesAsync(instancePath, instanceBytes);

var manifest = new DatasetManifest(
    SchemaVersion: 1,
    Generator: "ArrayFirstWorkload.Generator",
    Seed: seed,
    InstanceCount: count,
    NearInstanceCount: nearCount,
    FarInstanceCount: count - nearCount,
    InstanceStrideBytes: instanceStride,
    VertexCount: vertices.Length / 3,
    IndexCount: indices.Length,
    ByteOrder: "little-endian",
    TransformLayout: "16 float32 values, row-major; translation at offsets 48/52/56",
    ColorLayout: "4 float32 values at byte offset 64",
    Files: new Dictionary<string, FileRecord>
    {
        ["vertices.f32le"] = Describe(vertexPath),
        ["indices.u32le"] = Describe(indexPath),
        ["instances.f32le"] = Describe(instancePath)
    },
    Notes:
    [
        "Deterministic dataset generation only; this tool does not create a GPU device, submit commands, render pixels, or benchmark a backend.",
        "The first 10% (rounded up, minimum one) is the near cohort; cohort membership is by array index.",
        "Logical matrix layout must be explicitly adapted and documented for each graphics API."
    ]);

var manifestPath = Path.Combine(outputDirectory, "dataset-manifest.json");
await using (var stream = File.Create(manifestPath))
    await JsonSerializer.SerializeAsync(stream, manifest, new JsonSerializerOptions { WriteIndented = true });

Console.WriteLine($"Generated {count.ToString(CultureInfo.InvariantCulture)} instances ({instanceBytes.Length.ToString(CultureInfo.InvariantCulture)} bytes).");
Console.WriteLine($"Near cohort: {nearCount}; far cohort: {count - nearCount}; seed: {seed}.");
Console.WriteLine($"Manifest: {manifestPath}");
return 0;

static void WriteSingle(byte[] target, int offset, float value) =>
    BinaryPrimitives.WriteInt32LittleEndian(target.AsSpan(offset, sizeof(float)), BitConverter.SingleToInt32Bits(value));

static FileRecord Describe(string path)
{
    var bytes = File.ReadAllBytes(path);
    return new FileRecord(Path.GetFileName(path), bytes.Length, Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant());
}

sealed record FileRecord(string Name, long Bytes, string Sha256);

sealed record DatasetManifest(
    int SchemaVersion,
    string Generator,
    uint Seed,
    int InstanceCount,
    int NearInstanceCount,
    int FarInstanceCount,
    int InstanceStrideBytes,
    int VertexCount,
    int IndexCount,
    string ByteOrder,
    string TransformLayout,
    string ColorLayout,
    Dictionary<string, FileRecord> Files,
    string[] Notes);

sealed class DeterministicRandom
{
    private uint _state;

    public DeterministicRandom(uint seed) => _state = seed == 0 ? 0x9E3779B9u : seed;

    public uint NextUInt()
    {
        // xorshift32: fixed algorithm so datasets don't depend on System.Random implementation.
        var x = _state;
        x ^= x << 13;
        x ^= x >> 17;
        x ^= x << 5;
        _state = x;
        return x;
    }

    public float NextFloat(float minimum, float maximum)
    {
        var unit = (NextUInt() >> 8) * (1f / 16777216f);
        return minimum + (maximum - minimum) * unit;
    }
}
