using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using Mina.Extension;
using OpenType;
using OpenType.Tables;
using OpenType.Tables.PostScript;
using OpenType.Tables.TrueType;
using System.IO;

namespace PicoPDF.Benchmark;

public class FontLoadBench
{
    public static readonly Consumer _consumer = new();
    public const string TtfFontPath = "test-case/font/Noto_Sans_JP/NotoSansJP-Regular.ttf";
    public const string CffFontPath = "test-case/font/Noto_Sans_CJK/NotoSansCJKjp-Regular.otf";
    public IOpenTypeHeader Ttf = null!;
    public ushort Ttf_NumberOfHMetrics = 0;
    public ushort Ttf_NumberOfGlyphs = 0;
    public ushort Ttf_NumberOfLongVerMetrics = 0;
    public short Ttf_IndexToLocFormat = 0;
    public IOpenTypeHeader Cff = null!;
    public ushort Cff_NumberOfHMetrics = 0;
    public ushort Cff_NumberOfGlyphs = 0;
    public ushort Cff_NumberOfLongVerMetrics = 0;

    [GlobalSetup]
    public void Setup()
    {
        Directory.SetCurrentDirectory(Program.GetSolutionDirectory());

        Ttf = FontLoader.LoadTableRecords(TtfFontPath);

        using var ttf_stream = Ttf.Path.Open();
        var ttf_head = FontLoader.ReadTableRecord(Ttf, "head", ttf_stream, FontHeaderTable.ReadFrom).Try();
        var ttf_hhea = FontLoader.ReadTableRecord(Ttf, "hhea", ttf_stream, HorizontalHeaderTable.ReadFrom).Try();
        var ttf_maxp = FontLoader.ReadTableRecord(Ttf, "maxp", ttf_stream, MaximumProfileTable.ReadFrom).Try();
        var ttf_vhea = FontLoader.ReadTableRecord(Ttf, "vhea", ttf_stream, VerticalHeaderTable.ReadFrom);
        Ttf_NumberOfHMetrics = ttf_hhea.NumberOfHMetrics;
        Ttf_NumberOfGlyphs = ttf_maxp.NumberOfGlyphs;
        Ttf_NumberOfLongVerMetrics = ttf_vhea?.NumberOfLongVerMetrics ?? 0;
        Ttf_IndexToLocFormat = ttf_head.IndexToLocFormat;

        Cff = FontLoader.LoadTableRecords(CffFontPath);

        using var cff_stream = Cff.Path.Open();
        var cff_hhea = FontLoader.ReadTableRecord(Cff, "hhea", cff_stream, HorizontalHeaderTable.ReadFrom).Try();
        var cff_maxp = FontLoader.ReadTableRecord(Cff, "maxp", cff_stream, MaximumProfileTable.ReadFrom).Try();
        var cff_vhea = FontLoader.ReadTableRecord(Cff, "vhea", cff_stream, VerticalHeaderTable.ReadFrom);
        Cff_NumberOfHMetrics = cff_hhea.NumberOfHMetrics;
        Cff_NumberOfGlyphs = cff_maxp.NumberOfGlyphs;
        Cff_NumberOfLongVerMetrics = cff_vhea?.NumberOfLongVerMetrics ?? 0;
    }

    [Benchmark]
    public void TTF_sfnt_name()
    {
        var x = FontLoader.LoadTableRecords(TtfFontPath);
        _consumer.Consume(x);
    }

    [Benchmark]
    public void TTF_OS2()
    {
        using var stream = Ttf.Path.Open();
        var x = FontLoader.ReadTableRecord(Ttf, "OS/2", stream, OS2Table.ReadFrom);
        if (x is { }) _consumer.Consume(x);
    }

    [Benchmark]
    public void TTF_cmap()
    {
        using var stream = Ttf.Path.Open();
        var x = FontLoader.ReadTableRecord(Ttf, "cmap", stream, CMapTable.ReadFrom).Try();
        _consumer.Consume(x);
    }

    [Benchmark]
    public void TTF_head()
    {
        using var stream = Ttf.Path.Open();
        var x = FontLoader.ReadTableRecord(Ttf, "head", stream, FontHeaderTable.ReadFrom).Try();
        _consumer.Consume(x);
    }

