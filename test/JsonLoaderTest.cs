using PicoPDF.Loader;
using PicoPDF.Loader.Elements;
using PicoPDF.Loader.Sections;
using System;
using System.Drawing;
using System.Text.Json;
using System.Text.Json.Nodes;
using Xunit;
using XFontPath = PicoPDF.Loader.Sections.FontPath;

namespace PicoPDF.Test;

public class JsonLoaderTest
{
    public static JsonDocumentOptions Option { get; } = new() { AllowTrailingCommas = true, CommentHandling = JsonCommentHandling.Skip };

    [Fact]
    public void LoadElementError()
    {
        Assert.Equal(Assert.Throws<NullReferenceException>(() => _ = JsonLoader.LoadElement(JsonNode.Parse("""{ }""", null, Option)!)).Message, "Element 'X' was not found.");
        Assert.Equal(Assert.Throws<NullReferenceException>(() => _ = JsonLoader.LoadElement(JsonNode.Parse("""{"X": 10}""", null, Option)!)).Message, "Element 'Y' was not found.");
        Assert.Equal(Assert.Throws<NullReferenceException>(() => _ = JsonLoader.LoadElement(JsonNode.Parse("""{"Y": 20}""", null, Option)!)).Message, "Element 'X' was not found.");
        Assert.Equal(Assert.Throws<NullReferenceException>(() => _ = JsonLoader.LoadElement(JsonNode.Parse("""{"Type": "None"}""", null, Option)!)).Message, "Element 'X' was not found.");
        _ = Assert.Throws<Exception>(() => _ = JsonLoader.LoadElement(JsonNode.Parse("""{"Type": "None", "X": 10, "Y": 20}""", null, Option)!));
    }

    [Fact]
    public void ToFontPath()
    {
        var f1 = JsonLoader.ToFontPath(JsonNode.Parse(""" "Arial" """, null, Option)!);
        Assert.Equal(f1.Path, "Arial");
        Assert.Equal(f1.Embed, FontEmbeds.PossibleEmbed);

        var f2 = JsonLoader.ToFontPath(JsonNode.Parse("""{"Path": "Times New Roman"}""", null, Option)!);
        Assert.Equal(f2.Path, "Times New Roman");
        Assert.Equal(f2.Embed, FontEmbeds.PossibleEmbed);

        var f3 = JsonLoader.ToFontPath(JsonNode.Parse("""{"Path": "Courier New", "Embed": "NotEmbed"}""", null, Option)!);
        Assert.Equal(f3.Path, "Courier New");
        Assert.Equal(f3.Embed, FontEmbeds.NotEmbed);

        _ = Assert.Throws<ArgumentException>(() => _ = JsonLoader.ToFontPath(JsonNode.Parse("""{"Path": "Arial", "Embed": "None"}""", null, Option)!));
        Assert.Equal(Assert.Throws<NullReferenceException>(() => _ = JsonLoader.ToFontPath(JsonNode.Parse("""{"Embed": "NotEmbed"}""", null, Option)!)).Message, "Element 'Path' was not found.");
    }

    [Fact]
    public void ToFontPathArray()
    {
        var f0 = JsonLoader.ToFontPathArray(JsonNode.Parse("""null""", null, Option)!);
        Assert.Equal(f0.Length, 0);

        var f1 = JsonLoader.ToFontPathArray(JsonNode.Parse(""" "Arial" """, null, Option)!);
        Assert.Equal(f1.Length, 1);
        Assert.Equal(f1[0].Path, "Arial");
        Assert.Equal(f1[0].Embed, FontEmbeds.PossibleEmbed);

        var f2 = JsonLoader.ToFontPathArray(JsonNode.Parse("""[{"Path": "Times New Roman"}]""", null, Option)!);
        Assert.Equal(f2.Length, 1);
        Assert.Equal(f2[0].Path, "Times New Roman");
        Assert.Equal(f2[0].Embed, FontEmbeds.PossibleEmbed);

        var f3 = JsonLoader.ToFontPathArray(JsonNode.Parse("""[{"Path": "Courier New", "Embed": "NotEmbed"}, {"Path": "Arial"}]""", null, Option)!);
        Assert.Equal(f3.Length, 2);
        Assert.Equal(f3[0].Path, "Courier New");
        Assert.Equal(f3[0].Embed, FontEmbeds.NotEmbed);
        Assert.Equal(f3[1].Path, "Arial");
        Assert.Equal(f3[1].Embed, FontEmbeds.PossibleEmbed);
    }

