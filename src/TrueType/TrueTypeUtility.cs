using Mina.Binder;
using System;
using System.Linq;

namespace PicoPDF.TrueType;

public static class TrueTypeUtility
{
    public static readonly ComparerBinder<(int Start, int End)> CMap4RangeComparer = new() { Compare = (a, b) => (a.End < b.End ? -1 : a.Start > b.Start ? 1 : 0) };

    public static int CharToGIDCached(this TrueTypeFontInfo ttf, char c) => ttf.CMap4Cache.TryGetValue(c, out var gid) ? gid : (ttf.CMap4Cache[c] = CharToGID(ttf, c));

    public static int CharToGID(this TrueTypeFontInfo ttf, char c)
    {
        var seg = ttf.CMap4Range.BinarySearch((c, c), CMap4RangeComparer);
        var start = ttf.CMap4.StartCode[seg];
        if (c < start) return 0;

        var idrange = ttf.CMap4.IdRangeOffsets[seg];
        if (idrange == 0) return (c + ttf.CMap4.IdDelta[seg]) & 0xFFFF;

        var gindex = (idrange / 2) + c - start - (ttf.CMap4.SegCountX2 / 2) + seg;
        return (ttf.CMap4.GlyphIdArray[gindex] + ttf.CMap4.IdDelta[seg]) & 0xFFFF;
    }

    public static int MeasureString(this TrueTypeFontInfo ttf, string s) => s.Select(x => MeasureChar(ttf, x)).Sum();

    public static int MeasureChar(this TrueTypeFontInfo ttf, char c) => MeasureGID(ttf, CharToGIDCached(ttf, c));

    public static int MeasureGID(this TrueTypeFontInfo ttf, int gid)
    {
        var width = ttf.HorizontalMetrics.Metrics[Math.Min(gid, ttf.HorizontalHeader.NumberOfHMetrics - 1)].AdvanceWidth;
        return ttf.FontHeader.UnitsPerEm == 1000 ? width : width * 1000 / ttf.FontHeader.UnitsPerEm;
    }
}
