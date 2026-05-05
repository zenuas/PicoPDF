using OpenType.Outline;
using OpenType.Tables;
using OpenType.Tables.Colr;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;

namespace OpenType;

public static class ColorFont
{
    public static IOutline[]? ToOutline(IOpenTypeFont font, uint gid, ColorTable colr, ColorPaletteTable cpal)
    {
        var surfaces = new List<IOutline>();

        foreach (var record in colr.BaseGlyphRecords.Where(x => x.GlyphID == gid))
        {
            foreach (var layer in colr.LayerRecords[record.FirstLayerIndex..(record.FirstLayerIndex + record.NumberOfLayers)])
            {
                if (ToOutline(font, layer.GlyphID, colr, cpal) is { } outlines) surfaces.AddRange(outlines);
            }
        }

        for (var i = 0; i < colr.BaseGlyphListRecord?.BaseGlyphPaintRecord.Length; i++)
        {
            if (colr.BaseGlyphListRecord.BaseGlyphPaintRecord[i].GlyphID == gid)
            {
                surfaces.AddRange(ToOutline(font, [], colr.BaseGlyphListRecord.Paints[i], colr, cpal, Matrix3x2.Identity));
            }
        }
        return surfaces.Count == 0 ? null : [.. surfaces];
    }

    public static Color GetColor(ColorPaletteTable cpal, ushort paletteIndex, float alpha)
    {
        // A palette entry index value of 0xFFFF is a special case indicating that the text foreground color (defined by the application) should be used,
        // and must not be treated as an actual index into the CPAL ColorRecord array.
        if (paletteIndex == 0xFFFF) return Color.Transparent;

        var col = cpal.ColorRecords[paletteIndex];
        return Color.FromArgb((int)(col.Alpha * alpha), col.Red, col.Green, col.Blue);
    }

    public static IEdge[] ToEdges(IEdge[] edges, Matrix3x2 transform) => [.. edges.Select(x => ToEdge(x, transform))];

    public static IEdge ToEdge(IEdge edge, Matrix3x2 transform) => edge switch
    {
        Line line => new Line
        {
            Start = Vector2.Transform(line.Start, transform),
            End = Vector2.Transform(line.End, transform),
        },
        BezierCurve bezier => new BezierCurve
        {
            Start = Vector2.Transform(bezier.Start, transform),
            ControlPoint = [.. bezier.ControlPoint.Select(x => Vector2.Transform(x, transform))],
            End = Vector2.Transform(bezier.End, transform),
            ComplementPoint = bezier.ComplementPoint,
        },
        _ => throw new(),
    };

    public static IOutline[] ToOutline(IOpenTypeFont font, IOutline[] surfaces, IPaintFormat paint, ColorTable colr, ColorPaletteTable cpal, Matrix3x2 transform)
    {
        switch (paint)
        {
            case PaintColrLayers p:
                {
                    var layers = new List<Layer>();
                    foreach (var layer in colr.LayerListRecord!.Paints[(int)p.FirstLayerIndex..((int)p.FirstLayerIndex + p.NumberOfLayers)])
                    {
                        layers.Add(new() { Surfaces = ToOutline(font, surfaces, layer, colr, cpal, transform) });
                    }
                    return [.. layers];
                }

            case PaintSolid p:
                return [new SolidColorLayer { Color = GetColor(cpal, p.PaletteIndex, p.Alpha.FloatValue) }];

            case PaintVarSolid p:
                break;

            case PaintLinearGradient p:
                {
                    return [new LinearGradientLayer
                    {
                        XY1 = Vector2.Transform(new Vector2(p.X0, p.Y0), transform),
                        XY2 = Vector2.Transform(new Vector2(p.X1, p.Y1), transform),
                        StopColors = [.. p.ColorLine.ColorStops.Select(x => (x.StopOffset.FloatValue * 100F, GetColor(cpal, x.PaletteIndex, x.Alpha)))],
                        SpreadMethod =ExtendToSpreadMethod(p.ColorLine.Extend),
                    }];
                }

            case PaintVarLinearGradient p:
                break;

            case PaintRadialGradient p:
                {
                    return [new RadialGradientLayer
                    {
                        Cxy = Vector2.Transform(new Vector2(p.X0, p.Y0), transform),
                        Fxy = Vector2.Transform(new Vector2(p.X1, p.Y1), transform),
                        Fr = p.Radius0,
                        R = p.Radius1,
                        StopColors = [.. p.ColorLine.ColorStops.Select(x => (x.StopOffset.FloatValue * 100F, GetColor(cpal, x.PaletteIndex, x.Alpha)))],
                        SpreadMethod =ExtendToSpreadMethod(p.ColorLine.Extend),
                    }];
                }

            case PaintVarRadialGradient p:
                break;

            case PaintSweepGradient p:
                break;

            case PaintVarSweepGradient p:
                break;

            case PaintGlyph p:
                {
                    var outline = font.GIDToOutline(p.GlyphID, false);
                    var info = ToOutline(font, [], p.Paint, colr, cpal, transform);
                    var color_layer = info.OfType<IHaveColorLayer>().Where(x => x.ColorLayer is { }).FirstOrDefault()?.ColorLayer;

                    return [.. outline.Select(x => x is not Surface surface ? x : new Surface { Edges = ToEdges(surface.Edges, transform), ColorLayer = color_layer })];
                }

            case PaintColrGlyph p:
                break;

            case PaintTransform p:
                return ToOutline(font, surfaces, p.Paint, colr, cpal, transform * p.Transform.ToMatrix3x2());

            case PaintVarTransform p:
                break;

            case PaintTranslate p:
                break;

            case PaintVarTranslate p:
                break;

            case PaintScale p:
                return ToOutline(font, surfaces, p.Paint, colr, cpal, transform * Matrix3x2.CreateScale(p.ScaleX, p.ScaleY));

            case PaintVarScale p:
                break;

            case PaintScaleAroundCenter p:
                break;

            case PaintVarScaleAroundCenter p:
                break;

            case PaintScaleUniform p:
                break;

            case PaintVarScaleUniform p:
                break;

            case PaintScaleUniformAroundCenter p:
                break;

            case PaintVarScaleUniformAroundCenter p:
                break;

            case PaintRotate p:
                break;

            case PaintVarRotate p:
                break;

            case PaintRotateAroundCenter p:
                break;

            case PaintVarRotateAroundCenter p:
                break;

            case PaintSkew p:
                break;

            case PaintVarSkew p:
                break;

            case PaintSkewAroundCenter p:
                break;

            case PaintVarSkewAroundCenter p:
                break;

            case PaintComposite p:
                break;
        }
        return surfaces;
    }

    public static SpreadMethods ExtendToSpreadMethod(Extend extend) => extend switch
    {
        Extend.EXTEND_PAD => SpreadMethods.Pad,
        Extend.EXTEND_REPEAT => SpreadMethods.Repeat,
        Extend.EXTEND_REFLECT => SpreadMethods.Reflect,
        _ => throw new(),
    };
}