    [Benchmark]
    public void TTF_hhea()
    {
        using var stream = Ttf.Path.Open();
        var x = FontLoader.ReadTableRecord(Ttf, "hhea", stream, HorizontalHeaderTable.ReadFrom).Try();
        _consumer.Consume(x);
    }

    [Benchmark]
    public void TTF_maxp()
    {
        using var stream = Ttf.Path.Open();
        var x = FontLoader.ReadTableRecord(Ttf, "maxp", stream, MaximumProfileTable.ReadFrom).Try();
        _consumer.Consume(x);
    }

    [Benchmark]
    public void TTF_post()
    {
        using var stream = Ttf.Path.Open();
        var x = FontLoader.ReadTableRecord(Ttf, "post", stream, PostScriptTable.ReadFrom).Try();
        _consumer.Consume(x);
    }

    [Benchmark]
    public void TTF_hmtx()
    {
        using var stream = Ttf.Path.Open();
        var x = FontLoader.ReadTableRecord(Ttf, "hmtx", stream, x => HorizontalMetricsTable.ReadFrom(x, Ttf_NumberOfHMetrics, Ttf_NumberOfGlyphs)).Try();
        _consumer.Consume(x);
    }

    [Benchmark]
    public void TTF_vhea()
    {
        using var stream = Ttf.Path.Open();
        var x = FontLoader.ReadTableRecord(Ttf, "vhea", stream, VerticalHeaderTable.ReadFrom);
        if (x is { }) _consumer.Consume(x);
    }

    [Benchmark]
    public void TTF_vmtx()
    {
        using var stream = Ttf.Path.Open();
        var x = FontLoader.ReadTableRecord(Ttf, "vmtx", stream, x => VerticalMetricsTable.ReadFrom(x, Ttf_NumberOfLongVerMetrics, Ttf_NumberOfGlyphs));
        if (x is { }) _consumer.Consume(x);
    }

    [Benchmark]
    public void TTF_GPOS()
    {
        using var stream = Ttf.Path.Open();
        var x = FontLoader.ReadTableRecord(Ttf, "GPOS", stream, GlyphPositioningTable.ReadFrom);
        if (x is { }) _consumer.Consume(x);
    }

    [Benchmark]
    public void TTF_GSUB()
    {
        using var stream = Ttf.Path.Open();
        var x = FontLoader.ReadTableRecord(Ttf, "GSUB", stream, GlyphSubstitutionTable.ReadFrom);
        if (x is { }) _consumer.Consume(x);
    }

    [Benchmark]
    public void TTF_COLR()
    {
        using var stream = Ttf.Path.Open();
        var x = FontLoader.ReadTableRecord(Ttf, "COLR", stream, ColorTable.ReadFrom);
        if (x is { }) _consumer.Consume(x);
    }

    [Benchmark]
    public void TTF_CPAL()
    {
        using var stream = Ttf.Path.Open();
        var x = FontLoader.ReadTableRecord(Ttf, "CPAL", stream, ColorPaletteTable.ReadFrom);
        if (x is { }) _consumer.Consume(x);
    }

    [Benchmark]
    public void TTF_loca_glyf()
    {
        var stream = Ttf.Path.Open();
        using var glyf = new LazyGlyph()
        {
            Stream = stream,
            Count = Ttf_NumberOfGlyphs,
            IndexToLocationTableOffset = Ttf.TableRecords["loca"].Offset,
            GlyphTableOffset = Ttf.TableRecords["glyf"].Offset,
            IndexToLocFormat = Ttf_IndexToLocFormat,
        };
        foreach (var x in glyf)
        {
            _consumer.Consume(x);
        }
    }

    [Benchmark]
    public void TFF_preload()
    {
        var x = FontLoader.LoadTrueTypeFont(Ttf, new());
        _consumer.Consume(x);
    }

    [Benchmark]
    public void CFF_sfnt_name()
    {
        var x = FontLoader.LoadTableRecords(CffFontPath);
        _consumer.Consume(x);
    }

    [Benchmark]
    public void CFF_OS2()
    {
        using var stream = Cff.Path.Open();
        var x = FontLoader.ReadTableRecord(Cff, "OS/2", stream, OS2Table.ReadFrom);
        if (x is { }) _consumer.Consume(x);
    }

