using System;

namespace OpenType.Tables.CMap;

public interface ICMapFormat : IExportable
{
    public ushort Format { get; init; }

    public Func<int, uint> CreateCharToGID();
    public Func<uint, int?> CreateGIDToChar();
}
