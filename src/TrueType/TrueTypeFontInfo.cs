using Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PicoPDF.TrueType;

public class TrueTypeFontInfo
{
    public required string FontFamily { get; init; }
    public required string Style { get; init; }
    public required string FullFontName { get; init; }
    public required string PostScriptName { get; init; }
    public bool Loaded { get; set; } = false;
    public required string Path { get; init; }
    public required long Position { get; init; }
    public required Dictionary<string, TableRecord> TableRecords { get; init; }
    public FontHeaderTable FontHeader { get; set; }
    public MaximumProfileTable MaximumProfile { get; set; }
    public OS2Table OS2 { get; set; }
    public CMapFormat4 CMap4 { get; set; }
    public HorizontalHeaderTable HorizontalHeader { get; set; }
    public HorizontalMetricsTable HorizontalMetrics { get; set; }

    public int CharToGID(char c)
    {
        var seg = CMap4.EndCode.FindFirstIndex(x => c <= x);
        var start = CMap4.StartCode[seg];
        if (c < start) return 0;

        var idrange = CMap4.IdRangeOffsets[seg];
        if (idrange == 0) return (c + CMap4.IdDelta[seg]) & 0xFFFF;

        var gindex = (idrange / 2) + c - start - (CMap4.SegCountX2 / 2) + seg;
        return (CMap4.GlyphIdArray[gindex] + CMap4.IdDelta[seg]) & 0xFFFF;
    }

    public int MeasureString(string s) => s.Select(MeasureChar).Sum();

    public int MeasureChar(char c) => MeasureGID(CharToGID(c));

    public int MeasureGID(int gid)
    {
        var width = HorizontalMetrics.Metrics[Math.Min(gid, HorizontalHeader.NumberOfHMetrics - 1)].AdvanceWidth;
        return FontHeader.UnitsPerEm == 1000 ? width : width * 1000 / FontHeader.UnitsPerEm;
    }
}
