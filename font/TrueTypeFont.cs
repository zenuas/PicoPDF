using OpenType.Tables;
using OpenType.Tables.TrueType;
using System;
using System.Collections.Generic;

namespace OpenType;

public class TrueTypeFont : IOpenTypeRequiredTables
{
    public required string PostScriptName { get; init; }
    public required IFontPath Path { get; init; }
    public required long Position { get; init; }
    public required Dictionary<string, TableRecord> TableRecords { get; init; }
    public required OffsetTable Offset { get; init; }
    public required NameTable Name { get; init; }
    public required FontHeaderTable FontHeader { get; init; }
    public required MaximumProfileTable MaximumProfile { get; init; }
    public required PostScriptTable PostScript { get; init; }
    public required OS2Table? OS2 { get; init; }
    public required HorizontalHeaderTable HorizontalHeader { get; init; }
    public required HorizontalMetricsTable HorizontalMetrics { get; init; }
    public required CMapTable CMap { get; init; }
    public required Func<int, uint> CharToGID { get; init; }
    public required IndexToLocationTable IndexToLocation { get; init; }
    public required IGlyph[] Glyphs { get; init; }

    public required ColorBitmapDataTable? ColorBitmapData { get; init; }
    public required ColorBitmapLocationTable? ColorBitmapLocation { get; init; }
    public required ColorTable? Color { get; init; }
    public required ColorPaletteTable? ColorPalette { get; init; }
    public required StandardBitmapGraphicsTable? StandardBitmapGraphics { get; init; }
    public required ScalableVectorGraphicsTable? ScalableVectorGraphics { get; init; }
}
