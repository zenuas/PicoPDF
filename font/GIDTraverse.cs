using Mina.Extension;
using OpenType.Tables;
using OpenType.Tables.Colr;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenType;

public class GIDTraverse
{
    public required Func<uint, bool> GIDPreOrderCallback { get; init; }
    public Action<IPaintFormat>? PaintPostOrderCallback { get; init; }

    public uint[] Traverse(IEnumerable<uint> gids, ColorTable colr) => [.. gids.Select(x => EnumGID(x, colr)).Flatten()];

    public IEnumerable<uint> EnumGID(uint gid, ColorTable colr)
    {
        foreach (var record in colr.BaseGlyphRecords.Where(x => x.GlyphID == gid))
        {
            foreach (var layer in colr.LayerRecords[record.FirstLayerIndex..(record.FirstLayerIndex + record.NumberOfLayers)])
            {
                if (GIDPreOrderCallback(layer.GlyphID))
                {
                    yield return layer.GlyphID;
                    foreach (var x in EnumGID(layer.GlyphID, colr)) yield return x;
                }
            }
        }

        for (var i = 0; i < colr.BaseGlyphListRecord?.BaseGlyphPaintRecord.Length; i++)
        {
            if (colr.BaseGlyphListRecord.BaseGlyphPaintRecord[i].GlyphID == gid)
            {
                foreach (var x in EnumPaint(colr.BaseGlyphListRecord.Paints[i], colr)) yield return x;
            }
        }
    }

    public IEnumerable<uint> EnumPaint(IPaintFormat paint, ColorTable colr)
    {
        if (paint is PaintColrLayers paintColrLayers)
        {
            foreach (var layer in colr.LayerListRecord!.Paints[(int)paintColrLayers.FirstLayerIndex..((int)paintColrLayers.FirstLayerIndex + paintColrLayers.NumberOfLayers)])
            {
                foreach (var x in EnumPaint(layer, colr)) yield return x;
            }
        }
        else if (paint is PaintComposite paintComposite)
        {
            foreach (var x in EnumPaint(paintComposite.SourcePaint, colr)) yield return x;
            foreach (var x in EnumPaint(paintComposite.BackdropPaint, colr)) yield return x;
        }
        else
        {
            if (paint is IHaveGlyph glyph)
            {
                if (GIDPreOrderCallback(glyph.GlyphID)) yield return glyph.GlyphID;
            }
            if (paint is IHavePaint subpaint)
            {
                foreach (var x in EnumPaint(subpaint.Paint, colr)) yield return x;
            }
        }
        if (PaintPostOrderCallback is { }) PaintPostOrderCallback(paint);
    }
}
