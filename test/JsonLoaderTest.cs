using Pdf.Documents;
using PicoPDF.Loader;
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
    public void TypeCheck()
    {
        var node = JsonNode.Parse("""
{
    "NullValue": null,
    "EmptyString": "",
    "StringValue": "xyz",
    "IntValue": 123,
    "TrueValue": true,
    "FalseValue": false,
    "Array": [1, 2, 3],
    "Object": { "X": 10, "Y": 20, "Z": 30 },
}
""", null, Option)!;

        Assert.Null(node["InvalidName"]);
        Assert.Equal(node["InvalidName"]?.AsValue(), null);
        Assert.Equal(node["InvalidName"]?.AsValue() is { } a1 ? (int)a1 : 0, 0);
        Assert.Equal(node["InvalidName"]?.AsValue() is { } a2 ? a2.ToString() : "", "");

        Assert.Null(node["NullValue"]);
        Assert.Equal(node["NullValue"]?.AsValue(), null);
        Assert.Equal(node["NullValue"]?.AsValue() is { } b1 ? (int)b1 : 0, 0);
        Assert.Equal(node["NullValue"]?.AsValue() is { } b2 ? b2.ToString() : "", "");

        Assert.NotNull(node["EmptyString"]);
        Assert.Equal(node["EmptyString"]?.AsValue()?.ToString(), "");
        _ = Assert.Throws<InvalidOperationException>(() => node["EmptyString"]?.AsValue() is { } c1 ? (int)c1 : 0);
        Assert.Equal(node["EmptyString"]?.AsValue() is { } c2 ? c2.ToString() : "", "");

        Assert.NotNull(node["StringValue"]);
        Assert.Equal(node["StringValue"]?.AsValue()?.ToString(), "xyz");
        _ = Assert.Throws<InvalidOperationException>(() => node["StringValue"]?.AsValue() is { } d1 ? (int)d1 : 0);
        Assert.Equal(node["StringValue"]?.AsValue() is { } d2 ? d2.ToString() : "", "xyz");

        Assert.NotNull(node["IntValue"]);
        Assert.Equal((int)node["IntValue"]?.AsValue()!, 123);
        Assert.Equal(node["IntValue"]?.AsValue() is { } e1 ? (int)e1 : 0, 123);
        Assert.Equal(node["IntValue"]?.AsValue() is { } e2 ? e2.ToString() : "", "123");

        Assert.NotNull(node["TrueValue"]);
        Assert.Equal((bool)node["TrueValue"]?.AsValue()!, true);
        Assert.Equal(node["TrueValue"]?.AsValue() is { } f1 ? (bool)f1 : false, true);
        Assert.Equal(node["TrueValue"]?.AsValue() is { } f2 ? f2.ToString() : "", "true");

        Assert.NotNull(node["FalseValue"]);
        Assert.Equal((bool)node["FalseValue"]?.AsValue()!, false);
        Assert.Equal(node["FalseValue"]?.AsValue() is { } g1 ? (bool)g1 : true, false);
        Assert.Equal(node["FalseValue"]?.AsValue() is { } g2 ? g2.ToString() : "", "false");

        Assert.NotNull(node["Array"]);
        _ = Assert.Throws<InvalidOperationException>(() => node["Array"]?.AsValue());
        Assert.Equal(node["Array"]?.AsArray().Count, 3);

        Assert.NotNull(node["Object"]);
        _ = Assert.Throws<InvalidOperationException>(() => node["Object"]?.AsValue());
        Assert.Equal(node["Object"]?.AsObject().Count, 3);
    }

    [Fact]
    public void TypeCheckExtension()
    {
        var node = JsonNode.Parse("""
{
    "NullValue": null,
    "EmptyString": "",
    "StringValue": "xyz",
    "IntStringValue": "234",
    "DoubleStringValue": "234.5",
    "IntValue": 123,
    "DoubleValue": 123.4,
    "TrueValue": true,
    "FalseValue": false,
    "Array": [1, 2, 3],
    "Object": { "X": 10, "Y": 20, "Z": 30 },
}
""", null, Option)!;

        _ = Assert.Throws<NullReferenceException>(() => node.GetStringValue("InvalidName"));
        _ = Assert.Throws<NullReferenceException>(() => node.GetStringValue("NullValue"));
        Assert.Equal(node.GetStringValue("EmptyString"), "");
        Assert.Equal(node.GetStringValue("StringValue"), "xyz");
        Assert.Equal(node.GetStringValue("IntStringValue"), "234");
        Assert.Equal(node.GetStringValue("DoubleStringValue"), "234.5");
        Assert.Equal(node.GetStringValue("IntValue"), "123");
        Assert.Equal(node.GetStringValue("DoubleValue"), "123.4");
        Assert.Equal(node.GetStringValue("TrueValue"), "true");
        Assert.Equal(node.GetStringValue("FalseValue"), "false");
        _ = Assert.Throws<InvalidOperationException>(() => node.GetStringValue("Array"));
        _ = Assert.Throws<InvalidOperationException>(() => node.GetStringValue("Object"));

        _ = Assert.Throws<NullReferenceException>(() => node.GetIntValue("InvalidName"));
        _ = Assert.Throws<NullReferenceException>(() => node.GetIntValue("NullValue"));
        _ = Assert.Throws<InvalidOperationException>(() => node.GetIntValue("EmptyString"));
        _ = Assert.Throws<InvalidOperationException>(() => node.GetIntValue("StringValue"));
        _ = Assert.Throws<InvalidOperationException>(() => node.GetIntValue("IntStringValue"));
        _ = Assert.Throws<InvalidOperationException>(() => node.GetIntValue("DoubleStringValue"));
        Assert.Equal(node.GetIntValue("IntValue"), 123);
        _ = Assert.Throws<InvalidOperationException>(() => node.GetIntValue("DoubleValue"));
        _ = Assert.Throws<InvalidOperationException>(() => node.GetIntValue("TrueValue"));
        _ = Assert.Throws<InvalidOperationException>(() => node.GetIntValue("FalseValue"));
        _ = Assert.Throws<InvalidOperationException>(() => node.GetIntValue("Array"));
        _ = Assert.Throws<InvalidOperationException>(() => node.GetIntValue("Object"));

        _ = Assert.Throws<NullReferenceException>(() => node.GetDoubleValue("InvalidName"));
        _ = Assert.Throws<NullReferenceException>(() => node.GetDoubleValue("NullValue"));
        _ = Assert.Throws<InvalidOperationException>(() => node.GetDoubleValue("EmptyString"));
        _ = Assert.Throws<InvalidOperationException>(() => node.GetDoubleValue("StringValue"));
        _ = Assert.Throws<InvalidOperationException>(() => node.GetDoubleValue("IntStringValue"));
        _ = Assert.Throws<InvalidOperationException>(() => node.GetDoubleValue("DoubleStringValue"));
        Assert.Equal(node.GetDoubleValue("IntValue"), 123.0);
        Assert.Equal(node.GetDoubleValue("DoubleValue"), 123.4);
        _ = Assert.Throws<InvalidOperationException>(() => node.GetDoubleValue("TrueValue"));
        _ = Assert.Throws<InvalidOperationException>(() => node.GetDoubleValue("FalseValue"));
        _ = Assert.Throws<InvalidOperationException>(() => node.GetDoubleValue("Array"));
        _ = Assert.Throws<InvalidOperationException>(() => node.GetDoubleValue("Object"));

        Assert.Null(node.GetStringOrDefaultWithoutNullValue("InvalidName"));
        _ = Assert.Throws<NullReferenceException>(() => node.GetStringOrDefaultWithoutNullValue("NullValue"));
        Assert.Equal(node.GetStringOrDefaultWithoutNullValue("EmptyString"), "");
        Assert.Equal(node.GetStringOrDefaultWithoutNullValue("StringValue"), "xyz");
        Assert.Equal(node.GetStringOrDefaultWithoutNullValue("IntStringValue"), "234");
        Assert.Equal(node.GetStringOrDefaultWithoutNullValue("DoubleStringValue"), "234.5");
        Assert.Equal(node.GetStringOrDefaultWithoutNullValue("IntValue"), "123");
        Assert.Equal(node.GetStringOrDefaultWithoutNullValue("DoubleValue"), "123.4");
        Assert.Equal(node.GetStringOrDefaultWithoutNullValue("TrueValue"), "true");
        Assert.Equal(node.GetStringOrDefaultWithoutNullValue("FalseValue"), "false");
        _ = Assert.Throws<InvalidOperationException>(() => node.GetStringOrDefaultWithoutNullValue("Array"));
        _ = Assert.Throws<InvalidOperationException>(() => node.GetStringOrDefaultWithoutNullValue("Object"));

        Assert.Null(node.GetIntOrDefaultWithoutNullValue("InvalidName"));
        _ = Assert.Throws<NullReferenceException>(() => node.GetIntOrDefaultWithoutNullValue("NullValue"));
        _ = Assert.Throws<InvalidOperationException>(() => node.GetIntOrDefaultWithoutNullValue("EmptyString"));
        _ = Assert.Throws<InvalidOperationException>(() => node.GetIntOrDefaultWithoutNullValue("StringValue"));
        _ = Assert.Throws<InvalidOperationException>(() => node.GetIntOrDefaultWithoutNullValue("IntStringValue"));
        _ = Assert.Throws<InvalidOperationException>(() => node.GetIntOrDefaultWithoutNullValue("DoubleStringValue"));
        Assert.Equal(node.GetIntOrDefaultWithoutNullValue("IntValue"), 123);
        _ = Assert.Throws<InvalidOperationException>(() => node.GetIntOrDefaultWithoutNullValue("DoubleValue"));
        _ = Assert.Throws<InvalidOperationException>(() => node.GetIntOrDefaultWithoutNullValue("TrueValue"));
        _ = Assert.Throws<InvalidOperationException>(() => node.GetIntOrDefaultWithoutNullValue("FalseValue"));
        _ = Assert.Throws<InvalidOperationException>(() => node.GetIntOrDefaultWithoutNullValue("Array"));
        _ = Assert.Throws<InvalidOperationException>(() => node.GetIntOrDefaultWithoutNullValue("Object"));

        Assert.Null(node.GetDoubleOrDefaultWithoutNullValue("InvalidName"));
        _ = Assert.Throws<NullReferenceException>(() => node.GetDoubleOrDefaultWithoutNullValue("NullValue"));
        _ = Assert.Throws<InvalidOperationException>(() => node.GetDoubleOrDefaultWithoutNullValue("EmptyString"));
        _ = Assert.Throws<InvalidOperationException>(() => node.GetDoubleOrDefaultWithoutNullValue("StringValue"));
        _ = Assert.Throws<InvalidOperationException>(() => node.GetDoubleOrDefaultWithoutNullValue("IntStringValue"));
        _ = Assert.Throws<InvalidOperationException>(() => node.GetDoubleOrDefaultWithoutNullValue("DoubleStringValue"));
        Assert.Equal(node.GetDoubleOrDefaultWithoutNullValue("IntValue"), 123.0);
        Assert.Equal(node.GetDoubleOrDefaultWithoutNullValue("DoubleValue"), 123.4);
        _ = Assert.Throws<InvalidOperationException>(() => node.GetDoubleOrDefaultWithoutNullValue("TrueValue"));
        _ = Assert.Throws<InvalidOperationException>(() => node.GetDoubleOrDefaultWithoutNullValue("FalseValue"));
        _ = Assert.Throws<InvalidOperationException>(() => node.GetDoubleOrDefaultWithoutNullValue("Array"));
        _ = Assert.Throws<InvalidOperationException>(() => node.GetDoubleOrDefaultWithoutNullValue("Object"));
    }

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
        Assert.Equal(f1.Embed, FontEmbeds.PossibleEmbed | FontEmbeds.ConvertNone);

        var f2 = JsonLoader.ToFontPath(JsonNode.Parse("""{"Path": "Times New Roman"}""", null, Option)!);
        Assert.Equal(f2.Path, "Times New Roman");
        Assert.Equal(f2.Embed, FontEmbeds.PossibleEmbed | FontEmbeds.ConvertNone);

        var f3 = JsonLoader.ToFontPath(JsonNode.Parse("""{"Path": "Courier New", "Embed": "NotEmbed"}""", null, Option)!);
        Assert.Equal(f3.Path, "Courier New");
        Assert.Equal(f3.Embed, FontEmbeds.NotEmbed | FontEmbeds.ConvertNone);

        var f4 = JsonLoader.ToFontPath(JsonNode.Parse("""{"Path": "Courier New", "Embed": "ConvertToTrueType"}""", null, Option)!);
        Assert.Equal(f4.Path, "Courier New");
        Assert.Equal(f4.Embed, FontEmbeds.PossibleEmbed | FontEmbeds.ConvertToTrueType);

        var f5 = JsonLoader.ToFontPath(JsonNode.Parse("""{"Path": "Courier New", "Embed": "NotEmbed, ConvertToTrueType"}""", null, Option)!);
        Assert.Equal(f5.Path, "Courier New");
        Assert.Equal(f5.Embed, FontEmbeds.NotEmbed | FontEmbeds.ConvertToTrueType);

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

        var s8 = JsonLoader.LoadAllSides(JsonNode.Parse("""[]""", null, Option)!);
        Assert.Equal(s8.Left, 0);
        Assert.Equal(s8.Top, 0);
        Assert.Equal(s8.Right, 0);
        Assert.Equal(s8.Bottom, 0);

        var s9 = JsonLoader.LoadAllSides(JsonNode.Parse("""[10, 20, 30, 40, 50]""", null, Option)!);
        Assert.Equal(s9.Left, 40);
        Assert.Equal(s9.Top, 10);
        Assert.Equal(s9.Right, 20);
        Assert.Equal(s9.Bottom, 30);
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
