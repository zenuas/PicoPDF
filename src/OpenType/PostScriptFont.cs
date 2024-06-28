using PicoPDF.OpenType.PostScript;
using System.Collections.Generic;

namespace PicoPDF.OpenType;

public class PostScriptFont : IOpenTypeRequiredTables
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
    public required OS2Table OS2 { get; init; }
    public required HorizontalHeaderTable HorizontalHeader { get; init; }
    public required HorizontalMetricsTable HorizontalMetrics { get; init; }
    public required CMapTable CMap { get; init; }
    public required CMapFormat4 CMap4 { get; init; }
    public required List<(int Start, int End)> CMap4Range { get; init; }
    public Dictionary<char, int> CMap4Cache { get; init; } = [];
    public required CompactFontFormat CompactFontFormat { get; init; }
}
