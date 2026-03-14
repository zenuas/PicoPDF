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
        var callback = new GIDTraverse() { GIDPreOrderCallback = hash.Add };
        foreach (var gid in gids)
        {
            foreach (var x in callback.EnumGID(gid, colr)) yield return x;
        }
    }

    public static ColorTable ExtractColorTable(ColorTable colr, Dictionary<uint, uint> mapper)
    {
        var hash = new HashSet<uint>();
        var layerList = new List<IPaintFormat>();
        var paints = new Dictionary<IPaintFormat, IPaintFormat>();
        _ = new GIDTraverse()
        {
            GIDPreOrderCallback = hash.Add,
            PaintPostOrderCallback = paint =>
            {
                if (paints.ContainsKey(paint)) return;
                switch (paint)
                {
                    case PaintColrLayers p:
                        paints.Add(p, new PaintColrLayers
                        {
                            Format = p.Format,
                            NumberOfLayers = p.NumberOfLayers,
                            FirstLayerIndex = (uint)layerList.Count,
                        });
                        layerList.AddRange(colr.LayerListRecord!.Paints[(int)p.FirstLayerIndex..(int)(p.FirstLayerIndex + p.NumberOfLayers)]);
                        break;

                    case PaintSolid p:
                        paints.Add(p, new PaintSolid
                        {
                            Format = p.Format,
                            PaletteIndex = p.PaletteIndex,
                            Alpha = p.Alpha,
                        });
                        break;

                    case PaintVarSolid p:
                        paints.Add(p, new PaintVarSolid
                        {
                            Format = p.Format,
                            PaletteIndex = p.PaletteIndex,
                            Alpha = p.Alpha,
                            VarIndexBase = p.VarIndexBase,
                        });
                        break;

                    case PaintLinearGradient p:
                        paints.Add(p, new PaintLinearGradient
                        {
                            Format = p.Format,
                            ColorLineOffset = 0,
                            X0 = p.X0,
                            Y0 = p.Y0,
                            X1 = p.X1,
                            Y1 = p.Y1,
                            X2 = p.X2,
                            Y2 = p.Y2,
                            ColorLine = p.ColorLine,
                        });
                        break;

                    case PaintVarLinearGradient p:
                        paints.Add(p, new PaintVarLinearGradient
                        {
                            Format = p.Format,
                            ColorLineOffset = 0,
                            X0 = p.X0,
                            Y0 = p.Y0,
                            X1 = p.X1,
                            Y1 = p.Y1,
                            X2 = p.X2,
                            Y2 = p.Y2,
                            ColorLine = p.ColorLine,
                            VarIndexBase = p.VarIndexBase,
                        });
                        break;

                    case PaintRadialGradient p:
                        paints.Add(p, new PaintRadialGradient
                        {
                            Format = p.Format,
                            ColorLineOffset = 0,
                            X0 = p.X0,
                            Y0 = p.Y0,
                            Radius0 = p.Radius0,
                            X1 = p.X1,
                            Y1 = p.Y1,
                            Radius1 = p.Radius1,
                            ColorLine = p.ColorLine,
                        });
                        break;

                    case PaintVarRadialGradient p:
                        paints.Add(p, new PaintVarRadialGradient
                        {
                            Format = p.Format,
                            ColorLineOffset = 0,
                            X0 = p.X0,
                            Y0 = p.Y0,
                            Radius0 = p.Radius0,
                            X1 = p.X1,
                            Y1 = p.Y1,
                            Radius1 = p.Radius1,
                            VarIndexBase = p.VarIndexBase,
                            ColorLine = p.ColorLine,
                        });
                        break;

                    case PaintSweepGradient p:
                        paints.Add(p, new PaintSweepGradient
                        {
                            Format = p.Format,
                            ColorLineOffset = 0,
                            CenterX = p.CenterX,
                            CenterY = p.CenterY,
                            StartAngle = p.StartAngle,
                            EndAngle = p.EndAngle,
                            ColorLine = p.ColorLine,
                        });
                        break;

                    case PaintVarSweepGradient p:
                        paints.Add(p, new PaintVarSweepGradient
                        {
                            Format = p.Format,
                            ColorLineOffset = 0,
                            CenterX = p.CenterX,
                            CenterY = p.CenterY,
                            StartAngle = p.StartAngle,
                            EndAngle = p.EndAngle,
                            VarIndexBase = p.VarIndexBase,
                            ColorLine = p.ColorLine,
                        });
                        break;

                    case PaintGlyph p:
                        paints.Add(p, new PaintGlyph
                        {
                            Format = p.Format,
                            PaintOffset = 0,
                            GlyphID = (ushort)mapper[p.GlyphID],
                            Paint = paints[p.Paint],
                        });
                        break;

                    case PaintColrGlyph p:
                        paints.Add(p, new PaintColrGlyph
                        {
                            Format = p.Format,
                            GlyphID = (ushort)mapper[p.GlyphID],
                        });
                        break;

                    case PaintTransform p:
                        paints.Add(p, new PaintTransform
                        {
                            Format = p.Format,
                            PaintOffset = 0,
                            TransformOffset = 0,
                            Paint = paints[p.Paint],
                            Transform = p.Transform,
                        });
                        break;

                    case PaintVarTransform p:
                        paints.Add(p, new PaintVarTransform
                        {
                            Format = p.Format,
                            PaintOffset = 0,
                            TransformOffset = 0,
                            Paint = paints[p.Paint],
                            Transform = p.Transform,
                        });
                        break;

                    case PaintTranslate p:
                        paints.Add(p, new PaintTranslate
                        {
                            Format = p.Format,
                            PaintOffset = 0,
                            DX = p.DX,
                            DY = p.DY,
                            Paint = paints[p.Paint],
                        });
                        break;

                    case PaintVarTranslate p:
                        paints.Add(p, new PaintVarTranslate
                        {
                            Format = p.Format,
                            PaintOffset = 0,
                            DX = p.DX,
                            DY = p.DY,
                            Paint = paints[p.Paint],
                            VarIndexBase = p.VarIndexBase,
                        });
                        break;

                    case PaintScale p:
                        paints.Add(p, new PaintScale
                        {
                            Format = p.Format,
                            PaintOffset = 0,
                            ScaleX = p.ScaleX,
                            ScaleY = p.ScaleY,
                            Paint = paints[p.Paint],
                        });
                        break;

                    case PaintVarScale p:
                        paints.Add(p, new PaintVarScale
                        {
                            Format = p.Format,
                            PaintOffset = 0,
                            ScaleX = p.ScaleX,
                            ScaleY = p.ScaleY,
                            Paint = paints[p.Paint],
                            VarIndexBase = p.VarIndexBase,
                        });
                        break;

                    case PaintScaleAroundCenter p:
                        paints.Add(p, new PaintScaleAroundCenter
                        {
                            Format = p.Format,
                            PaintOffset = 0,
                            ScaleX = p.ScaleX,
                            ScaleY = p.ScaleY,
                            CenterX = p.CenterX,
                            CenterY = p.CenterY,
                            Paint = paints[p.Paint],
                        });
                        break;

                    case PaintVarScaleAroundCenter p:
                        paints.Add(p, new PaintVarScaleAroundCenter
                        {
                            Format = p.Format,
                            PaintOffset = 0,
                            ScaleX = p.ScaleX,
                            ScaleY = p.ScaleY,
                            CenterX = p.CenterX,
                            CenterY = p.CenterY,
                            Paint = paints[p.Paint],
                            VarIndexBase = p.VarIndexBase,
                        });
                        break;

                    case PaintScaleUniform p:
                        paints.Add(p, new PaintScaleUniform
                        {
                            Format = p.Format,
                            PaintOffset = 0,
                            Scale = p.Scale,
                            Paint = paints[p.Paint],
                        });
                        break;

                    case PaintVarScaleUniform p:
                        paints.Add(p, new PaintVarScaleUniform
                        {
                            Format = p.Format,
                            PaintOffset = 0,
                            Scale = p.Scale,
                            Paint = paints[p.Paint],
                            VarIndexBase = p.VarIndexBase,
                        });
                        break;

                    case PaintScaleUniformAroundCenter p:
                        paints.Add(p, new PaintScaleUniformAroundCenter
                        {
                            Format = p.Format,
                            PaintOffset = 0,
                            Scale = p.Scale,
                            CenterX = p.CenterX,
                            CenterY = p.CenterY,
                            Paint = paints[p.Paint],
                        });
                        break;

                    case PaintVarScaleUniformAroundCenter p:
                        paints.Add(p, new PaintVarScaleUniformAroundCenter
                        {
                            Format = p.Format,
                            PaintOffset = 0,
                            Scale = p.Scale,
                            CenterX = p.CenterX,
                            CenterY = p.CenterY,
                            Paint = paints[p.Paint],
                            VarIndexBase = p.VarIndexBase,
                        });
                        break;

                    case PaintRotate p:
                        paints.Add(p, new PaintRotate
                        {
                            Format = p.Format,
                            PaintOffset = 0,
                            Angle = p.Angle,
                            Paint = paints[p.Paint],
                        });
                        break;

                    case PaintVarRotate p:
                        paints.Add(p, new PaintVarRotate
                        {
                            Format = p.Format,
                            PaintOffset = 0,
                            Angle = p.Angle,
                            Paint = paints[p.Paint],
                            VarIndexBase = p.VarIndexBase,
                        });
                        break;

                    case PaintRotateAroundCenter p:
                        paints.Add(p, new PaintRotateAroundCenter
                        {
                            Format = p.Format,
                            PaintOffset = 0,
                            Angle = p.Angle,
                            CenterX = p.CenterX,
                            CenterY = p.CenterY,
                            Paint = paints[p.Paint],
                        });
                        break;

                    case PaintVarRotateAroundCenter p:
                        paints.Add(p, new PaintVarRotateAroundCenter
                        {
                            Format = p.Format,
                            PaintOffset = 0,
                            Angle = p.Angle,
                            CenterX = p.CenterX,
                            CenterY = p.CenterY,
                            Paint = paints[p.Paint],
                            VarIndexBase = p.VarIndexBase,
                        });
                        break;

                    case PaintSkew p:
                        paints.Add(p, new PaintSkew
                        {
                            Format = p.Format,
                            PaintOffset = 0,
                            XSkewAngle = p.XSkewAngle,
                            YSkewAngle = p.YSkewAngle,
                            Paint = paints[p.Paint],
                        });
                        break;

                    case PaintVarSkew p:
                        paints.Add(p, new PaintVarSkew
                        {
                            Format = p.Format,
                            PaintOffset = 0,
                            XSkewAngle = p.XSkewAngle,
                            YSkewAngle = p.YSkewAngle,
                            Paint = paints[p.Paint],
                            VarIndexBase = p.VarIndexBase,
                        });
                        break;

                    case PaintSkewAroundCenter p:
                        paints.Add(p, new PaintSkewAroundCenter
                        {
                            Format = p.Format,
                            PaintOffset = 0,
                            XSkewAngle = p.XSkewAngle,
                            YSkewAngle = p.YSkewAngle,
                            CenterX = p.CenterX,
                            CenterY = p.CenterY,
                            Paint = paints[p.Paint],
                        });
                        break;

                    case PaintVarSkewAroundCenter p:
                        paints.Add(p, new PaintVarSkewAroundCenter
                        {
                            Format = p.Format,
                            PaintOffset = 0,
                            XSkewAngle = p.XSkewAngle,
                            YSkewAngle = p.YSkewAngle,
                            CenterX = p.CenterX,
                            CenterY = p.CenterY,
                            Paint = paints[p.Paint],
                            VarIndexBase = p.VarIndexBase,
                        });
                        break;

                    case PaintComposite p:
                        paints.Add(p, new PaintComposite
                        {
                            Format = p.Format,
                            SourcePaintOffset = 0,
                            CompositeMode = p.CompositeMode,
                            BackdropPaintOffset = 0,
                            SourcePaint = paints[p.SourcePaint],
                            BackdropPaint = paints[p.BackdropPaint],
                        });
                        break;
                }
            },
        }.Traverse(mapper.Keys, colr);

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
                var glyphsWithIndex = glyphs.BaseGlyphPaintRecord.Zip(Lists.Sequence(0)).ToDictionary(x => x.First.GlyphID, x => x.Second);
                var newGlyphs = mapper
                    .Where(kv => glyphsWithIndex.ContainsKey((ushort)kv.Key))
                    .Select(kv => (NewGID: kv.Value, PaintIndex: glyphsWithIndex[(ushort)kv.Key]))
                    .ToArray();

                return new BaseGlyphListRecord
                {
                    NumberBaseGlyphPaintRecords = 0,
                    BaseGlyphPaintRecord = [.. newGlyphs.Select(x => ((ushort)x.NewGID, 0U))],
                    Paints = [.. newGlyphs.Select(x => paints[glyphs.Paints[x.PaintIndex]])],
                };
            });

        var clipListRecord = colr.ClipListRecord?.To(record =>
        {
            var clipList = new List<(ushort GlyphID, ClipBoxFormat ClipBoxFormat)>();
            foreach (var (oldGID, newGID) in mapper.OrderBy(kv => kv.Value))
            {
                var index = record.Clips.FindFirstIndex(x => x.StartGlyphID <= oldGID && oldGID <= x.EndGlyphID);
                if (index >= 0) clipList.Add(((ushort)newGID, record.ClipBoxFormats[index]));
            }

            return new ClipListRecord
            {
                Format = record.Format,
                NumberClips = 0,
                Clips = [.. clipList.Select(x => (x.GlyphID, x.GlyphID, 0))],
                ClipBoxFormats = [.. clipList.Select(x => x.ClipBoxFormat)],
            };
        });

        return new()
        {
            Version = colr.Version,
            NumberBaseGlyphRecords = 0,
            BaseGlyphRecordsOffset = 0,
            LayerRecordsOffset = 0,
            NumberLayerRecords = 0,
            BaseGlyphRecords = baseGlyphRecords,
            LayerRecords = [.. layerRecords],
            BaseGlyphListRecord = baseGlyphListRecord?.BaseGlyphPaintRecord.Length == 0 ? null : baseGlyphListRecord,
            LayerListRecord = layerList.Count == 0 ? null : new LayerListRecord { NumberOfLayers = 0, PaintOffsets = [], Paints = [.. layerList.Select(x => paints[x])] },
            ClipListRecord = clipListRecord?.Clips.Length == 0 ? null : clipListRecord,
            DeltaSetIndexMapRecord = colr.DeltaSetIndexMapRecord,
            ItemVariationStoreRecord = colr.ItemVariationStoreRecord,
        };
    }
}
