using Mina.Extension;
using OpenType.Tables;
using OpenType.Tables.CMap;
using OpenType.Tables.Colr;
using OpenType.Tables.PostScript;
using OpenType.Tables.TrueType;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenType;

public static class FontExtract
{
    public static TrueTypeFont Extract(TrueTypeFont font, FontExtractOption opt)
    {
        var chars = opt.ExtractChars.Order().ToArray();
        var gids = chars.Select(font.CharToGID).To(xs => font.Color is { } ? GetGIDWithColorGlyph([.. xs], font.Color) : xs).ToArray();
        var char_glyph = gids
            .Select((c, i) => (Index: i, GID: c))
            .ToDictionary(x => x.GID, x => (
                    Char: x.Index < chars.Length ? chars[x.Index] : 0,
                    NewGID: (uint)(x.Index + 1),
                    OldGID: x.GID,
                    Glyph: font.Glyphs[x.GID],
                    HorizontalMetrics: font.HorizontalMetrics.Metrics[Math.Min(x.GID, font.HorizontalHeader.NumberOfHMetrics - 1)]
                ));
        var gid_glyph = char_glyph.Values
            .DistinctBy(x => x.NewGID)
            .ToDictionary(x => x.NewGID, x => (x.Glyph, x.HorizontalMetrics));
        var num_of_glyph = (int)gid_glyph.Keys.Max();

        var name = ExtractNameTable(font.Name, opt);
        var maxp = CopyMaximumProfileTable(font.MaximumProfile, (ushort)(num_of_glyph + 1));
        var hhea = CopyHorizontalHeaderTable(font.HorizontalHeader, (ushort)(num_of_glyph + 1));
        var cmapN = CMapFormat12.CreateFormat(gids[0..chars.Length].ToDictionary(x => char_glyph[x].Char, x => char_glyph[x].NewGID));
        var cmap = CreateCMapTable(cmapN, opt);
        var colr = font.Color is null ? null : ExtractColorTable(font.Color, char_glyph.ToDictionary(kv => kv.Key, kv => kv.Value.NewGID));

        var hmtx = new HorizontalMetricsTable()
        {
            Metrics = [.. Lists.RangeTo(1, num_of_glyph)
                .Select(x => gid_glyph.TryGetValue((uint)x, out var glyph) ? glyph.HorizontalMetrics : font.HorizontalMetrics.Metrics[0])
                .Prepend(font.HorizontalMetrics.Metrics[0])],
            LeftSideBearing = [],
        };

        var glyf = Lists.RangeTo(1, num_of_glyph)
            .Select(x => gid_glyph.TryGetValue((uint)x, out var glyph) ? glyph.Glyph : new NotdefGlyph())
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
            ColorBitmapData = null,
            ColorBitmapLocation = null,
            Color = colr,
            ColorPalette = font.ColorPalette,
            StandardBitmapGraphics = null,
            ScalableVectorGraphics = null,
        };
    }

    public static PostScriptFont Extract(PostScriptFont font, FontExtractOption opt)
    {
        var chars = opt.ExtractChars.Order().ToArray();
        var gids = chars.Select(font.CharToGID).To(xs => font.Color is { } ? GetGIDWithColorGlyph([.. xs], font.Color) : xs).ToArray();
        var charsets = font.CompactFontFormat.TopDict.Charsets.Try();
        var char_glyph = gids
            .Select((c, i) => (Index: i, GID: c))
            .ToDictionary(x => x.GID, x => (
                    Char: x.Index < chars.Length ? chars[x.Index] : 0,
                    NewGID: (uint)(x.Index + 1),
                    OldGID: x.GID,
                    Glyph: font.CompactFontFormat.TopDict.CharStrings[x.GID],
                    Charset: x.GID == 0 ? (ushort)0 : charsets.Glyph[x.GID - 1],
                    HorizontalMetrics: font.HorizontalMetrics.Metrics[Math.Min(x.GID, font.HorizontalHeader.NumberOfHMetrics - 1)],
                    FontDictSelect: x.GID >= font.CompactFontFormat.TopDict.FontDictSelect.Length ? (byte)0 : font.CompactFontFormat.TopDict.FontDictSelect[x.GID]
                ));
        var gid_glyph = char_glyph.Values
            .DistinctBy(x => x.NewGID)
            .ToDictionary(x => x.NewGID, x => (x.Glyph, x.Charset, x.HorizontalMetrics, x.FontDictSelect));
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
        var num_of_glyph = (int)gid_glyph.Keys.Max();

        var name = ExtractNameTable(font.Name, opt);
        var maxp = CopyMaximumProfileTable(font.MaximumProfile, (ushort)(num_of_glyph + 1));
        var hhea = CopyHorizontalHeaderTable(font.HorizontalHeader, (ushort)(num_of_glyph + 1));
        var cmapN = CMapFormat12.CreateFormat(gids[0..chars.Length].ToDictionary(x => char_glyph[x].Char, x => char_glyph[x].NewGID));
        var cmap = CreateCMapTable(cmapN, opt);
        var colr = font.Color is null ? null : ExtractColorTable(font.Color, char_glyph.ToDictionary(kv => kv.Key, kv => kv.Value.NewGID));

        var hmtx = new HorizontalMetricsTable()
        {
            Metrics = [.. Lists.RangeTo(1, num_of_glyph)
                .Select(x => gid_glyph.TryGetValue((uint)x, out var glyph) ? glyph.HorizontalMetrics : font.HorizontalMetrics.Metrics[0])
                .Prepend(font.HorizontalMetrics.Metrics[0])],
            LeftSideBearing = [],
        };

        var char_strings = Lists.RangeTo(1, num_of_glyph)
            .Select(x => gid_glyph.TryGetValue((uint)x, out var glyph) ? glyph.Glyph : font.CompactFontFormat.TopDict.CharStrings[0])
            .Prepend(font.CompactFontFormat.TopDict.CharStrings[0])
            .ToArray();

        var fdselect = font.CompactFontFormat.TopDict.FontDictSelect.Length == 0
            ? []
            : Lists.RangeTo(0, num_of_glyph)
                .Select(x => gid_glyph.TryGetValue((uint)x, out var glyph) ? glyph.FontDictSelect : (byte)0)
                .ToArray();

        var global_subr = font.CompactFontFormat.GlobalSubroutines;
        var global_subr_mark = new HashSet<int>();
        var local_subr_mark = new Dictionary<byte, HashSet<int>>();
        var fdselect_unique = fdselect.Order().Distinct().ToArray();
        fdselect_unique.Each(x => local_subr_mark.Add(fdselect_index[x], []));
        for (var i = 0; i < char_strings.Length; i++)
        {
            var fdindex = gid_glyph[(uint)i].FontDictSelect;
            var local_subr = font.CompactFontFormat.TopDict.FontDictArray[fdindex].PrivateDict?.LocalSubroutines ?? [];
            var current_subr_mark = local_subr_mark[fdselect_index[fdindex]];
            Subroutine.EnumSubroutines(char_strings[i], local_subr, global_subr, (global, index) => (global ? global_subr_mark : current_subr_mark).Add(index));
        }

        var fdarray = fdselect_unique
            .Select(x =>
            {
                var dict = font.CompactFontFormat.TopDict.FontDictArray[x];
                var private_dict = dict.PrivateDict;
                if (private_dict is { })
                {
                    var subr_mark = local_subr_mark[fdselect_index[x]];
                    private_dict = private_dict.Clone(subr: [.. private_dict.LocalSubroutines.Select((x, i) => subr_mark.Contains(i) ? x : [11])]);
                }
                return dict.Clone(private_dict);
            })
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
            GlobalSubroutines = [.. global_subr.Select((x, i) => global_subr_mark.Contains(i) ? x : [11])],
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
            ColorBitmapData = null,
            ColorBitmapLocation = null,
            Color = colr,
            ColorPalette = font.ColorPalette,
            StandardBitmapGraphics = null,
            ScalableVectorGraphics = null,
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

    public static IEnumerable<uint> GetGIDWithColorGlyph(uint[] gids, ColorTable colr)
    {
        foreach (var x in gids) yield return x;

        var hash = gids.ToHashSet();
        foreach (var gid in gids)
        {
            foreach (var x in EnumGIDWithColorGlyph(gid, colr, hash.Add)) yield return x;
        }
    }

    public static IEnumerable<uint> EnumGIDWithColorGlyph(uint gid, ColorTable colr, Func<uint, bool> f)
    {
        foreach (var record in colr.BaseGlyphRecords.Where(x => x.GlyphID == gid))
        {
            foreach (var layer in colr.LayerRecords[record.FirstLayerIndex..(record.FirstLayerIndex + record.NumberOfLayers)])
            {
                if (f(layer.GlyphID))
                {
                    yield return layer.GlyphID;
                    foreach (var x in EnumGIDWithColorGlyph(layer.GlyphID, colr, f)) yield return x;
                }
            }
        }

        for (var i = 0; i < colr.BaseGlyphListRecord?.BaseGlyphPaintRecord.Length; i++)
        {
            if (colr.BaseGlyphListRecord.BaseGlyphPaintRecord[i].GlyphID == gid)
            {
                foreach (var x in EnumGIDWithColorGlyph(colr.BaseGlyphListRecord.Paints[i], colr, f)) yield return x;
            }
        }
    }

    public static IEnumerable<uint> EnumGIDWithColorGlyph(IPaintFormat paint, ColorTable colr, Func<uint, bool> f)
    {
        if (paint is PaintColrLayers paintColrLayers)
        {
            foreach (var layer in colr.LayerRecords[(int)paintColrLayers.FirstLayerIndex..((int)paintColrLayers.FirstLayerIndex + paintColrLayers.NumberOfLayers)])
            {
                if (f(layer.GlyphID))
                {
                    yield return layer.GlyphID;
                    foreach (var x in EnumGIDWithColorGlyph(layer.GlyphID, colr, f)) yield return x;
                }
            }
        }
        else if (paint is PaintComposite paintComposite)
        {
            foreach (var x in EnumGIDWithColorGlyph(paintComposite.SourcePaint, colr, f)) yield return x;
            foreach (var x in EnumGIDWithColorGlyph(paintComposite.BackdropPaint, colr, f)) yield return x;
        }
        else
        {
            if (paint is IHaveGlyph glyph)
            {
                if (f(glyph.GlyphID)) yield return glyph.GlyphID;
            }
            if (paint is IHavePaint subpaint)
            {
                foreach (var x in EnumGIDWithColorGlyph(subpaint.Paint, colr, f)) yield return x;
            }
        }
    }

    public static ColorTable ExtractColorTable(ColorTable colr, Dictionary<uint, uint> mapper)
    {
        var layerRecords = new List<LayerRecord>();

        var baseGlyphRecords = colr.BaseGlyphRecords
            .Where(kv => mapper.ContainsKey(kv.GlyphID))
            .Select(record =>
            {
                layerRecords.AddRange(colr.LayerRecords[record.FirstLayerIndex..(record.FirstLayerIndex + record.NumberOfLayers)]
                    .Select(x => new LayerRecord() { GlyphID = (ushort)mapper[x.GlyphID], PaletteIndex = x.PaletteIndex }));

                return new BaseGlyphRecord { GlyphID = (ushort)mapper[record.GlyphID], FirstLayerIndex = (ushort)(layerRecords.Count - record.NumberOfLayers), NumberOfLayers = record.NumberOfLayers };
            })
            .ToArray();

        var baseGlyphListRecord = colr.BaseGlyphListRecord?.To(glyphs =>
            {
                var glyphsWithIndex = glyphs.BaseGlyphPaintRecord.Zip(Lists.Sequence(0)).ToArray();
                var newGlyphs = mapper.Select(x => glyphsWithIndex.Where(r => mapper.ContainsKey(r.First.GlyphID)).FirstOrDefault())
                    .Where(x => x != default)
                    .ToArray();

                return new BaseGlyphListRecord
                {
                    NumberBaseGlyphPaintRecords = 0,
                    BaseGlyphPaintRecord = [.. newGlyphs.Select(x => (x.First.GlyphID, 0U))],
                    Paints = [.. newGlyphs.Select(x => glyphs.Paints[x.Second])],
                };
            });

        return new()
        {
            Version = colr.Version,
            NumberBaseGlyphRecords = (ushort)baseGlyphRecords.Length,
            BaseGlyphRecordsOffset = 0,
            LayerRecordsOffset = 0,
            NumberLayerRecords = (ushort)layerRecords.Count,
            BaseGlyphRecords = baseGlyphRecords,
            LayerRecords = [.. layerRecords],
            BaseGlyphListRecord = baseGlyphListRecord?.BaseGlyphPaintRecord.Length == 0 ? null : baseGlyphListRecord,
            LayerListRecord = colr.LayerListRecord,
            ClipListRecord = colr.ClipListRecord,
            DeltaSetIndexMapRecord = colr.DeltaSetIndexMapRecord,
            ItemVariationStoreRecord = colr.ItemVariationStoreRecord,
        };
    }
}
