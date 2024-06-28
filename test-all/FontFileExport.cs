using PicoPDF.OpenType;
using PicoPDF.Pdf.Font;
using System.IO;
using System.Linq;

namespace PicoPDF.TestAll;

public static class FontFileExport
{
    public static void Export(FontRegister fontreg, Option opt)
    {
        var font = fontreg.Get(opt.FontFileExport);
        if (font is TrueTypeFont ttf) Export(ttf, opt);
    }

    public static void Export(TrueTypeFont font, Option opt)
    {
        var chars = opt.FontExportChars.Order();
        var char_glyph = chars
            .Select((c, i) => (Char: c, Index: (ushort)i))
            .ToDictionary(x => x.Char, x => (x.Index, Glyph: font.Glyphs[font.MeasureChar(x.Char)]));

        var cmap4 = CMapFormat4.CreateFormat(chars.ToDictionary(x => x, x => char_glyph[x].Index));
        var cmap4_range = FontLoader.CreateCMap4Range(cmap4);

        var ttf = new TrueTypeFont()
        {
            FontFamily = font.FontFamily,
            Style = font.Style,
            FullFontName = font.FullFontName,
            PostScriptName = font.PostScriptName,
            Path = font.Path,
            Position = font.Position,
            TableRecords = font.TableRecords,
            Offset = font.Offset,
            Name = font.Name,
            FontHeader = font.FontHeader,
            MaximumProfile = font.MaximumProfile,
            PostScript = font.PostScript,
            OS2 = font.OS2,
            HorizontalHeader = font.HorizontalHeader,
            HorizontalMetrics = font.HorizontalMetrics,
            CMap = new() { Version = 0, NumberOfTables = 1, EncodingRecords = [new() { PlatformID = (ushort)PlatformEncodings.UnicodeBMPOnly >> 16, EncodingID = (ushort)((uint)PlatformEncodings.UnicodeBMPOnly & 0xFFFF), Offset = 0 }] },
            CMap4 = cmap4,
            CMap4Range = cmap4_range,
            IndexToLocation = font.IndexToLocation,
            Glyphs = chars.Select(x => char_glyph[x].Glyph).ToArray(),
        };

        using var stream = new MemoryStream();
        FontExporter.Export(ttf, stream);
    }
}
