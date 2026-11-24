using System.Xml.Linq;
using TheSingularityWorkshop.Visualize;
using Xunit;

namespace TheSingularityWorkshop.Visualize.Tests;

public sealed class VisualizerSvgDocumentTests
{
    [Fact]
    public void SerializationIsDeterministicAndProducesParseableXml()
    {
        var document = new VisualizerSvgDocument(
            320, 200,
            [
                new VisualizerSvgRectangle("panel", 1.25, 2, 30, 40, "#112233"),
                new VisualizerSvgPolygon("triangle", [(0, 0), (10.5, 0), (5, 9)], "#AABBCC")
            ],
            "#FFFFFF");

        var first = document.ToSvg();
        var second = document.ToSvg();

        Assert.Equal(first, second);
        var xml = XDocument.Parse(first);
        XNamespace svg = "http://www.w3.org/2000/svg";
        Assert.Equal("320", (string?)xml.Root!.Attribute("width"));
        Assert.Equal(new[] { "visualize-background", "panel", "triangle" },
            xml.Root.Elements().Select(element => (string?)element.Attribute("id")).ToArray());
        Assert.Equal("0,0 10.5,0 5,9", (string?)xml.Root.Elements(svg + "polygon").Single().Attribute("points"));
    }

    [Fact]
    public void NumericFormattingIsInvariantAcrossCurrentCultures()
    {
        var original = System.Globalization.CultureInfo.CurrentCulture;
        try
        {
            System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo("fr-FR");
            var document = new VisualizerSvgDocument(10.5, 20,
                [new VisualizerSvgRectangle("r", 1.5, 2.5, 3.5, 4.5, "#abcdef")]);
            Assert.Contains("width=\"10.5\"", document.ToSvg());
            Assert.Contains("x=\"1.5\"", document.ToSvg());
        }
        finally
        {
            System.Globalization.CultureInfo.CurrentCulture = original;
        }
    }

