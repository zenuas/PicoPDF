using System.Collections.Generic;

namespace PicoPDF.OpenType;

public class FontLoading : IFont
{
    public required string FontFamily { get; init; }
    public required string Style { get; init; }
    public required string FullFontName { get; init; }
    public required string PostScriptName { get; init; }
    public required IFontPath Path { get; init; }
    public required long Position { get; init; }
    public required Dictionary<string, TableRecord> TableRecords { get; init; }
    public required OffsetTable Offset { get; init; }
}
