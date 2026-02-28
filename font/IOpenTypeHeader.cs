using OpenType.Tables;
using System.Collections.Generic;

namespace OpenType;

public interface IOpenTypeHeader
{
    public string PostScriptName { get; init; }
    public IFontPath Path { get; init; }
    public long Position { get; init; }
    public Dictionary<string, TableRecord> TableRecords { get; init; }
    public OffsetTable Offset { get; init; }
    public NameTable Name { get; init; }
}
