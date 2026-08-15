using Mina.Extension;
using OpenType.Tables;
using OpenType.Tables.Subtable;
using Svg.Outline;
using System;
using System.Linq;

namespace OpenType;

public interface IOpenTypeFont : IOpenTypeHeader
{
    public FontHeaderTable FontHeader { get; init; }
    public MaximumProfileTable MaximumProfile { get; init; }
    public PostScriptTable PostScript { get; init; }
    public OS2Table? OS2 { get; init; }
    public HorizontalHeaderTable HorizontalHeader { get; init; }
    public HorizontalMetricsTable HorizontalMetrics { get; init; }
    public CMapTable CMap { get; init; }
    public Func<int, uint> CharToGID { get; init; }
    public Func<uint, bool, IOutline[]> GIDToOutline { get; init; }

    public VerticalHeaderTable? VerticalHeader { get; init; }
    public VerticalMetricsTable? VerticalMetrics { get; init; }
    public GlyphPositioningTable? GlyphPositioning { get; init; }
    public GlyphSubstitutionTable? GlyphSubstitution { get; init; }
    public Func<uint, ValueRecord?> GetPositionPlacement { get; init; }

    public ColorBitmapDataTable? ColorBitmapData { get; init; }
    public ColorBitmapLocationTable? ColorBitmapLocation { get; init; }
    public ColorTable? Color { get; init; }
    public ColorPaletteTable? ColorPalette { get; init; }
    public StandardBitmapGraphicsTable? StandardBitmapGraphics { get; init; }
    public ScalableVectorGraphicsTable? ScalableVectorGraphics { get; init; }

    public double HorizontalMeasureString(string s) => s.ToUtf32CharArray().Select(HorizontalMeasureChar).Sum();

    public double HorizontalMeasureChar(int c) => HorizontalMeasureGID(CharToGID(c));

    public double HorizontalMeasureGID(uint gid) => (double)GetAdvanceWidth(gid) / FontHeader.UnitsPerEm;

    public double VerticalMeasureString(string s) => s.ToUtf32CharArray().Select(VerticalMeasureChar).Sum();

    public double VerticalMeasureChar(int c) => VerticalMeasureGID(CharToGID(c));

    public double VerticalMeasureGID(uint gid) => (double)(GetAdvanceHeight(gid) is { } ah ? (ah.Height - ah.TopSideBearing) : 0) / FontHeader.UnitsPerEm;

    // If numberOfHMetrics is less than the total number of glyphs,
    // then the hMetrics array is followed by an array for the left side bearing values of the remaining glyphs.
    public int GetAdvanceWidth(uint gid)
    {
        var pos = GetPositionPlacement(gid);
        return HorizontalMetrics.Metrics[Math.Min(gid, HorizontalHeader.NumberOfHMetrics - 1)].AdvanceWidth.Value - (pos?.XPlacement ?? 0) + (pos?.XAdvance ?? 0);
    }

    public (int Height, int TopSideBearing)? GetAdvanceHeight(uint gid)
    {
        if (gid == 0 || VerticalMetrics is not { } vmtx) return null;

        var index = gid - 1;
        var pos = GetPositionPlacement(gid);
        if (index < vmtx.Metrics.Length)
        {
            var metric = vmtx.Metrics[index];
            return (metric.AdvanceHeight.Value + (pos?.YAdvance ?? 0), -(pos?.YPlacement ?? metric.TopSideBearing.Value));
        }
        else
        {
            // This array contains the top sidebearings of glyphs not represented in the first array, and all the glyphs in this array must have the same advance height as the last entry in the vMetrics array.
            // All entries in this array are therefore monospaced.
            // The number of entries in this array is calculated by subtracting the value of numOfLongVerMetrics from the number of glyphs in the font.
            // The sum of glyphs represented in the first array plus the glyphs represented in the second array therefore equals the number of glyphs in the font.
            return (vmtx.Metrics[^1].AdvanceHeight.Value + (pos?.YAdvance ?? 0), -(pos?.YPlacement ?? vmtx.TopSideBearing[index - vmtx.Metrics.Length].Value));
        }
    }
}
