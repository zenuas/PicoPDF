using System.Collections.Generic;

namespace PicoPDF.OpenType;

public class FontTableRecords : IOpenTypeHeader
{
    public required string PostScriptName { get; init; }
    public required IFontPath Path { get; init; }
    public required long Position { get; init; }
    public required Dictionary<string, TableRecord> TableRecords { get; init; }
    public required OffsetTable Offset { get; init; }
    public required NameTable Name { get; init; }
}
