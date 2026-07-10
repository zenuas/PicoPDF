using Mina.Extension;
using OpenType.Tables;
using OpenType.Tables.CMap;
using OpenType.Tables.Colr;
using Svg.Outline;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace OpenType;

public static class FontExtract
{
    public static GenericFont Extract(IOpenTypeFont font, FontExtractOption opt)
    {
        var outputs = CreateCharToGIDMetrics(font, [.. opt.ExtractChars.Order()], opt.IsColorSupport);
        var char_gids = outputs[0..opt.ExtractChars.Length].Select(x => (x.Char, x.NewGID)).ToArray();
        var gid_glyph = outputs.ToDictionary(x => x.NewGID, x =>
        {
            var outline = font.GIDToOutline(x.OldGID, false);
            var points = EnumAllPoints(outline).ToArray();
            return (
                Outline: outline,
                x.HorizontalMetrics,
                NoOutline: points.Length == 0,
                Notdef: false,
                Ascender: points.Length > 0 ? points.Select(x => x.Y).Max() : 0f,
                Descender: points.Length > 0 ? points.Select(x => x.Y).Min() : 0f,
                XMin: points.Length > 0 ? points.Select(x => x.X).Min() : 0f,
                XMax: points.Length > 0 ? points.Select(x => x.X).Max() : 0f,
                x.Char
            );
        });
        gid_glyph.Add(0, (Outline: font.GIDToOutline(0, false), font.HorizontalMetrics.Metrics[0], true, true, 0f, 0f, 0f, 0f, 0));
        var num_of_glyph_include_notdef = (int)gid_glyph.Keys.Max() + 1;
        var (colr, cpal) = !opt.IsColorSupport || font.Color is null || font.ColorPalette is null ? (null, null) : ExtractColorTable(font.Color, font.ColorPalette, outputs.ToDictionary(x => x.OldGID, x => x.NewGID));

        var hmetrics = Lists.RangeTo(0, num_of_glyph_include_notdef - 1)
            .Select(x => gid_glyph.TryGetValue((uint)x, out var v) ? v.HorizontalMetrics : font.HorizontalMetrics.Metrics[0])
            .ToArray();

        var head = font.FontHeader with
        {
            XMin = (short)gid_glyph.Values.Select(x => x.XMin).Min(),
            YMin = (short)gid_glyph.Values.Select(x => x.Descender).Min(),
            XMax = (short)gid_glyph.Values.Select(x => x.XMax).Max(),
            YMax = (short)gid_glyph.Values.Select(x => x.Ascender).Max(),
        };

        var hhea = font.HorizontalHeader with
        {
            Ascender = head.YMax,
            Descender = head.YMin,
            AdvanceWidthMax = hmetrics.Select(x => x.AdvanceWidth.Value).Max(),
            MinLeftSideBearing = gid_glyph.Values.Where(x => !x.NoOutline).Select(x => x.HorizontalMetrics.LeftSideBearing.Value).Min(),
            MinRightSideBearing = (int)gid_glyph.Values.Where(x => !x.NoOutline).Select(x => x.HorizontalMetrics.AdvanceWidth.Value - (x.HorizontalMetrics.LeftSideBearing.Value + x.XMax - x.XMin)).Min(),
            XMaxExtent = (int)gid_glyph.Values.Select(x => x.HorizontalMetrics.LeftSideBearing.Value + (x.XMax - x.XMin)).Max(),
            NumberOfHMetrics = (ushort)num_of_glyph_include_notdef,
        };

        var os2 = font.OS2 is null ? null : font.OS2 with
        {
            XAvgCharWidth = (short)gid_glyph.Values.Where(x => !x.NoOutline).Select(x => x.XMax - x.XMin).Average(),
            UsFirstCharIndex = (ushort)Math.Min(gid_glyph.Values.Where(x => !x.Notdef).Select(x => x.Char).Min(), 0xFFFF),
            UsLastCharIndex = (ushort)Math.Min(gid_glyph.Values.Where(x => !x.Notdef).Select(x => x.Char).Max(), 0xFFFF),
            STypoAscender = head.YMax,
            STypoDescender = head.YMin,
            UsWinAscent = head.YMax,
            UsWinDescent = head.YMin,
        };

        GenericFont newfont = null!;
        return newfont = new()
        {
            PostScriptName = font.PostScriptName,
            Path = font.Path,
            Position = font.Position,
            TableRecords = font.TableRecords,
            Offset = font.Offset,
            Name = ExtractNameTable(font.Name, opt),
            FontHeader = head,
            MaximumProfile = font.MaximumProfile with { NumberOfGlyphs = (ushort)num_of_glyph_include_notdef },
            PostScript = font.PostScript,
            OS2 = os2,
            HorizontalHeader = hhea,
            HorizontalMetrics = new() { Metrics = hmetrics, LeftSideBearing = [] },
            CMap = CreateCMapTable(opt, char_gids),
            CharToGID = CreateCharToGID(char_gids),
            GIDToOutline = (gid, iscolor) => (iscolor && colr is { } && cpal is { } ? ColorFont.ToOutline(newfont, gid, colr, cpal) : null) ?? gid_glyph[gid].Outline,
            ColorBitmapData = font.ColorBitmapData,
            ColorBitmapLocation = font.ColorBitmapLocation,
            Color = colr,
            ColorPalette = cpal,
            StandardBitmapGraphics = font.StandardBitmapGraphics,
            ScalableVectorGraphics = font.ScalableVectorGraphics,
        };
    }

    public static (int Char, uint NewGID, uint OldGID, HorizontalMetrics HorizontalMetrics)[] CreateCharToGIDMetrics(IOpenTypeFont font, int[] chars, bool iscolor) => [.. chars
        .Select(font.CharToGID)
        .To(xs => iscolor && font.Color is { } ? GetGIDWithColorGlyph([.. xs], font.Color) : xs)
        .Select((x, i) => (
            Char: i < chars.Length ? chars[i] : 0,
            NewGID: (uint)(i + 1),
            OldGID: x,
            HorizontalMetrics: font.HorizontalMetrics.Metrics[Math.Min(x, font.HorizontalHeader.NumberOfHMetrics - 1)]
        ))];

    public static NameTable ExtractNameTable(NameTable name, FontExtractOption opt)
    {
        var names = name.NameRecords.Where(name_record => opt.OutputNames.Contains(x =>
                (x.PlatformID is null || (x.PlatformID == name_record.NameRecord.PlatformID)) &&
                (x.EncodingID is null || (x.EncodingID == name_record.NameRecord.EncodingID)) &&
                (x.LanguageID is null || (x.LanguageID == name_record.NameRecord.LanguageID)) &&
                (x.NameID is null || (x.NameID == name_record.NameRecord.NameID))
            )).ToArray();

        return new() { Format = 0, Count = (ushort)names.Length, StringOffset = 0, NameRecords = names };
    }

    public static Func<int, uint> CreateCharToGID((int Char, uint GID)[] char_gids)
    {
        var dict = char_gids.ToDictionary(x => x.Char, x => x.GID);
        return c => dict.TryGetValue(c, out var gid) ? gid : 0;
    }
    public static IEnumerable<Vector2> EnumAllPoints(IOutline[] outlines)
    {
        foreach (var edge in outlines.OfType<Surface>().Select(x => x.Edges).Flatten())
        {
            switch (edge)
            {
                case Line line:
                    yield return line.Start;
                    yield return line.End;
                    break;

                case BezierCurve bezier:
                    yield return bezier.Start;
                    foreach (var cp in bezier.ControlPoint) yield return cp;
                    yield return bezier.End;
                    break;
            }
        }
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
}