    [Fact]
    public void DocumentRejectsDuplicateIds()
    {
        var error = Assert.Throws<ArgumentException>(() => new VisualizerSvgDocument(10, 10,
        [
            new VisualizerSvgRectangle("same", 0, 0, 1, 1, "#000000"),
            new VisualizerSvgRectangle("same", 1, 1, 1, 1, "#ffffff")
        ]));
        Assert.Contains("Duplicate SVG primitive id", error.Message);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(double.NaN, 10)]
    [InlineData(10, double.PositiveInfinity)]
    public void DocumentRejectsInvalidViewport(double width, double height)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new VisualizerSvgDocument(width, height, []));
    }

    [Fact]
    public void PolygonRejectsNonFiniteCoordinates()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new VisualizerSvgDocument(10, 10,
            [new VisualizerSvgPolygon("p", [(0, 0), (1, 0), (double.NaN, 1)], "#ffffff")]));
    }

    [Fact]
    public void IdAndColorAreValidatedRatherThanInterpolatedIntoMarkup()
    {
        Assert.Throws<ArgumentException>(() =>
            new VisualizerSvgDocument(10, 10,
                [new VisualizerSvgRectangle("bad id", 0, 0, 1, 1, "#ffffff")]));
        Assert.Throws<ArgumentException>(() =>
            new VisualizerSvgDocument(10, 10,
                [new VisualizerSvgRectangle("ok", 0, 0, 1, 1, "red; onload=alert(1)")]));
    }

    [Theory]
    [InlineData("-0.1")]
    [InlineData("1.1")]
    [InlineData("NaN")]
    [InlineData("0,5")]
    public void PrimitiveRejectsInvalidFillOpacity(string opacity)
    {
        var attributes = new Dictionary<string, string> { ["fill-opacity"] = opacity };
        Assert.Throws<ArgumentOutOfRangeException>(() => new VisualizerSvgDocument(10, 10,
            [new VisualizerSvgRectangle("r", 0, 0, 1, 1, "#ffffff", attributes)]));
    }

    [Theory]
    [InlineData("0")]
    [InlineData("1")]
    public void PrimitiveAcceptsInvariantFillOpacityAtBounds(string opacity)
    {
        var attributes = new Dictionary<string, string> { ["fill-opacity"] = opacity };
        var document = new VisualizerSvgDocument(10, 10,
            [new VisualizerSvgRectangle("r", 0, 0, 1, 1, "#ffffff", attributes)]);
        Assert.Contains($"fill-opacity=\"{opacity}\"", document.ToSvg());
    }

    [Fact]
    public void DocumentSnapshotsPrimitiveMetadata()
    {
        var attributes = new Dictionary<string, string> { ["data-entity-id"] = "101" };
        var primitive = new VisualizerSvgRectangle("r", 0, 0, 1, 1, "#ffffff", attributes);
        var document = new VisualizerSvgDocument(10, 10, [primitive]);
        var originalSvg = document.ToSvg();

        attributes["data-entity-id"] = "202";

        Assert.Equal(originalSvg, document.ToSvg());
        Assert.Contains("data-entity-id=\"101\"", document.ToSvg());
    }

    [Fact]
    public void DocumentReservesBackgroundIdentifierWhenBackgroundIsRendered()
    {
        Assert.Throws<ArgumentException>(() => new VisualizerSvgDocument(10, 10,
            [new VisualizerSvgRectangle("visualize-background", 0, 0, 1, 1, "#ffffff")],
            "#000000"));
    }


    [Fact]
    public void CircleAndTextPrimitivesSerializeAsSemanticSvgElements()
    {
        var document = new VisualizerSvgDocument(320, 200,
        [
            new VisualizerSvgCircle("actor-token", 48.5, 72, 12, "#55B8A6",
                new Dictionary<string, string> { ["data-entity-id"] = "4101" }),
            new VisualizerSvgText("actor-label", 48.5, 96, "Scout & Guide", "#FFFFFF",
                12, "sans-serif", "bold", "middle")
        ]);

        var svg = document.ToSvg();
        var xml = XDocument.Parse(svg);
        XNamespace ns = "http://www.w3.org/2000/svg";
        var circle = xml.Root!.Element(ns + "circle")!;
        var label = xml.Root.Element(ns + "text")!;

        Assert.Equal("48.5", (string?)circle.Attribute("cx"));
        Assert.Equal("4101", (string?)circle.Attribute("data-entity-id"));
        Assert.Equal("Scout & Guide", label.Value);
        Assert.Equal("middle", (string?)label.Attribute("text-anchor"));
        Assert.Contains("Scout &amp; Guide", svg, StringComparison.Ordinal);
    }

    [Fact]
    public void CircleAndTextPrimitivesAreSnapshottedAndDeterministic()
    {
        var attributes = new Dictionary<string, string> { ["data-entity-id"] = "7" };
        var document = new VisualizerSvgDocument(100, 100,
        [
            new VisualizerSvgCircle("token", 10, 20, 5, "#ffffff", attributes),
            new VisualizerSvgText("label", 10, 32, "Token", "#ffffff")
        ]);
        var original = document.ToSvg();
        attributes["data-entity-id"] = "8";

        Assert.Equal(original, document.ToSvg());
        Assert.Contains("data-entity-id=\"7\"", document.ToSvg(), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(double.NaN, 0, 2)]
    [InlineData(0, double.PositiveInfinity, 2)]
    [InlineData(0, 0, -1)]
    public void CircleRejectsInvalidGeometry(double x, double y, double radius)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new VisualizerSvgDocument(10, 10, [new VisualizerSvgCircle("circle", x, y, radius, "#ffffff")]));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void TextRejectsInvalidFontSize(double fontSize)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new VisualizerSvgDocument(10, 10, [new VisualizerSvgText("label", 0, 0, "text", "#ffffff", fontSize)]));
    }

    [Fact]
    public void TextRejectsUnsupportedPresentationValues()
    {
        Assert.Throws<ArgumentException>(() =>
            new VisualizerSvgDocument(10, 10, [new VisualizerSvgText("label", 0, 0, "text", "#ffffff", FontWeight: "900")]));
        Assert.Throws<ArgumentException>(() =>
            new VisualizerSvgDocument(10, 10, [new VisualizerSvgText("label", 0, 0, "text", "#ffffff", TextAnchor: "unsafe")]));
    }

}
