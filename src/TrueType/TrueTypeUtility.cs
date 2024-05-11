using Mina.Extension;
using System;
using System.Linq;

namespace PicoPDF.TrueType;

public static class TrueTypeUtility
{
    public static int CharToGID(this TrueTypeFontInfo ttf, char c) => ttf.CMap4.CharToGID(c);

    public static int CharToGID(this CMapFormat4 CMap4, char c)
    {
        var seg = CMap4.EndCode.FindFirstIndex(x => c <= x);
        var start = CMap4.StartCode[seg];
        if (c < start) return 0;

        var idrange = CMap4.IdRangeOffsets[seg];
        if (idrange == 0) return (c + CMap4.IdDelta[seg]) & 0xFFFF;

        var gindex = (idrange / 2) + c - start - (CMap4.SegCountX2 / 2) + seg;
        return (CMap4.GlyphIdArray[gindex] + CMap4.IdDelta[seg]) & 0xFFFF;
    }

    public static int MeasureString(this TrueTypeFontInfo ttf, string s) => s.Select(x => MeasureChar(ttf, x)).Sum();

    public static int MeasureChar(this TrueTypeFontInfo ttf, char c) => MeasureGID(ttf, CharToGID(ttf, c));

    public static int MeasureGID(this TrueTypeFontInfo ttf, int gid)
    {
        var width = ttf.HorizontalMetrics.Metrics[Math.Min(gid, ttf.HorizontalHeader.NumberOfHMetrics - 1)].AdvanceWidth;
        return ttf.FontHeader.UnitsPerEm == 1000 ? width : width * 1000 / ttf.FontHeader.UnitsPerEm;
    }
}