    [Fact]
    public void LoadPageSize()
    {
        var s1 = JsonLoader.LoadPageSize(JsonNode.Parse("""null""", null, Option)!);
        Assert.Equal(s1.Width, 595);
        Assert.Equal(s1.Height, 842);

        var s2 = JsonLoader.LoadPageSize(JsonNode.Parse(""" "B4" """, null, Option)!);
        Assert.Equal(s2.Width, 709);
        Assert.Equal(s2.Height, 1001);

        var s3 = JsonLoader.LoadPageSize(JsonNode.Parse(""" [100, 200] """, null, Option)!);
        Assert.Equal(s3.Width, 100);
        Assert.Equal(s3.Height, 200);

        var s4 = JsonLoader.LoadPageSize(JsonNode.Parse(""" {"Width": 300, "Height": 400} """, null, Option)!);
        Assert.Equal(s4.Width, 300);
        Assert.Equal(s4.Height, 400);
    }

    [Fact]
    public void LoadAllSides()
    {
        var s1 = JsonLoader.LoadAllSides(JsonNode.Parse("""null""", null, Option)!);
        Assert.Equal(s1.Left, 0);
        Assert.Equal(s1.Top, 0);
        Assert.Equal(s1.Right, 0);
        Assert.Equal(s1.Bottom, 0);

        var s2 = JsonLoader.LoadAllSides(JsonNode.Parse("""10""", null, Option)!);
        Assert.Equal(s2.Left, 10);
        Assert.Equal(s2.Top, 10);
        Assert.Equal(s2.Right, 10);
        Assert.Equal(s2.Bottom, 10);

        var s3 = JsonLoader.LoadAllSides(JsonNode.Parse("""[10]""", null, Option)!);
        Assert.Equal(s3.Left, 10);
        Assert.Equal(s3.Top, 10);
        Assert.Equal(s3.Right, 10);
        Assert.Equal(s3.Bottom, 10);

        var s4 = JsonLoader.LoadAllSides(JsonNode.Parse("""[10, 20]""", null, Option)!);
        Assert.Equal(s4.Left, 20);
        Assert.Equal(s4.Top, 10);
        Assert.Equal(s4.Right, 20);
        Assert.Equal(s4.Bottom, 10);

        var s5 = JsonLoader.LoadAllSides(JsonNode.Parse("""[10, 20, 30]""", null, Option)!);
        Assert.Equal(s5.Left, 20);
        Assert.Equal(s5.Top, 10);
        Assert.Equal(s5.Right, 20);
        Assert.Equal(s5.Bottom, 30);

        var s6 = JsonLoader.LoadAllSides(JsonNode.Parse("""[10, 20, 30, 40]""", null, Option)!);
        Assert.Equal(s6.Left, 40);
        Assert.Equal(s6.Top, 10);
        Assert.Equal(s6.Right, 20);
        Assert.Equal(s6.Bottom, 30);

        var s7 = JsonLoader.LoadAllSides(JsonNode.Parse("""{"Left": 10, "Top": 20, "Right": 30, "Bottom": 40}""", null, Option)!);
        Assert.Equal(s7.Left, 10);
        Assert.Equal(s7.Top, 20);
        Assert.Equal(s7.Right, 30);
        Assert.Equal(s7.Bottom, 40);
    }

