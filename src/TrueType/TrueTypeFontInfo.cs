using System.Collections.Generic;

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
}
