using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace TheSingularityWorkshop.Visualize;

/// <summary>A small, deterministic SVG scene owned by Visualize, independent of GUI packages.</summary>
/// <remarks>This initial primitive contract is intentionally narrow; it is not a GUI tree or a full scene graph.</remarks>
public sealed class VisualizerSvgDocument
{
    private static readonly Regex ColorPattern = new(@"^(#[0-9A-Fa-f]{6}([0-9A-Fa-f]{2})?|none|currentColor)$", RegexOptions.CultureInvariant);
    private static readonly Regex IdPattern = new(@"^[A-Za-z_][A-Za-z0-9_.-]*$", RegexOptions.CultureInvariant);

    /// <summary>Creates a validated SVG document from an ordered set of primitives.</summary>
    /// <param name="width">Finite, positive viewport width.</param>
    /// <param name="height">Finite, positive viewport height.</param>
    /// <param name="primitives">Primitives to snapshot and serialize in order.</param>
    /// <param name="background">SVG background fill, or <c>none</c> for transparency.</param>
    /// <param name="role">Optional accessible SVG role.</param>
    public VisualizerSvgDocument(double width, double height, IEnumerable<VisualizerSvgPrimitive> primitives, string background = "none", string? role = null)
    {
        if (!double.IsFinite(width) || width <= 0)
            throw new ArgumentOutOfRangeException(nameof(width), "SVG width must be finite and greater than zero.");
        if (!double.IsFinite(height) || height <= 0)
            throw new ArgumentOutOfRangeException(nameof(height), "SVG height must be finite and greater than zero.");
        ArgumentNullException.ThrowIfNull(primitives);
        ValidateColor(background, nameof(background));
        if (role is not null && (role.Length == 0 || role.Any(char.IsControl)))
            throw new ArgumentException("SVG role must be non-empty and contain no control characters.", nameof(role));

        Width = width;
        Height = height;
        Background = background;
        Role = role;
        var snapshot = primitives.Select(SnapshotPrimitive).ToArray();
        var ids = new HashSet<string>(StringComparer.Ordinal);
        if (background != "none")
            ids.Add("visualize-background");
        foreach (var primitive in snapshot)
        {
            ArgumentNullException.ThrowIfNull(primitive);
            ValidateId(primitive.Id);
            if (!ids.Add(primitive.Id))
                throw new ArgumentException($"Duplicate SVG primitive id '{primitive.Id}'.", nameof(primitives));
            primitive.Validate();
        }
        Primitives = Array.AsReadOnly(snapshot);
    }

    private static VisualizerSvgPrimitive SnapshotPrimitive(VisualizerSvgPrimitive primitive)
    {
        ArgumentNullException.ThrowIfNull(primitive);
        IReadOnlyDictionary<string, string>? attributes = primitive.Attributes is null
            ? null
            : new ReadOnlyDictionary<string, string>(
                new Dictionary<string, string>(primitive.Attributes, StringComparer.Ordinal));

        return primitive switch
        {
            VisualizerSvgRectangle rectangle => new VisualizerSvgRectangle(
                rectangle.Id, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, rectangle.Fill, attributes),
            VisualizerSvgPolygon polygon => new VisualizerSvgPolygon(
                polygon.Id, polygon.Points, polygon.Fill, attributes),
            _ => throw new NotSupportedException(
                $"SVG primitive type '{primitive.GetType().FullName}' is not supported by this document.")
        };
    }

    /// <summary>Gets the SVG viewport width.</summary>
    public double Width { get; }
    /// <summary>Gets the SVG viewport height.</summary>
    public double Height { get; }
    /// <summary>Gets the background fill, or <c>none</c> when transparent.</summary>
    public string Background { get; }
    /// <summary>Gets the optional accessible role.</summary>
    public string? Role { get; }
    /// <summary>Gets the validated, immutable ordered primitive snapshot.</summary>
    public IReadOnlyList<VisualizerSvgPrimitive> Primitives { get; }

    /// <summary>Serializes the scene as stable UTF-8-compatible SVG text with invariant numeric formatting.</summary>
    public string ToSvg()
    {
        var settings = new XmlWriterSettings
        {
            OmitXmlDeclaration = true,
            Indent = false,
            NewLineHandling = NewLineHandling.None,
            ConformanceLevel = ConformanceLevel.Document
        };
        var builder = new StringBuilder();
        using (var writer = XmlWriter.Create(builder, settings))
        {
            writer.WriteStartElement("svg", "http://www.w3.org/2000/svg");
            writer.WriteAttributeString("width", Format(Width));
            writer.WriteAttributeString("height", Format(Height));
            writer.WriteAttributeString("viewBox", $"0 0 {Format(Width)} {Format(Height)}");
            if (Role is not null)
                writer.WriteAttributeString("role", Role);
            if (Background != "none")
            {
                writer.WriteStartElement("rect");
                writer.WriteAttributeString("id", "visualize-background");
                writer.WriteAttributeString("x", "0");
                writer.WriteAttributeString("y", "0");
                writer.WriteAttributeString("width", Format(Width));
                writer.WriteAttributeString("height", Format(Height));
                writer.WriteAttributeString("fill", Background);
                writer.WriteEndElement();
            }

            foreach (var primitive in Primitives)
                primitive.WriteTo(writer);
            writer.WriteEndElement();
        }
        return builder.ToString();
    }

    internal static string Format(double value) => value.ToString("R", CultureInfo.InvariantCulture);

