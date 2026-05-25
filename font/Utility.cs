using Mina.Extension;
using OpenType.Tables;
using System;
using System.Linq;

namespace OpenType;

public static class Utility
{
    public static bool ContainTrueType(this OffsetTable self) => self.SfntVersion == 0x00010000 || self.SfntVersion == 0x74727565;

    public static bool ContainCFF(this OffsetTable self) => self.SfntVersion == 0x4F54544F;

    public static double MeasureString(this IOpenTypeFont font, string s) => s.ToUtf32CharArray().Select(x => MeasureChar(font, x)).Sum();

    public static double MeasureChar(this IOpenTypeFont font, int c) => MeasureGID(font, font.CharToGID(c));

    public static double MeasureGID(this IOpenTypeFont font, uint gid) => (double)font.GetAdvanceWidth(gid) / font.FontHeader.UnitsPerEm;

    // If numberOfHMetrics is less than the total number of glyphs,
    // then the hMetrics array is followed by an array for the left side bearing values of the remaining glyphs.
    public static int GetAdvanceWidth(this IOpenTypeFont font, uint gid) => font.HorizontalMetrics.Metrics[Math.Min(gid, font.HorizontalHeader.NumberOfHMetrics - 1)].AdvanceWidth;
}
