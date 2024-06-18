using System.Collections.Generic;

namespace PicoPDF.OpenType;

public class FontInfo
{
    public required string FontFamily { get; init; }
    public required string Style { get; init; }
    public required string FullFontName { get; init; }
    public required string PostScriptName { get; init; }
    public bool Loaded { get; set; } = false;
    public required string Path { get; init; }
    public required long Position { get; init; }
    public required Dictionary<string, TableRecord> TableRecords { get; init; }
    public required OffsetTable Offset { get; set; }
    public FontHeaderTable FontHeader { get; set; } = default!;
    public MaximumProfileTable MaximumProfile { get; set; } = default!;
    public PostScriptTable PostScript { get; set; } = default!;
    public OS2Table OS2 { get; set; } = default!;
    public EncodingRecord[] EncodingRecords { get; set; } = default!;
    public CMapFormat4 CMap4 { get; set; } = default!;
    public List<(int Start, int End)> CMap4Range { get; init; } = [];
    public Dictionary<char, int> CMap4Cache { get; init; } = [];
    public HorizontalHeaderTable HorizontalHeader { get; set; } = default!;
    public HorizontalMetricsTable HorizontalMetrics { get; set; } = default!;
    public CompactFontFormat? CompactFontFormat { get; set; }
}
