using PicoPDF.OpenType;
using PicoPDF.Pdf.Font;
using System.IO;

namespace PicoPDF.TestAll;

public static class FontFileExport
{
    public static void Export(FontRegister fontreg, Option opt)
    {
        var font = fontreg.Get(FontRegister.GetFontFilePath(opt.FontFileExport));
        if (font is TrueTypeFont ttf) Export(ttf, opt);
    }

    public static void Export(TrueTypeFont font, Option opt)
    {
        var cmap4 = CreateCMap4(opt.FontExportChars);
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
            NameRecords = font.NameRecords,
            FontHeader = font.FontHeader,
            MaximumProfile = font.MaximumProfile,
            PostScript = font.PostScript,
            OS2 = font.OS2,
            EncodingRecords = [new() { PlatformID = (ushort)PlatformEncodings.UnicodeBMPOnly >> 16, EncodingID = (ushort)PlatformEncodings.UnicodeBMPOnly & 0xFF, Offset = 0 }],
            CMap4 = cmap4,
            CMap4Range = cmap4_range,
            HorizontalHeader = font.HorizontalHeader,
            HorizontalMetrics = font.HorizontalMetrics,
            IndexToLocation = font.IndexToLocation,
            Glyphs = [],
        };

        using var stream = new MemoryStream();
        FontExport.Export(ttf, stream);
    }

    public static CMapFormat4 CreateCMap4(string s)
    {
        return new()
        {
            Format = 4,
            Length = (ushort)s.Length,
            Language = 0,
            SegCountX2 = 0,
            SearchRange = 0,
            EntrySelector = 0,
            RangeShift = 0,
            EndCode = [],
            ReservedPad = 0,
            StartCode = [],
            IdDelta = [],
            IdRangeOffsets = [],
            GlyphIdArray = [],
        };
    }
}
