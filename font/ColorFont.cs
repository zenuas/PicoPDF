using OpenType.Outline;
using OpenType.Tables;
using OpenType.Tables.Colr;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

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
                surfaces.AddRange(ToOutline(font, [], colr.BaseGlyphListRecord.Paints[i], colr, cpal));
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

    public static IOutline[] ToOutline(IOpenTypeFont font, IOutline[] surfaces, IPaintFormat paint, ColorTable colr, ColorPaletteTable cpal)
    {
        switch (paint)
        {
            case PaintColrLayers p:
                {
                    var layers = new List<Layer>();
                    foreach (var layer in colr.LayerListRecord!.Paints[(int)p.FirstLayerIndex..((int)p.FirstLayerIndex + p.NumberOfLayers)])
                    {
                        layers.Add(new() { Surfaces = ToOutline(font, surfaces, layer, colr, cpal) });
                    }
                    return [.. layers];
                }

            case PaintSolid p:
                return [.. surfaces.OfType<Surface>().Select(x => new Surface { Edges = x.Edges, Color = GetColor(cpal, p.PaletteIndex, (float)p.Alpha) })];

            case PaintVarSolid p:
                break;

            case PaintLinearGradient p:
                break;

            case PaintVarLinearGradient p:
                break;

            case PaintRadialGradient p:
                break;

            case PaintVarRadialGradient p:
                break;

            case PaintSweepGradient p:
                break;

            case PaintVarSweepGradient p:
                break;

            case PaintGlyph p:
                return ToOutline(font, font.GIDToOutline(p.GlyphID), p.Paint, colr, cpal);

            case PaintColrGlyph p:
                break;

            case PaintTransform p:
                break;

            case PaintVarTransform p:
                break;

            case PaintTranslate p:
                break;

            case PaintVarTranslate p:
                break;

            case PaintScale p:
                break;

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
}
