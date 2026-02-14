using Mina.Extension;
using PicoPDF.OpenType.Tables;
using System;
using System.Linq;

namespace PicoPDF.OpenType;

public static class Utility
{
    public static bool ContainTrueType(this OffsetTable self) => self.Version == 0x00010000 || self.Version == 0x74727565;

    public static bool ContainCFF(this OffsetTable self) => self.Version == 0x4F54544F;

    public static int MeasureString(this IOpenTypeRequiredTables font, string s) => s.ToUtf32CharArray().Select(x => MeasureChar(font, x)).Sum();

    public static int MeasureChar(this IOpenTypeRequiredTables font, int c) => MeasureGID(font, font.CharToGID(c));

    public static int MeasureGID(this IOpenTypeRequiredTables font, uint gid)
    {
        var width = font.HorizontalMetrics.Metrics[Math.Min(gid, font.HorizontalHeader.NumberOfHMetrics - 1)].AdvanceWidth;
        return font.FontHeader.UnitsPerEm == 1000 ? width : width * 1000 / font.FontHeader.UnitsPerEm;
    }
}
