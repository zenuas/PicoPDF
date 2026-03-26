using OpenType.Tables;
using System;

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
    public Func<uint, IOutline[]> GIDToOutline { get; init; }

    public ColorBitmapDataTable? ColorBitmapData { get; init; }
    public ColorBitmapLocationTable? ColorBitmapLocation { get; init; }
    public ColorTable? Color { get; init; }
    public ColorPaletteTable? ColorPalette { get; init; }
    public StandardBitmapGraphicsTable? StandardBitmapGraphics { get; init; }
    public ScalableVectorGraphicsTable? ScalableVectorGraphics { get; init; }
}
