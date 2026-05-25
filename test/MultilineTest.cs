using Mina.Extension;
using Mina.Reflection;
using OpenType;
using OpenType.Tables;
using PicoPDF.Loader.Sections;
using PicoPDF.Pdf.Documents;
using PicoPDF.Pdf.Font;
using Svg.Outline;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace PicoPDF.Test;

public class MultilineTest
{
    public class OpenTypeFontMock : IOpenTypeFont
    {
        public string PostScriptName { get; init; } = "";
        public IFontPath Path { get; init; } = null!;
        public long Position { get; init; }
        public IReadOnlyDictionary<string, TableRecord> TableRecords { get; init; } = null!;
        public OffsetTable Offset { get; init; } = null!;
        public NameTable Name { get; init; } = null!;

        public FontHeaderTable FontHeader { get; init; } = Expressions.GetNew<FontHeaderTable>()();
        public MaximumProfileTable MaximumProfile { get; init; } = null!;
        public PostScriptTable PostScript { get; init; } = null!;
        public OS2Table? OS2 { get; init; }
        public HorizontalHeaderTable HorizontalHeader { get; init; } = Expressions.GetNew<HorizontalHeaderTable>()();
        public HorizontalMetricsTable HorizontalMetrics { get; init; } = Expressions.GetNew<HorizontalMetricsTable>()();
        public CMapTable CMap { get; init; } = null!;
        public Func<int, uint> CharToGID { get; init; } = (_) => 0;
        public Func<uint, bool, IOutline[]> GIDToOutline { get; init; } = null!;

        public ColorBitmapDataTable? ColorBitmapData { get; init; }
        public ColorBitmapLocationTable? ColorBitmapLocation { get; init; }
        public ColorTable? Color { get; init; }
        public ColorPaletteTable? ColorPalette { get; init; }
        public StandardBitmapGraphicsTable? StandardBitmapGraphics { get; init; }
        public ScalableVectorGraphicsTable? ScalableVectorGraphics { get; init; }

        public OpenTypeFontMock()
        {
            // All glyphs are AdvanceWidth is 9
            Expressions.SetProperty<FontHeaderTable, ushort>("UnitsPerEm")(FontHeader, 1);
            Expressions.SetProperty<HorizontalHeaderTable, ushort>("NumberOfHMetrics")(HorizontalHeader, 1);
            Expressions.SetProperty<HorizontalMetricsTable, HorizontalMetrics[]>("Metrics")(HorizontalMetrics, [new() { AdvanceWidth = 9, LeftSideBearing = 0 }]);
        }
    }
    public class Type0FontMock : Type0Font
    {
        public static Type0FontMock Create() => new()
        {
            Name = "Type0FontMock",
            Font = new OpenTypeFontMock(),
            FontEmbed = FontEmbeds.PossibleEmbed,
            FontRegister = null!,
            Encoding = "",
            FontDictionary = null!,
        };
    }

    public static readonly Type0Font[] Fonts = [Type0FontMock.Create()];

    public static string[] GetText(string line, double size, double width, ILineBreakRule? linebreak_rule = null) => [..
            Contents.GetTextFont(line, Fonts, size, width, linebreak_rule ?? new NoneLineBreakRule())
            .Select(x => x.Select(y => y.Text).Join(""))
        ];

    [Fact]
    public void NoLineBreakTest()
    {
        Assert.Equal(GetText("abc", 1, 0), new[] { "abc" });
        Assert.Equal(GetText("abcd", 1, 0), new[] { "abcd" });
        Assert.Equal(GetText("abcde", 1, 0), new[] { "abcde" });
        Assert.Equal(GetText("abcdef", 1, 0), new[] { "abcdef" });
        Assert.Equal(GetText("abcdefg", 1, 0), new[] { "abcdefg" });
    }

    [Fact]
    public void LineBreakTest()
    {
        Assert.Equal(GetText("abc", 1, 30), new[] { "abc" });
        Assert.Equal(GetText("abcd", 1, 30), new[] { "abc", "d" });
        Assert.Equal(GetText("abcde", 1, 30), new[] { "abc", "de" });
        Assert.Equal(GetText("abcdef", 1, 30), new[] { "abc", "def" });
        Assert.Equal(GetText("abcdefg", 1, 30), new[] { "abc", "def", "g" });
    }

    [Fact]
    public void LineBreakRuleTest()
    {
        var linebrak_rule = new GenericLineBreakRule();

        Assert.Equal(GetText("ab,", 1, 30, linebrak_rule), new[] { "ab," });
        Assert.Equal(GetText("abc,", 1, 30, linebrak_rule), new[] { "ab", "c," });
        Assert.Equal(GetText("abcd,", 1, 30, linebrak_rule), new[] { "abc", "d," });
        Assert.Equal(GetText("abcde,", 1, 30, linebrak_rule), new[] { "abc", "de," });
        Assert.Equal(GetText("abcdef,", 1, 30, linebrak_rule), new[] { "abc", "de", "f," });

        Assert.Equal(GetText("ab{x}", 1, 30, linebrak_rule), new[] { "ab", "{x}" });
        Assert.Equal(GetText("abc{x}", 1, 30, linebrak_rule), new[] { "abc", "{x}" });
        Assert.Equal(GetText("abcd{x}", 1, 30, linebrak_rule), new[] { "abc", "d{", "x}" });
        Assert.Equal(GetText("abcde{x}", 1, 30, linebrak_rule), new[] { "abc", "de", "{x}" });
        Assert.Equal(GetText("abcdef{x}", 1, 30, linebrak_rule), new[] { "abc", "def", "{x}" });
    }

    [Fact]
    public void JapaneseLineBreakRuleTest()
    {
        var linebrak_rule = new JapaneseLineBreakRule();

        Assert.Equal(GetText("ab,", 1, 30, linebrak_rule), new[] { "ab," });
        Assert.Equal(GetText("abc,", 1, 30, linebrak_rule), new[] { "ab", "c," });
        Assert.Equal(GetText("abcd,", 1, 30, linebrak_rule), new[] { "abc", "d," });
        Assert.Equal(GetText("abcde,", 1, 30, linebrak_rule), new[] { "abc", "de," });
        Assert.Equal(GetText("abcdef,", 1, 30, linebrak_rule), new[] { "abc", "de", "f," });

        Assert.Equal(GetText("ab{x}", 1, 30, linebrak_rule), new[] { "ab{", "x}" });
        Assert.Equal(GetText("abc{x}", 1, 30, linebrak_rule), new[] { "abc", "{x}" });
        Assert.Equal(GetText("abcd{x}", 1, 30, linebrak_rule), new[] { "abc", "d{x", "}" });
        Assert.Equal(GetText("abcde{x}", 1, 30, linebrak_rule), new[] { "abc", "de{", "x}" });
        Assert.Equal(GetText("abcdef{x}", 1, 30, linebrak_rule), new[] { "abc", "def", "{x}" });
    }
}
