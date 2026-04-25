using Mina.Extension;
using OpenType.Outline;
using OpenType.Tables;
using OpenType.Tables.CMap;
using OpenType.Tables.Colr;
using OpenType.Tables.PostScript;
using OpenType.Tables.TrueType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace OpenType;

public static class FontExtract
{
    public static IOpenTypeFont Extract(IOpenTypeFont font, FontExtractOption opt) => font switch
    {
        TrueTypeFont ttf => Extract(ttf, opt),
        PostScriptFont psf => Extract(psf, opt),
        NoOutlineFont noo => Extract(noo, opt),
        _ => throw new(),
    };

    public static TrueTypeFont Extract(TrueTypeFont font, FontExtractOption opt)
    {
        var outputs = CreateCharToGIDMetrics(font, [.. opt.ExtractChars.Order()]);
        var char_gids = outputs[0..opt.ExtractChars.Length].Select(x => (x.Char, x.NewGID)).ToArray();
        var gid_glyph = outputs.ToDictionary(x => x.NewGID, x => (Glyph: font.Glyphs[(int)x.OldGID], x.HorizontalMetrics));
        var num_of_glyph = (int)gid_glyph.Keys.Max();
        var (colr, cpal) = font.Color is null || font.ColorPalette is null ? (null, null) : ExtractColorTable(font.Color, font.ColorPalette, outputs.ToDictionary(x => x.OldGID, x => x.NewGID));

        var glyphs = Lists.RangeTo(1, num_of_glyph)
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
            Name = ExtractNameTable(font.Name, opt),
            FontHeader = font.FontHeader,
            MaximumProfile = CopyMaximumProfileTable(font.MaximumProfile, (ushort)(num_of_glyph + 1)),
            PostScript = font.PostScript,
            OS2 = font.OS2,
            HorizontalHeader = CopyHorizontalHeaderTable(font.HorizontalHeader, (ushort)(num_of_glyph + 1)),
            HorizontalMetrics = CreateHorizontalMetricsTable(num_of_glyph, font.HorizontalMetrics.Metrics[0], x => gid_glyph.TryGetValue(x, out var v) ? v.HorizontalMetrics : null),
            CMap = CreateCMapTable(opt, char_gids),
            CharToGID = CreateCharToGID(char_gids),
            GIDToOutline = gid => glyphs[gid].ToOutline(glyphs),
            Glyphs = glyphs,
            ColorBitmapData = null,
            ColorBitmapLocation = null,
            Color = colr,
            ColorPalette = cpal,
            StandardBitmapGraphics = null,
            ScalableVectorGraphics = null,
            DisposeAction = null,
        };
    }

    public static PostScriptFont Extract(PostScriptFont font, FontExtractOption opt)
    {
        var validchars = opt.ExtractChars.Where(x => font.CharToGID(x) != 0).Order().ToArray();
        var outputs = CreateCharToGIDMetrics(font, validchars).Append((Char: 0, NewGID: 0u, OldGID: 0u, HorizontalMetrics: font.HorizontalMetrics.Metrics[0])).ToArray();
        var char_gids = outputs[0..validchars.Length].Select(x => (x.Char, x.NewGID)).ToArray();
        var gid_glyph = outputs.ToDictionary(x => x.NewGID, x => (
                Glyph: font.CompactFontFormat.TopDict.CharStrings[x.OldGID],
                x.HorizontalMetrics,
                Outline: font.GIDToOutline(x.OldGID))
            );
        var num_of_glyph = (int)gid_glyph.Keys.Max();
        var (colr, cpal) = font.Color is null || font.ColorPalette is null ? (null, null) : ExtractColorTable(font.Color, font.ColorPalette, outputs.ToDictionary(x => x.OldGID, x => x.NewGID));

        var dict = font.CompactFontFormat.TopDict.Dict.ToDictionary();
        _ = dict.TryAdd(1230, [/* SID.StandardStrings[388] = "Regular" */388]);
        var top_dict = new DictData
        {
            Strings = [],
            Dict = dict,
            CharStrings = [.. Lists.RangeTo(0, num_of_glyph).Select(x => OutlineToCharStrings(gid_glyph[(uint)x].Outline, 0))],
            Charsets = new()
            {
                Format = 0,
                Glyph = [.. Lists.RangeTo(1, num_of_glyph).Select(x => (ushort)x)],
            },
            PrivateDict = font.CompactFontFormat.TopDict.PrivateDict,
            FontDictArray = [new DictData { Dict = [] }],
            FontDictSelect = [.. Lists.Repeat<byte>(0).Take(num_of_glyph + 1)],
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
            GlobalSubroutines = [],
        };

        return new()
        {
            PostScriptName = font.PostScriptName,
            Path = font.Path,
            Position = font.Position,
            TableRecords = font.TableRecords,
            Offset = font.Offset,
            Name = ExtractNameTable(font.Name, opt),
            FontHeader = font.FontHeader,
            MaximumProfile = CopyMaximumProfileTable(font.MaximumProfile, (ushort)(num_of_glyph + 1)),
            PostScript = font.PostScript,
            OS2 = font.OS2,
            HorizontalHeader = CopyHorizontalHeaderTable(font.HorizontalHeader, (ushort)(num_of_glyph + 1)),
            HorizontalMetrics = CreateHorizontalMetricsTable(num_of_glyph, font.HorizontalMetrics.Metrics[0], x => gid_glyph.TryGetValue(x, out var v) ? v.HorizontalMetrics : null),
            CMap = CreateCMapTable(opt, char_gids),
            CharToGID = CreateCharToGID(char_gids),
            GIDToOutline = cff.ToOutline,
            CompactFontFormat = cff,
            ColorBitmapData = null,
            ColorBitmapLocation = null,
            Color = colr,
            ColorPalette = cpal,
            StandardBitmapGraphics = null,
            ScalableVectorGraphics = null,
        };
    }

    public static NoOutlineFont Extract(NoOutlineFont font, FontExtractOption opt)
    {
        var outputs = CreateCharToGIDMetrics(font, [.. opt.ExtractChars.Order()]);
        var char_gids = outputs[0..opt.ExtractChars.Length].Select(x => (x.Char, x.NewGID)).ToArray();
        var gid_hmtx = outputs.ToDictionary(x => x.NewGID, x => x.HorizontalMetrics);
        var num_of_glyph = (int)gid_hmtx.Keys.Max();
        var (colr, cpal) = font.Color is null || font.ColorPalette is null ? (null, null) : ExtractColorTable(font.Color, font.ColorPalette, outputs.ToDictionary(x => x.OldGID, x => x.NewGID));

        return new()
        {
            PostScriptName = font.PostScriptName,
            Path = font.Path,
            Position = font.Position,
            TableRecords = font.TableRecords,
            Offset = font.Offset,
            Name = ExtractNameTable(font.Name, opt),
            FontHeader = font.FontHeader,
            MaximumProfile = CopyMaximumProfileTable(font.MaximumProfile, (ushort)(num_of_glyph + 1)),
            PostScript = font.PostScript,
            OS2 = font.OS2,
            HorizontalHeader = CopyHorizontalHeaderTable(font.HorizontalHeader, (ushort)(num_of_glyph + 1)),
            HorizontalMetrics = CreateHorizontalMetricsTable(num_of_glyph, font.HorizontalMetrics.Metrics[0], x => gid_hmtx.GetOrDefault(x)),
            CMap = CreateCMapTable(opt, char_gids),
            CharToGID = CreateCharToGID(char_gids),
            GIDToOutline = _ => [],
            ColorBitmapData = null,
            ColorBitmapLocation = null,
            Color = colr,
            ColorPalette = cpal,
            StandardBitmapGraphics = null,
            ScalableVectorGraphics = null,
        };
    }

    public static (int Char, uint NewGID, uint OldGID, HorizontalMetrics HorizontalMetrics)[] CreateCharToGIDMetrics(IOpenTypeFont font, int[] chars) => [.. chars
        .Select(font.CharToGID)
        .To(xs => font.Color is { } ? GetGIDWithColorGlyph([.. xs], font.Color) : xs)
        .Select((x, i) => (
            Char: i < chars.Length ? chars[i] : 0,
            NewGID: (uint)(i + 1),
            OldGID: x,
            HorizontalMetrics: font.HorizontalMetrics.Metrics[Math.Min(x, font.HorizontalHeader.NumberOfHMetrics - 1)]
        ))];

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

    public static HorizontalMetricsTable CreateHorizontalMetricsTable(int num_of_glyph, HorizontalMetrics notdef, Func<uint, HorizontalMetrics?> f) => new()
    {
        Metrics = [notdef, .. Lists.RangeTo(1, num_of_glyph).Select(x => f((uint)x) ?? notdef)],
        LeftSideBearing = [],
    };

    public static Func<int, uint> CreateCharToGID((int Char, uint GID)[] char_gids)
    {
        var dict = char_gids.ToDictionary(x => x.Char, x => x.GID);
        return c => dict.TryGetValue(c, out var gid) ? gid : 0;
    }

    public static Func<uint, int?> CreateGIDToChar((int Char, uint GID)[] char_gids)
    {
        var dict = char_gids.ToDictionary(x => x.GID, x => x.Char);
        return gid => dict.TryGetValue(gid, out var c) ? c : null;
    }

    public static CMapTable CreateCMapTable(FontExtractOption opt, (int Char, uint GID)[] char_gids)
    {
        var records = opt.OutputCMap
            .Select(x => (x.PlatformID, x.EncodingID, CMapFormat: CreateCMapFormat(x.CMapFormat, char_gids)))
            .Where(x => x.CMapFormat is not null)
            .ToArray();

        return new()
        {
            Version = 0,
            NumberOfTables = (ushort)records.Length,
            EncodingRecords = records.ToDictionary(
                x => new EncodingRecord { PlatformID = (ushort)x.PlatformID, EncodingID = (ushort)x.EncodingID, Offset = 0 },
                x => x.CMapFormat!
            ),
        };
    }

    public static ICMapFormat? CreateCMapFormat(CMapFormats format, (int Char, uint GID)[] char_gids) => format switch
    {
        CMapFormats.Format0 => char_gids.Where(x => x.Char <= 0xFF).To(x => x.IsEmpty() ? null : (ICMapFormat)CMapFormat0.CreateFormat([.. x.Select(x => (char.ConvertFromUtf32(x.Char).First(), (byte)x.GID))])),
        CMapFormats.Format4 => char_gids.Where(x => x.Char <= 0xFFFF).To(x => x.IsEmpty() ? null : (ICMapFormat)CMapFormat4.CreateFormat([.. x.Select(x => (char.ConvertFromUtf32(x.Char).First(), (ushort)x.GID))])),
        CMapFormats.Format12 => CMapFormat12.CreateFormat(char_gids),
        CMapFormats.Format13 => CMapFormat13.CreateFormat(char_gids),
        CMapFormats.Format14 => CMapFormat14.CreateFormat(char_gids),
        _ => null,
    };

    public static IEnumerable<uint> GetGIDWithColorGlyph(uint[] gids, ColorTable colr)
    {
        foreach (var x in gids) yield return x;

        var callback = new GIDTraverse() { GIDPreOrderCallback = gids.ToHashSet().Add };
        foreach (var gid in gids)
        {
            foreach (var x in callback.EnumGID(gid, colr)) yield return x;
        }
    }

    public static (ColorTable, ColorPaletteTable) ExtractColorTable(ColorTable colr, ColorPaletteTable cpal, Dictionary<uint, uint> mapper)
    {
        var colorPalettes = new Dictionary<ushort, ushort>();

        ushort addPalette(ushort palette) =>
            palette == 0xFFFF ? (ushort)0xFFFF :
            colorPalettes.TryGetValue(palette, out var x) ? x :
            colorPalettes[palette] = (ushort)colorPalettes.Count;

        ColorLine addColorLine(ColorLine colorLine) => new()
        {
            Extend = colorLine.Extend,
            NumberOfStops = 0,
            ColorStops = [.. colorLine.ColorStops.Select(x => new ColorStop { StopOffset = x.StopOffset, PaletteIndex = addPalette(x.PaletteIndex), Alpha = x.Alpha })],
        };

        VarColorLine addVarColorLine(VarColorLine colorLine) => new()
        {
            Extend = colorLine.Extend,
            NumberOfStops = 0,
            ColorStops = [.. colorLine.ColorStops.Select(x => new VarColorStop() { StopOffset = x.StopOffset, PaletteIndex = addPalette(x.PaletteIndex), Alpha = x.Alpha, VarIndexBase = x.VarIndexBase })],
        };

        var layerList = new List<IPaintFormat>();
        var paints = new Dictionary<IPaintFormat, IPaintFormat>();
        _ = new GIDTraverse()
        {
            GIDPreOrderCallback = new HashSet<uint>().Add,
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
                            PaletteIndex = addPalette(p.PaletteIndex),
                            Alpha = p.Alpha,
                        });
                        break;

                    case PaintVarSolid p:
                        paints.Add(p, new PaintVarSolid
                        {
                            Format = p.Format,
                            PaletteIndex = addPalette(p.PaletteIndex),
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
                            ColorLine = addColorLine(p.ColorLine),
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
                            ColorLine = addVarColorLine(p.ColorLine),
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
                            ColorLine = addColorLine(p.ColorLine),
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
                            ColorLine = addVarColorLine(p.ColorLine),
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
                            ColorLine = addColorLine(p.ColorLine),
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
                            ColorLine = addVarColorLine(p.ColorLine),
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
            .Where(x => mapper.ContainsKey(x.GlyphID))
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

                return newGlyphs.Length == 0 ? null :
                    new BaseGlyphListRecord
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

            return clipList.Count == 0 ? null :
                new ClipListRecord
                {
                    Format = record.Format,
                    NumberClips = 0,
                    Clips = [.. clipList.Select(x => (x.GlyphID, x.GlyphID, 0))],
                    ClipBoxFormats = [.. clipList.Select(x => x.ClipBoxFormat)],
                };
        });

        return (
            new()
            {
                Version = colr.Version,
                NumberBaseGlyphRecords = 0,
                BaseGlyphRecordsOffset = 0,
                LayerRecordsOffset = 0,
                NumberLayerRecords = 0,
                BaseGlyphRecords = baseGlyphRecords,
                LayerRecords = [.. layerRecords],
                BaseGlyphListRecord = baseGlyphListRecord,
                LayerListRecord = layerList.Count == 0 ? null : new LayerListRecord { NumberOfLayers = 0, PaintOffsets = [], Paints = [.. layerList.Select(x => paints[x])] },
                ClipListRecord = clipListRecord,
                DeltaSetIndexMapRecord = colr.DeltaSetIndexMapRecord,
                ItemVariationStoreRecord = colr.ItemVariationStoreRecord,
            },
            new()
            {
                Version = 0,
                NumberOfPaletteEntries = 0,
                NumberOfPalettes = 0,
                NumberOfColorRecords = 0,
                ColorRecordsArrayOffset = 0,
                ColorRecordIndices = cpal.ColorRecordIndices,
                PaletteTypesArrayOffset = 0,
                PaletteLabelsArrayOffset = 0,
                PaletteEntryLabelsArrayOffset = 0,
                ColorRecords = [.. colorPalettes.OrderBy(kv => kv.Value).Select(kv => cpal.ColorRecords[kv.Key])],
                PaletteTypes = [],
                PaletteLabels = [],
                PaletteEntryLabels = [],
            });
    }

    public static byte[] OutlineToCharStrings(IOutline[] outlines, int nominalWidthX)
    {
        var char_strings = new List<byte>();
        char_strings.AddRange(Subroutine.NumberToBytes(GetCanvasWidth(outlines).Width - nominalWidthX));

        var current = new Vector2(0, 0);
        foreach (var outline in outlines)
        {
            switch (outline)
            {
                case Surface surface when surface.Edges.Length > 0:
                    {
                        var start = surface.Edges.First().Start;
                        char_strings.AddRange(Subroutine.NumberToBytes(start.X - current.X));
                        char_strings.AddRange(Subroutine.NumberToBytes(start.Y - current.Y));
                        char_strings.Add((byte)CharstringCommandCodes.Rmoveto);
                        current = start;

                        foreach (var edge in surface.Edges)
                        {
                            switch (edge)
                            {
                                case Line line:
                                    char_strings.AddRange(Subroutine.NumberToBytes(line.End.X - current.X));
                                    char_strings.AddRange(Subroutine.NumberToBytes(line.End.Y - current.Y));
                                    char_strings.Add((byte)CharstringCommandCodes.Rlineto);
                                    current = line.End;
                                    break;

                                case BezierCurve bezier when bezier.ControlPoint.Length == 2:
                                    {
                                        var cp1 = bezier.ControlPoint[0];
                                        var cp2 = bezier.ControlPoint[1];
                                        char_strings.AddRange(Subroutine.NumberToBytes(cp1.X - current.X));
                                        char_strings.AddRange(Subroutine.NumberToBytes(cp1.Y - current.Y));
                                        char_strings.AddRange(Subroutine.NumberToBytes(cp2.X - cp1.X));
                                        char_strings.AddRange(Subroutine.NumberToBytes(cp2.Y - cp1.Y));
                                        char_strings.AddRange(Subroutine.NumberToBytes(bezier.End.X - cp2.X));
                                        char_strings.AddRange(Subroutine.NumberToBytes(bezier.End.Y - cp2.Y));
                                        char_strings.Add((byte)CharstringCommandCodes.Rrcurveto);
                                        current = bezier.End;
                                        break;
                                    }
                            }
                        }
                        break;
                    }
            }
        }
        char_strings.Add((byte)CharstringCommandCodes.Endchar);
        return [.. char_strings];
    }

    public static (float Width, float Left) GetCanvasWidth(IOutline[] outlines)
    {
        foreach (var outline in outlines)
        {
            switch (outline)
            {
                case Surface surface:
                    return (surface.XMax - surface.XMin, surface.XMin);
            }
        }
        return (0, 0);
    }
}
