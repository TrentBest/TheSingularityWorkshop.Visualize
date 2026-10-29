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
        Assert.Equal("10.5,0 5,9", (string?)xml.Root.Elements(svg + "polygon").Single().Attribute("points"));
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

    [Fact]
    public void PrimitiveAcceptsInvariantFillOpacityAtBounds()
    {
        var attributes = new Dictionary<string, string> { ["fill-opacity"] = "0" };
        var document = new VisualizerSvgDocument(10, 10,
            [new VisualizerSvgRectangle("r", 0, 0, 1, 1, "#ffffff", attributes)]);
        Assert.Contains("fill-opacity=\"0\"", document.ToSvg());
    }

    [Fact]
    public void DocumentReservesBackgroundIdentifierWhenBackgroundIsRendered()
    {
        Assert.Throws<ArgumentException>(() => new VisualizerSvgDocument(10, 10,
            [new VisualizerSvgRectangle("visualize-background", 0, 0, 1, 1, "#ffffff")],
            "#000000"));
    }

}
