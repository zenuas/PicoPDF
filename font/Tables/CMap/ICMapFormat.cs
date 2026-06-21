using System;

namespace OpenType.Tables.CMap;

public interface ICMapFormat
{
    public ushort Format { get; init; }

    public Func<int, uint> CreateCharToGID();
}