    [Fact]
    public void LoadTextElement()
    {
        var e1 = JsonLoader.LoadTextElement(10, 20, "name", JsonNode.Parse("""{"Type": "TextElement", "Text": "Hello, World!", "Size": 30}""", null, Option)!);
        Assert.Equal(e1.X, 10);
        Assert.Equal(e1.Y, 20);
        Assert.Equal("Hello, World!", e1.Text);
        Assert.Equal(e1.Size, 30);

        var e2 = JsonLoader.LoadTextElement(10, 20, "name", JsonNode.Parse("""{"Type": "TextElement", "Text": "Hello, World!2", "Size": 31, "Font": ["Arial"]}""", null, Option)!);
        Assert.Equal("Hello, World!2", e2.Text);
        Assert.Equal(e2.Size, 31);
        Assert.Equivalent(e2.Font, new XFontPath[] { new() { Path = "Arial", Embed = FontEmbeds.PossibleEmbed } });

        var e3 = JsonLoader.LoadTextElement(10, 20, "name", JsonNode.Parse("""{"Type": "TextElement", "Text": "Hello, World!3", "Size": 32, "Font": ["Arial", { "Path": "Times New Roman", "Embed": "NotEmbed" }]}""", null, Option)!);
        Assert.Equal("Hello, World!3", e3.Text);
        Assert.Equal(e3.Size, 32);
        Assert.Equivalent(e3.Font, new XFontPath[] { new() { Path = "Arial", Embed = FontEmbeds.PossibleEmbed }, new() { Path = "Times New Roman", Embed = FontEmbeds.NotEmbed } });

        var e4 = JsonLoader.LoadTextElement(10, 20, "name", JsonNode.Parse("""{"Type": "TextElement", "Text": "Hello, World!4", "Size": 33, "Alignment": "End"}""", null, Option)!);
        Assert.Equal("Hello, World!4", e4.Text);
        Assert.Equal(e4.Size, 33);
        Assert.Equal(e4.Alignment, TextAlignments.End);

        var e5 = JsonLoader.LoadTextElement(10, 20, "name", JsonNode.Parse("""{"Type": "TextElement", "Text": "Hello, World!5", "Size": 34, "Style": "Underline, Stroke"}""", null, Option)!);
        Assert.Equal("Hello, World!5", e5.Text);
        Assert.Equal(e5.Size, 34);
        Assert.Equal(e5.Style, TextStyles.Underline | TextStyles.Stroke);

        var e6 = JsonLoader.LoadTextElement(10, 20, "name", JsonNode.Parse("""{"Type": "TextElement", "Text": "Hello, World!6", "Size": 35, "Width": 100}""", null, Option)!);
        Assert.Equal("Hello, World!6", e6.Text);
        Assert.Equal(e6.Size, 35);
        Assert.Equal(e6.Width, 100);

        var e7 = JsonLoader.LoadTextElement(10, 20, "name", JsonNode.Parse("""{"Type": "TextElement", "Text": "Hello, World!7", "Size": 36, "Height": 200}""", null, Option)!);
        Assert.Equal("Hello, World!7", e7.Text);
        Assert.Equal(e7.Size, 36);
        Assert.Equal(e7.Height, 200);

        var e8 = JsonLoader.LoadTextElement(10, 20, "name", JsonNode.Parse("""{"Type": "TextElement", "Text": "Hello, World!8", "Size": 37, "Color": "Red"}""", null, Option)!);
        Assert.Equal("Hello, World!8", e8.Text);
        Assert.Equal(e8.Size, 37);
        Assert.True(e8.Color is { });
        Assert.Equal(e8.Color, Color.Red);

        var e9 = JsonLoader.LoadTextElement(10, 20, "name", JsonNode.Parse("""{"Type": "TextElement", "Text": "Hello, World!9", "Size": 38, "Color": "#010203"}""", null, Option)!);
        Assert.Equal("Hello, World!9", e9.Text);
        Assert.Equal(e9.Size, 38);
        Assert.True(e9.Color is { });
        Assert.Equal(e9.Color.Value.R, 1);
        Assert.Equal(e9.Color.Value.G, 2);
        Assert.Equal(e9.Color.Value.B, 3);
    }
}
