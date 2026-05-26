using Mina.Extension;
using OpenType.Tables;
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

    public ColorBitmapDataTable? ColorBitmapData { get; init; }
    public ColorBitmapLocationTable? ColorBitmapLocation { get; init; }
    public ColorTable? Color { get; init; }
    public ColorPaletteTable? ColorPalette { get; init; }
    public StandardBitmapGraphicsTable? StandardBitmapGraphics { get; init; }
    public ScalableVectorGraphicsTable? ScalableVectorGraphics { get; init; }

    public double MeasureString(string s) => s.ToUtf32CharArray().Select(x => MeasureChar(x)).Sum();

    public double MeasureChar(int c) => MeasureGID(CharToGID(c));

    public double MeasureGID(uint gid) => (double)GetAdvanceWidth(gid) / FontHeader.UnitsPerEm;

    // If numberOfHMetrics is less than the total number of glyphs,
    // then the hMetrics array is followed by an array for the left side bearing values of the remaining glyphs.
    public int GetAdvanceWidth(uint gid) => HorizontalMetrics.Metrics[Math.Min(gid, HorizontalHeader.NumberOfHMetrics - 1)].AdvanceWidth;
}
