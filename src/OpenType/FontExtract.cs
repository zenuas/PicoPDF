using Mina.Extension;
using PicoPDF.OpenType.Tables;
using PicoPDF.OpenType.Tables.TrueType;
using System;
using System.Linq;

namespace PicoPDF.OpenType;

public static class FontExtract
{
    public static TrueTypeFont Extract(TrueTypeFont font, FontExtractOption opt)
    {
        var chars = opt.ExtractChars.Order().ToArray();
        var char_glyph = chars
            .Select((c, i) => (Char: c, Index: (ushort)(i + 1), GID: font.CharToGIDCached(c)))
            .ToDictionary(x => x.Char, x => (x.Index, Glyph: font.Glyphs[x.GID], HorizontalMetrics: font.HorizontalMetrics.Metrics[Math.Min(x.GID, font.HorizontalHeader.NumberOfHMetrics - 1)]));
        var gid_glyf = char_glyph.Values
            .DistinctBy(x => x.Index)
            .ToDictionary(x => x.Index, x => (x.Glyph, x.HorizontalMetrics));
        var num_of_glyf = gid_glyf.Keys.Max();

        var cmap4 = CMapFormat4.CreateFormat(chars.ToDictionary(x => x, x => char_glyph[x].Index));
        var cmap4_range = FontLoader.CreateCMap4Range(cmap4);

        var names = font.Name.NameRecords.Where(x => opt.OutputNames.Contains(y =>
                (y.PlatformID is null || ((ushort)y.PlatformID == x.NameRecord.PlatformID)) &&
                (y.EncodingID is null || ((ushort)y.EncodingID == x.NameRecord.EncodingID)) &&
                (y.LanguageID is null || (y.LanguageID == x.NameRecord.LanguageID)) &&
                (y.NameID is null || ((ushort)y.NameID == x.NameRecord.NameID))
            )).ToArray();
        var name = new NameTable() { Format = 0, Count = (ushort)names.Length, StringOffset = 0, NameRecords = names };

        var cmaps = opt.OutputCMap
            .Select(x => new EncodingRecord { PlatformID = (ushort)x.PlatformID, EncodingID = (ushort)x.EncodingID, Offset = 0 })
            .ToArray();

        var maxp = new MaximumProfileTable()
        {
            Version = font.MaximumProfile.Version,
            NumberOfGlyphs = (ushort)(num_of_glyf + 1),
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

        var hhea = new HorizontalHeaderTable()
        {
            MajorVersion = font.HorizontalHeader.MajorVersion,
            MinorVersion = font.HorizontalHeader.MinorVersion,
            Ascender = font.HorizontalHeader.Ascender,
            Descender = font.HorizontalHeader.Descender,
            LineGap = font.HorizontalHeader.LineGap,
            AdvanceWidthMax = font.HorizontalHeader.AdvanceWidthMax,
            MinLeftSideBearing = font.HorizontalHeader.MinLeftSideBearing,
            MinRightSideBearing = font.HorizontalHeader.MinRightSideBearing,
            XMaxExtent = font.HorizontalHeader.XMaxExtent,
            CaretSlopeRise = font.HorizontalHeader.CaretSlopeRise,
            CaretSlopeRun = font.HorizontalHeader.CaretSlopeRun,
            CaretOffset = font.HorizontalHeader.CaretOffset,
            Reserved1 = font.HorizontalHeader.Reserved1,
            Reserved2 = font.HorizontalHeader.Reserved2,
            Reserved3 = font.HorizontalHeader.Reserved3,
            Reserved4 = font.HorizontalHeader.Reserved4,
            MetricDataFormat = font.HorizontalHeader.MetricDataFormat,
            NumberOfHMetrics = (ushort)(num_of_glyf + 1),
        };

        var hmtx = new HorizontalMetricsTable()
        {
            Metrics = Lists.RangeTo(1, num_of_glyf)
                .Select(x => gid_glyf.TryGetValue((ushort)x, out var glyf) ? glyf.HorizontalMetrics : font.HorizontalMetrics.Metrics[0])
                .Prepend(font.HorizontalMetrics.Metrics[0])
                .ToArray(),
            LeftSideBearing = [],
        };

        return new()
        {
            PostScriptName = font.PostScriptName,
            Path = font.Path,
            Position = font.Position,
            TableRecords = font.TableRecords,
            Offset = font.Offset,
            Name = name,
            FontHeader = font.FontHeader,
            MaximumProfile = maxp,
            PostScript = font.PostScript,
            OS2 = font.OS2,
            HorizontalHeader = hhea,
            HorizontalMetrics = hmtx,
            CMap = new() { Version = 0, NumberOfTables = (ushort)cmaps.Length, EncodingRecords = cmaps.ToDictionary(x => x, _ => (ICMapFormat?)cmap4) },
            CMap4 = cmap4,
            CMap4Range = cmap4_range,
            IndexToLocation = null!,
            Glyphs = Lists.RangeTo(1, num_of_glyf)
                .Select(x => gid_glyf.TryGetValue((ushort)x, out var glyf) ? glyf.Glyph : new NotdefGlyph())
                .Prepend(new NotdefGlyph())
                .ToArray(),
        };
    }
}
