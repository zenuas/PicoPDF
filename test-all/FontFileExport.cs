using PicoPDF.OpenType;
using PicoPDF.Pdf.Font;
using System.IO;
using System.Linq;

namespace PicoPDF.TestAll;

public static class FontFileExport
{
    public static void Export(FontRegister fontreg, Option opt)
    {
        var font = fontreg.GetComplete(opt.FontFileExport);
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

        var maxp = new MaximumProfileTable()
        {
            Version = font.MaximumProfile.Version,
            NumberOfGlyphs = (ushort)opt.FontExportChars.Length,
            MaxPoints = font.MaximumProfile.MaxPoints,
            MaxContours = font.MaximumProfile.MaxContours,
            MaxCompositePoints = font.MaximumProfile.MaxCompositePoints,
            MaxCompositeContours = font.MaximumProfile.MaxCompositeContours,
            MaxZones = font.MaximumProfile.MaxZones,
            MaxTwilightPoints = font.MaximumProfile.MaxTwilightPoints,
            MaxStorage = font.MaximumProfile.MaxStorage,
            MaxFunctionDefs = font.MaximumProfile.MaxFunctionDefs,
            MaxInstructionDefs = font.MaximumProfile.MaxInstructionDefs,
            MaxStackElements = font.MaximumProfile.MaxStackElements,
            MaxSizeOfInstructions = font.MaximumProfile.MaxSizeOfInstructions,
            MaxComponentElements = font.MaximumProfile.MaxComponentElements,
            MaxComponentDepth = font.MaximumProfile.MaxComponentDepth,
        };

        var ttf = new TrueTypeFont()
        {
            PostScriptName = font.PostScriptName,
            Path = font.Path,
            Position = font.Position,
            TableRecords = font.TableRecords,
            Offset = font.Offset,
            Name = font.Name,
            FontHeader = font.FontHeader,
            MaximumProfile = maxp,
            PostScript = font.PostScript,
            OS2 = font.OS2,
            HorizontalHeader = font.HorizontalHeader,
            HorizontalMetrics = font.HorizontalMetrics,
            CMap = new() { Version = 0, NumberOfTables = 1, EncodingRecords = new() { [new() { PlatformID = (ushort)Platforms.Unicode, EncodingID = (ushort)Encodings.Unicode2_0_BMPOnly, Offset = 0 }] = cmap4 } },
            CMap4 = cmap4,
            CMap4Range = cmap4_range,
            IndexToLocation = font.IndexToLocation,
            Glyphs = chars.Select(x => char_glyph[x].Glyph).ToArray(),
        };

        using var stream = new MemoryStream();
        FontExporter.Export(ttf, stream);
    }
}