    [Benchmark]
    public void CFF_cmap()
    {
        using var stream = Cff.Path.Open();
        var x = FontLoader.ReadTableRecord(Cff, "cmap", stream, CMapTable.ReadFrom).Try();
        _consumer.Consume(x);
    }

    [Benchmark]
    public void CFF_head()
    {
        using var stream = Cff.Path.Open();
        var x = FontLoader.ReadTableRecord(Cff, "head", stream, FontHeaderTable.ReadFrom).Try();
        _consumer.Consume(x);
    }

    [Benchmark]
    public void CFF_hhea()
    {
        using var stream = Cff.Path.Open();
        var x = FontLoader.ReadTableRecord(Cff, "hhea", stream, HorizontalHeaderTable.ReadFrom).Try();
        _consumer.Consume(x);
    }

    [Benchmark]
    public void CFF_maxp()
    {
        using var stream = Cff.Path.Open();
        var x = FontLoader.ReadTableRecord(Cff, "maxp", stream, MaximumProfileTable.ReadFrom).Try();
        _consumer.Consume(x);
    }

    [Benchmark]
    public void CFF_post()
    {
        using var stream = Cff.Path.Open();
        var x = FontLoader.ReadTableRecord(Cff, "post", stream, PostScriptTable.ReadFrom).Try();
        _consumer.Consume(x);
    }

    [Benchmark]
    public void CFF_hmtx()
    {
        using var stream = Cff.Path.Open();
        var x = FontLoader.ReadTableRecord(Cff, "hmtx", stream, x => HorizontalMetricsTable.ReadFrom(x, Cff_NumberOfHMetrics, Cff_NumberOfGlyphs)).Try();
        _consumer.Consume(x);
    }

    [Benchmark]
    public void CFF_vhea()
    {
        using var stream = Cff.Path.Open();
        var x = FontLoader.ReadTableRecord(Cff, "vhea", stream, VerticalHeaderTable.ReadFrom);
        if (x is { }) _consumer.Consume(x);
    }

    [Benchmark]
    public void CFF_vmtx()
    {
        using var stream = Cff.Path.Open();
        var x = FontLoader.ReadTableRecord(Cff, "vmtx", stream, x => VerticalMetricsTable.ReadFrom(x, Cff_NumberOfLongVerMetrics, Cff_NumberOfGlyphs));
        if (x is { }) _consumer.Consume(x);
    }

    [Benchmark]
    public void CFF_GPOS()
    {
        using var stream = Cff.Path.Open();
        var x = FontLoader.ReadTableRecord(Cff, "GPOS", stream, GlyphPositioningTable.ReadFrom);
        if (x is { }) _consumer.Consume(x);
    }

    [Benchmark]
    public void CFF_GSUB()
    {
        using var stream = Cff.Path.Open();
        var x = FontLoader.ReadTableRecord(Cff, "GSUB", stream, GlyphSubstitutionTable.ReadFrom);
        if (x is { }) _consumer.Consume(x);
    }

    [Benchmark]
    public void CFF_COLR()
    {
        using var stream = Cff.Path.Open();
        var x = FontLoader.ReadTableRecord(Cff, "COLR", stream, ColorTable.ReadFrom);
        if (x is { }) _consumer.Consume(x);
    }

    [Benchmark]
    public void CFF_CPAL()
    {
        using var stream = Cff.Path.Open();
        var x = FontLoader.ReadTableRecord(Cff, "CPAL", stream, ColorPaletteTable.ReadFrom);
        if (x is { }) _consumer.Consume(x);
    }

    [Benchmark]
    public void CFF_CFF()
    {
        using var stream = Cff.Path.Open();
        var x = FontLoader.ReadTableRecord(Cff, "CFF ", stream, CompactFontFormat.ReadFrom).Try();
        for (var i = 0u; i < Cff_NumberOfGlyphs; i++)
        {
            _consumer.Consume(x.ToOutline(i));
        }
    }

    [Benchmark]
    public void CFF_preload()
    {
        var x = FontLoader.LoadPostScriptFont(Cff, new());
        _consumer.Consume(x);
    }
}
