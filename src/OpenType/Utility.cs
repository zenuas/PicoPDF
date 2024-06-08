using Mina.Binder;
using System;
using System.Linq;

namespace PicoPDF.OpenType;

public static class Utility
{
    public static readonly ComparerBinder<(int Start, int End)> CMap4RangeComparer = new() { Compare = (a, b) => (a.End < b.End ? -1 : a.Start > b.Start ? 1 : 0) };

    public static bool ContainTrueType(this OffsetTable self) => self.Version == 0x00010000;

    public static bool ContainCFF(this OffsetTable self) => self.Version == 0x4F54544F;

    public static int CharToGIDCached(this FontInfo font, char c) => font.CMap4Cache.TryGetValue(c, out var gid) ? gid : (font.CMap4Cache[c] = CharToGID(font, c));

    public static int CharToGID(this FontInfo font, char c)
    {
        var seg = font.CMap4Range.BinarySearch((c, c), CMap4RangeComparer);
        var start = font.CMap4.StartCode[seg];
        if (c < start) return 0;

        var idrange = font.CMap4.IdRangeOffsets[seg];
        if (idrange == 0) return (c + font.CMap4.IdDelta[seg]) & 0xFFFF;

        var gindex = (idrange / 2) + c - start - (font.CMap4.SegCountX2 / 2) + seg;
        return (font.CMap4.GlyphIdArray[gindex] + font.CMap4.IdDelta[seg]) & 0xFFFF;
    }

    public static int MeasureString(this FontInfo font, string s) => s.Select(x => MeasureChar(font, x)).Sum();

    public static int MeasureChar(this FontInfo font, char c) => MeasureGID(font, CharToGIDCached(font, c));

    public static int MeasureGID(this FontInfo font, int gid)
    {
        var width = font.HorizontalMetrics.Metrics[Math.Min(gid, font.HorizontalHeader.NumberOfHMetrics - 1)].AdvanceWidth;
        return font.FontHeader.UnitsPerEm == 1000 ? width : width * 1000 / font.FontHeader.UnitsPerEm;
    }
}
