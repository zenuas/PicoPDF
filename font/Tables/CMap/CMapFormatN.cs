using System;

namespace OpenType.Tables.CMap;

public class CMapFormatN : ICMapFormat
{
    public required ushort Format { get; init; }

    public Func<int, uint> CreateCharToGID() => _ => 0;
}