    internal static void ValidateColor(string color, string parameterName)
    {
        ArgumentNullException.ThrowIfNull(color, parameterName);
        if (!ColorPattern.IsMatch(color))
            throw new ArgumentException("Color must be #RRGGBB, #RRGGBBAA, none, or currentColor.", parameterName);
    }

    internal static void ValidateId(string id)
    {
        ArgumentNullException.ThrowIfNull(id);
        if (!IdPattern.IsMatch(id))
            throw new ArgumentException("SVG ids must begin with a letter or underscore and contain only letters, digits, underscore, period, or hyphen.", nameof(id));
    }
}

/// <summary>Base type for Visualize-owned SVG drawing primitives.</summary>
public abstract record VisualizerSvgPrimitive(string Id, string Fill, IReadOnlyDictionary<string, string>? Attributes = null)
{
    internal void ValidateCommon()
    {
        VisualizerSvgDocument.ValidateId(Id);
        VisualizerSvgDocument.ValidateColor(Fill, nameof(Fill));
        if (Attributes is null) return;
        foreach (var attribute in Attributes)
        {
            if (attribute.Key is not ("fill-opacity" or "data-representation" or "data-entity-id"))
                throw new ArgumentException($"SVG primitive attribute '{attribute.Key}' is not allowed.", nameof(Attributes));
            if (string.IsNullOrEmpty(attribute.Value) || attribute.Value.Any(char.IsControl))
                throw new ArgumentException($"SVG primitive attribute '{attribute.Key}' has an invalid value.", nameof(Attributes));
            if (attribute.Key == "fill-opacity" &&
                (!double.TryParse(attribute.Value, System.Globalization.NumberStyles.Float, CultureInfo.InvariantCulture, out var opacity) ||
                 !double.IsFinite(opacity) || opacity < 0 || opacity > 1))
                throw new ArgumentOutOfRangeException(nameof(Attributes), "SVG fill-opacity must be an invariant-culture number between zero and one.");
        }
    }

    internal void WriteAttributes(XmlWriter writer)
    {
        if (Attributes is null) return;
        foreach (var attribute in Attributes.OrderBy(pair => pair.Key, StringComparer.Ordinal))
            writer.WriteAttributeString(attribute.Key, attribute.Value);
    }

    internal abstract void Validate();
    internal abstract void WriteTo(XmlWriter writer);
}

/// <summary>A filled rectangle in SVG viewport coordinates.</summary>
public sealed record VisualizerSvgRectangle(string Id, double X, double Y, double Width, double Height, string Fill, IReadOnlyDictionary<string, string>? Attributes = null)
    : VisualizerSvgPrimitive(Id, Fill, Attributes)
{
    internal override void Validate()
    {
        ValidateCommon();
        if (!double.IsFinite(X) || !double.IsFinite(Y))
            throw new ArgumentOutOfRangeException(nameof(X), "Rectangle coordinates must be finite.");
        if (!double.IsFinite(Width) || Width < 0)
            throw new ArgumentOutOfRangeException(nameof(Width), "Rectangle width must be finite and non-negative.");
        if (!double.IsFinite(Height) || Height < 0)
            throw new ArgumentOutOfRangeException(nameof(Height), "Rectangle height must be finite and non-negative.");
    }

    internal override void WriteTo(XmlWriter writer)
    {
        writer.WriteStartElement("rect");
        writer.WriteAttributeString("id", Id);
        writer.WriteAttributeString("x", VisualizerSvgDocument.Format(X));
        writer.WriteAttributeString("y", VisualizerSvgDocument.Format(Y));
        writer.WriteAttributeString("width", VisualizerSvgDocument.Format(Width));
        writer.WriteAttributeString("height", VisualizerSvgDocument.Format(Height));
        writer.WriteAttributeString("fill", Fill);
        WriteAttributes(writer);
        writer.WriteEndElement();
    }
}

/// <summary>A filled triangle/polygon described by an ordered list of viewport-coordinate pairs.</summary>
public sealed record VisualizerSvgPolygon : VisualizerSvgPrimitive
{
    /// <summary>Creates a filled polygon from viewport-coordinate points.</summary>
    /// <param name="id">Unique SVG identifier.</param>
    /// <param name="points">Ordered polygon vertices.</param>
    /// <param name="fill">Validated SVG fill color.</param>
    /// <param name="attributes">Optional allow-listed metadata attributes.</param>
    public VisualizerSvgPolygon(string id, IEnumerable<(double X, double Y)> points, string fill, IReadOnlyDictionary<string, string>? attributes = null)
        : base(id, fill, attributes)
    {
        ArgumentNullException.ThrowIfNull(points);
        Points = Array.AsReadOnly(points.ToArray());
    }

    /// <summary>Gets the immutable ordered viewport-coordinate vertices.</summary>
    public IReadOnlyList<(double X, double Y)> Points { get; }

    internal override void Validate()
    {
        ValidateCommon();
        if (Points.Count < 3)
            throw new ArgumentException("An SVG polygon requires at least three points.", nameof(Points));
        if (Points.Any(point => !double.IsFinite(point.X) || !double.IsFinite(point.Y)))
            throw new ArgumentOutOfRangeException(nameof(Points), "Polygon coordinates must be finite.");
    }

    internal override void WriteTo(XmlWriter writer)
    {
        writer.WriteStartElement("polygon");
        writer.WriteAttributeString("id", Id);
        writer.WriteAttributeString("points", string.Join(" ", Points.Select(point =>
            $"{VisualizerSvgDocument.Format(point.X)},{VisualizerSvgDocument.Format(point.Y)}")));
        writer.WriteAttributeString("fill", Fill);
        WriteAttributes(writer);
        writer.WriteEndElement();
    }
}
