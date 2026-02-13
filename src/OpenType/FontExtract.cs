using Mina.Extension;
using PicoPDF.OpenType.Tables;
using PicoPDF.OpenType.Tables.PostScript;
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
            .Select((c, i) => (Char: c, Index: i + 1, GID: font.CharToGID(c)))
            .ToDictionary(x => x.Char, x => (
                    x.Index,
                    Glyph: font.Glyphs[x.GID],
                    HorizontalMetrics: font.HorizontalMetrics.Metrics[Math.Min(x.GID, font.HorizontalHeader.NumberOfHMetrics - 1)]
                ));
        var gid_glyph = char_glyph.Values
            .DistinctBy(x => x.Index)
            .ToDictionary(x => x.Index, x => (x.Glyph, x.HorizontalMetrics));
        var num_of_glyph = gid_glyph.Keys.Max();

        var name = ExtractNameTable(font.Name, opt);
        var maxp = CopyMaximumProfileTable(font.MaximumProfile, (ushort)(num_of_glyph + 1));
        var hhea = CopyHorizontalHeaderTable(font.HorizontalHeader, (ushort)(num_of_glyph + 1));
        var cmapN = CMapFormat12.CreateFormat(chars.ToDictionary(x => x, x => (uint)char_glyph[x].Index));
        var cmap = CreateCMapTable(cmapN, opt);

        var hmtx = new HorizontalMetricsTable()
        {
            Metrics = [.. Lists.RangeTo(1, num_of_glyph)
                .Select(x => gid_glyph.TryGetValue(x, out var glyph) ? glyph.HorizontalMetrics : font.HorizontalMetrics.Metrics[0])
                .Prepend(font.HorizontalMetrics.Metrics[0])],
            LeftSideBearing = [],
        };

        var glyf = Lists.RangeTo(1, num_of_glyph)
            .Select(x => gid_glyph.TryGetValue(x, out var glyph) ? glyph.Glyph : new NotdefGlyph())
            .Prepend(new NotdefGlyph())
            .ToArray();

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
            CMap = cmap,
            CharToGID = cmapN.CreateCharToGID(),
            IndexToLocation = null!,
            Glyphs = glyf,
        };
    }

    public static PostScriptFont Extract(PostScriptFont font, FontExtractOption opt)
    {
        var chars = opt.ExtractChars.Order().ToArray();
        var charsets = font.CompactFontFormat.TopDict.Charsets.Try();
        var char_glyph = chars
            .Select((c, i) => (Char: c, Index: i + 1, GID: font.CharToGID(c)))
            .ToDictionary(x => x.Char, x => (
                    x.Index,
                    Glyph: font.CompactFontFormat.TopDict.CharStrings[x.GID],
                    Charset: x.GID == 0 ? (ushort)0 : charsets.Glyph[x.GID - 1],
                    HorizontalMetrics: font.HorizontalMetrics.Metrics[Math.Min(x.GID, font.HorizontalHeader.NumberOfHMetrics - 1)],
                    FontDictSelect: x.GID >= font.CompactFontFormat.TopDict.FontDictSelect.Length ? (byte)0 : font.CompactFontFormat.TopDict.FontDictSelect[x.GID]
                ));
        var gid_glyph = char_glyph.Values
            .DistinctBy(x => x.Index)
            .ToDictionary(x => x.Index, x => (x.Glyph, x.Charset, x.HorizontalMetrics, x.FontDictSelect));
        var fdselect_index = char_glyph.Values
            .Select(x => x.FontDictSelect)
            .Distinct()
            .Order()
            .Select((x, i) => (FontDictSelect: x, Index: i + 1))
            .ToDictionary(x => x.FontDictSelect, x => (byte)x.Index);
        if (font.CompactFontFormat.TopDict.FontDictSelect.Length > 0)
        {
            gid_glyph[0] = (font.CompactFontFormat.TopDict.CharStrings[0], 0, font.HorizontalMetrics.Metrics[0], font.CompactFontFormat.TopDict.FontDictSelect[0]);
            fdselect_index[font.CompactFontFormat.TopDict.FontDictSelect[0]] = 0;
        }
        var num_of_glyph = gid_glyph.Keys.Max();

        var name = ExtractNameTable(font.Name, opt);
        var maxp = CopyMaximumProfileTable(font.MaximumProfile, (ushort)(num_of_glyph + 1));
        var hhea = CopyHorizontalHeaderTable(font.HorizontalHeader, (ushort)(num_of_glyph + 1));
        var cmapN = CMapFormat12.CreateFormat(chars.ToDictionary(x => x, x => (uint)char_glyph[x].Index));
        var cmap = CreateCMapTable(cmapN, opt);

        var hmtx = new HorizontalMetricsTable()
        {
            Metrics = [.. Lists.RangeTo(1, num_of_glyph)
                .Select(x => gid_glyph.TryGetValue(x, out var glyph) ? glyph.HorizontalMetrics : font.HorizontalMetrics.Metrics[0])
                .Prepend(font.HorizontalMetrics.Metrics[0])],
            LeftSideBearing = [],
        };

        var char_strings = Lists.RangeTo(1, num_of_glyph)
            .Select(x => gid_glyph.TryGetValue(x, out var glyph) ? glyph.Glyph : font.CompactFontFormat.TopDict.CharStrings[0])
            .Prepend(font.CompactFontFormat.TopDict.CharStrings[0])
            .ToArray();

        var fdselect = font.CompactFontFormat.TopDict.FontDictSelect.Length == 0
            ? []
            : Lists.RangeTo(0, num_of_glyph)
                .Select(x => gid_glyph.TryGetValue(x, out var glyph) ? glyph.FontDictSelect : (byte)0)
                .ToArray();

        var fdarray = fdselect
            .Order()
            .Distinct()
            .Select(x => font.CompactFontFormat.TopDict.FontDictArray[x].Clone())
            .ToArray();

        var top_dict = new DictData
        {
            Strings = font.CompactFontFormat.TopDict.Strings,
            Dict = font.CompactFontFormat.TopDict.Dict.ToDictionary(),
            CharStrings = char_strings,
            Charsets = new()
            {
                Format = 0,
                Glyph = [.. Lists.RangeTo(1, num_of_glyph).Select(x => (ushort)x)],
            },
            PrivateDict = font.CompactFontFormat.TopDict.PrivateDict,
            FontDictArray = fdarray,
            FontDictSelect = [.. fdselect.Select(x => fdselect_index[x])],
        };

        var cff = new CompactFontFormat
        {
            Major = font.CompactFontFormat.Major,
            Minor = font.CompactFontFormat.Minor,
            HeaderSize = font.CompactFontFormat.HeaderSize,
            OffsetSize = font.CompactFontFormat.OffsetSize,
            Names = font.CompactFontFormat.Names,
            TopDict = top_dict,
            Strings = font.CompactFontFormat.Strings,
            GlobalSubroutines = font.CompactFontFormat.GlobalSubroutines,
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
            CMap = cmap,
            CharToGID = cmapN.CreateCharToGID(),
            CompactFontFormat = cff,
        };
    }

    public static NameTable ExtractNameTable(NameTable name, FontExtractOption opt)
    {
        var names = name.NameRecords.Where(name_record => opt.OutputNames.Contains(x =>
                (x.PlatformID is null || ((ushort)x.PlatformID == name_record.NameRecord.PlatformID)) &&
                (x.EncodingID is null || ((ushort)x.EncodingID == name_record.NameRecord.EncodingID)) &&
                (x.LanguageID is null || (x.LanguageID == name_record.NameRecord.LanguageID)) &&
                (x.NameID is null || ((ushort)x.NameID == name_record.NameRecord.NameID))
            )).ToArray();

        return new() { Format = 0, Count = (ushort)names.Length, StringOffset = 0, NameRecords = names };
    }

    public static MaximumProfileTable CopyMaximumProfileTable(MaximumProfileTable maxp, ushort num_of_glyph_with_notdef) => new()
    {
        Version = maxp.Version,
        NumberOfGlyphs = num_of_glyph_with_notdef,
        MaxPoints = maxp.MaxPoints,
        MaxContours = maxp.MaxContours,
        MaxCompositePoints = maxp.MaxCompositePoints,
        MaxCompositeContours = maxp.MaxCompositeContours,
        MaxZones = maxp.MaxZones,
        MaxTwilightPoints = maxp.MaxTwilightPoints,
        MaxStorage = maxp.MaxStorage,
        MaxFunctionDefs = maxp.MaxFunctionDefs,
        MaxInstructionDefs = maxp.MaxInstructionDefs,
        MaxStackElements = maxp.MaxStackElements,
        MaxSizeOfInstructions = maxp.MaxSizeOfInstructions,
        MaxComponentElements = maxp.MaxComponentElements,
        MaxComponentDepth = maxp.MaxComponentDepth,
    };

    public static HorizontalHeaderTable CopyHorizontalHeaderTable(HorizontalHeaderTable hhea, ushort num_of_glyph_with_notdef) => new()
    {
        MajorVersion = hhea.MajorVersion,
        MinorVersion = hhea.MinorVersion,
        Ascender = hhea.Ascender,
        Descender = hhea.Descender,
        LineGap = hhea.LineGap,
        AdvanceWidthMax = hhea.AdvanceWidthMax,
        MinLeftSideBearing = hhea.MinLeftSideBearing,
        MinRightSideBearing = hhea.MinRightSideBearing,
        XMaxExtent = hhea.XMaxExtent,
        CaretSlopeRise = hhea.CaretSlopeRise,
        CaretSlopeRun = hhea.CaretSlopeRun,
        CaretOffset = hhea.CaretOffset,
        Reserved1 = hhea.Reserved1,
        Reserved2 = hhea.Reserved2,
        Reserved3 = hhea.Reserved3,
        Reserved4 = hhea.Reserved4,
        MetricDataFormat = hhea.MetricDataFormat,
        NumberOfHMetrics = num_of_glyph_with_notdef,
    };

    public static CMapTable CreateCMapTable(ICMapFormat cmap_format, FontExtractOption opt)
    {
        var cmaps = opt.OutputCMap
            .Select(x => new EncodingRecord { PlatformID = (ushort)x.PlatformID, EncodingID = (ushort)x.EncodingID, Offset = 0 })
            .ToArray();
        return new()
        {
            Version = 0,
            NumberOfTables = (ushort)cmaps.Length,
            EncodingRecords = cmaps.ToDictionary(x => x, _ => cmap_format),
        };
    }
}
